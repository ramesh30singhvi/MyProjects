import { HttpErrorResponse } from "@angular/common/http";
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { Observable } from "rxjs";
import { first, switchMap } from "rxjs/operators";
import { IKeywordTrigger, IOption } from "src/app/models/audits/audits.model";
import { AddEditForm, IFormVersion, IFormStatus, FormStatuses, IFormDetails, IFormField } from "src/app/models/forms/forms.model";
import { AuditServiceApi } from "src/app/services/audit-api.service";
import { FormServiceApi } from "src/app/services/form-api.service";
import {Location} from '@angular/common';
import { FormService } from "../services/form.service";
import { CRITERIA_KEYWORD, TRACKER_KEYWORD, TRACKER_MDS, TWENTY_FOUR_HOUR_KEYWORD } from "src/app/common/constants/audit-constants";
import { ISection, ITrackerQuestion, QuestionGroup } from "src/app/models/audits/questions.model";
import { ConfirmationDialogComponent } from "src/app/shared/confirmation-dialog/confirmation-dialog.component";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { EditFormModalComponent } from "../edit-form-modal/edit-form-modal.component";
import { Title } from "@angular/platform-browser";
import { ReportsService } from "../../services/reports.service";
import { RolesEnum } from "../../models/roles.model";
import { AuthService } from "../../services/auth.service";

@Component({
  selector: "app-forms-edit",
  templateUrl: "./forms-edit.component.html",
  styleUrls: ["./forms-edit.component.scss"],
})

export class FormsEditComponent implements OnInit {
  public formVersionId: number;

  public formVersion: IFormVersion;
  public formVersions: IFormVersion[] = [];
  public keywords: IKeywordTrigger;

  public questionGroups: QuestionGroup[];
  public formFields: IFormField[]
  public sections: ISection[];

  public trackerQuestions: ITrackerQuestion[];

  public addEditForm: AddEditForm = new AddEditForm();

  public status: IFormStatus;
  changeDetection: ChangeDetectionStrategy.OnPush
  public organizations$: Observable<IOption[]>;
  public auditTypes$: Observable<IOption[]>;;
 
  public errors: any[];
  public isAdmin: boolean;
  public formStatuses = FormStatuses;

  public yesOrNo = [
    {
      id: 0, name: 'No'
    },
    {
      id: 1, name: 'Yes'
    }
  ]

  constructor(
    private auditServiceApi: AuditServiceApi,
    private formServiceApi: FormServiceApi,
    private reportService: ReportsService,
    private formService: FormService,
    private activateRoute: ActivatedRoute,
    private location: Location,
    private router: Router,
    private ref: ChangeDetectorRef,
    private modalService: NgbModal,
    private titleService: Title,
    private authService: AuthService
  ) {
    this.activateRoute.paramMap
    .pipe(switchMap(params => params.getAll('id')))
    .subscribe(data=> this.formVersionId = +data);
  }

  ngOnInit() {
    this.titleService.setTitle("SHARP forms");

    this.getFormVersion();
    this.getFormVersions();
    this.isAdmin = this.authService.isUserInRole(RolesEnum.Admin);
    console.log(this.isAdmin);
    if(!this.formVersionId) {
      this.organizations$ = this.auditServiceApi.getOrganizationOptions();
      this.auditTypes$ = this.formServiceApi.getAuditTypeOptions();
      
    }
  }

  public onOrganizationChanged(organization: IOption) {
    this.clearNameError();
  }


  public onNameChanged(name: string) {
    this.clearNameError();
  }

  public onCreateClick() {
    if(!this.addEditForm) {
      return;
    }

    if(!this.formVersionId) {
      this.formServiceApi.addForm(this.addEditForm)
      .pipe(first())
      .subscribe({
        next : (formVersion: IFormVersion) => this.handleSaveResponseSuccess(formVersion),
        error: (response: HttpErrorResponse) => this.handleResponseError(response)
      });
    }
  }

  public getHighAlertClass( value: boolean) {
    if (this.addEditForm.useHighAlert == value && value == false)
      return "no-label";
    if (this.addEditForm.useHighAlert == value && value == true)
      return "yes-label";

    return "";
  }
  public onHighAlertValueChanged(event) {
    this.addEditForm.useHighAlert = !this.addEditForm.useHighAlert;
    this.ref.detectChanges();
  }
  public onEditClick() {
    this.formServiceApi.editForm(this.formVersionId)
    .pipe(first())
    .subscribe({
      next: (formDetails: any) => this.handleGetFormDetailsSuccess(formDetails),
      error: (response: HttpErrorResponse) => this.handleResponseError(response),
    });
  }

  public onPublishClick() {
    this.formServiceApi.publishForm(this.formVersionId, this.addEditForm.allowEmptyComment, this.addEditForm.disableCompliance, this.addEditForm.useHighAlert, this.addEditForm.AHTime)
    .pipe(first())
    .subscribe((result: boolean) => {
      if(result){
        this.formVersion.status = FormStatuses.Published.id;
        this.status = FormStatuses.Published;
      }
    });
  }
  public onEditFormClick() {
    const modalRef = this.modalService.open(EditFormModalComponent);
    modalRef.componentInstance.formId = this.formVersion.form.id;
    modalRef.componentInstance.formName = this.addEditForm.name;

    modalRef.result.then(
      (r) => {
        this.addEditForm.name = r.formName;
      },
      () => { }
    );
  }
  public onDeleteClick() {
    if(this.formVersion?.status != 1){
      return;
    }

    const modalRef = this.modalService.open(ConfirmationDialogComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.confirmationBoxTitle = 'Confirmation?';
    modalRef.componentInstance.confirmationMessage = `Do you want to delete the form (Version: ${this.formVersion?.version})?`;

    modalRef.result.then((userResponse) => {
      if(userResponse) {
        this.formServiceApi.deleteForm(this.formVersionId)
        .pipe(first())
        .subscribe((formDetails: any) => {
          if(!formDetails){
            this.router.navigate([`forms-management`]);
            return;
          }

          this.handleGetFormDetailsSuccess(formDetails);
        });
      }
    });
  }

  public onArchiveFormClick(): void {
    this.setFormState(this.formVersion.form.id, false);
  }

  public onUnarchiveFormClick(): void {
    this.setFormState(this.formVersion.form.id, true);
  }

  private getFormVersion() {
    if(this.formVersionId){
      this.formServiceApi.getFormVersion(this.formVersionId)
      .pipe(first())
      .subscribe((formDetails: any) => {
        this.handleGetFormDetailsSuccess(formDetails);
      });
    }
  }

  private getFormVersions() {
    if(this.formVersionId){
      this.formServiceApi.getFormVersions(this.formVersionId)
      .pipe(first())
      .subscribe((formVersions: any) => {
        this.formVersions = formVersions;
      });
    }
  }

  private handleGetFormDetailsSuccess(formDetails: any) {
    if(formDetails) {

      switch(formDetails.formVersion.form.auditType.name){
        case TWENTY_FOUR_HOUR_KEYWORD:
          this.keywords = formDetails.keywords;
        case CRITERIA_KEYWORD:
          this.questionGroups = formDetails.questionGroups;
          this.formFields = formDetails.formFields;
        case TRACKER_KEYWORD:
          this.trackerQuestions = formDetails.questions;
        case TRACKER_MDS:
          this.sections = formDetails.sections;
        default:
          if(formDetails.formVersion){
            this.handleSaveResponseSuccess(formDetails.formVersion);
          }
          break;
      }
    }
  }

  private handleResponseSuccess(formVersion: IFormVersion){
    this.formVersionId = formVersion.id;
    this.formVersion = formVersion;
    this.setFormVersionToAddEditForm(formVersion);
    this.status = this.formService.getStatus(formVersion.status);
  }

  private handleSaveResponseSuccess(formVersion: IFormVersion){
    this.handleResponseSuccess(formVersion);
    this.location.replaceState(`forms-management/${formVersion.id}`);
  }

  private handleResponseError(response: HttpErrorResponse){
    this.errors = response.error.errors;
    console.error(response);
  }

  private clearNameError() {
    if(this.errors && this.errors['Name']) {
      this.errors['Name'] = null;
    }
  }

  private setFormVersionToAddEditForm(formVersion: IFormVersion){
    this.addEditForm = {
      id: formVersion.id,
      name: formVersion.form.name,
      organization: formVersion.organization,
      auditType: formVersion.form.auditType,
      allowEmptyComment: formVersion.form.allowEmptyComment,
      disableCompliance: formVersion.form.disableCompliance,
      useHighAlert: formVersion.form.useHighAlert,
      AHTime: formVersion.form.ahTime
    };
  }

  private setFormState(formId: number, state: boolean): void {
    this.formServiceApi.setFormState(formId, state)
    .pipe(first())
    .subscribe({
      next: (result: boolean) => {
        if(result) {
          this.formVersion.form.isActive = state;
        }
      },
      error: (response: HttpErrorResponse) =>
      {
        this.errors = response?.error?.errors;
        console.error(response);
      }
    });
  }

  hasFormVersions(): boolean {
    return this.formVersions.length > 1;
  }

  goToFormVersion(formVersion: IFormVersion) {
    console.log("GOTO", formVersion);
    window.location.href = "forms-management/" + formVersion.id;
  }
}
