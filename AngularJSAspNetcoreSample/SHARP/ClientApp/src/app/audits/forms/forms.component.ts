import { HttpErrorResponse } from "@angular/common/http";
import {
    ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  ElementRef,
  EventEmitter,
  OnDestroy,
  OnInit,
  Output,
  ViewChild,
  ViewEncapsulation,
} from "@angular/core";
import { FormGroup } from "@angular/forms";
import { Title } from "@angular/platform-browser";
import { ActivatedRoute, Router } from "@angular/router";
import {
  NgbCalendar,
  NgbDate,
  NgbDateParserFormatter,
  NgbDateStruct,
  NgbModal,
} from "@ng-bootstrap/ng-bootstrap";
import * as moment from "moment";
import { BehaviorSubject, Observable, Subscription } from "rxjs";
import { first, switchMap } from "rxjs/operators";
import {
  CRITERIA_KEYWORD,
  TRACKER_KEYWORD,
  TRACKER_MDS,
  TWENTY_FOUR_HOUR_KEYWORD,
} from "src/app/common/constants/audit-constants";
import { MM_DD_YYYY_DOT } from "src/app/common/constants/date-constants";
import { Answer } from "src/app/models/audits/answers.model";
import {
  AuditStatuses,
  IAuditStatus,
  IFacilityOption,
  IFormOption,
  IOption,
  IResidents,
  IStatus,
} from "src/app/models/audits/audits.model";
import {
  FieldBase,
  IFieldBase,
  IGroup,
  ISection,
  Question,
  QuestionGroup,
} from "src/app/models/audits/questions.model";
import {
  ICriteriaFormDetails,
  IFormField,
  IFormFieldItem,
  IFormFieldValue,
  IHighAlert,
  IMdsFormDetails,
  ITrackerFormDetails,
} from "src/app/models/forms/forms.model";
import { RolesEnum } from "src/app/models/roles.model";
import { AuthService } from "src/app/services/auth.service";
import { ControlService } from "src/app/services/control.service";
import { FacilityService } from "src/app/services/facility.service";
import { FormServiceApi } from "src/app/services/form-api.service";
import { AuditStatusButtonsComponent } from "src/app/shared/audit-status-buttons/audit-status-buttons.component";
import { Audit } from "../../models/audits/audits.model";
import { AuditServiceApi } from "../../services/audit-api.service";
import { AuditService } from "../services/audit.service";
import { ConfirmationDialogComponent } from "src/app/shared/confirmation-dialog/confirmation-dialog.component";
import { SimpleAlertDialogComponent } from "src/app/shared/simple-alert-dialog/simple-alert-dialog.component";
import { GroupForm, SectionForm } from "../mds/mds.component";
import { CriteriaComponent } from "../criteria/criteria.component";


@Component({
  templateUrl: "./forms.component.html",
  styleUrls: ["./forms.component.scss"],
  encapsulation: ViewEncapsulation.None,
})
export class FormsComponent implements OnInit, OnDestroy {
  @ViewChild(AuditStatusButtonsComponent, { static: false })


  private statusBattonsComponent: AuditStatusButtonsComponent | undefined;
  private criteriaComponent: CriteriaComponent | undefined
  public subHeaders: FieldBase<any>[] = [];
  public subHeaderForm: FormGroup = new FormGroup({});

  public sectionForms: SectionForm[] = [];

  model: NgbDateStruct = Object.create(null);
  public usehighAlert: boolean;
  public status: IAuditStatus;
  public statuses = AuditStatuses;

  audit: Audit = new Audit(1);

  isEditable: boolean;
  useHighAlert: boolean;

  organizations$: Observable<IOption[]>;
  facilities$: Observable<IFacilityOption[]>;
  forms$: Observable<IFormOption[]>;

  public hoveredDate: NgbDate | null = null;

  public fromDate: NgbDate | null;
  public toDate: NgbDate | null;

  public maxDate: NgbDate | null;

  public errors: any[];

  public selectedForm: IFormOption;
  public highAlertCategories$: Observable<IOption[]>;
  public criteriaFormDetails: ICriteriaFormDetails;
  public mdsFormDetails: IMdsFormDetails;

  public isAuditor: boolean;
  public isTriggered: boolean;
  public isAdmin: boolean;
  public isReviewer: boolean;
  public keyword = 'residentName';
  public residents = [];

  public canEditTriggeredAudit: boolean;
  public haveRightToViewPdf: boolean;

  private subscription: Subscription;

  private params: any | null;

  constructor(
    public formatter: NgbDateParserFormatter,
    public calendar: NgbCalendar,
    private auditServiceApi: AuditServiceApi,
    private formServiceApi: FormServiceApi,
    private auditService: AuditService,
    private facilityServiceApi: FacilityService,
    private activateRoute: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private controlService: ControlService,
    private titleService: Title,
    private modalService: NgbModal,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {
    this.subscription = new Subscription();

    this.isAuditor = this.authService.isUserInRole(RolesEnum.Auditor);
    this.isAdmin = this.authService.isUserInRole(RolesEnum.Admin);
    this.isReviewer = this.authService.isUserInRole(RolesEnum.Reviewer);
    this.isTriggered = false;

    this.haveRightToViewPdf = this.auditService.haveRightToViewPdf();
    this.highAlertCategories$ = this.auditServiceApi.getHighAlertCategories();
    this.router.routeReuseStrategy.shouldReuseRoute = function () {
      return false;
    };

    this.subscription.add(
      this.activateRoute.paramMap
        .pipe(switchMap((params) => params.getAll("id")))
        .subscribe((data) => (this.audit.id = +data))
    );

    this.subscription.add(
      this.auditService.isEditable$.subscribe((isEditable) => {
        this.isEditable = isEditable;
      })
    );

    if (!this.audit.id) {
      this.auditService.setEditable(true);
    }
    this.getTriggeredByKeyword();

  }
  public getTriggeredByKeyword() {
    if (this.audit.id) {
      this.auditService.isAllowToDeleteTriggerAudit(this.audit.id)
        .subscribe((response) => {
          this.isTriggered = response;
        },
          (error) => {
            this.handleResponseError(error);
          });
    }
  }
  public dotsClick(event: MouseEvent): void {
    event.stopPropagation();
  }

  ngOnInit() {
    this.titleService.setTitle("SHARP audits");

    this.getAuditDetails();

    if (!this.audit.id) {
      this.organizations$ = this.auditServiceApi.getOrganizationOptions();
    }
   
    this.subscription.add(this.route.queryParams.subscribe(params => {
      this.setParams(params);
    }));

  }

  ngOnDestroy(): void {
    this.auditService.setAudit(null);
    this.subscription.unsubscribe();
  }
  public changeHighAlert(checked) {
    this.audit.isReadyForNextStatus = false;
    this.useHighAlert = checked;
    if (this.useHighAlert) {
      this.setResidentToHighAlertDescription(this.audit.resident);
      let formName: string = this.audit.form?.name.toLowerCase();

      if (formName.includes("death")) {
        this.highAlertCategories$.subscribe(opt => {
          let foundhighAlertCategories = opt.find(o => o.name.toLowerCase().includes("death"));
          if (foundhighAlertCategories)
            this.onHighAlertCategroryChanged(foundhighAlertCategories);
        });
      }
      else if (formName.includes("abuse)")) {
        this.highAlertCategories$.subscribe(opt => {
          let foundhighAlertCategories = opt.find(o => o.name.toLowerCase().includes("abuse"));
          if (foundhighAlertCategories)
            this.onHighAlertCategroryChanged(foundhighAlertCategories);
        });
      }
      else if (formName.includes("indwelling catheter")) {
        this.highAlertCategories$.subscribe(opt => {
          let foundhighAlertCategories = opt.find(o => o.name.toLowerCase().includes("indwelling catheter"));
          if (foundhighAlertCategories)
            this.onHighAlertCategroryChanged(foundhighAlertCategories);
        });
      }
      else if (formName.includes("wound")) {
        this.highAlertCategories$.subscribe(opt => {
          let foundhighAlertCategories = opt.find(o => o.name.toLowerCase().includes("wound"));
          if (foundhighAlertCategories)
            this.onHighAlertCategroryChanged(foundhighAlertCategories);
        });
      }
      else if (formName.includes("antipsychotic")) {
        this.highAlertCategories$.subscribe(opt => {
          let foundhighAlertCategories = opt.find(o => o.name.toLowerCase().includes("antipsychotic"));
          if (foundhighAlertCategories)
            this.onHighAlertCategroryChanged(foundhighAlertCategories);
        });
      }
      else if (formName.includes("antibiotic")) {
        this.highAlertCategories$.subscribe(opt => {
          let foundhighAlertCategories = opt.find(o => o.name.toLowerCase().includes("uti"));
          if (foundhighAlertCategories)
            this.onHighAlertCategroryChanged(foundhighAlertCategories);
        });
      }
      else if (formName.includes("fall")) {
        this.highAlertCategories$.subscribe(opt => {
          let foundhighAlertCategories = opt.find(o => o.name.toLowerCase().includes("fall"));
          if (foundhighAlertCategories)
            this.onHighAlertCategroryChanged(foundhighAlertCategories);
        });
      }
      else if (formName.includes("discharge") && formName.includes("hospital")) {
        this.highAlertCategories$.subscribe(opt => {
          let foundhighAlertCategories = opt.find(o => o.name.toLowerCase().includes("er transfer"));
          if (foundhighAlertCategories)
            this.onHighAlertCategroryChanged(foundhighAlertCategories);
        }); 
      }

      if (formName.includes("death") ||
        formName.includes("change in condition") ||
        formName.includes("behavior") ||
        formName.includes("abuse") ||
        formName.includes("indwelling catheter") ||
        formName.includes("wound") ||
        formName.includes("antipsychotic") ||
        formName.includes("antibiotic") ||
        formName.includes("fall") ||
        (formName.includes("discharge") && formName.includes("hospital"))) {
        if (this.criteriaFormDetails.questionGroups && this.criteriaFormDetails.questionGroups.length > 0 && this.audit.highAlertNotes?.trim().length == 0) {
          let htmlData = this.criteriaFormDetails.questionGroups[0].questions.filter(x => x.sequence == 1)[0].answer?.auditorComment
          let textContent = this.getTextFromHTML(htmlData);
          this.audit.highAlertNotes = textContent;
        }
      }
    }
  }
  get hasHighAlert() {
    return this.audit?.form.useHighAlert;
  }

  public get showHighAlert() {
    return this.isEditable ;
  }
  private setParams(params) {

    this.params = params;
    if (params.organization) {
      this.organizations$.subscribe(opt => {
        let foundOrganization = opt.find(o => o.id == params.organization)
        this.audit.organization = foundOrganization;
        this.onOrganizationChanged(foundOrganization);
      })
    }


  }

  public handleSetAuditStatusSuccess(auditDetails: any) {
    this.handleResponseSuccess(auditDetails?.audit);
    this.setAuditByType(auditDetails);
  }

  onOrganizationChanged(organization: IOption): void {
    this.clearFacility();
    this.clearForm();

    if (!organization) {
      return;
    }

    this.facilities$ = this.facilityServiceApi.getFacilityOptions(
      organization.id
    );

    if (this.params.facility) {
      this.facilities$.subscribe(opt => {
        let foundFacility = opt.find(o => o.id == this.params.facility)
        this.audit.facility = foundFacility;
        this.onFacilityChanged(foundFacility);
      })
    }

    this.forms$ = this.formServiceApi.getFormVersionOptions(organization.id);

    if (this.params.form) {
      this.forms$.subscribe(opt => {
        let foundForm = opt.find(o => o.formId == this.params.form)
        this.audit.form = foundForm;
        this.onFormChanged(foundForm);
      })
    }
  }

  public onFacilityChanged(facility: IFacilityOption): void {
    this.clearDate();

    if (!facility) {
      return;
    }

    if (this.errors && this.errors["FacilityId"]) {
      this.errors["FacilityId"] = null;
    }

    this.maxDate = this.getDateFieldMaxValue(
      this.audit.facility,
      this.audit.form
    );
  }

  public onHighAlertDescriptionChanged(event) {
    if (this.audit.form.auditType.name === CRITERIA_KEYWORD) {
      this.audit.isReadyForNextStatus = false;
      this.audit.highAlertDescription = event;
    }
  }
  public onHighAlertCategroryChanged(categ: IOption) {
    this.audit.isReadyForNextStatus = false;
    if (!categ) {
      return;
    }
    if (this.errors && this.errors["HighAlertCategory"]) {
      this.errors["HighAlertCategory"] = null;
    }
    if (this.audit.form.auditType.name === CRITERIA_KEYWORD) {
      this.audit.highAlertCategory = categ;
    }
   
  }
 
  public onChangeDescription(value) {
    this.audit.isReadyForNextStatus = false;
    if (value != "") {
      if (this.errors && this.errors["HighAlertDescription"]) 
        this.errors["HighAlertDescription"] = null;
    }
  }
  public onFormChanged(form: IFormOption): void {
    this.clearDate();

    if (!form) {
      return;
    }

    if (this.errors && this.errors["FormVersionId"]) {
      this.errors["FormVersionId"] = null;
    }

    this.maxDate = this.getDateFieldMaxValue(
      this.audit.facility,
      this.audit.form
    );

    if (form.auditType.name === CRITERIA_KEYWORD) {
      this.getCriteriaFormDetails(form);
     
    } else if (form.auditType.name === TRACKER_KEYWORD) {
      this.getTrackerFormQuestions(form);
    } else if (form.auditType.name === TRACKER_MDS) {
      this.getMdsFormDetails(form);
    }
  }
 
  public onSaveClick() {
    if (!this.audit || !this.isEditable) {
      return;
    }

    if (this.audit.form.auditType.name === CRITERIA_KEYWORD && this.useHighAlert) {
      if (this.errors == undefined)
        this.errors = [];

      if (this.audit.highAlertCategory == null) {
        this.errors["HighAlertCategory"] = "Please select the category";
        this.cdr.detectChanges();
        return;
      }

      if (this.audit.highAlertDescription == "") {
        this.errors["HighAlertDescription"] = "Please enter the description";
        this.cdr.detectChanges();
        return;
      }

    }

    if (!this.useHighAlert)
      this.audit.highAlertCategory = null;

    this.audit.incidentDateFrom = this.fromDate
      ? moment(this.formatter.format(this.fromDate), MM_DD_YYYY_DOT).toDate()
      : null;
    this.audit.incidentDateTo = this.toDate
      ? moment(this.formatter.format(this.toDate), MM_DD_YYYY_DOT).toDate()
      : null;

    if (this.audit.form.auditType.name === CRITERIA_KEYWORD) {
      this.audit.values = this.mapCriteriaAnswers();
      this.audit.subHeaderValues = this.mapCriteriaSubHeaders();
    } else if (this.audit.form.auditType.name == TRACKER_MDS) {
      this.audit.subHeaderValues = this.mapMdsSubHeaders();
    }


    if (!this.audit.id) {
      this.auditServiceApi
        .addAudit(this.audit)
        .pipe(first())
        .subscribe({
          next: (auditDetails: any) => {
            this.handleResponseSuccess(auditDetails?.audit);
            this.setAuditByType(auditDetails);
            this.handleAddResponseSuccess(auditDetails?.audit.id);
          },
          error: (response: HttpErrorResponse) => {
            this.handleResponseError(response);
            if (response.status == 404) {
              this.showRetryError();
            }
          },
        });
    } else {
      this.auditServiceApi
        .editAudit(this.audit)
        .pipe(first())
        .subscribe({
          next: (auditDetails: any) => {
            this.handleResponseSuccess(auditDetails?.audit);
            this.setAuditByType(auditDetails);
          },
          error: (response: HttpErrorResponse) => {
            this.handleResponseError(response);
            if (response.status == 404) {
              this.showRetryError();
            }
          },
        });
    }
  }

  showRetryError(): void {
    const modalRef = this.modalService.open(SimpleAlertDialogComponent, {
      modalDialogClass: "custom-modal",
    });
    modalRef.componentInstance.title = "An error has occurred";
    modalRef.componentInstance.message = `An error occurred while attempting to establish a connection to the server. Please check your connection and try again.`;

    modalRef.result.then(
      (userResponse) => {
        if (userResponse === true) {
          this.onSaveClick();
        }
      },
      () => {
        /*user closed the message box*/
      }
    );
  }

  public onDownloadPdfClick() {
    this.auditService.downloadPdf(this.audit.id);
  }

  public onDownloadExcelClick() {
    this.auditService.downloadExcel(this.audit.id);
  }

  public onDuplicateAuditClick(auditId: number): void {
    if (!auditId) {
      return;
    }

    this.auditServiceApi
      .duplicateAudit(auditId)
      .pipe(first())
      .subscribe({
        next: (newAuditId: number) => {
          this.auditService.redirectToAudit(newAuditId);
        },
        error: (response: HttpErrorResponse) => {
          this.handleResponseError(response);
        },
      });
  }

  public onArchiveAuditClick(auditId: number) {
    if (!auditId) {
      return;
    }

    this.auditServiceApi.archiveAudit(auditId).subscribe(
      (response) => {
        //this.gridApi?.onFilterChanged();
        this.getAuditDetails();
      },
      (error) => {
        this.handleResponseError(error);
      }
    );
  }
  public onUnarchiveAuditClick(auditId: number) {
    if (!auditId) {
      return;
    }

    this.auditServiceApi.unArchiveAudit(auditId).subscribe(
      (response) => {
        this.getAuditDetails();
        //this.gridApi?.onFilterChanged();
      },
      (error) => {
        this.handleResponseError(error);
      }
    );
  }

  public onDeleteAuditClick(auditId: number) {
    if (!auditId) {
      return;
    }

    const modalRef = this.modalService.open(ConfirmationDialogComponent, {
      modalDialogClass: "custom-modal",
    });
    modalRef.componentInstance.confirmationBoxTitle = "Confirmation?";
    modalRef.componentInstance.confirmationMessage = `Are you sure you want to delete this audit?`;

    modalRef.result.then(
      (userResponse) => {
        if (userResponse) {
          this.auditServiceApi.deleteAudit(auditId).subscribe(
            (response) => {
              //this.gridApi?.onFilterChanged();
              this.getAuditDetails();
            },
            (error) => {
              this.handleResponseError(error);
            }
          );
        }
      },
      () => {
        /*user closed the message box*/
      }
    );
  }
  public onUndeleteAuditClick(auditId: number) {
    if (!auditId) {
      return;
    }

    this.auditServiceApi.unDeleteAudit(auditId).subscribe(
      (response) => {
        this.getAuditDetails();
        //this.gridApi?.onFilterChanged();
      },
      (error) => {
        this.handleResponseError(error);
      }
    );
  }

  public onSaveDuplicatedAuditClick(): void {
    if (!this.audit) {
      return;
    }

    this.audit.incidentDateFrom = this.fromDate
      ? moment(this.formatter.format(this.fromDate), MM_DD_YYYY_DOT).toDate()
      : null;
    this.audit.incidentDateTo = this.toDate
      ? moment(this.formatter.format(this.toDate), MM_DD_YYYY_DOT).toDate()
      : null;

    this.auditServiceApi
      .updateDuplicatedAudit(this.audit)
      .pipe(first())
      .subscribe({
        next: (auditDetails: any) => {
          this.handleResponseSuccess(auditDetails?.audit);
          this.setAuditByType(auditDetails);
        },
        error: (response: HttpErrorResponse) => {
          this.handleResponseError(response);
        },
      });
  }

  handleAddResponseSuccess(auditId: number) {
    this.router.navigate([`audits/${auditId}`]);
  }

  handleResponseError(response: HttpErrorResponse) {
    this.errors = response.error.errors;
  }

  public onRoomChanged(event): void {
    this.audit.isReadyForNextStatus = false;

    if (this.errors && this.errors["Room"]) {
      this.errors["Room"] = null;
    }
  }

  public onIdentifierChanged(event): void {
    this.audit.isReadyForNextStatus = false;

    if (this.errors && this.errors["Identifier"]) {
      this.errors["Identifier"] = null;
    }
  }

  public onResidentChanged(event): void {
    this.audit.isReadyForNextStatus = false;

    if (this.errors && this.errors["Resident"]) {
      this.errors["Resident"] = null;
    }
  }
  countChanges = 0;
  public handleCriteriaAuditChanged(isCriteriaAuditChanged): void {
    this.audit.isReadyForNextStatus = false;

    if (isCriteriaAuditChanged)
      this.countChanges++;
    if (this.countChanges == 2 && isCriteriaAuditChanged) {
      this.onSaveClick();
      this.countChanges = 0;
    }

  }

  getAuditDetails() {
    if (this.audit.id) {
      this.auditServiceApi
        .getAuditDetails(this.audit.id)
        .pipe(first())
        .subscribe(
          (auditDetails: any) => {
            if (
              !this.auditService.haveRightToViewAudit(
                auditDetails?.audit?.submittedByUser?.userId
              )
            ) {
              this.router.navigate([`audits`]);
            }

            this.handleResponseSuccess(auditDetails?.audit);
            this.setAuditByType(auditDetails);
          },
          (error: HttpErrorResponse) => {
            this.handleResponseError(error);
          }
        );

      this.auditServiceApi
        .getResidents(this.audit.id)
        .pipe(first())
        .subscribe(
          (residents: any) => {
            this.residents = residents;
            console.log("Got residents", residents);
          },
          (error: HttpErrorResponse) => {
            this.handleResponseError(error);
          }
        );
    }

  }

  private handleResponseSuccess(audit: Audit): void {
    this.audit = audit;

    if (this.audit.form.auditType.name === CRITERIA_KEYWORD) {
      this.useHighAlert = this.audit.highAlertCategory != null;
      this.cdr.detectChanges();
    }

    this.status = this.auditService.getStatus(audit.status);

    this.canEditTriggeredAudit =
      (this.status.id === AuditStatuses.Triggered.id ||
        this.status.id === AuditStatuses.Duplicated.id) &&
      this.auditService.isAuditorOwnerOfAudit(
        this.audit?.submittedByUser?.userId
      );

    this.auditService.isEditable(
      this.status?.id,
      this.audit.submittedByUser?.userId,
      this.audit.state
    );

    this.auditService.setAudit(audit);

    if (audit.incidentDateFrom) {
      const dateFrom = moment(audit.incidentDateFrom).local();
      this.fromDate = new NgbDate(
        dateFrom.year(),
        dateFrom.month() + 1,
        dateFrom.date()
      );
    }

    if (audit.incidentDateTo) {
      const dateTo = moment(audit.incidentDateTo).local();
      this.toDate = new NgbDate(
        dateTo.year(),
        dateTo.month() + 1,
        dateTo.date()
      );
    }

    this.maxDate = this.getDateFieldMaxValue(
      this.audit.facility,
      this.audit.form
    );

    this.statusBattonsComponent?.setActions(this.status?.id);
  }

  private setAuditByType(auditDetails: any): void {


    if (auditDetails?.audit.form.auditType.name === TWENTY_FOUR_HOUR_KEYWORD) {
      this.auditService.setHourKeyword({
        keywords: auditDetails.keywords,
        matchedKeywords: auditDetails.matchedKeywords,
      });
    } else if (auditDetails?.audit.form.auditType.name === CRITERIA_KEYWORD) {
      this.criteriaFormDetails = auditDetails?.formVersion;

      this.subHeaders = [];

      this.criteriaFormDetails?.formFields?.forEach((formField: IFormField) => {
        const option: IFieldBase = {
          id: formField.id,
          key: formField.id.toString(),
          required: formField.isRequired,
          label: formField.labelName,
          order: formField.sequence,
          value: formField.value?.value,
          options: formField.items?.map((item: IFormFieldItem) => {
            return { id: item.id, value: item.value };
          }),
        };

        const control = this.controlService.getControl(
          formField?.fieldType?.id,
          option
        );
        control.showError = false;

        this.subHeaders.push(control);
      });

      this.subHeaderForm = this.controlService.toFormGroup(this.subHeaders);

      if (!this.isEditable) {
        this.controlService.disableForm(this.subHeaderForm);
      } else {
        this.controlService.enableForm(this.subHeaderForm);
      }
    } else if (auditDetails?.audit.form.auditType.name === TRACKER_MDS) {
      this.mdsFormDetails = auditDetails?.formVersion;

      this.sectionForms = [];

      this.mdsFormDetails.sections.forEach((section: ISection) => {
        var sectionForm: SectionForm = {
          name: section.name,
          groups: []
        }
        section.groups.forEach((group: IGroup) => {

          var questions: FieldBase<any>[] = [];

          group.formFields.forEach((formField: IFormField) => {

            let existingFormField = this.mdsFormDetails.formFields.find(fm => fm.id == formField.id);

            const option: IFieldBase = {
              id: formField.id,
              key: formField.id.toString(),
              required: formField.isRequired,
              label: formField.labelName,
              order: formField.sequence,
              value: existingFormField.value?.value,
              options: formField.items?.map((item: IFormFieldItem) => {
                return { id: item.id, value: item.value };
              }),
            }

            const control = this.controlService.getControl(
              formField?.fieldType?.id,
              option
            );
            control.showError = false;

            questions.push(control);

          });

          var formGroup = this.controlService.toFormGroup(questions);

          if (!this.isEditable) {
            this.controlService.disableForm(formGroup);
          } else {
            this.controlService.enableForm(formGroup);
          }

          var groupForm: GroupForm = {
            name: group.name,
            form: formGroup,
            questions: questions
          }
          sectionForm.groups.push(groupForm);
        });
        this.sectionForms.push(sectionForm);
      });

    } else if (auditDetails?.audit.form.auditType.name === TRACKER_KEYWORD) {
      this.auditService.setTracker({
        questions: auditDetails?.formVersion?.questions,
        sortModel: auditDetails?.sortModel,
        auditDetails: auditDetails,
        pivotAnswerGroups: auditDetails?.pivotAnswerGroups,
      });
    }




  }

  onDateSelection(date: NgbDate) {
    if (!this.fromDate && !this.toDate) {
      this.fromDate = date;
    } else if (
      this.fromDate &&
      !this.toDate &&
      date &&
      date.after(this.fromDate)
    ) {
      this.toDate = date;
    } else {
      this.toDate = null;
      this.fromDate = date;
    }

    if (this.errors && this.errors["IncidentDateFrom"]) {
      this.errors["IncidentDateFrom"] = null;
    }
  }

  isHovered(date: NgbDate) {
    return (
      this.fromDate &&
      !this.toDate &&
      this.hoveredDate &&
      date.after(this.fromDate) &&
      date.before(this.hoveredDate)
    );
  }

  isInside(date: NgbDate) {
    return this.toDate && date.after(this.fromDate) && date.before(this.toDate);
  }

  isRange(date: NgbDate) {
    return (
      date.equals(this.fromDate) ||
      (this.toDate && date.equals(this.toDate)) ||
      this.isInside(date) ||
      this.isHovered(date)
    );
  }

  rangeFormat(dateFrom: NgbDate | null, dateTo: NgbDate | null): string {
    let dateRange: string = "";

    if (dateFrom && dateTo && !dateFrom.equals(dateTo)) {
      dateRange = `${this.formatter.format(dateFrom)} - ${this.formatter.format(
        dateTo
      )}`;
    } else {
      dateRange = this.formatter.format(dateFrom);
    }

    return dateRange;
  }

  getDateFieldMaxValue(facility: IFacilityOption, form: IFormOption): NgbDate {
    if (
      facility &&
      facility.timeZoneOffset &&
      form &&
      form.auditType?.name === TWENTY_FOUR_HOUR_KEYWORD
    ) {
      const currentDateTimeUtc = moment().utc().format();
      const facilityLocalDateTime = moment(currentDateTimeUtc).utcOffset(
        facility.timeZoneOffset
      );
      const facilityMaxDate = facilityLocalDateTime
        //.subtract(1, "day")
        .format(MM_DD_YYYY_DOT);

      const ngDateStruct = this.formatter.parse(facilityMaxDate);
      return new NgbDate(
        ngDateStruct.year,
        ngDateStruct.month,
        ngDateStruct.day
      );
    }

    return null;
  }

  clearFacility() {
    this.audit.facility = null;
    this.facilities$ = null;

    this.clearForm();
  }

  clearForm() {
    this.audit.form = null;
    this.forms$ = null;

    this.clearDate();
  }

  clearDate() {
    if (this.audit?.form?.auditType.name === TWENTY_FOUR_HOUR_KEYWORD) {
      this.fromDate = null;
      this.toDate = null;
      this.maxDate = null;
    }
  }

  private getMdsFormDetails(form: IFormOption) {

    this.formServiceApi
      .getFormVersion(form.id)
      .pipe(first())
      .subscribe((mdsFormDetails: IMdsFormDetails) => {
        this.mdsFormDetails = mdsFormDetails;

        this.mdsFormDetails.sections.forEach((section: ISection) => {
          var sectionForm: SectionForm = {
            name: section.name,
            groups: []
          }
          section.groups.forEach((group: IGroup) => {

            var questions: FieldBase<any>[] = [];

            group.formFields.forEach((formField: IFormField) => {

              let existingFormField = this.mdsFormDetails.formFields.find(fm => fm.id == formField.id);

              const option: IFieldBase = {
                id: formField.id,
                key: formField.id.toString(),
                required: formField.isRequired,
                label: formField.labelName,
                order: formField.sequence,
                value: existingFormField.value?.value,
                options: formField.items?.map((item: IFormFieldItem) => {
                  return { id: item.id, value: item.value };
                }),
              }

              const control = this.controlService.getControl(
                formField?.fieldType?.id,
                option
              );
              control.showError = false;

              questions.push(control);

            });

            var formGroup = this.controlService.toFormGroup(questions);

            this.controlService.disableForm(formGroup);

            var groupForm: GroupForm = {
              name: group.name,
              form: formGroup,
              questions: questions
            }
            sectionForm.groups.push(groupForm);
          });
          this.sectionForms.push(sectionForm);
        });

      });
  }

  private getCriteriaFormDetails(form: IFormOption) {
    this.formServiceApi
      .getFormVersion(form.id)
      .pipe(first())
      .subscribe((criteriaFormDetails: ICriteriaFormDetails) => {
        this.criteriaFormDetails = criteriaFormDetails;

        this.subHeaders = [];

        this.criteriaFormDetails?.formFields?.forEach(
          (formField: IFormField) => {
            const option: IFieldBase = {
              id: formField.id,
              key: formField.id.toString(),
              required: formField.isRequired,
              label: formField.labelName,
              order: formField.sequence,
              value: formField.value?.value,
              options: formField.items?.map((item: IFormFieldItem) => {
                return { id: item.id, value: item.value };
              }),
            };

            const control = this.controlService.getControl(
              formField?.fieldType?.id,
              option
            );
            control.showError = false;

            this.subHeaders.push(control);
          }
        );

        this.subHeaderForm = this.controlService.toFormGroup(this.subHeaders);

        this.controlService.disableForm(this.subHeaderForm);
      });
  }

  private getTrackerFormQuestions(form: IFormOption) {
    this.formServiceApi
      .getFormVersion(form.id)
      .pipe(first())
      .subscribe((trackerFormDetails: ITrackerFormDetails) => {
        this.auditService.setTracker({
          auditDetails: null,
          questions: trackerFormDetails.questions,
        });
      });
  }

  private mapCriteriaAnswers(): Answer[] {
    let answers: Answer[] = [];

    this.criteriaFormDetails.questionGroups.forEach((group: QuestionGroup) => {
      return group.questions?.forEach((question: Question) => {
        if (
          question.answer &&
          (question.answer.id > 0 ||
            question.answer.value ||
            question.answer.auditorComment)
        ) {
          answers.push(question.answer);
        }

        question.subQuestions?.forEach((subQuestion: Question) => {
          if (
            subQuestion.answer &&
            (subQuestion.answer.id > 0 ||
              subQuestion.answer.value ||
              subQuestion.answer.auditorComment)
          ) {
            answers.push(subQuestion.answer);
          }
        });
      });
    });

    return answers;
  }

  private mapMdsSubHeaders(): IFormFieldValue[] {
    let subHeaderValues: IFormFieldValue[] = [];

    this.sectionForms.forEach((sectionForm: SectionForm) => {
      sectionForm.groups.forEach((groupForm: GroupForm) => {

        const formValues = groupForm.form.value;

        Object.keys(formValues).map((key) => {

          let value = formValues[key];

          const subHeader = groupForm.questions.find((fm) => fm.key == key);

          if (!subHeader) {
            return;
          }

          subHeaderValues.push({
            id: this.mdsFormDetails.formFields.find(
              (subHeader: IFormField) => subHeader.id.toString() === key
            )?.value?.id,
            formFieldId: subHeader.id,
            value: this.controlService.parseToStorageValue(subHeader, value)
          });

        });

      });
    });


    return subHeaderValues.filter(
      (value) => value?.value !== null
    );
  }

  private mapCriteriaSubHeaders(): IFormFieldValue[] {
    let subHeaderValues: IFormFieldValue[] = [];

    const formValues = this.subHeaderForm.value;

    subHeaderValues = Object.keys(formValues).map((key) => {
      let value = formValues[key];

      const subHeader = this.subHeaders.find(
        (subHeader) => subHeader.key === key
      );

      if (!subHeader) {
        return;
      }

      return {
        id: this.criteriaFormDetails.formFields.find(
          (subHeader: IFormField) => subHeader.id.toString() === key
        )?.value?.id,
        formFieldId: subHeader.id,
        value: this.controlService.parseToStorageValue(subHeader, value),
      };
    });

    return subHeaderValues.filter(
      (value) => /*value?.id ||*/ value?.value !== null
    );
  }

  setResidentToHighAlertDescription(resident: string) {

    let newValue = resident && resident.length > 0 ? "<" + resident + ">" : "";
    if (this.audit.highAlertDescription == "") {
      this.audit.highAlertDescription = newValue;
    } else {
      let resPrevStart = this.audit.highAlertDescription.indexOf("<");
      let resPrevEnd = this.audit.highAlertDescription.indexOf(">");
      let startText = ""; let endText = "";
      if (resPrevStart > -1)
        startText = this.audit.highAlertDescription.substring(0, resPrevStart - 1);
      if (resPrevEnd > -1)
        endText = this.audit.highAlertDescription.substring(resPrevEnd + 1, this.audit.highAlertDescription.length);

      this.audit.highAlertDescription = startText + newValue + endText;

    }
  }


  selectEvent(item: IResidents) {
    console.log(item);
    if (item.residentName!=null) {
      // do something with selected item
      this.audit.resident = item.residentName;
      this.onResidentChanged(item.residentName);
      this.setResidentToHighAlertDescription(item.residentName);
      this.cdr.detectChanges();

      this.audit.room = item.room;
      this.onRoomChanged(item.room);
      this.cdr.detectChanges();
    }

  }

  onChangeSearch(val: string) {
    this.audit.resident = val;
    this.onResidentChanged(val);
    this.cdr.detectChanges();
    // fetch remote data from here
    // And reassign the 'data' which is binded to 'data' property.
  }

  onFocused(e){
    // do something when input is focused
  }

  public handleCriteriaAnswerCommentFocusout(question: Question): void {
    if (question && question.sequence == 1) {
      if (this.useHighAlert && this.audit.highAlertNotes?.trim().length == 0) {
        let formName: string = this.audit.form?.name.toLowerCase();
        if (formName.includes("death") ||
          formName.includes("change in condition") ||
          formName.includes("behavior") ||
          formName.includes("abuse") ||
          formName.includes("indwelling catheter") ||
          formName.includes("wound") ||
          formName.includes("antipsychotic") ||
          formName.includes("antibiotic") ||
          formName.includes("fall") ||
          (formName.includes("discharge") && formName.includes("hospital"))) {
          let textContent = this.getTextFromHTML(question.answer?.auditorComment);
          this.audit.highAlertNotes = textContent;
        }
      }
    }
  }

  getTextFromHTML(htmlData: string): string {
    let textContent: string = "";
    if (htmlData) {
      const el = document.createElement('div')
      el.innerHTML = htmlData
      textContent = el.textContent
    }
    return textContent;
  }

}
