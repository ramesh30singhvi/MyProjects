import { Component, OnInit } from "@angular/core";
import { Title } from "@angular/platform-browser";
import { OrganizationService } from "../../services/organization.service";
import { FacilityService } from "../../services/facility.service";
import { ReportsService } from "../../services/reports.service";
import { IOption } from "../../models/audits/audits.model";
import { first } from "rxjs";
import {
  NgbDate,
  NgbDateParserFormatter,
} from "@ng-bootstrap/ng-bootstrap";
import { FormServiceApi } from "src/app/services/form-api.service";

@Component({
  selector: "app-custom-report-criteria",
  templateUrl: "./criteria-report.component.html",
  styleUrls: ["./criteria-report.component.scss"],
})
export class CriteriaReportComponent implements OnInit {
  public organizations: IOption[] = [];
  public selectedOrganization: IOption | undefined;

  public facilities: IOption[] = [];
  public selectedFacilities: IOption[] | undefined;

  public forms: IOption[] = [];
  public selectedForms: IOption[] | undefined;

  public questions: IOption[] = [];
  public selectedQuestions: IOption[] | undefined;

  public compliantResidentsAnswers: IOption[] = [
    { id: 1, name: "Yes" },
    { id: 2, name: "No" },
    { id: 3, name: "N/A" },
  ];
  public selectedCompliantResidentsAnswer: IOption =
    this.compliantResidentsAnswers[0];

  public canGenerate = false;

  public fromAuditDate: NgbDate | null;
  public toAuditDate: NgbDate | null;
  public auditHoveredDate: NgbDate | null = null;

  public fromDate: NgbDate | null;
  public toDate: NgbDate | null;
  public hoveredDate: NgbDate | null = null;

  constructor(
    private titleService: Title,
    private organizationServiceApi: OrganizationService,
    private facilityServiceApi: FacilityService,
    private reportsServiceApi: ReportsService,
    private formServiceApi: FormServiceApi,
    public formatter: NgbDateParserFormatter
  ) {
    this.getOrganizations();
  }

  ngOnInit() {
    this.titleService.setTitle("Criteria Reports");
  }

  // Organizations
  private getOrganizations() {
    this.organizationServiceApi
      .getOrganizationOptions()
      .pipe(first())
      .subscribe((organizations: IOption[]) => {
        this.organizations = organizations;
      });
  }
  public onOrganizationDropdownOpened(): void {
    this.getOrganizations();
  }
  public onOrganizationChanged(organizations: IOption[]): void {
    this.selectedFacilities = undefined;
    this.selectedForms = undefined;
    this.forms = [];
    this.facilities = [];
    if (this.selectedOrganization) {
      this.fetchForms();
      this.fetchFacilities();
    }
    this.checkIfCanGenerate();
  }

  // Forms
  private async fetchForms() {
    this.formServiceApi
      .getFormVersionOptions(this.selectedOrganization.id, "Criteria")
      .pipe(first())
      .subscribe({
        next: (forms: IOption[]) => {
          forms.unshift({
            id: 0,
            name: "Select All",
          });
          this.forms = forms;
        },
        complete: () => {},
      });
  }
  public onFormSelected(form: IOption): void {
    this.questions = [];
    this.selectedQuestions = undefined;
    if (this.selectedForms) {
      var _selectedForms = this.selectedForms;

      if (_selectedForms.findIndex((form) => form.id == 0) > -1) {
        _selectedForms = this.forms;
      }

      this.formServiceApi
        .getQuestions(_selectedForms.map((form) => form.id))
        .pipe(first())
        .subscribe({
          next: (questions: IOption[]) => {
            questions.unshift({
              id: 0,
              name: "Select All",
            });
            this.questions = questions;
          },
          complete: () => {},
        });
    }
    this.checkIfCanGenerate();
  }

  public onCriteriaSelected(criteria: IOption): void {
    this.checkIfCanGenerate();
  }

  // Facilities
  private async fetchFacilities() {
    this.facilityServiceApi
      .getFacilityFilteredOptions(
        null,
        0,
        9999,
        [this.selectedOrganization]
          ?.filter((org: IOption) => org.id > 0)
          .map((org: IOption) => org.id)
      )
      .pipe(first())
      .subscribe({
        next: (facilities: IOption[]) => {
          facilities.unshift({
            id: 0,
            name: "Select All",
          });

          this.facilities = facilities;
        },
        complete: () => {},
      });
  }
  public onFacilityChanged(facilities: IOption[]): void {
    this.checkIfCanGenerate();
  }
  public checkIfCanGenerate() {
    this.canGenerate =
      this.selectedOrganization != null &&
      this.selectedFacilities != undefined &&
      this.fromAuditDate != null &&
      this.selectedForms != undefined &&
      this.selectedQuestions != undefined &&
      this.fromDate != null;
  }

  public downloadReport() {
    var _selectedForms = this.selectedForms;

    if (_selectedForms.findIndex((form) => form.id == 0) > -1) {
      _selectedForms = this.forms;
    }

    var _selectedQuestions = this.selectedQuestions;

    if (_selectedQuestions.findIndex((question) => question.id == 0) > -1) {
      _selectedQuestions = this.questions;
    }

    this.reportsServiceApi.getDownloadCriteria(
      this.selectedOrganization.id,
      this.selectedFacilities.map((facility) => facility.id),
      this.formatter.format(this.fromAuditDate),
      this.formatter.format(this.toAuditDate),
      this.formatter.format(this.fromDate),
      this.formatter.format(this.toDate),
      _selectedForms.map((form) => form.id),
      _selectedQuestions.map((question) => question.id),
      this.selectedCompliantResidentsAnswer.id
    );
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
  }

  public onAuditDateSelection(date: NgbDate) {
    if (!this.fromAuditDate && !this.toAuditDate) {
      this.fromAuditDate = date;
    } else if (
      this.fromAuditDate &&
      !this.toAuditDate &&
      date &&
      date.after(this.fromAuditDate)
    ) {
      this.toAuditDate = date;
    } else {
      this.toAuditDate = null;
      this.fromAuditDate = date;
    }
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

  public isAuditHovered(date: NgbDate) {
    return (
      this.fromAuditDate &&
      !this.toAuditDate &&
      this.auditHoveredDate &&
      date.after(this.fromAuditDate) &&
      date.before(this.auditHoveredDate)
    );
  }

  public isInside(date: NgbDate) {
    return this.toDate && date.after(this.fromDate) && date.before(this.toDate);
  }

  public isAuditInside(date: NgbDate) {
    return this.toAuditDate && date.after(this.fromAuditDate) && date.before(this.toAuditDate);
  }

  public isRange(date: NgbDate) {
    return (
      date.equals(this.fromDate) ||
      (this.toDate && date.equals(this.toDate)) ||
      this.isInside(date) ||
      this.isHovered(date)
    );
  }

  public isAuditRange(date: NgbDate) {
    return (
      date.equals(this.fromAuditDate) ||
      (this.toAuditDate && date.equals(this.toAuditDate)) ||
      this.isInside(date) ||
      this.isHovered(date)
    );
  }

  public rangeFormat(dateFrom: NgbDate | null, dateTo: NgbDate | null): string {
    let dateRange = "";

    if (dateFrom && dateTo && !dateFrom.equals(dateTo)) {
      dateRange = `${this.formatter.format(dateFrom)} - ${this.formatter.format(
        dateTo
      )}`;
    } else {
      dateRange = this.formatter.format(dateFrom);
    }

    return dateRange;
  }
}
