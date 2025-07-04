import { Component, Input, OnInit } from "@angular/core";
import { FormBuilder, FormControl, FormGroup, Validators } from "@angular/forms";
import { NgbActiveModal, NgbDate, NgbDateAdapter, NgbDateParserFormatter, NgbModal, NgbTimeAdapter, NgbTimepickerConfig } from "@ng-bootstrap/ng-bootstrap";
import * as moment from "moment";
import { NgxSpinnerService } from "ngx-spinner";
import { ToastrService } from "ngx-toastr";
import { first } from "rxjs";
import { NgbTimeStringAdapter } from "src/app/audits/keyword-input-section/keyword-input-section.component";
import { SPINNER_TYPE } from "src/app/common/constants/audit-constants";
import { MM_DD_YYYY_DOT, MM_DD_YYYY_SLASH, YYYY_MM_DD_DASH } from "src/app/common/constants/date-constants";
import { IOption, IUserTimeZone } from "src/app/models/audits/audits.model";
import { IEditMemo, IMemo } from "src/app/models/memos/memos.model";
import { IUserOrganizations } from "src/app/models/users/users.model";
import { AuthService } from "src/app/services/auth.service";
import { MemoServiceApi } from "src/app/services/memo-api.service";
import { UserService } from "src/app/services/user.service";
import { ConfirmationDialogComponent } from "src/app/shared/confirmation-dialog/confirmation-dialog.component";
import { CustomDateParserAdapter, CustomDateParserFormatter } from "src/app/shared/datepicker-adapters";

@Component({
  selector: "app-memo-edit",
  templateUrl: "./memo-edit.component.html",
  styleUrls: ["./memo-edit.component.scss"],
  providers: [
    {provide: NgbDateAdapter, useClass: CustomDateParserAdapter},
    { provide: NgbDateParserFormatter, useClass: CustomDateParserFormatter },
    NgbTimepickerConfig,
    { provide: NgbTimeAdapter, useClass: NgbTimeStringAdapter }
  ],
})

export class MemoEditComponent implements OnInit {
  @Input() title: string;
  @Input() actionButtonLabel: string;
  //@Input() organizations: IOption[];

  @Input() editMemo: IMemo;

  public organizations: IOption[];

  public memoForm : FormGroup;

  public minDate: NgbDate | null;
  
  public spinnerType: string;

  public errors: any[] = [];

  constructor(
    public activeModal: NgbActiveModal,
    private formBuilder: FormBuilder,
    private spinner: NgxSpinnerService,
    private dateAdapter: NgbDateAdapter<string>,
    private modalService: NgbModal,
    private memoServiceApi: MemoServiceApi,
    private authServise: AuthService,
    private userServiseApi: UserService,
    private toastr: ToastrService,
  ) { 
    this.userServiseApi
    .getUserTimeZone(this.authServise.getCurrentUserSharpId())
    .pipe(first())
    .subscribe({
      next: (timeZone: IUserTimeZone) => {
        const dateStruct = this.dateAdapter.fromModel(moment(timeZone.userTimeZoneDateTime).format(MM_DD_YYYY_DOT));
        this.minDate = NgbDate.from(dateStruct);
      }
    });
  }

  ngOnInit() {
    const validityDateModel = this.editMemo?.validityDate 
    ? moment(this.editMemo?.validityDate).format(MM_DD_YYYY_DOT) 
    : null;

    this.memoForm = this.formBuilder.group({    
      organization: new FormControl(null),
      validityDate: new FormControl(validityDateModel),
      text: new FormControl(this.editMemo?.text, Validators.required),
    });

    this.userServiseApi.getOrganizationOptions()
    .pipe(first())
    .subscribe({
      next: (userOrganizations: IUserOrganizations) => {
        this.organizations = userOrganizations.organizations;

        if(!userOrganizations.filteredByUserId || (this.editMemo && (!this.editMemo.organizations || this.editMemo.organizations.length === 0))) {
            this.organizations.unshift({id: null, name: 'All'});
        }

        this.memoForm.patchValue({organization: this.editMemo?.organizations?.[0] ?? this.organizations?.[0]});
      }
    });

    this.spinnerType = SPINNER_TYPE;
  }

  public onSaveClick(): void {
    if(!this.memoForm.valid) {
      return;
    }

    const selectedOrg = this.memoForm.controls["organization"].value;
    const selectedValidityDate = this.memoForm.controls["validityDate"].value;

    const editMemo: IEditMemo = {
      id: this.editMemo?.id,
      organizationIds: selectedOrg && selectedOrg.id !== null ? [selectedOrg?.id] : [],
      validityDate: selectedValidityDate ? moment(selectedValidityDate, MM_DD_YYYY_DOT).format(YYYY_MM_DD_DASH) : null,
      text: this.memoForm.controls["text"].value,
    };

    if(this.editMemo?.id) {
      this.memoServiceApi.editMemo(editMemo)
      .pipe(first())
      .subscribe({
        next: (memo: IMemo) => {
          if(memo){
            this.activeModal.close(memo);
          }
        },
        error: (response: any) => {
          this.errors = response.error?.errors;

          if(response.error?.errorMessage) {
            this.toastr.error(response.error?.errorMessage);
          }

          console.log(response);
        },
        complete: () => {this.spinner.hide("memoEditSpinner")}
      });
    } else {
      this.memoServiceApi.addMemo(editMemo)
      .pipe(first())
      .subscribe({
        next: (memo: IMemo) => {
          if(memo){
            this.activeModal.close(memo);
          }
        },
        error: (response: any) => {
          this.errors = response.error?.errors;

          if(response.error?.errorMessage) {
            this.toastr.error(response.error?.errorMessage);
          }

          console.log(response);
        },
        complete: () => {this.spinner.hide("memoEditSpinner")}
      });
    }
  }

  public onMemoDeleteClick(): void {
    const modalRef = this.modalService.open(ConfirmationDialogComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.confirmationBoxTitle = 'Confirmation?';
    modalRef.componentInstance.confirmationMessage = `Do you want to delete this memo?`;
    
    modalRef.result.then((userResponse) => {
      if(userResponse) {
        this.spinner.show("memoEditSpinner");
        this.memoServiceApi.deleteMemo(this.editMemo?.id)
        .pipe(first())
        .subscribe({
          next: (result: boolean) => {
            if(result){
              this.activeModal.close({...this.editMemo, isDeleted: true});
            }
          },
          error: (response: any) => {
            if(response.error?.errorMessage) {
              this.toastr.error(response.error?.errorMessage);
            }
            
            this.spinner.hide("memoEditSpinner");
            console.log(response);
          },
          complete: () => {this.spinner.hide("memoEditSpinner")}});
      }
    });
  }

  public onTextChanged(): void {
    if (this.errors && this.errors["Text"]) {
      this.errors["Text"] = null;
    }
  }
}
