import { Component, OnInit } from '@angular/core';
import { AgRendererComponent } from "ag-grid-angular";
import { ICellRendererParams, IAfterGuiAttachedParams, GridApi } from "ag-grid-community";
import { IReportAIStatus, ReportAIStatuses } from "src/app/models/reports/reports.model";

@Component({
  selector: 'app-status-ai-report',
  templateUrl: './status-ai-report.component.html',
  styleUrls: ['./status-ai-report.component.scss']
})
export class StatusAiReportComponent implements AgRendererComponent {
  public status: IReportAIStatus;
  public color: string;

  private gridApi: GridApi;

  constructor() {

  }
  refresh(params: ICellRendererParams): boolean {
    return true;
  }

  agInit(params: ICellRendererParams): void {
    this.gridApi = params.api;
    const data = params.data;

    if (data) {
      this.status = Object.values(ReportAIStatuses).find((status) => status.id === data.status);
    } else {
      return;
    }
  }

  afterGuiAttached?(params?: IAfterGuiAttachedParams): void { }
}
