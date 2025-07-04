import { Component, OnInit } from "@angular/core";
import { AgRendererComponent } from "ag-grid-angular";
import { ICellRendererParams, IAfterGuiAttachedParams, GridApi } from "ag-grid-community";
import { IReportRequestStatus, ReportRequestStatuses } from "src/app/models/report-requests/report-requests.model";

@Component({
  selector: "app-status-request",
  templateUrl: "./status-request-component.html",
  styleUrls: ["./status-request-component.scss"]
})

export class StatusRequestComponent implements AgRendererComponent {
  public status: IReportRequestStatus;
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

    if(data){
      this.status = Object.values(ReportRequestStatuses).find((status) => status.id === data.status);
    } else {
      return;
    }
  }

  afterGuiAttached?(params?: IAfterGuiAttachedParams): void { }
}
