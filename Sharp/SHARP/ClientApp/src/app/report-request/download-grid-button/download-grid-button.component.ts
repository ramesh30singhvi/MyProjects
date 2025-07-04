import { Component } from "@angular/core";
import { AgRendererComponent } from "ag-grid-angular";
import { ICellRendererParams, IAfterGuiAttachedParams, GridApi } from "ag-grid-community";
import { ReportRequestServiceApi } from "src/app/services/report-request-api.service";

@Component({
  selector: "app-download-grid-button",
  templateUrl: "./download-grid-button.component.html",
  styleUrls: ["./download-grid-button.component.scss"]
})

export class DownloadGridButtonComponent implements AgRendererComponent {
  public report: string;
  
  private gridApi: GridApi;
  
  constructor(
    private reportRequestServiceApi: ReportRequestServiceApi
  ) { 

  }

  refresh(params: ICellRendererParams): boolean {
    return true;
  }

  agInit(params: ICellRendererParams): void {
    this.gridApi = params.api;
    const data = params.data;

    if(data){
      this.report = data.report;
    } else {
      return;
    }
  }

  afterGuiAttached?(params?: IAfterGuiAttachedParams): void { }

  public onDownloadClick() {
    this.reportRequestServiceApi.downloadPdf(this.report);
  }
}
