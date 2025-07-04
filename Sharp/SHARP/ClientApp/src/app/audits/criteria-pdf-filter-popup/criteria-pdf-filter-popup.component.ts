import { HttpErrorResponse } from "@angular/common/http";
import { Component, OnInit, ViewEncapsulation } from "@angular/core";
import { NgbActiveModal, NgbDate, NgbDateParserFormatter } from "@ng-bootstrap/ng-bootstrap";
import * as moment from "moment";
import { NgxSpinnerService } from "ngx-spinner";
import { ToastrService } from "ngx-toastr";
import { Observable } from "rxjs";
import { first } from "rxjs/operators";
import { SPINNER_TYPE } from "src/app/common/constants/audit-constants";
import { MM_DD_YYYY_DOT, YYYY_MM_DD_DASH } from "src/app/common/constants/date-constants";
import { IFacilityOption, IFormOption, IOption } from "src/app/models/audits/audits.model";
import { IMessageResponse, MessageStatusEnum } from "src/app/models/common.model";
import { AuditServiceApi } from "src/app/services/audit-api.service";
import { FacilityService } from "src/app/services/facility.service";
import { ReportRequestServiceApi } from "src/app/services/report-request-api.service";
import { FormServiceApi } from "../../services/form-api.service";
import { AuditService } from "../services/audit.service";

@Component({
  encapsulation: ViewEncapsulation.None,
  selector: "app-criteria-pdf-filter-popup",
  templateUrl: "./criteria-pdf-filter-popup.component.html",
  styleUrls: ["./criteria-pdf-filter-popup.component.scss"],
})
export class CriteriaPdfFilterPopupComponent implements OnInit {
  public organizations$: Observable<IOption[]>;
  public auditTypes$: Observable<IOption[]>;
  public facilities$: Observable<IFacilityOption[]>;
  public forms$: Observable<IFormOption[]>;

  public organization: IOption;
  public facility: IFacilityOption;
  public form: IFormOption;
  public auditType: IOption;
  public hoveredDate: NgbDate | null = null;

  public fromDate: NgbDate | null;
  public toDate: NgbDate | null;

  public error: string;

  public spinnerType: string;
  excludedAuditTypes: Array<string> = ['24 hour keyword'];
  constructor(
    private auditServiceApi: AuditServiceApi,
    private reportRequestServiceApi: ReportRequestServiceApi,
    private formServiceApi: FormServiceApi,
    private auditService: AuditService,
    private facilityServiceApi: FacilityService,
    public formatter: NgbDateParserFormatter,
    public activeModal: NgbActiveModal,
    private spinner: NgxSpinnerService,
    private toastr: ToastrService,
  ) {
    this.spinnerType = SPINNER_TYPE;
  }

  ngOnInit() {
    this.spinner.hide("criteriaPdfSpinner");

    this.organizations$ = this.auditServiceApi.getOrganizationOptions();
    this.auditTypes$ = this.formServiceApi.getAuditTypeOptionsExcluded(this.excludedAuditTypes);
  }
  public onAuditTypeChanged(auditType: IOption) {
    this.organization = null;
    this.facility = null;
    this.form = null;
  }
  public onOrganizationChanged(organization: IOption): void {
    this.clearFacility();
    this.clearForm();

    if (!organization) {
      return;
    }

    this.facilities$ = this.facilityServiceApi.getFacilityOptions(organization.id);

    this.forms$ = this.formServiceApi.getFormOptions(organization.id, this.auditType.name);
  }

  public onFacilityChanged(facility: IFacilityOption): void {
    if (!facility) {
      return;
    }
  }

  public onFormChanged(form: IFormOption): void {
    this.clearError();

    if (!form) {
      return;
    }

    this.form = form;
  }

  public onDateSelection(date: NgbDate) {
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

    this.clearError();
  }

  public isHovered(date: NgbDate) {
    return (
      this.fromDate &&
      !this.toDate &&
      this.hoveredDate &&
      date.after(this.fromDate) &&
      date.before(this.hoveredDate)
    );
  }

  public isInside(date: NgbDate) {
    return this.toDate && date.after(this.fromDate) && date.before(this.toDate);
  }

  public isRange(date: NgbDate) {
    return (
      date.equals(this.fromDate) ||
      (this.toDate && date.equals(this.toDate)) ||
      this.isInside(date) ||
      this.isHovered(date)
    );
  }

  public rangeFormat(dateFrom: NgbDate | null, dateTo: NgbDate | null): string {
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

  public onDownloadClick() {
    this.error = null;

    this.reportRequestServiceApi
      .addReportRequest({
        auditType: this.auditType.name,
        organization: this.organization,
        facility: this.facility,
        form: this.form,
        dateFrom: this.fromDate
          ? moment(this.formatter.format(this.fromDate), MM_DD_YYYY_DOT).format(
              YYYY_MM_DD_DASH
            )
          : null,
        dateTo: this.toDate
          ? moment(this.formatter.format(this.toDate), MM_DD_YYYY_DOT).format(
              YYYY_MM_DD_DASH
            )
          : null,
      })
      .pipe(first())
      .subscribe({
        next: (result: IMessageResponse) => {
          if(result){
            switch(result.status){
              case MessageStatusEnum.success:
                this.toastr.success(result.message);
                this.activeModal.close(true);
                break;
              case MessageStatusEnum.error:
                this.toastr.error(result.message);
                break;
              default:
                this.toastr.info(result.message); 
                break; 
            }          
          };
        },
        error: (error: HttpErrorResponse) => {
          this.spinner.hide("criteriaPdfSpinner");
          this.error = error.message;
          console.log(error);
        },
        complete: () => this.spinner.hide("criteriaPdfSpinner")
      });
  }

  private clearFacility() {
    this.facility = null;
    this.facilities$ = null;

    this.clearForm();
  }

  private clearForm() {
    this.form = null;
    this.forms$ = null;

    this.clearError();
  }

  private clearDate() {
    this.fromDate = null;
    this.toDate = null;
  }

  private clearError() {
    this.error = null;
  }
}
