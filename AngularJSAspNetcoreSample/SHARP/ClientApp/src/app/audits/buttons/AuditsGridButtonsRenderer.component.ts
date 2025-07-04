import { Component} from "@angular/core";
import { AgRendererComponent } from "ag-grid-angular";
import { ICellRendererParams, IAfterGuiAttachedParams, GridApi } from "ag-grid-community";
import { first } from "rxjs/operators";
import { RolesEnum } from "src/app/models/roles.model";
import { AuthService } from "src/app/services/auth.service";
import { AuditService } from "../services/audit.service";

@Component({
  templateUrl: "./AuditsGridButtonsRenderer.component.html",
  styleUrls: ["./AuditsGridButtonsRenderer.component.scss"]
})

export class AuditsGridButtonsRendererComponent implements AgRendererComponent {
  public auditId: number;
  public auditUserId: string;
  public auditType: string;
  public status: number;
  public isReadyForNextStatus: boolean;
  public haveRightToViewAudit: boolean;
  public haveRightToViewPdf: boolean;
  public reportTypeId: number = 0;

  private gridApi: GridApi;

  constructor(
    private authService: AuthService,
    private auditService: AuditService) { 
  }

  refresh(params: ICellRendererParams): boolean {
    return true;
  }

  agInit(params: ICellRendererParams): void {
    this.gridApi = params.api;
    const data = params.data;

    if(data && data.id){
      this.auditId = data.id;
      this.auditUserId = data.auditorUserId
      this.auditType = data.auditType;
      this.status = data.status;
      this.reportTypeId = data.reportTypeId ?? 2;

      this.isReadyForNextStatus = data.isReadyForNextStatus;
      this.haveRightToViewAudit = this.auditService.haveRightToViewAudit(data.auditorUserId);
      this.haveRightToViewPdf = this.auditService.haveRightToViewPdf();
      
    } else {
      return;
    }
  }

  afterGuiAttached?(params?: IAfterGuiAttachedParams): void {}

  handleSetAuditStatusSuccess(){
    this.gridApi.refreshInfiniteCache();
  }

  public onDownloadPdfClick() {
    this.auditService.downloadPdf(this.auditId);
  }

  public onDownloadExcelClick() {
    this.auditService.downloadExcel(this.auditId);
  }
}
