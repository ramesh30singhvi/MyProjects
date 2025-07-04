import { Component, OnInit, ViewEncapsulation } from "@angular/core";
import {
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
} from "@angular/forms";
import { Observable, Subscription } from "rxjs";
import { first } from "rxjs/operators";
import {
    Audit,
  IAuditTrackerAnswer,
  IOption,
  ITracker,
} from "src/app/models/audits/audits.model";
import {
  FieldBase,
  IFieldBase,
  ITrackerOption,
  ITrackerQuestion,
} from "src/app/models/audits/questions.model";
import { AuditServiceApi } from "src/app/services/audit-api.service";
import { AuditService } from "../services/audit.service";
import { v4 as uuidv4 } from "uuid";
import { HttpErrorResponse } from "@angular/common/http";
import { ControlService } from "src/app/services/control.service";
import { NgbDateParserFormatter } from "@ng-bootstrap/ng-bootstrap";
import { FormFieldTypes, IFormFieldItem } from "src/app/models/forms/forms.model";

@Component({
  selector: "app-tracker",
  templateUrl: "./tracker.component.html",
  styleUrls: ["./tracker.component.scss"],
  encapsulation: ViewEncapsulation.None
})
export class TrackerComponent implements OnInit {
  public trackerForm: FormGroup;
  public highAlertForm: FormGroup;
  public isEditable: boolean;

  public keywords: ITrackerQuestion[] = [];

  public questions: FieldBase<any>[] = [];
  public showHighAlert: boolean = false;
  public pivotAnswerGroups: any[] = [];

  public selectedAnswerGroup: any;
  public selectedDescription: string = "";
  public selectedNotes: string = "";
  public fieldTypes = FormFieldTypes;
  selectedHighAlertCategory: IOption;
  private auditId: number;
  private _audit: Audit;
  private subscription: Subscription;
  public highAlertCategories$: Observable<IOption[]>;
  constructor(
    private auditServiceApi: AuditServiceApi,
    private auditService: AuditService,
    private controlService: ControlService,
    private formBuilder: FormBuilder,
    public formatter: NgbDateParserFormatter
  ) {
    this.subscription = new Subscription();

    this.trackerForm = this.formBuilder.group({});
    this.highAlertCategories$ = this.auditServiceApi.getHighAlertCategories();


    this.subscription.add(
      this.auditService.isEditable$.subscribe((isEditable) => {
        this.isEditable = isEditable;
      })
    );
    this.highAlertForm = this.formBuilder.group({
      highAlertCategory: new FormControl({ value: null }, Validators.required),
      highAlertDescription: new FormControl({ value: '' }, Validators.required),
      highAlertNotes: new FormControl({ value: '' }),
    }); 
    this.subscription.add(
      this.auditService.audit$.subscribe((audit) => {
        this.auditId = audit?.id;
        this._audit = audit;
        this.showHighAlert = this._audit?.form?.useHighAlert;
      })
    );

    this.subscription.add(
      this.auditService.tracker$.subscribe((tracker) => {
        console.log("tracker", tracker);
        console.log("audit ", this.auditId);

        this.keywords = tracker?.questions;
        this.pivotAnswerGroups = tracker?.pivotAnswerGroups;
        this.selectedAnswerGroup = null;

        this.questions = [];

        this.keywords?.forEach((keyword, index) => {
          if(tracker?.sortModel && tracker?.sortModel.orderBy === keyword.id.toString()){
            keyword.sortOrder = tracker?.sortModel.sortOrder;
          }

          const options: IFieldBase = {
            id: keyword.id,
            key: `question_${keyword.id}`,
            required: keyword.trackerOption.isRequired,
            label: keyword.question,
            order: keyword.sequence,
            value: this.getResidentName(keyword),
            options: keyword.trackerOption.items?.map(
              (item: IFormFieldItem) => {
                return { id: item.id, value:item.value };
              }
            ),
          };

          this.questions.push(
            this.controlService.getControl(
              keyword.trackerOption?.fieldType?.id,
              options
            )
          );
        });

        this.trackerForm = this.controlService.toFormGroup(this.questions);

        if (this.isEditable && this.auditId) {
          this.controlService.enableForm(this.trackerForm);
          if (this.trackerUseHighAlert)
            this.controlService.enableForm(this.highAlertForm);
        } else {
          this.controlService.disableForm(this.trackerForm);
          this.controlService.disableForm(this.highAlertForm);
        }
      })
    );
  }
  getResidentName(keyword: ITrackerQuestion): string{
   
    if (keyword != null && keyword.question != null && this._audit?.id) {
      if (keyword.question.toLowerCase() == "resident name" || keyword.question.toLowerCase() == "resident" || keyword.question.toLowerCase().indexOf("resident") != -1) {
       // this.showHighAlert = this._audit.form.useHighAlert;// true;
        if (this._audit.resident != null) {
          return this._audit.resident;
        }
      }
    }
    return null;
  }
  public errors: any[];
  trackerUseHighAlert: boolean;
  public useHighAlert(value) {
    console.log("use hif value " + value);

    this.trackerUseHighAlert = value;
    console.log(this.residentName);

    if (this.trackerUseHighAlert)
      this.OnResidentNameChanged(this.residentName);
  }
  ngOnInit() {
    if (this.isEditable) {
      this.controlService.enableForm(this.trackerForm);
    } else {
      this.controlService.disableForm(this.trackerForm);
    }
  }

  public get submitButtonTitle(): string {
    return this.selectedAnswerGroup?.GroupId ? "Save" : "Add";
  }

  residentName: string = "";
  public OnResidentNameChanged(resident: string) {

    let controlValue = this.highAlertForm.controls['highAlertDescription']?.value;
  
    let prevValue = (typeof controlValue === "string") ? controlValue : "";
    let newValue = resident?.length > 0 ? "<" + resident + ">" : resident;
    this.residentName = resident;

    if (prevValue != "") {
      let resPrevStart = prevValue?.indexOf("<");
      let resPrevEnd = prevValue?.indexOf(">");
      let startText = ""; let endText = "";
      if (resPrevStart > -1)
        startText = prevValue?.substring(0, resPrevStart - 1);
      if (resPrevEnd > -1)
        endText = prevValue?.substring(resPrevEnd + 1, prevValue?.length);

      newValue = startText + newValue + endText;

    }

    this.selectedDescription = newValue;
    this.highAlertForm.controls['highAlertDescription']?.patchValue(newValue);
  }
  public submit() {
    if (!this.isFormValid() || !this.isEditable) {
      return;
    }
    if (this.trackerUseHighAlert) {
      if (!this.highAlertForm.valid) {
        if (this.errors == undefined)
          this.errors = [];
        if (this.highAlertForm.controls["highAlertCategory"].value == null)
          this.errors['HighAlertCategory'] = "Please select category";
        return;
      }
    }
    const trackerFormKeys = Object.keys(this.trackerForm.value);

    const groupId = this.selectedAnswerGroup?.GroupId ?? uuidv4();
    let newAnswers: IAuditTrackerAnswer[] = [];

    trackerFormKeys.forEach((key) => {
      const question = this.questions.find((question) => question.key === key);

      if(!question) {
        return;
      }

      let value = this.trackerForm.value[key];
      
      newAnswers.push({
        id: this.selectedAnswerGroup?.[question.id]?.id,
        auditId: this.auditId,
        questionId: question.id,
        answer: this.controlService.parseToStorageValue(question, value),
        groupId,

      });
    });
    let highAlert = null;
    if (this.trackerUseHighAlert) {
      highAlert = this.highAlertForm?.value;
    }

    if (!this.selectedAnswerGroup?.GroupId) {
      this.auditServiceApi
        .addAuditTrackerAnswer(this.auditId, newAnswers, highAlert)
        .pipe(first())
        .subscribe({
          next: (tracker: ITracker) => {

            this.pivotAnswerGroups = tracker.pivotAnswerGroups;

            this.clearForm();
          },
          error: (response: HttpErrorResponse) => {
            console.error(response);
          },
        });
    } else {
      this.auditServiceApi
        .editAuditTrackerAnswer(this.auditId, groupId, newAnswers,highAlert)
        .pipe(first())
        .subscribe({
          next: (tracker: ITracker) => {
            this.pivotAnswerGroups = tracker.pivotAnswerGroups;

            this.clearForm();
          },
          error: (response: HttpErrorResponse) => {
            console.error(response);
          },
        });
    }
  }

  public onAnswersDeleteClick(groupId: string): void {
    this.auditServiceApi
      .deleteAuditTrackerAnswers(this.auditId, groupId)
      .pipe(first())
      .subscribe((isDeleted: boolean) => {
        if (isDeleted) {
          this.clearForm();

          this.pivotAnswerGroups = this.pivotAnswerGroups.filter(
            (group) => group.GroupId !== groupId
          );
        }
      });
  }

  public onTrackerAswersGroupClick(group: any): void {
    this.selectedAnswerGroup = group;
    var highAlert = null;
    this.questions.forEach((question: FieldBase<string>) => {
      if(!this.selectedAnswerGroup) {
        return;
      }

      question.value = this.selectedAnswerGroup[question.id].answer;
      console.log(this.selectedAnswerGroup[question.id]);

      if (highAlert == null)
       highAlert = this.selectedAnswerGroup[question.id].
        highAlertAuditValue;

      this.trackerForm.patchValue({
        [question.key]: this.controlService.parseToControlValue(
          question.controlType,
          question
        ),
      });
      
    });



    this.trackerUseHighAlert = highAlert != null;
    this.selectedHighAlertCategory = highAlert?.highAlertCategory;
    this.highAlertForm.patchValue({
      highAlertCategory: highAlert?.highAlertCategory,
      highAlertNotes: highAlert?.highAlertNotes,
      highAlertDescription: highAlert?.highAlertDescription

    });
    this.selectedDescription = highAlert?.highAlertDescription;
    this.selectedNotes = highAlert?.highAlertNotes;
  }

  public onHeaderColumnClick(keywordId: number): void {
    if(!this.isEditable) {
      return;
    }

    this.keywords
      .filter(((keyword: ITrackerQuestion) => keyword.id != keywordId))
      .forEach((keyword: ITrackerQuestion) => keyword.sortOrder = null);

    let keyword = this.keywords.find((keyword: ITrackerQuestion) => keyword.id === keywordId);

    if(!keyword){
      return;
    }

    let order;

    switch(keyword.sortOrder) {
      case undefined:
      case null:
        order = 'asc';
        break;
      case 'asc':
        order = 'desc';
        break;
      default:
        order = null;
        break;
    }

    this.auditServiceApi.saveAuditSortModel(this.auditId, {orderBy: keywordId.toString(), sortOrder: order})
    .pipe(first())
    .subscribe({
      next: (tracker: ITracker) => {
        this.pivotAnswerGroups = tracker.pivotAnswerGroups;

        let keyword = this.keywords.find((keyword: ITrackerQuestion) => keyword.id.toString() === tracker.sortModel?.orderBy);
        if(keyword){
          keyword.sortOrder = tracker.sortModel?.sortOrder;
        } else {
          this.keywords.forEach((keyword: ITrackerQuestion) => keyword.sortOrder = null);
        }
      },
      error: (response: HttpErrorResponse) => {
        console.error(response);
      },
    });
  }

  public isFormValid(): boolean {
    return this.trackerForm.valid && Object.entries(this.trackerForm.value).every((formArray) => {
      const key = formArray[0];
      const value = formArray[1];

      if (value instanceof FormGroup) {
        const isRequired = this.questions.find((question) => question.key === key)?.required;
        const toggleValue = value.value;
        return isRequired ? Object.values(toggleValue).includes(true) : true;
      }

      return true;
    });
  }

  public onClearClick(): void {
    this.clearForm();
  }

  private clearForm(): void {
    this.selectedAnswerGroup = null;
    this.trackerUseHighAlert = false;
   
    this.controlService.clearForm(this.trackerForm);
    this.highAlertForm.reset();

    this.questions
    .filter((q: FieldBase<any>) => q.controlType === FormFieldTypes.textArea.fieldType)
    .map((q: FieldBase<any>) => {
      q.editor.setContent('')
    });
  }

  public getGroupCompliance(group: any) {
    var totalQuestions = 0;
    var totalAnsweredQuestions = 0;
    this.keywords.filter(keyword => keyword.trackerOption.compliance==true).forEach(keyword => {
      totalQuestions++;
      if (group[keyword.id].formattedAnswer.toLowerCase() != "NO".toLowerCase()) {
        totalAnsweredQuestions++;
      }

      if (keyword.question.toLocaleLowerCase().includes("has the resident been refusing") && group[keyword.id].formattedAnswer.toLowerCase() == "NO".toLowerCase()) {
        totalAnsweredQuestions++;
      }
    })
    const totalCompliance = (totalAnsweredQuestions / totalQuestions) * 100;
    return Math.round(totalCompliance);
  }


  public get getTotalComplianceQuestions(): number {
    return this.keywords.filter(keyword => keyword.trackerOption.compliance==true).length;
  }
  public get getTotalCompliance(): number {
    var totalCompliance = 0;


    if (this.pivotAnswerGroups == undefined) return 0;
    this.pivotAnswerGroups.forEach(group => {
      totalCompliance+=this.getGroupCompliance(group)
    });
    return Math.round(totalCompliance) / this.pivotAnswerGroups.length;
  }

  public get hasComplianceQuestions(): boolean {
    return this.keywords?.filter(keyword => keyword.trackerOption.compliance==true).length > 0;
  }
}
