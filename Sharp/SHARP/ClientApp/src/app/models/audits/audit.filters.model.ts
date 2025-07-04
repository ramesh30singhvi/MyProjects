import { IFacilityOption, IFormOption, IOption } from "./audits.model";

export interface AuditFiltersModel {
  organizationName?: Array<IFilterOption>;
  facilityName?: Array<IFilterOption>;
  formName?: Array<IFilterOption>;
  auditType?: Array<IFilterOption>;
  auditorName?: Array<IFilterOption>;
  status?: Array<IFilterOption>;
  state?: number;
}
export interface ReportFiltersModel {
  organizationName?: IFilterOption;
  reportName?: IFilterOption;
  facilityName?: Array<IFilterOption>;
  reportType?: Array<IFilterOption>;
  reportRange?: Array<IFilterOption>;
  reportCategory?: Array<IFilterOption>;

 
}
export interface FilterValues {
  values: Array<string>;
}

export interface IFilterOption {
  id: number,
  value: string,
}

export enum DatePeriodEnum {
  Undefined = 0,
  Today = 1,
  LastSevenDays
}


export interface IPdfFilterModel {
  auditType: string;
  organization: IOption;
  facility: IFacilityOption;
  form: IFormOption;
  dateFrom: string;
  dateTo: string;
}
