import { Injectable } from "@angular/core";
import { ReportAIStatusEnum, ReportAIStatuses, IReportAIStatus } from "../../../models/reports/reports.model";

@Injectable()
export class KeywordAIReportService {

  constructor() { }

  public getStatus(statusId: number): IReportAIStatus {
    const statuses = Object.values(ReportAIStatuses);

    return statuses.find((status) => status.id === statusId);
  }


  public getStatusByLabel(label: string): IReportAIStatus {
    const statuses = Object.values(ReportAIStatuses);

    return statuses.find((status) => status.label === label);
  }

}
