import { 
  ApexAxisChartSeries, 
  ApexChart, 
  ApexDataLabels, 
  ApexFill, 
  ApexLegend, 
  ApexNonAxisChartSeries, 
  ApexPlotOptions, 
  ApexResponsive, 
  ApexStroke, 
  ApexXAxis, 
  ApexYAxis } from "ng-apexcharts";
import { AuditStatusEnum, IAuditStatus, IOption } from "../audits/audits.model";

export type PieChartOptions = {
    colors: string[];
    series: ApexNonAxisChartSeries;
    chart: ApexChart;
    responsive: ApexResponsive[];
    labels: string[];
    legend: ApexLegend;
    dataLabels: ApexDataLabels;
    stroke: ApexStroke;
    plotOptions: ApexPlotOptions;
  };

  export type BarChartOptions = {
    colors: string[];
    series: ApexAxisChartSeries;
    chart: ApexChart;
    responsive: ApexResponsive[];
    labels: string[];
    legend: ApexLegend;
    dataLabels: ApexDataLabels;
    stroke: ApexStroke;
    plotOptions: ApexPlotOptions;
    xaxis?: ApexXAxis;
    yaxis?: ApexYAxis;
    fill: ApexFill;
  };

  export enum TimeFrameEnum {
    All,
    Today,
    ThisWeek,
    ThisMonth,
    CustomRange,
  }

  export const TimeFrames: IOption[] = [
    //{id: TimeFrameEnum.All, name: 'All'},
    {id: TimeFrameEnum.Today, name: 'Today'},
    {id: TimeFrameEnum.ThisWeek, name: 'This week'},
    {id: TimeFrameEnum.ThisMonth, name: 'This month'},
    {id: TimeFrameEnum.CustomRange, name: 'Custom Range'},
  ]

  export enum DueDateEnum {
    All,
    Today,
    Later,
  }

  export interface IAuditKPIApi {
    organization: IOption,
    auditStatus: AuditStatusEnum;
    auditCount: number;
  }

  export interface IAuditKPI {
    auditStatus: IAuditStatus;
    auditCount: number;
  }

  export interface DashboardFilter {
    timeFrame: string;
    organizationIds: number[] | null;
    facilityIds: number[] | null;
    formIds: number[] | null;
    dueDateType: DueDateEnum;
    //dueDate: string;
  }

  export interface IAuditsDueDateCounts {
    today: number;
    later: number;
  }