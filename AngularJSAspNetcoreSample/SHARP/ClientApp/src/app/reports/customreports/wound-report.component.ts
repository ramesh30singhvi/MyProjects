import { Component, OnInit, ViewChild } from "@angular/core";
import { Title } from "@angular/platform-browser";
import { NgbDate, NgbDateParserFormatter } from "@ng-bootstrap/ng-bootstrap";
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexFill,
  ApexLegend,
  ApexPlotOptions,
  ApexResponsive,
  ApexStroke,
  ApexTitleSubtitle,
  ApexTooltip,
  ApexXAxis,
  ApexYAxis,
  ChartComponent,
} from "ng-apexcharts";
import { first } from "rxjs";
import { IFacilityOption, IOption } from "src/app/models/audits/audits.model";
import { FallReport } from "src/app/models/reports/fall-reports.model";
import { WoundReport } from "src/app/models/reports/wound-reports.model";
import { FacilityService } from "src/app/services/facility.service";
import { OrganizationService } from "src/app/services/organization.service";
import { ReportsService } from "src/app/services/reports.service";

export type ChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  title: ApexTitleSubtitle;
  colors: string[];
  yaxis?: ApexYAxis;
  labels: string[];
  legend: ApexLegend;
  plotOptions: ApexPlotOptions;
  dataLabels: ApexDataLabels;
  fill: ApexFill;
  tooltip: ApexTooltip;
  stroke: ApexStroke;
};

@Component({
  selector: "app-custom-report-wound",
  templateUrl: "./wound-report.component.html",
  styleUrls: ["./wound-report.component.scss"],
})
export class WoundReportComponent implements OnInit {
  public selectedTimeFrame: IOption;

  public woundReport: WoundReport;

  public organizations: IOption[] = [];
  public selectedOrganization: IOption;

  public facilities: IFacilityOption[] = [];
  public selectedFacility: IFacilityOption;

  public selectedYear: IOption;
  public availableYears: IOption[] = [];

  public availableMonths: IOption[] = [];
  public selectedMonths: IOption[] = [];

  public canGenerate: boolean = false;

  public byMonthChartOptions: Partial<ChartOptions>;
  public byUlcerChartOptions: Partial<ChartOptions>;
  public byOtherTypesChartOptions: Partial<ChartOptions>[] = [];

  constructor(
    private titleService: Title,
    private organizationServiceApi: OrganizationService,
    private facilityServiceApi: FacilityService,
    private reportServiceApi: ReportsService,
    public formatter: NgbDateParserFormatter
  ) {
    this.getOrganizations();

    var minYear = 2010;
    var maxYear = new Date().getFullYear();

    for (var i = minYear; i <= maxYear; i++) {
      this.availableYears.push({
        id: i,
        name: i.toString(),
      });
    }
    this.availableYears.reverse();
  }

  ngOnInit(): void {
    this.titleService.setTitle("Quarterly Wound Analysis and Trend");
  }

  public onOrganizationChanged(organizations: IOption[]): void {
    this.selectedFacility = null;
    this.selectedYear = null;
    this.selectedMonths = [];
    this.facilities = [];
    if (this.selectedOrganization) {
      this.fetchFacilities();
    }
    this.checkIfCanGenerate();
  }

  public onOrganizationDropdownOpened(): void {
    this.getOrganizations();
  }
  public onFacilityChanged(facility: IOption): void {
    this.selectedYear = null;
    this.selectedMonths = [];
    this.checkIfCanGenerate();
  }

  private getOrganizations() {
    this.organizationServiceApi
      .getOrganizationOptions()
      .pipe(first())
      .subscribe((organizations: IOption[]) => {
        this.organizations = organizations;
      });
  }

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
          this.facilities = facilities;
        },
        complete: () => {},
      });
  }

  public checkIfCanGenerate() {
    this.canGenerate =
      this.selectedOrganization != null &&
      this.selectedFacility != null &&
      this.selectedYear != null &&
      this.selectedMonths.length > 0;
  }

  public onYearChanged(year: IOption) {
    this.selectedMonths = [];

    this.availableMonths = [];

    const currentYear = new Date().getFullYear();
    const currentMonth = new Date().getMonth();

    if (currentYear == year.id) {
      for (let i = 1; i <= currentMonth + 1; i++) {
        let date = new Date(`${year.id}-${i}-1`);
        this.availableMonths.push({
          id: i,
          name: new Intl.DateTimeFormat("en-US", { month: "long" }).format(
            date
          ),
        });
      }
    } else {
      for (let i = 1; i <= 12; i++) {
        let date = new Date(`${year.id}-${i}-1`);
        this.availableMonths.push({
          id: i,
          name: new Intl.DateTimeFormat("en-US", { month: "long" }).format(
            date
          ),
        });
      }
    }
    this.checkIfCanGenerate();
  }

  public onQuarterChanged(year: IOption) {
    this.checkIfCanGenerate();
  }

  public generateReport() {
    this.reportServiceApi
      .getWoundReport(
        this.selectedOrganization.id,
        this.selectedFacility.id,
        this.selectedYear.id,
        this.selectedMonths.map((month) => month.id).sort()
      )
      .pipe(first())
      .subscribe((woundReport: WoundReport) => {
        this.woundReport = woundReport;
        this.initCharts();
      });
  }

  getSelectedMonthsName() {
    return this.selectedMonths.map((month) => month.name).join(", ");
  }

  private initCharts() {
    this.initByMonthChart();
    this.initByUlcer();
    this.initByOtherTypes();
  }

  public getTotalIncidents() {
    return this.woundReport.byMonths.reduce(
      (acc, { total }) => (acc += +(total || 0)),
      0
    );
  }

  public getTotalInHouseIncidents() {
    return this.woundReport.byMonths.reduce(
      (acc, { inHouseAcquired }) => (acc += +(inHouseAcquired || 0)),
      0
    );
  }

  public getTotalReHospitalizationIncidents() {
    return this.woundReport.byMonths.reduce(
      (acc, { reHospitalization }) => (acc += +(reHospitalization || 0)),
      0
    );
  }

  private initByMonthChart() {
    this.byMonthChartOptions = {
      colors: ["#4473c4", "#ed7c31", "#a5a5a5"],

      series: this.woundReport.byMonths.map((month) => ({
        name: `${month.name}`,
        data: [month.total, month.inHouseAcquired, month.reHospitalization],
      })),

      chart: {
        width: "100%",
        height: 450,
        type: "bar",
        toolbar: {
          show: false,
        },
      },

      xaxis: {
        categories: [
          "Wound Incidents",
          "In-House Acquired",
          "Re-Hospitalization",
        ],
      },

      tooltip: {
        y: {
          formatter: function (val) {
            return "$ " + val + " incidents";
          },
        },
      },

      dataLabels: {
        enabled: true,
        style: {
          colors: ["#333"],
        },
        offsetY: -20,
      },
      stroke: {
        show: true,
        width: 2,
      },

      legend: {
        position: "right",
      },

      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: "35%",
          dataLabels: {
            position: "top",
          },
        },
      },
    };
  }

  public getUlcers() {
    let ulcers = this.woundReport.byMonths[0].byTypes
      .filter((type) => type.name.toLowerCase().includes("stage"))
      .sort(function (a, b) {
        if (a.name < b.name) {
          return -1;
        }
        if (a.name > b.name) {
          return 1;
        }
        return 0;
      });

    return ulcers;
  }

  public getOtherTypes() {
    let types = this.woundReport.byMonths[0].byTypes
      .filter((type) => type.name.toLowerCase().includes("stage") == false)
      .sort(function (a, b) {
        if (a.name < b.name) {
          return -1;
        }
        if (a.name > b.name) {
          return 1;
        }
        return 0;
      });

    return types;
  }

  public getUlcerCount(month: any, ulcer: any) {
    let _ulcer = month.byTypes.find((type) => type.name == ulcer.name);
    return _ulcer.count;
  }

  public getOtherTypeCount(month: any, type: any) {
    let _type = month.byTypes.find((t) => t.name == type.name);
    return _type.count;
  }

  public getUlcerPercentage(ulcer: any) {
    let totalUlcerType = 0;
    let totalUlcer = 0;
    this.woundReport.byMonths.forEach((byMonth) => {
      let _ulcer = byMonth.byTypes.find((type) => type.name == ulcer.name);
      totalUlcerType += _ulcer.count;
    });

    this.woundReport.byMonths.forEach((byMonth) => {
      byMonth.byTypes
        .filter((type) => type.name.toLowerCase().includes("stage"))
        .forEach((type) => {
          totalUlcer += type.count;
        });
    });

    return (totalUlcerType * 100) / totalUlcer;
  }

  public getTotalUlcerByMonth(month: any) {
    let totalUlcer = 0;
    month.byTypes
      .filter((type) => type.name.toLowerCase().includes("stage"))
      .forEach((type) => {
        totalUlcer += type.count;
      });
    return totalUlcer;
  }

  public getTotalOtherTypeByMonth(month: any) {
    let total = 0;
    month.byTypes
      .filter((type) => type.name.toLowerCase().includes("stage") == false)
      .forEach((type) => {
        total += type.count;
      });
    return total;
  }

  private initByUlcer() {
    this.byUlcerChartOptions = {
      colors: ["#4473c4", "#ed7c31", "#a5a5a5"],

      series: this.woundReport.byMonths.map((month) => ({
        name: `${month.name}`,
        data: month.byTypes
          .filter((type) => type.name.toLocaleLowerCase().includes("stage"))
          .sort(function (a, b) {
            if (a.name < b.name) {
              return -1;
            }
            if (a.name > b.name) {
              return 1;
            }
            return 0;
          })
          .map((month) => month.count),
      })),

      chart: {
        width: "100%",
        height: 450,
        type: "bar",
        toolbar: {
          show: false,
        },
      },

      xaxis: {
        categories: this.getUlcers().map((type) =>
          type.name.replace("Pressure Ulcer", "")
        ),
      },

      tooltip: {
        y: {
          formatter: function (val) {
            return "$ " + val + " incidents";
          },
        },
      },

      dataLabels: {
        enabled: true,
        style: {
          colors: ["#333"],
        },
        offsetY: -20,
      },
      stroke: {
        show: true,
        width: 2,
      },

      legend: {
        position: "right",
      },

      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: "35%",
          dataLabels: {
            position: "top",
          },
        },
      },
    };
  }

  private initByOtherTypes() {
    this.woundReport.byMonths[0].byTypes
      .filter((type) => type.name.toLowerCase().includes("stage") == false)
      .forEach((type) => {
        this.byOtherTypesChartOptions.push({
          colors: ["#4473c4", "#ed7c31", "#a5a5a5"],

          series: [
            {
              name: type.name,
              data: this.woundReport.byMonths.map(
                (month) => month.byTypes.find((t) => t.name == type.name).count
              ),
            },
          ],

          chart: {
            width: "100%",
            height: 450,
            type: "line",
            toolbar: {
              show: false,
            },
          },

          xaxis: {
            categories: this.woundReport.byMonths.map((month) => month.name),
          },

          tooltip: {
            y: {
              formatter: function (val) {
                return "$ " + val + " incidents";
              },
            },
          },

          dataLabels: {
            enabled: true,
            style: {
              colors: ["#333"],
            },
            offsetY: -20,
          },
          stroke: {
            show: true,
            width: 2,
          },

          legend: {
            position: "right",
          },
          title: {
            text: type.name,
            align: "center",
          },
          plotOptions: {
            bar: {
              horizontal: false,
              columnWidth: "35%",
              dataLabels: {
                position: "top",
              },
            },
          },
        });
      });

    this.byOtherTypesChartOptions.push({
      colors: ["#4473c4", "#ed7c31", "#a5a5a5"],

      series: [
        {
          name: "Pressure Ulcer",
          data: this.woundReport.byMonths.map((month) =>
            month.byTypes
              .filter((t) => t.name.toLowerCase().includes("stage"))
              .reduce((acc, { count }) => (acc += +(count || 0)), 0)
          ),
        },
      ],

      chart: {
        width: "100%",
        height: 450,
        type: "line",
        toolbar: {
          show: false,
        },
      },

      xaxis: {
        categories: this.woundReport.byMonths.map((month) => month.name),
      },

      tooltip: {
        y: {
          formatter: function (val) {
            return "$ " + val + " incidents";
          },
        },
      },

      dataLabels: {
        enabled: true,
        style: {
          colors: ["#333"],
        },
        offsetY: -20,
      },
      stroke: {
        show: true,
        width: 2,
      },

      legend: {
        position: "right",
      },
      title: {
        text: "Pressure Ulcer",
        align: "center",
      },
      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: "35%",
          dataLabels: {
            position: "top",
          },
        },
      },
    });
  }

  public downloadPDF() {
    this.reportServiceApi.getDownloadWoundReport(
      this.selectedOrganization.id,
      this.selectedFacility.id,
      this.selectedYear.id,
      this.selectedMonths.map((month) => month.id).sort()
    );
  }
}
