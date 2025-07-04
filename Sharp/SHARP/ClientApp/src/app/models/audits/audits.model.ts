import {
  ICriteriaFormDetails,
  IFormField,
  IFormFieldValue,
  IHighAlert,
} from "../forms/forms.model";
import { Answer, AnswerGroup } from "./answers.model";
import { ISection, ISortModel, ITrackerQuestion } from "./questions.model";

export interface IAudit {
  id: string;
  submittedByUserFullName: string;
  facility: string;
  status: string;
}

export enum AuditStatusEnum {
  Triggered,
  InProgress,
  WaitingForApproval,
  Disapproved,
  Approved,
  Reopened,
  Submitted,
  Duplicated,
}

export enum AuditStateEnum {
  Active = 1,
  Archived = 2,
  Deleted = 3,
}

export interface IAuditStatus {
  id: number;
  label: string;
  color: string;
}

export const AuditStatuses = {
  Triggered: {
    id: AuditStatusEnum.Triggered,
    label: "Triggered",
    color: "#54667A",
  },
  InProgress: {
    id: AuditStatusEnum.InProgress,
    label: "In Progress",
    color: "#FF9500",
  },
  WaitingForApproval: {
    id: AuditStatusEnum.WaitingForApproval,
    label: "Waiting for Approval",
    color: "#BD10E0",
  },
  Disapproved: {
    id: AuditStatusEnum.Disapproved,
    label: "Disapproved",
    color: "#FF375F",
  },
  Approved: {
    id: AuditStatusEnum.Approved,
    label: "Approved",
    color: "#007AAF",
  },
  Reopened: {
    id: AuditStatusEnum.Reopened,
    label: "Reopened",
    color: "#909BA8",
  },
  Submitted: {
    id: AuditStatusEnum.Submitted,
    label: "Submitted",
    color: "#43910A",
  },
  Duplicated: {
    id: AuditStatusEnum.Duplicated,
    label: "Duplicated",
    color: "#5b3000",
  },
};

export const Actions = {
  Edit: { id: 1, label: "Edit", color: "#FF9500" },
  SendForApproval: { id: 2, label: "Send for Approval", color: "#BD10E0" },
  ReopenToInProgress: { id: 3, label: "Reopen", color: "#FF375F" },
  ReopenToReopened: { id: 4, label: "Reopen", color: "#FF375F" },
  Approve: { id: 5, label: "Approve", color: "#007AAF" },
  Disapprove: { id: 6, label: "Disapprove", color: "#909BA8" },
  Submit: { id: 7, label: "Submit", color: "#43910A" },
};


export class Audit {
  constructor(state) {
    this.state = state;
  }
  id: number;
  auditType: IOption;
  facility: IFacilityOption;
  form: IFormOption;
  incidentDateFrom: Date;
  incidentDateTo: Date;
  identifier: string;
  organization: IOption;
  reason: string;
  room: string;
  status: number;
  state: number;
  submittedByUser: IUserOption;
  submittedDate: Date;
  totalCompliance: number;
  totalNA: number;
  totalNO: number;
  totalYES: number;
  unit: string;
  resident: string;
  values: Answer[];
  subHeaderValues: IFormFieldValue[];
  fieldValues: IFormFieldValue[];
  highAlertCategory: IOption;
  highAlertDescription: string;
  highAlertNotes: string;
  isReadyForNextStatus: boolean;
  lastDeletedDate: Date;
  reportTypeId: number;
}

export interface IStatus {
  statusId: number;
  reason?: string;
}
export interface IAuditDetails {
  audit: Audit;
}

export interface IResidents {
  residentName: string;
  room: string;
}

export interface IKeywordsAuditDetails extends IAuditDetails {
  keywords?: IOption[];
  matchedKeywords: IAuditKeyword[];
}

export interface ICriteriaAuditDetails extends IAuditDetails {
  formVersion: ICriteriaFormDetails;
  answers?: Answer[];
}

export interface IOption {
  id?: number;
  name: string;
}

export interface IKeyword {
  id?: number;
  name: string;
  hidden?: number;
}

export interface IKeywordTrigger extends IKeyword {
  trigger?: boolean;
  formsTriggeredByKeyword: IOption[];
}
export interface IUserOption {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  userId: string;
}

export interface IAuditAction {
  id: number;
  label: string;
  classes: string;
}

export interface IFacilityOption extends IOption {
  timeZoneOffset?: number;
  organizationId?: number;
}

export interface IFormOption extends IOption {
  auditType: IOption;
  isActive: boolean;
  organizationId: number;
  reportRange: IOption;
  formId?: number;
  disableCompliance: number;
  allowEmptyComment: number;
  useHighAlert: boolean;
  ahTime: number ;
}

export interface IFacilityTimeZoneOption extends IOption {
  timeZoneOffset?: number;
  timeZoneShortName: string;
}

export interface IProgressNote {
  id: number;
  resident: string;
  createdDate: Date;
  effectiveDate: Date;
  progressNoteType: string;
  progressNoteText: string;
  progressNoteHtml: string;
  createdBy: string;
}

export interface IProgressNoteDetails {
  keywordsTotalCount: number;
  progressNotes: IProgressNote[];
  values: IAuditKeyword[];
  timeZoneOffset: number;
}

export interface IProgressNoteKeyword {
  keywordId: string;
  noteId: number;
  resident?: string;
  createdDate?: Date;
  effectiveDate?: Date;
  noteIndex: number;
  keywordIndex: number;
  timeZoneOffset?: number;
}

export interface IAuditKeyword {
  id?: number;
  auditId: number;
  formVersionId: number;
  keyword: IOption;
  resident?: string;
  progressNoteDate: Date;
  progressNoteTime?: string;
  progressNoteDateTime: string;
  description: string;
  highAlertAuditValue: IHighAlert;
}

export interface IAuditTrackerAnswer {
  id?: number;
  auditId: number;
  questionId: number;
  answer: string;
  formattedAnswer?: string;
  groupId: string;
  typeId?: number;
}

export interface IAuditTrackerAnswerGroup {
  groupId: string;
  answers: IAuditTrackerAnswer[];

}

export interface IHourKeyword {
  keywords?: IKeyword[];
  matchedKeywords?: IAuditKeyword[];
}

export interface ICriteria {
  answerGroups?: AnswerGroup[];
}

export interface ITracker {
  questions: ITrackerQuestion[];
  sortModel?: ISortModel;
  auditDetails: IAuditDetails;
  //answerGroups?: IAuditTrackerAnswerGroup[];
  pivotAnswerGroups?: any[];
}

export interface IMds {
  sections: ISection[];
}

export interface IUserTimeZone {
  userTimeZoneInfo: any;
  userTimeZoneDateTime: string;
}
