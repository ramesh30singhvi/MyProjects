import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, Injectable, Input, OnDestroy, OnInit, Renderer2, ViewEncapsulation } from "@angular/core";
import { AuditService } from "../services/audit.service";
import { Observable, of, Subscription } from 'rxjs';
import { AuditServiceApi } from "../../services/audit-api.service";
import { find, findIndex, first, map } from "rxjs/operators";
import {  IAuditKeyword, IHourKeyword, IKeyword, IOption, IProgressNoteKeyword } from "src/app/models/audits/audits.model";
import * as moment from "moment";
import { formatDate } from "@angular/common";
import { FormBuilder, FormControl, FormGroup, Validators } from "@angular/forms";
import { HttpErrorResponse } from "@angular/common/http";
import { MM_DD_YYYY_DOT, MM_DD_YYYY_HH_MM_A_SLASH, YYYY_MM_DD_DASH } from "src/app/common/constants/date-constants";
import { NgbDate, NgbDateAdapter, NgbDateParserFormatter, NgbTimepickerConfig, NgbTimeStruct, NgbTimeAdapter } from "@ng-bootstrap/ng-bootstrap";
import { CustomDateParserAdapter, CustomDateParserFormatter } from "src/app/shared/datepicker-adapters";
import { IHighAlert } from "../../models/forms/forms.model";

const pad = (i: number): string => i < 10 ? `0${i}` : `${i}`;

@Injectable()
export class NgbTimeStringAdapter extends NgbTimeAdapter<string> {

  fromModel(value: string | null): NgbTimeStruct | null {
    if (!value) {
      return null;
    }
    const split = value.split(':');
    return {
      hour: parseInt(split[0], 10),
      minute: parseInt(split[1], 10),
      second: parseInt(split[2], 10)
    };
  }

  toModel(time: NgbTimeStruct | null): string | null {
    return time != null ? `${pad(time.hour)}:${pad(time.minute)}:${pad(time.second)}` : null;
  }
}

@Component({
  selector: "app-keyword-input-section",
  templateUrl: "./keyword-input-section.component.html",
  styleUrls: ["./keyword-input-section.component.scss"],
  providers: [
    {provide: NgbDateAdapter, useClass: CustomDateParserAdapter},
    { provide: NgbDateParserFormatter, useClass: CustomDateParserFormatter },
    NgbTimepickerConfig,
    { provide: NgbTimeAdapter, useClass: NgbTimeStringAdapter }
  ],
  encapsulation: ViewEncapsulation.None,
})

export class KeywordInputSectionComponent implements OnInit, AfterViewInit, OnDestroy {

  public isEditable: boolean;

  public keywords: IKeyword[] = [];
  public selectedKeyword: IKeyword = null;
  public selectedHighAlertCategory: IOption = null;
  public selectedProgressNoteKeyword: IProgressNoteKeyword = null;
  public selectedAuditKeyword: IAuditKeyword;

  public matchedKeywords: IAuditKeyword[] = [];

  public keywordForm : FormGroup;

  public errors: any[];

  public auditId: number;
  public auditUseHighAlert;
  private auditDateFrom: Date;
  private auditDateTo: Date;
  private formVersionId: number;

  private defaultDate: string = '';

  public minDate: NgbDate | null;
  public maxDate: NgbDate | null;

  private subscription: Subscription;
  public highAlertCategories$: Observable<IOption[]>;

  private hourElement: any;
  private prevHour: number;
  private minuteElement: any;

  constructor(
    private auditServiceApi: AuditServiceApi,
    private auditService: AuditService,
    private formBuilder: FormBuilder,
    private renderer: Renderer2,
    private timePickerConfig: NgbTimepickerConfig, private cdr: ChangeDetectorRef,
    private elem: ElementRef) {
      this.timePickerConfig.seconds = false;
      this.timePickerConfig.spinners = false;
      this.keywordForm = this.formBuilder.group({
        resident: new FormControl({value: '', disabled: !this.isEditable}, Validators.required),
        customKeyword: new FormControl({value: '', disabled: !this.isEditable}),
        date: new FormControl({value: '', disabled: !this.isEditable}),
        time: new FormControl({value: '', disabled: !this.isEditable}),
        description: new FormControl({ value: '', disabled: !this.isEditable }, Validators.required),
        highAlertCategory: new FormControl({ value:null, disabled: !this.isEditable }),
        highAlertDescription: new FormControl({ value: '', disabled: !this.isEditable }),
        highAlertNotes: new FormControl({ value: '', disabled: !this.isEditable }),
      });



    this.highAlertCategories$ = this.auditServiceApi.getHighAlertCategories();
    this.subscription = new Subscription();

    this.subscription.add(
      this.auditService
      .isEditable$
      .subscribe((isEditable) => {
        this.isEditable = isEditable;

        if(isEditable) {
          this.keywordForm.enable();
        } else {
          this.keywordForm.disable();
        }
      }));

    this.subscription.add(
      this.auditService
      .audit$
      .subscribe((audit) => {
        this.auditId = audit?.id;
        this.formVersionId = audit?.form?.id;
        this.auditDateFrom = audit?.incidentDateFrom;
        this.auditDateTo = audit?.incidentDateTo;
  
        if(this.auditId && this.isEditable) {
          this.keywordForm.enable();
     


          if(this.isDateDisabled()) {
            this.keywordForm.controls['date'].disable();
            this.defaultDate =  moment(this.auditDateFrom).format(MM_DD_YYYY_DOT);
          } else {
            this.keywordForm.controls['date'].setValidators(Validators.required);

            const minDateMoment = moment(this.auditDateFrom);
            const maxDateMoment = moment(this.auditDateTo);

            this.minDate = new NgbDate(minDateMoment.year(), minDateMoment.month() + 1, minDateMoment.date());
            this.maxDate = new NgbDate(maxDateMoment.year(), maxDateMoment.month() + 1, maxDateMoment.date());
          }
        } else {
          this.keywordForm.disable();
        }
      }));

    this.subscription.add(
      this.auditService
      .hourKeyword$
      .subscribe((hourKeyword: IHourKeyword) => {
        console.log("hourKeyword", hourKeyword);
        if(hourKeyword?.keywords && hourKeyword?.keywords.length > 0) {
          this.keywords = hourKeyword.keywords.filter(keyword => keyword.hidden == null);
          this.selectedKeyword = hourKeyword.keywords[0];
          this.handleKeywordChanged(this.selectedKeyword);
        }

        this.matchedKeywords = hourKeyword?.matchedKeywords;
      }));

    this.subscription.add(
      this.auditService
      .progressNoteKeyword$
      .subscribe((progressNoteKeyword) => {
        this.selectedProgressNoteKeyword = progressNoteKeyword;
        this.keywordForm.reset();
        this.selectedAuditKeyword = null;

        if(progressNoteKeyword)
        {
          this.keywordForm?.setValue({
            resident: progressNoteKeyword.resident || '',
            date: progressNoteKeyword.effectiveDate
            ? moment(progressNoteKeyword.effectiveDate).utcOffset(progressNoteKeyword?.timeZoneOffset).format(MM_DD_YYYY_DOT)
            : this.defaultDate,
            time: progressNoteKeyword.effectiveDate ? moment(progressNoteKeyword.effectiveDate).utcOffset(progressNoteKeyword?.timeZoneOffset).format('HH:mm') : '',
            description: ''
          });
        }
      }));
  }

  ngOnInit() {
    this.clearForm();
  }

  ngAfterViewInit(){
    this.hourElement = this.elem.nativeElement.querySelector('[placeholder="HH"]');
    this.minuteElement = this.elem.nativeElement.querySelector('[placeholder="MM"]');

    this.unlistener = this.renderer.listen(this.hourElement, "input", event => {
      this.timeChanged(event.target.value);
    });
  }

  private unlistener: () => void;

  ngOnDestroy(): void {
    this.auditService.setKeyword(null);
    this.auditService.setHourKeyword(null);
    this.auditService.setProgressNoteKeyword(null);
    this.subscription.unsubscribe();

    this.unlistener();
  }

  public get submitButtonTitle(): string {
    return this.selectedAuditKeyword?.id ? 'Save' : 'Add';
  }

  public onKeywordChanged(keyword: IOption): void {
    this.handleKeywordChanged(keyword);
  }
  onChangeResident(resident: string) {

    let prevValue:string = this.keywordForm.controls["highAlertDescription"]?.value;


    let newValue = resident?.length > 0 ? "<" + resident + ">" : "";
    if (prevValue != null) {
      let resPrevStart = prevValue.indexOf("<");
      let resPrevEnd = prevValue.indexOf(">");
      let startText = ""; let endText = "";
      if (resPrevStart > -1 )
        startText = prevValue.substring(0, resPrevStart - 1);
      if (resPrevEnd > -1)
        endText = prevValue.substring(resPrevEnd + 1, prevValue.length);

      newValue = startText + newValue + endText;
    }
    this.keywordForm.controls["highAlertDescription"]?.patchValue(newValue);
  }
 
  public onPrevKeywordClick(): void {
    if (this.keywords.length === 0) {
      return;
    }

    if (!this.selectedKeyword) {
      this.selectedKeyword = this.keywords[0];
      return;
    }

    let keywordIndex = this.keywords.findIndex(keyword => keyword.id === this.selectedKeyword.id);

    if (keywordIndex == 0)
      this.selectedKeyword = this.keywords[this.keywords.length - 1];
    else
      this.selectedKeyword = this.keywords[keywordIndex - 1];

    this.handleKeywordChanged(this.selectedKeyword);
  }

  public onNextKeywordClick(): void {
    if(this.keywords.length === 0) {
      return;
    }

    if(!this.selectedKeyword) {
      this.selectedKeyword = this.keywords[0];
      return;
    }

    let keywordIndex = this.keywords.findIndex(keyword => keyword.id === this.selectedKeyword.id);

    if (keywordIndex == this.keywords.length - 1)
      this.selectedKeyword = this.keywords[0];
    else
      this.selectedKeyword = this.keywords[keywordIndex + 1];

    this.handleKeywordChanged(this.selectedKeyword);
  }

  public clearClick() {
    this.clearForm();
    return false;
  }

  public onHighAlertChanged(event) {
    this.auditUseHighAlert = event.target.checked;
    if (this.auditUseHighAlert) {
      var resident = this.keywordForm.controls["resident"]?.value;
      resident = "<" + resident + ">";
      this.keywordForm.controls["highAlertDescription"]?.patchValue(resident);
    }
  }
  public onHighAlertCategroryChanged(categ: IOption) {
   
    if (!categ) {
      return;
    }
    if (this.errors && this.errors["HighAlertCategory"]) {
      this.errors["HighAlertCategory"] = null;
    }

  }
  
  public onChangeDescription(value) {

    if (value != "") {
      if (this.errors && this.errors["HighAlertDescription"])
        this.errors["HighAlertDescription"] = null;
    }
  }
  public submit() {
    if(this.keywordForm.invalid) {
      return;
    }

    const noteDate = this.keywordForm.controls['date'].value;
    const noteTime = this.keywordForm.controls['time'].value;

    if (this.auditUseHighAlert == false) {
      this.keywordForm.controls["highAlertCategory"].reset();
      this.keywordForm.controls["highAlertDescription"].reset();
      this.keywordForm.controls["highAlertNotes"].reset();
    } else {
      if (this.errors == undefined)
        this.errors = [];
      if (this.keywordForm.controls["highAlertCategory"].value == null) {
        this.errors["HighAlertCategory"] = "Please select category";
        return;
      }
      if (this.keywordForm.controls["highAlertDescription"].value == null) {
        this.errors["HighAlertDescription"] = "Please enter description";
        return;
      }

    }
    this.errors = [];
    const auditKeyword: IAuditKeyword = {
      ...this.keywordForm.value,
      id: this.selectedAuditKeyword?.id,
      auditId: this.auditId,
      keywordId: this.selectedKeyword.id,
      progressNoteDate: moment(noteDate, MM_DD_YYYY_DOT).format(YYYY_MM_DD_DASH),
      progressNoteTime: noteTime,
      formVersionId: this.formVersionId,

    };

    console.log(auditKeyword);
    if(!auditKeyword.id) {
      this.auditServiceApi.addAuditKeyword(auditKeyword)
        .pipe(first())
        .subscribe((keywordValue: IAuditKeyword) => {

        if (keywordValue.keyword!=null) {
          this.matchedKeywords.unshift({...keywordValue, keyword: keywordValue.keyword});
        } else {
          this.matchedKeywords.unshift({...keywordValue, keyword: this.selectedKeyword});
        }

        this.clearForm();
      },
      (response: HttpErrorResponse) =>
      {
        this.errors = response.error.errors;
        console.error(response);
      });
    } else {

      this.auditServiceApi.editAuditKeyword(auditKeyword)
      .pipe(first())
      .subscribe((keywordValue: IAuditKeyword) => {
        this.matchedKeywords = this.matchedKeywords.map((keyword: IAuditKeyword)=>
         {return keyword.id === keywordValue.id
            ? {
              ...keyword,
              resident: keywordValue.resident,
              progressNoteDateTime: keywordValue.progressNoteDateTime,
              progressNoteDate: keywordValue.progressNoteDate,
              progressNoteTime: keywordValue.progressNoteTime,
              description: keywordValue.description,
              highAlertAuditValue: keywordValue.highAlertAuditValue
             }
            : keyword
         });


        this.clearForm();
      },
      (response: HttpErrorResponse) =>
      {
        this.errors = response.error.errors;
        console.error(response);
      });
    }
  }

  public onKeywordDeleteClick(auditKeywordId: number) {
      this.auditServiceApi.deleteKeyword(auditKeywordId)
      .pipe(first())
      .subscribe((isDeleted: boolean) => {
        if (isDeleted) {
          this.clearForm();
          this.matchedKeywords = this.matchedKeywords.filter((auditKeyword) => auditKeyword.id !== auditKeywordId);
        }
      });
  }

  public onAuditKeywordClick(auditKeyword: IAuditKeyword) {

    console.log("auditKeyword", auditKeyword)

    this.selectedKeyword = auditKeyword.keyword;
    this.handleKeywordChanged(auditKeyword.keyword);

    this.selectedAuditKeyword = auditKeyword;

    if (auditKeyword.highAlertAuditValue == null)
      this.auditUseHighAlert = false;
    else if (auditKeyword.highAlertAuditValue?.id > 0) {
      this.auditUseHighAlert = true;
    }
  

    this.keywordForm?.setValue({
      resident: auditKeyword.resident || '',
      date: auditKeyword.progressNoteDate
            ? moment(auditKeyword.progressNoteDate).format(MM_DD_YYYY_DOT)
            : this.defaultDate,
      time: auditKeyword.progressNoteTime?.substring(0, 5) ?? '',
      description: auditKeyword.description,
      customKeyword: null,
      highAlertCategory: auditKeyword.highAlertAuditValue != undefined ? auditKeyword.highAlertAuditValue?.highAlertCategory : null,
      highAlertDescription: auditKeyword.highAlertAuditValue != undefined ? auditKeyword.highAlertAuditValue?.highAlertDescription : null,
      highAlertNotes: auditKeyword.highAlertAuditValue != undefined ? auditKeyword.highAlertAuditValue?.highAlertNotes : null
    });
  }

  public timeChanged(value: string): void {
    if(this.hourElement !== document.activeElement || !value){
      return;
    }

    const hour = Number.parseInt(value.substring(0, 2));

    if(hour > 2) {
      this.minuteElement.focus();
      this.minuteElement.select();
    }
  }

  private handleKeywordChanged(keyword: IOption): void {
    this.clearForm();
    this.auditService.setKeyword(keyword);
  }

  private clearForm(): void {
    this.selectedProgressNoteKeyword = null;
    this.selectedAuditKeyword = null;
    this.selectedHighAlertCategory = null;
    this.auditUseHighAlert = false;
    this.keywordForm.reset();

    this.keywordForm?.patchValue({
      date: this.defaultDate,
    });
  }

  private isDateDisabled() {
    return !this.auditDateTo || this.auditDateFrom === this.auditDateTo;
  }
}
