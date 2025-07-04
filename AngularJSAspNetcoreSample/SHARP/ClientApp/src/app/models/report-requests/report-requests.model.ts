import { IFilterOption } from "../audits/audit.filters.model";

export interface ReportRequestFiltersModel {
  organizationName?: Array<IFilterOption>;
  facilityName?: Array<IFilterOption>;
  formName?: Array<IFilterOption>;
  auditType?: Array<IFilterOption>;
  userfullName?: Array<IFilterOption>;
  status?: Array<IFilterOption>;
}

export enum ReportRequestStatusEnum {
  Failed,
  InProcess,
  Success,
}

export interface IReportRequestStatus {
  id: number,
  label: string,
  color: string
}

export const ReportRequestStatuses = {
  Failed: {id: ReportRequestStatusEnum.Failed, label: 'Failed', color: '#FF375F'},
  InProccess: {id: ReportRequestStatusEnum.InProcess, label: 'In Proccess', color: '#FF9500'},
  Success: {id: ReportRequestStatusEnum.Success, label: 'Success', color: '#43910A'},
}
