import { FacilityModel } from "../organizations/facility.model";
import { IOption } from "../audits/audits.model";

export type DashboardInput = {
  id: number;
  name: string;
  facilities: FacilityModel[];
  dashboardInputTables: DashboardInputTable[];
  dashboardInputSummaries: DashboardInputSummary[];
};

export type DashboardInputTable = {
  id: number;
  name: string;
  dashboardInputGroups: DashboardInputGroup[];
};

export type DashboardInputGroup = {
  id: number;
  name: string;
  dashboardInputElements: DashboardInputElement[];
};

export type DashboardInputElement = {
  id: number;
  formId?: number;
  name: string;
  keyword: string;
  dashboardInputValues: DashboardInputValue[];
};

export type DashboardInputValue = {
  id: number;
  value: number;
  facilityId: number;
  facility?: FacilityModel;
  elementId: number;
  date: string;
};

export type DashboardInputSummary = {
  auditor: string;
  dashboardInputSummaryShift: DashboardInputSummaryShift[];
}

export type DashboardInputSummaryShift = {
  name: string;
  total: number;
  formNames: string[];
}

export enum DateRangeEnum {
  All,
  Today,
  ThisWeek,
  ThisMonth,
  CustomRange,
}

export const DateRanges: IOption[] = [
  //{id: TimeFrameEnum.All, name: 'All'},
  { id: DateRangeEnum.Today, name: 'Today' },
  { id: DateRangeEnum.ThisWeek, name: 'This week' },
  { id: DateRangeEnum.ThisMonth, name: 'This month' },
  { id: DateRangeEnum.CustomRange, name: 'Custom Range' },
]

export interface DashboardInputFilter {
  organizationId: number | null;
  dateRange: string;
  dateRangeFromTo: DateFromTo;
  inputTab: InputTabsEnum;
}

export enum InputTabsEnum {
  Daily,
  Weekly,
  Monthly,
  AuditorProductivity,
}

export interface IInputTableCounts {
  daily: number,
  weekly: number,
  monthly: number,
  auditorProductivity: number,
}

export type DateFromTo = {
  dateFrom: string;
  dateTo: string;
}
