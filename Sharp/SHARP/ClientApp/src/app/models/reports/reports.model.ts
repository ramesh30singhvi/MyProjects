import { IFilterOption } from "../audits/audit.filters.model";
import { IOption } from "../audits/audits.model";

export interface Report {
  id: number;
  name: string;
  tableauUrl: string;
  reportUrl: string;
}

export enum ReportAIStatusEnum {
  Triggered,
  InProgress,
  WaitingForApproval,
  Disapproved,
  Approved,
  Reopened,
  Submitted,
}

export interface IReportAIStatus {
  id: number;
  label: string;
  color: string;
}

export const ReportAIStatuses = {
  Triggered: {
    id: ReportAIStatusEnum.Triggered,
    label: "Triggered",
    color: "#54667A",
  },
  InProgress: {
    id: ReportAIStatusEnum.InProgress,
    label: "In Progress",
    color: "#FF9500",
  },
  WaitingForApproval: {
    id: ReportAIStatusEnum.WaitingForApproval,
    label: "Waiting for Approval",
    color: "#BD10E0",
  },
  Disapproved: {
    id: ReportAIStatusEnum.Disapproved,
    label: "Disapproved",
    color: "#FF375F",
  },
  Approved: {
    id: ReportAIStatusEnum.Approved,
    label: "Approved",
    color: "#007AAF",
  },
  Reopened: {
    id: ReportAIStatusEnum.Reopened,
    label: "Reopened",
    color: "#909BA8",
  },
  Submitted: {
    id: ReportAIStatusEnum.Submitted,
    label: "Submitted",
    color: "#43910A",
  },
};

export const ReportAIActions = {
  Edit: { id: 1, label: "Edit", color: "#FF9500" },
  SendForApproval: { id: 2, label: "Send for Approval", color: "#BD10E0" },
  ReopenToInProgress: { id: 3, label: "Reopen", color: "#FF375F" },
  ReopenToReopened: { id: 4, label: "Reopen", color: "#FF375F" },
  Approve: { id: 5, label: "Approve", color: "#007AAF" },
  Disapprove: { id: 6, label: "Disapprove", color: "#909BA8" },
  Submit: { id: 7, label: "Submit", color: "#43910A" },
};

export interface IReportAIAction {
  id: number;
  label: string;
  classes: string;
}

export interface KeywordAIReportFiltersModel {
  organization?: Array<IFilterOption>;
  facility?: Array<IFilterOption>;
  status?: Array<IFilterOption>;
  state?: number;
};

export interface KeywordAIReportGridItem {
  id: number;
  organization: string;
  facility: string;
  status: string;
}

export interface IReportAIContent {
  id: number;
  organizationId: number;
  facilityId: number;
  summaryAI: string;
  keywords: string;
  pdfFileName: string;
  containerName: string;
  auditorName: string;
  auditTime: string;
  auditDate: Date;
  filteredDate: string;
  createdAt: Date;
  status: ReportAIStatusEnum;
  organization: IOption;
  facility: IOption;
  submittedDate: Date;
  state?: AIAuditStateEnum;
}

export enum AIAuditStateEnum {
  Active = 1,
  Archived = 2,
  Deleted = 3,
}
