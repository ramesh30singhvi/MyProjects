import { Component, ElementRef, OnInit, ViewChild } from "@angular/core";
import {
  AuditStatuses,
  IFacilityOption,
  IFormOption,
  IOption,
} from "../models/audits/audits.model";
import {
  ChartComponent,
  ApexNonAxisChartSeries,
  ApexResponsive,
  ApexChart,
} from "ng-apexcharts";

import {
  PieChartOptions,
  DashboardFilter,
  DueDateEnum,
  IAuditKPI,
  IAuditKPIApi,
  IAuditsDueDateCounts,
  TimeFrameEnum,
  TimeFrames,
  BarChartOptions,
} from "../models/dashboard/dashboard.model";
import {
  NgbDate,
  NgbDateParserFormatter,
  NgbInputDatepicker,
  NgbModal,
} from "@ng-bootstrap/ng-bootstrap";
import { OrganizationService } from "../services/organization.service";
import { first } from "rxjs";
import { FacilityService } from "../services/facility.service";
import { FormServiceApi } from "../services/form-api.service";
import { DashboardService } from "../services/dashboard.service";
import * as moment from "moment";
import {
  MM_DD_YYYY_HH_MM_A_SLASH,
  MM_DD_YYYY_HH_MM_SS_SLASH,
  MM_DD_YYYY_SLASH,
  YYYY_MM_DD_DASH,
} from "../common/constants/date-constants";
import { LocalStoreRepository } from "../services/repository/local-store-repository";
import { RawInfoComponent } from "./raw-info/raw-info.component";
import { transformDate } from "../common/helpers/dates-helper";

const DASHBOARD_FILTER_STATE = "dashboardFilterState";

@Component({
  selector: "app-dashboard",
  templateUrl: "./dashboard.component.html",
  styleUrls: ["./dashboard.component.scss"],
})
export class DashboardComponent implements OnInit {
  //@ViewChild("pieChart") pieChart: ChartComponent;
  @ViewChild("barChart") barChart: ChartComponent;
  //public pieChartOptions: Partial<PieChartOptions>;
  public barChartOptions: Partial<BarChartOptions>;

  @ViewChild("datepicker") datepicker: NgbInputDatepicker;

  public timeFrameEnum = TimeFrameEnum;
  public timeFrames: IOption[] = TimeFrames;
  public selectedTimeFrame: IOption;

  public hoveredDate: NgbDate | null = null;
  public fromDate: NgbDate | null;
  public toDate: NgbDate | null;
  public selectedFromDate: NgbDate | null;
  public selectedToDate: NgbDate | null;

  public organizations: IOption[];
  public selectedOrganizations: IOption[];

  public facilitiesBuffer: IFacilityOption[] = [];
  public facilityLoading: boolean = false;
  public facilitySearchTerm: string = "";
  private facilityLoaded: boolean = false;
  public selectedFacilities: IFacilityOption[];

  public formsBuffer: IFormOption[] = [];
  public formLoading: boolean = false;
  public formSearchTerm: string = "";
  private formLoaded: boolean = false;
  public selectedForms: IFormOption[];

  public dueDate = DueDateEnum;
  public auditsDueDateCounts: IAuditsDueDateCounts;
  public selectedDueDate: number = 0;

  public auditOrganizationKPIs: IAuditKPIApi[];
  public auditKPIs: any[];
  public isKPIExist: boolean;

  // Sorted Audits
  public sortedAuditKPIs: any[];

  private bufferSize = 50;
  private numberOfItemsFromEndBeforeFetchingMore = 10;

  private filtersRepository: LocalStoreRepository<any>;

  private observer;

  constructor(
    private dashboardServiceApi: DashboardService,
    private organizationServiceApi: OrganizationService,
    private facilityServiceApi: FacilityService,
    private formServiceApi: FormServiceApi,
    public formatter: NgbDateParserFormatter,
    private modalService: NgbModal,
    private host: ElementRef
  ) {
    this.getOrganizations();
    this.sortedAuditKPIs = [];

    this.auditKPIs = Object.values(AuditStatuses).map((status) => {
      return {
        auditStatus: status,
      };
    });

    this.filtersRepository = new LocalStoreRepository<any>(
      DASHBOARD_FILTER_STATE
    );

    const dashboardFilterState = this.filtersRepository.load();

    if (dashboardFilterState) {
      this.selectedTimeFrame =
        dashboardFilterState.timeFrame?.selectedTimeFrame;
      const fromDate = dashboardFilterState.timeFrame?.fromDate;
      this.fromDate = fromDate
        ? new NgbDate(fromDate.year, fromDate.month, fromDate.day)
        : null;
      this.selectedFromDate = fromDate
        ? new NgbDate(fromDate.year, fromDate.month, fromDate.day)
        : null;
      const toDate = dashboardFilterState.timeFrame?.toDate;
      this.toDate = toDate
        ? new NgbDate(toDate.year, toDate.month, toDate.day)
        : null;
      this.selectedToDate = toDate
        ? new NgbDate(toDate.year, toDate.month, toDate.day)
        : null;

      this.selectedOrganizations = dashboardFilterState.organizations;
      this.selectedFacilities = dashboardFilterState.facilities;
      this.selectedForms = dashboardFilterState.forms;

      this.selectedDueDate = dashboardFilterState.dueDate;
    }

    //this.setChartOptions();
    this.setBarChartOptions();
  }

  ngOnInit() {
    this.getAuditKPI();
    this.getDueDateCounts();

    this.observer = new ResizeObserver((entries) => {
      this.barChart?.updateSeries(this.barChartOptions.series);
    });

    this.observer.observe(
      this.host.nativeElement.querySelector(".body-wrapper")
    );
  }

  ngOnDestroy() {
    this.observer.unobserve(
      this.host.nativeElement.querySelector(".body-wrapper")
    );
  }

  public onTimeFrameChanged(timeFrame: IOption): void {
    this.selectedTimeFrame = timeFrame;
    this.fromDate = null;
    this.toDate = null;
    this.selectedFromDate = null;
    this.selectedToDate = null;

    if (this.selectedTimeFrame?.id === this.timeFrameEnum.CustomRange) {
      this.datepicker.toggle();
    } else {
      this.getAuditKPI();
      this.getDueDateCounts();
      this.saveDashboardFilterState();
    }
  }

  public onApplyCustomRangeFilter(): void {
    this.selectedFromDate = this.fromDate;
    this.selectedToDate = this.toDate;

    if (this.selectedFromDate) {
      this.getAuditKPI();
      this.getDueDateCounts();
    } else {
      this.onTimeFrameChanged(null);
    }

    this.saveDashboardFilterState();
  }

  public onCancelCustomRangeFilterClick(): void {}

  public onOrganizationChanged(organizations: IOption[]): void {
    this.facilitiesBuffer = [];
    this.facilityLoaded = false;

    if (this.selectedOrganizations && this.selectedOrganizations.length > 0) {
      this.selectedFacilities = this.selectedFacilities?.filter(
        (facility: IFacilityOption) =>
          this.selectedOrganizations
            ?.map((org: IOption) => org.id)
            .includes(facility.organizationId)
      );

      this.selectedForms = this.selectedForms?.filter((form: IFormOption) =>
        this.selectedOrganizations
          ?.map((org: IOption) => org.id)
          .includes(form.organizationId)
      );
    }

    this.getAuditKPI();
    this.getDueDateCounts();
    this.saveDashboardFilterState();
  }

  public onOrganizationDropdownOpened(): void {
    this.getOrganizations();
  }

  public onFacilityChanged(facility: IOption): void {
    this.getAuditKPI();
    this.getDueDateCounts();
    this.saveDashboardFilterState();
  }

  public onFacilityDropdownOpened(): void {
    this.facilitySearchTerm = "";
    this.facilitiesBuffer = [];
    this.facilityLoaded = false;

    this.fetchFacilityMore();
  }

  public onFacilityScrollToEnd() {
    if (this.facilityLoading || this.facilityLoaded) {
      return;
    }

    this.fetchFacilityMore();
  }

  public facilitySearch(search: any) {
    this.facilitySearchTerm = search.term;
    this.facilitiesBuffer = [];
    this.facilityLoaded = false;

    this.fetchFacilityMore();
  }

  public onFormChanged(form: IOption): void {
    this.getAuditKPI();
    this.getDueDateCounts();
    this.saveDashboardFilterState();
  }

  public onFormDropdownOpened(): void {
    this.formSearchTerm = "";
    this.formsBuffer = [];
    this.formLoaded = false;

    this.fetchFormMore();
  }

  public onFormScrollToEnd() {
    if (this.formLoading || this.formLoaded) {
      return;
    }

    this.fetchFormMore();
  }

  public formSearch(search: any) {
    this.formSearchTerm = search.term;
    this.formsBuffer = [];
    this.formLoaded = false;

    this.fetchFormMore();
  }

  public onDueDateClick(dueDate: number): void {
    this.getAuditKPI();
    this.saveDashboardFilterState();
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

    this.selectedTimeFrame = {
      ...this.selectedTimeFrame,
      name: this.rangeFormat(this.fromDate, this.toDate),
    };
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
      dateRange = dateFrom ? this.formatter.format(dateFrom) : "";
    }

    return dateRange;
  }

  public onRawInfoClick(): void {
    const modalRef = this.modalService.open(RawInfoComponent, {
      modalDialogClass: "custom-modal raw-info-modal",
      centered: true,
    });
    modalRef.componentInstance.auditOrganizationKPIs =
      this.auditOrganizationKPIs;
    modalRef.componentInstance.organizations =
      this.selectedOrganizations && this.selectedOrganizations.length > 0
        ? this.selectedOrganizations
        : this.organizations;
  }

  private getOrganizations() {
    this.organizationServiceApi
      .getOrganizationOptions()
      .pipe(first())
      .subscribe((organizations: IOption[]) => {
        this.organizations = organizations;
      });
  }

  private async fetchFacilityMore() {
    this.facilityLoading = true;

    this.facilityServiceApi
      .getFacilityFilteredOptions(
        this.facilitySearchTerm,
        this.facilitiesBuffer?.length,
        this.bufferSize,
        this.selectedOrganizations
          ?.filter((org: IOption) => org.id > 0)
          .map((org: IOption) => org.id)
      )
      .pipe(first())
      .subscribe({
        next: (facilities: IOption[]) => {
          this.facilitiesBuffer = this.facilitiesBuffer.concat(facilities);
          this.facilityLoaded = facilities?.length < this.bufferSize;
        },
        complete: () => {
          this.facilityLoading = false;
        },
      });
  }

  private async fetchFormMore() {
    this.formLoading = true;

    this.formServiceApi
      .getFormFilteredOptions(
        this.formSearchTerm,
        this.formsBuffer?.length,
        this.bufferSize,
        this.selectedOrganizations
          ?.filter((org: IOption) => org.id > 0)
          .map((org: IOption) => org.id)
      )
      .pipe(first())
      .subscribe({
        next: (forms: IFormOption[]) => {
          this.formsBuffer = this.formsBuffer.concat(forms);
          this.formLoaded = forms?.length < this.bufferSize;
        },
        complete: () => {
          this.formLoading = false;
        },
      });
  }

  private getDueDateCounts(): void {
    const filter: DashboardFilter = this.getDashboardFilter();

    this.dashboardServiceApi
      .getAuditsDueDateCount(filter)
      .pipe(first())
      .subscribe((counts: IAuditsDueDateCounts) => {
        this.auditsDueDateCounts = counts;
      });
  }

  private getAuditKPI(): void {
    const filter: DashboardFilter = this.getDashboardFilter();

    this.dashboardServiceApi
      .getAuditKPIs(filter)
      .pipe(first())
      .subscribe((auditKPIs: IAuditKPIApi[]) => {
        this.auditOrganizationKPIs = auditKPIs;

        this.auditKPIs = this.auditKPIs.map(({ auditStatus }) => {
          return {
            auditStatus,
            auditCount: auditKPIs
              .filter(
                (auditKPI: IAuditKPIApi) =>
                  auditKPI.auditStatus === auditStatus.id
              )
              .reduce((prev, cur) => prev + cur.auditCount, 0),
          };
        });

        this.sortedAuditKPIs = [];

        this.sortedAuditKPIs.push(
          this.auditKPIs.find(
            (auditKpi) => auditKpi.auditStatus.id == AuditStatuses.Triggered.id
          )
        );
        this.sortedAuditKPIs.push(
          this.auditKPIs.find(
            (auditKpi) => auditKpi.auditStatus.id == AuditStatuses.Duplicated.id
          )
        );
        this.sortedAuditKPIs.push(
          this.auditKPIs.find(
            (auditKpi) => auditKpi.auditStatus.id == AuditStatuses.InProgress.id
          )
        );
        this.sortedAuditKPIs.push(
          this.auditKPIs.find(
            (auditKpi) =>
              auditKpi.auditStatus.id == AuditStatuses.WaitingForApproval.id
          )
        );
        this.sortedAuditKPIs.push(
          this.auditKPIs.find(
            (auditKpi) => auditKpi.auditStatus.id == AuditStatuses.Approved.id
          )
        );
        this.sortedAuditKPIs.push(
          this.auditKPIs.find(
            (auditKpi) => auditKpi.auditStatus.id == AuditStatuses.Submitted.id
          )
        );

        const auditCounts = this.auditKPIs
          ?.filter(
            (auditKpi) =>
              auditKpi.auditStatus.id != AuditStatuses.Disapproved.id &&
              auditKpi.auditStatus.id != AuditStatuses.Reopened.id
          )
          ?.map((auditKPI: IAuditKPI) => auditKPI.auditCount);

        //this.pieChartOptions.series = [...auditCounts];

        let maxCount = Math.max(...auditCounts);
        maxCount = maxCount < 5 ? 5 : maxCount;

        this.barChartOptions = {
          ...this.barChartOptions,
          series: [
            {
              ...this.barChartOptions.series,
              name: "Count",
              data: auditCounts,
            },
          ],
          yaxis: {
            ...this.barChartOptions.yaxis,
            tickAmount: maxCount < 10 ? maxCount : 10,
            max: maxCount,
          },
        };

        this.isKPIExist = this.auditKPIs?.some(
          (auditKPI: IAuditKPI) => auditKPI.auditCount > 0
        );
      });
  }

  private getDashboardFilter(): DashboardFilter {
    const filter: DashboardFilter = {
      organizationIds: this.selectedOrganizations?.map(
        (org: IOption) => org.id
      ),
      facilityIds: this.selectedFacilities?.map(
        (facility: IOption) => facility.id
      ),
      formIds: this.selectedForms?.map((form: IOption) => form.id),
      timeFrame: this.buildTimeFrameFilter(),
      dueDateType: this.selectedDueDate,
    };

    return filter;
  }

  private buildTimeFrameFilter(): string {
    if (!this.selectedTimeFrame) {
      return null;
    }

    switch (this.selectedTimeFrame.id) {
      case TimeFrameEnum.Today:
        return "day";

      case TimeFrameEnum.ThisWeek:
        return "week";

      case TimeFrameEnum.ThisMonth:
        return "month";

      case TimeFrameEnum.CustomRange:
        if (!this.selectedFromDate && !this.selectedToDate) {
          return null;
        }

        return this.selectedToDate
          ? JSON.stringify({
              firstCondition: transformDate({
                dateFrom: this.formatter.format(this.selectedFromDate),
                dateTo: this.formatter.format(this.selectedToDate),
                type: "inRange",
              }),
            })
          : JSON.stringify({
              firstCondition: transformDate({
                dateFrom: this.formatter.format(this.selectedFromDate),
                dateTo: null,
                type: "equals",
              }),
            });

      default:
        return null;
    }
  }

  private saveDashboardFilterState(): void {
    this.filtersRepository.save({
      timeFrame: {
        selectedTimeFrame: this.selectedTimeFrame,
        fromDate: this.selectedFromDate,
        toDate: this.selectedToDate,
      },
      organizations: this.selectedOrganizations,
      facilities: this.selectedFacilities,
      forms: this.selectedForms,
      dueDate: this.selectedDueDate,
    });
  }
  //Pie chart removed from dashboard for ticket ACS-21
  //private setChartOptions(): void {
  //  this.pieChartOptions = {
  //    colors: this.auditKPIs
  //      ?.filter(
  //        (auditKpi) =>
  //          auditKpi.auditStatus.id != AuditStatuses.Disapproved.id &&
  //          auditKpi.auditStatus.id != AuditStatuses.Reopened.id
  //      )
  //      ?.map((auditKPI: IAuditKPI) => auditKPI.auditStatus.color),
  //    series: this.auditKPIs
  //      ?.filter(
  //        (auditKpi) =>
  //          auditKpi.auditStatus.id != AuditStatuses.Disapproved.id &&
  //          auditKpi.auditStatus.id != AuditStatuses.Reopened.id
  //      )
  //      ?.map(() => 0),
  //    chart: {
  //      width: 640,
  //      height: 640,
  //      type: "pie",
  //    },
  //    labels: this.auditKPIs
  //      ?.filter(
  //        (auditKpi) =>
  //          auditKpi.auditStatus.id != AuditStatuses.Disapproved.id &&
  //          auditKpi.auditStatus.id != AuditStatuses.Reopened.id
  //      )
  //      ?.map((auditKPI: IAuditKPI) => auditKPI.auditStatus.label),
  //    responsive: [
  //      {
  //        breakpoint: 480,
  //        options: {
  //          chart: {
  //            width: 200,
  //          },
  //          legend: {
  //            position: "bottom",
  //          },
  //        },
  //      },
  //    ],
  //    legend: {
  //      position: "left",
  //      fontSize: "14px",
  //      markers: {
  //        width: 6,
  //        height: 12,
  //        radius: 2,
  //        offsetX: -5,
  //      },
  //      itemMargin: {
  //        vertical: 3,
  //      },
  //      offsetY: -10,
  //      offsetX: -10,
  //    },
  //    dataLabels: {
  //      textAnchor: "start",
  //      style: {
  //        fontSize: "12",
  //        colors: ["#fff"],
  //        fontWeight: 200,
  //      },
  //    },
  //    stroke: {
  //      width: 0,
  //    },
  //    plotOptions: {
  //      pie: {
  //        dataLabels: {
  //          offset: -5,
  //        },
  //      },
  //    },
  //  };
  //}

  private setBarChartOptions(): void {
    this.barChartOptions = {
      colors: this.auditKPIs?.map(
        (auditKPI: IAuditKPI) => auditKPI.auditStatus.color
      ),
      series: [{ data: [] }],
      chart: {
        type: "bar",
        height: 320,
        toolbar: {
          show: false,
        },
      },
      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: "80%",
          distributed: true,
          borderRadius: 10,
        },
      },
      dataLabels: {
        enabled: false,
      },
      legend: {
        show: false,
      },
      xaxis: {
        categories: this.auditKPIs?.map(
          (auditKPI: IAuditKPI) => auditKPI.auditStatus.label
        ),
        labels: {
          show: false,
        },
        axisTicks: {
          show: false,
        },
        tooltip: {
          enabled: false,
        },
        axisBorder: {
          show: false,
        },
      },
      yaxis: {
        forceNiceScale: false,
        decimalsInFloat: undefined,
      },
      fill: {
        opacity: 1,
      },
    };
  }
}
