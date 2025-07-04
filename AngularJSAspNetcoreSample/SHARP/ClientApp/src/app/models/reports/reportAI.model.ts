import { IOption } from "../audits/audits.model";
import { AIAuditStateEnum, ReportAIStatusEnum } from "./reports.model";

export interface PCCNotes {

  patientId: string;
  patientName: string;
  patientNotes: string;
  date: string;
  time: string;
  reportId: number;
  facilityId: number;
  facilityName: string;
  dateTimeNotes: string;

}
export interface AIAudit {
  id: number;
  organizationName: string;
  facilityName: string;
  auditorName: string;
  auditTime: string;
  auditDate: string;
  filteredDate: string;
  createdAt: Date;
  status: ReportAIStatusEnum;
  organization: IOption;
  facility: IOption;
  submittedDate: Date;
  sentForApprovalDate: Date;
  state?: AIAuditStateEnum;
  values: AIProgressNotes[];
}
export interface AIProgressNotes {
  patientId: string;
  patientName: string;
  dateTime: string;
  reportId: number;
  date: string;
  time: string;
  facilityId: number;
  facilityName: string;
  id: number;

  summaries: AIKeywordSummary[];
  
}
export interface AIKeywordSummary {
  summary: string;
  keyword: string;
  auditAIPatientPdfNotesID: number;
  accept: boolean;
  id: number;
  
}
export interface AIServiceRespond {

  items: AIKeywordSummary[];
  error: string;
}
