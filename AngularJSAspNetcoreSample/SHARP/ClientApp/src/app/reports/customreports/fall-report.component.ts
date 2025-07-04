import { Component, OnInit, ViewChild } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { NgbDate, NgbDateParserFormatter } from '@ng-bootstrap/ng-bootstrap';
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
} from 'ng-apexcharts';
import { first } from 'rxjs';
import { IFacilityOption, IOption } from 'src/app/models/audits/audits.model';
import { FallReport } from 'src/app/models/reports/fall-reports.model';
import { FacilityService } from 'src/app/services/facility.service';
import { OrganizationService } from 'src/app/services/organization.service';
import { ReportsService } from 'src/app/services/reports.service';

interface ChartOptions {
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
}

@Component({
  selector: 'app-custom-report-fall',
  templateUrl: './fall-report.component.html',
  styleUrls: ['./fall-report.component.scss'],
})
export class FallReportComponent implements OnInit {
  public selectedTimeFrame: IOption;

  public fallReport: FallReport;

  public organizations: IOption[] = [];
  public selectedOrganization: IOption;

  public facilities: IFacilityOption[] = [];
  public selectedFacility: IFacilityOption;

  public selectedYear: IOption;
  public availableYears: IOption[] = [];

  public availableMonths: IOption[] = [];
  public selectedMonths: IOption[] = [];

  public canGenerate = false;

  public byMonthChartOptions: Partial<ChartOptions>;
  public byDayChartOptions: Partial<ChartOptions>;
  public byShiftChartOptions: Partial<ChartOptions>;
  public byPlaceChartOptions: Partial<ChartOptions>;
  public byActivityChartOptions: Partial<ChartOptions>;

  public byMonthChart: string | null;
  public byDayChart: string | null;
  public byShiftChart: string | null;
  public byPlaceChart: string | null;
  public byActivityChart: string | null;

  constructor(
    private titleService: Title,
    private organizationServiceApi: OrganizationService,
    private facilityServiceApi: FacilityService,
    private reportServiceApi: ReportsService,
    public formatter: NgbDateParserFormatter
  ) {
    this.getOrganizations();

    const minYear = 2010;
    const maxYear = new Date().getFullYear();

    for (let i = minYear; i <= maxYear; i++) {
      this.availableYears.push({
        id: i,
        name: i.toString(),
      });
    }
    this.availableYears.reverse();
  }

  ngOnInit(): void {
    this.titleService.setTitle('Quarterly Fall Analysis and Trend');
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
        const date = new Date(`${year.id}-${i}-1`);
        this.availableMonths.push({
          id: i,
          name: new Intl.DateTimeFormat('en-US', { month: 'long' }).format(
            date
          ),
        });
      }
    } else {
      for (let i = 1; i <= 12; i++) {
        const date = new Date(`${year.id}-${i}-1`);
        this.availableMonths.push({
          id: i,
          name: new Intl.DateTimeFormat('en-US', { month: 'long' }).format(
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
      .getFallReport(
        this.selectedOrganization.id,
        this.selectedFacility.id,
        this.selectedYear.id,
        this.selectedMonths.map((month) => month.id).sort()
      )
      .pipe(first())
      .subscribe((fallReport: FallReport) => {
        this.fallReport = fallReport;
        this.initCharts();
      });
  }

  private initCharts() {
    this.initByMonthChart();
    this.initByDay();
    this.initByShift();
    this.initByPlace();
    this.initByActivity();
  }

  getTotalActivity() {
    return this.fallReport.byActivity.reduce(
      (acc, { count }) => (acc += +(count || 0)),
      0
    );
  }

  getTotalPlaces() {
    return this.fallReport.byPlace.reduce(
      (acc, { count }) => (acc += +(count || 0)),
      0
    );
  }

  getTotalShifts() {
    return this.fallReport.byShift.reduce(
      (acc, { byTime }) =>
        (acc += +(
          byTime.reduce((acc, { count }) => (acc += +(count || 0)), 0) || 0
        )),
      0
    );
  }

  getTotalMonths() {
    return this.fallReport.byMonth.reduce(
      (acc, { total }) => (acc += +(total || 0)),
      0
    );
  }

  getTotalMonthsMajorInjury() {
    return this.fallReport.byMonth.reduce(
      (acc, { majorInjury }) => (acc += +(majorInjury || 0)),
      0
    );
  }

  getTotalMonthsSentToHospital() {
    return this.fallReport.byMonth.reduce(
      (acc, { sentToHospital }) => (acc += +(sentToHospital || 0)),
      0
    );
  }

  getTotalMajorInjuryPercentage() {
    return (this.getTotalMonthsMajorInjury() * 100) / this.getTotalMonths();
  }

  getTotalSentToHospitalPercentage() {
    return (this.getTotalMonthsSentToHospital() * 100) / this.getTotalMonths();
  }

  getSelectedMonthsName() {
    return this.selectedMonths.map((month) => month.name).join(', ');
  }

  getTotalSundays() {
    return this.fallReport.byMonth.reduce(
      (acc, { byDay }) => (acc += +(byDay.sunday || 0)),
      0
    );
  }

  getTotalMondays() {
    return this.fallReport.byMonth.reduce(
      (acc, { byDay }) => (acc += +(byDay.monday || 0)),
      0
    );
  }

  getTotalTuesdays() {
    return this.fallReport.byMonth.reduce(
      (acc, { byDay }) => (acc += +(byDay.tuesday || 0)),
      0
    );
  }

  getTotalWednesdays() {
    return this.fallReport.byMonth.reduce(
      (acc, { byDay }) => (acc += +(byDay.wednesday || 0)),
      0
    );
  }

  getTotalThursdays() {
    return this.fallReport.byMonth.reduce(
      (acc, { byDay }) => (acc += +(byDay.thursday || 0)),
      0
    );
  }

  getTotalFridays() {
    return this.fallReport.byMonth.reduce(
      (acc, { byDay }) => (acc += +(byDay.friday || 0)),
      0
    );
  }

  getTotalSaturdays() {
    return this.fallReport.byMonth.reduce(
      (acc, { byDay }) => (acc += +(byDay.saturday || 0)),
      0
    );
  }

  private initByMonthChart() {
    this.byMonthChartOptions = {
      colors: ['#4473c4', '#ed7c31', '#a5a5a5'],

      series: this.fallReport.byMonth.map((month) => ({
        name: `${month.name} ${this.selectedYear.name}`,
        data: [month.total, month.majorInjury, month.sentToHospital],
      })),

      chart: {
        id: 'byMonthChart',
        width: '100%',
        height: 450,
        type: 'bar',
        toolbar: {
          show: false,
        },
      },

      xaxis: {
        categories: [
          'Total fall',
          'Falls with Major Injury',
          'Sent to Hospital',
        ],
      },
      yaxis: {
        title: {
          text: `MONTHLY TREND ${this.selectedYear.name}`,
        },
      },

      tooltip: {
        y: {
          formatter: function (val) {
            return '$ ' + val + ' incidents';
          },
        },
      },

      dataLabels: {
        enabled: true,
        style: {
          colors: ['#333'],
        },
        offsetY: -20,
      },
      stroke: {
        show: true,
        width: 2,
      },

      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: '35%',
          dataLabels: {
            position: 'top',
          },
        },
      },
    };
  }

  private initByDay() {
    this.byDayChartOptions = {
      colors: ['#4473c4', '#ed7c31', '#a5a5a5'],

      series: this.fallReport.byMonth.map((month) => ({
        name: `${month.name} ${this.selectedYear.name}`,
        data: [
          month.byDay.sunday,
          month.byDay.monday,
          month.byDay.tuesday,
          month.byDay.wednesday,
          month.byDay.thursday,
          month.byDay.friday,
          month.byDay.saturday,
        ],
      })),

      chart: {
        id: 'byDayChart',
        width: 900,
        height: 450,
        type: 'bar',
        toolbar: {
          show: false,
        },
      },

      xaxis: {
        categories: [
          'Sunday',
          'Monday',
          'Tuesday',
          'Wednesday',
          'Thursday',
          'Friday',
          'Saturday',
        ],
      },
      yaxis: {
        title: {
          text: `MONTHLY TREND ${this.selectedYear.name}`,
        },
      },

      dataLabels: {
        enabled: true,
        style: {
          colors: ['#333'],
        },
        offsetY: -20,
      },
      stroke: {
        show: true,
        width: 2,
      },

      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: '35%',
          dataLabels: {
            position: 'top',
          },
        },
      },
    };
  }

  private initByShift() {
    this.byShiftChartOptions = {
      colors: ['#4473c4'],

      series: [
        {
          name: 'Total incidents',
          data: this.fallReport.byShift.flatMap((byShift) =>
            byShift.byTime.flatMap((byTime) => byTime.count)
          ),
        },
      ],

      chart: {
        id: 'byShiftChart',
        width: 900,
        height: 450,
        type: 'bar',
        toolbar: {
          show: false,
        },
      },

      xaxis: {
        categories: this.fallReport.byShift.flatMap((byShift) =>
          byShift.byTime.flatMap((byTime) => byTime.name)
        ),
      },
      yaxis: {
        title: {
          text: `MONTHLY TREND ${this.selectedYear.name}`,
        },
      },

      dataLabels: {
        enabled: true,
        style: {
          colors: ['#333'],
        },
        offsetY: -20,
      },
      stroke: {
        show: true,
        width: 2,
      },

      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: '15%',
          dataLabels: {
            position: 'top',
          },
        },
      },
    };
  }

  private initByPlace() {
    this.byPlaceChartOptions = {
      colors: ['#4473c4'],

      series: [
        {
          name: 'Total incidents',
          data: this.fallReport.byPlace.flatMap((byPlace) => byPlace.count),
        },
      ],

      chart: {
        id: 'byPlaceChart',
        width: 900,
        height: 450,
        type: 'bar',
        toolbar: {
          show: false,
        },
      },

      xaxis: {
        categories: this.fallReport.byPlace.flatMap((byPlace) => byPlace.name),
      },
      yaxis: {
        title: {
          text: `MONTHLY TREND ${this.selectedYear.name}`,
        },
      },

      dataLabels: {
        enabled: true,
        style: {
          colors: ['#333'],
        },
        offsetY: -20,
      },
      stroke: {
        show: true,
        width: 2,
      },

      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: '15%',
          dataLabels: {
            position: 'top',
          },
        },
      },
    };
  }

  private initByActivity() {
    this.byActivityChartOptions = {
      colors: ['#4473c4'],

      series: [
        {
          name: 'Total incidents',
          data: this.fallReport.byActivity.flatMap(
            (byActivity) => byActivity.count
          ),
        },
      ],

      chart: {
        id: 'byActivityChart',
        width: 900,
        height: 450,
        type: 'bar',
        toolbar: {
          show: false,
        },
      },

      xaxis: {
        categories: this.fallReport.byActivity.flatMap(
          (byActivity) => byActivity.name
        ),
      },
      yaxis: {
        title: {
          text: `MONTHLY TREND ${this.selectedYear.name}`,
        },
      },

      dataLabels: {
        enabled: true,
        style: {
          colors: ['#333'],
        },
        offsetY: -20,
      },
      stroke: {
        show: true,
        width: 2,
      },

      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: '15%',
          dataLabels: {
            position: 'top',
          },
        },
      },
    };
  }

  public downloadPDF() {
    this.reportServiceApi.getDownloadFallReport(
      this.selectedOrganization.id,
      this.selectedFacility.id,
      this.selectedYear.id,
      this.selectedMonths.map((month) => month.id).sort()
    );
  }
}
