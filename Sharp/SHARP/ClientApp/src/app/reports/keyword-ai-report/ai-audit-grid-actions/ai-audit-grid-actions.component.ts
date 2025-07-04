import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from "@angular/common/http";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { ICellRendererAngularComp } from "ag-grid-angular";
import { ICellRendererParams, IAfterGuiAttachedParams, GridApi } from "ag-grid-community";
import { Router } from '@angular/router';
import { AuthService } from "../../../services/auth.service";
import { ReportAIService } from '../../../services/reportAI.service';
import { ConfirmationDialogComponent } from "../../../shared/confirmation-dialog/confirmation-dialog.component";
import { AIAuditStateEnum, ReportAIStatuses } from '../../../models/reports/reports.model';
import { RolesEnum } from "src/app/models/roles.model";

@Component({
  selector: 'app-ai-audit-grid-actions',
  templateUrl: './ai-audit-grid-actions.component.html',
})
export class AiAuditGridActionsComponent implements ICellRendererAngularComp {

  public reportAIContentId: number;
  public state: number;
  public isAuditor: boolean;
  public isTrigger: boolean;
  public isAdmin: boolean;
  public isReviewer: boolean;
  public editbuttonText: string = "Edit";

  public statuses = ReportAIStatuses;
  public status: number;

  private gridApi: GridApi;

  constructor(
    private router: Router,
    private authService: AuthService,
    private reportAIService: ReportAIService,
    private modalService: NgbModal,
  ) {
    this.isAdmin = this.authService.isUserInRole(RolesEnum.Admin);
    this.isReviewer = this.authService.isUserInRole(RolesEnum.Reviewer);
    this.isAuditor = this.authService.isUserInRole(RolesEnum.Auditor);
  }

  refresh(params: ICellRendererParams): boolean {
    return true;
  }

  agInit(params: any): void {
    this.gridApi = params.api;

    const data = params.data;
    if (data && data.id) {
      let id = data.id;

      this.reportAIContentId = id;
      this.state = data.state;
      this.status = data.status;

      if (data.status != ReportAIStatuses.Submitted.id) {
        this.editbuttonText = "Edit";
      } else if (data.status == ReportAIStatuses.Submitted.id) {
        this.editbuttonText = "View";
      }
    }
  }

  public dotsClick(event: MouseEvent): void {
    event.stopPropagation();
  }

  public onEditClick(reportAIContentId: number) {
    if (reportAIContentId) {
      this.router.navigate(['reports/editAIAudit/' + reportAIContentId]);
    }
  }

  public onDeleteClick(reportAIContentId: number) {
    if (!reportAIContentId) {
      return;
    }

    const modalRef = this.modalService.open(ConfirmationDialogComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.confirmationBoxTitle = 'Confirmation?';
    modalRef.componentInstance.confirmationMessage = `Are you sure you want to delete this AI audit?`;

    modalRef.result.then((userResponse) => {
      if (userResponse) {
        this.reportAIService
          .updateAIAudit(reportAIContentId, AIAuditStateEnum.Deleted)
          .subscribe({
            next: (response: boolean) => {
              this.gridApi?.onFilterChanged();
            },
            error: (response: HttpErrorResponse) => {
              this.handleResponseError(response);
            },
          });
      }
    }, () => { /*user closed the message box*/ });
  }

  public onUndeleteClick(reportAIContentId: number) {
    if (!reportAIContentId) {
      return;
    }
    this.reportAIService
      .updateAIAudit(reportAIContentId, AIAuditStateEnum.Active)
      .subscribe({
        next: (response: boolean) => {
          this.gridApi?.onFilterChanged();
        },
        error: (response: HttpErrorResponse) => {
          this.handleResponseError(response);
        },
      });
  }

  private handleResponseError(response: HttpErrorResponse): void {
    console.error(response);
  }

}
