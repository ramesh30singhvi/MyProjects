import { IFacilityOption, IOption } from "../audits/audits.model";
import { IReportAIStatus, ReportAIStatusEnum } from "./reports.model";

export interface ReportAIItem {
  Acceptable: number;
  Date: string;
  ID: string;
  Name:string;
  Original: string;
  PdfText: string;
  SearchWord: string;
  Summary: string;
  UserSummary: string;
}
export interface AIResult {
  error: string;
  jsonResult: string;
  date: string;
  time: string;
  user: string;
  auditDate: Date;
  organization: IOption;
  facility: IFacilityOption;
  keywords: string;
  status: ReportAIStatusEnum;
  reportFileName: string;
  containerName: string;
}
