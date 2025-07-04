import { Component, OnInit } from "@angular/core";
import { AgRendererComponent } from "ag-grid-angular";
import { ICellRendererParams, IAfterGuiAttachedParams } from "ag-grid-community";
import { AuditStatuses, IAuditStatus } from "src/app/models/audits/audits.model";
import { AuditService } from "../services/audit.service";

@Component({
  selector: "app-reason",
  templateUrl: "./reason.component.html",
  styleUrls: ["./reason.component.scss"]
})

export class ReasonComponent implements AgRendererComponent {
  public reason: string;
  public status: IAuditStatus;

  constructor(private auditService: AuditService) { }

  refresh(params: ICellRendererParams): boolean {
    return true;
  }

  agInit(params: ICellRendererParams): void {
    const data = params.data;

    if(!data){
      return;
    }

    this.status = this.auditService.getStatus(data.status);

    if(data.status === AuditStatuses.Disapproved.id) {
      this.reason = data.reason;
    } 
  }

  afterGuiAttached?(params?: IAfterGuiAttachedParams): void {}
}
