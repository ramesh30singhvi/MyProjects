import { HttpErrorResponse } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { AgRendererComponent } from "ag-grid-angular";
import { ICellRendererParams, IAfterGuiAttachedParams, GridApi } from "ag-grid-community";
import {
  NgbModal
} from "@ng-bootstrap/ng-bootstrap";
import { first } from "rxjs";
import { RolesEnum } from "src/app/models/roles.model";
import { AuditServiceApi } from "src/app/services/audit-api.service";
import { AuthService } from "src/app/services/auth.service";
import { AuditService } from "../services/audit.service";
import { ConfirmationDialogComponent } from "src/app/shared/confirmation-dialog/confirmation-dialog.component";

@Component({
  selector: "app-AuditOptionsRenderer",
  templateUrl: "./AuditOptionsRenderer.component.html",
  styleUrls: ["./AuditOptionsRenderer.component.scss"]
})

export class AuditOptionsRendererComponent  implements AgRendererComponent {
  public auditId: number;
  public isAuditor: boolean;
  public isTrigger: boolean;
  public isAdmin: boolean;
  public isReviewer: boolean;
  public state: number;
  private gridApi: GridApi;

  constructor(
    private auditServiceApi: AuditServiceApi,
    private auditService: AuditService,
    private authService: AuthService,
    private modalService: NgbModal,
    ) { 
    this.isAdmin = this.authService.isUserInRole(RolesEnum.Admin);
    this.isReviewer = this.authService.isUserInRole(RolesEnum.Reviewer);

  }

  refresh(params: ICellRendererParams): boolean {
    return true;
  }

  agInit(params: ICellRendererParams): void {
    this.gridApi = params.api;
    const data = params.data;
    if(data && data.id){
      this.auditId = data.id;
      this.isAuditor = this.authService.isUserInRole(RolesEnum.Auditor);
      this.isTrigger = false;
      this.state = data.state;
      this.auditService.isAllowToDeleteTriggerAudit(this.auditId)
        .subscribe((response) => {
          this.isTrigger = response;
         },
         (error) => {
                this.handleResponseError(error);
         });

    } else {
      return;
    }
  }

  afterGuiAttached?(params?: IAfterGuiAttachedParams): void { }

  public onDuplicateAuditClick(auditId: number) {
    if(!auditId) {
      return;
    }

    this.auditServiceApi
    .duplicateAudit(auditId)
    .pipe(first())
    .subscribe({
      next: (newAuditId: number) => {
        this.auditService.redirectToAudit(newAuditId);
      },
      error: (response: HttpErrorResponse) => {
        this.handleResponseError(response);
      },
    });
  }
  public onArchiveAuditClick(auditId: number) {
    if (!auditId) {
      return;
    }

    this.auditServiceApi
      .archiveAudit(auditId)
      .subscribe((response) => {
        this.gridApi?.onFilterChanged();
      },
        (error) => {
          this.handleResponseError(error);
      });
  }
  public onUnarchiveAuditClick(auditId: number) {
    if (!auditId) {
      return;
    }

    this.auditServiceApi
      .unArchiveAudit(auditId)
      .subscribe((response) => {
        this.gridApi?.onFilterChanged();
      },
      (error) => {
        this.handleResponseError(error);
      });
  }

  public onDeleteAuditClick(auditId: number) {
    if (!auditId) {
      return;
    }

    const modalRef = this.modalService.open(ConfirmationDialogComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.confirmationBoxTitle = 'Confirmation?';
    modalRef.componentInstance.confirmationMessage = `Are you sure you want to delete this audit?`;

    modalRef.result.then((userResponse) => {
      if (userResponse) {
        this.auditServiceApi
          .deleteAudit(auditId)
          .subscribe((response) => {
            this.gridApi?.onFilterChanged();
          },
            (error) => {
              this.handleResponseError(error);
            });
      }
    },
      () => { /*user closed the message box*/ });
  }
  public onUndeleteAuditClick(auditId: number) {
    if (!auditId) {
      return;
    }

    this.auditServiceApi
      .unDeleteAudit(auditId)
      .subscribe((response) => {
        this.gridApi?.onFilterChanged();
      },
        (error) => {
          this.handleResponseError(error);
        });
  }

  private handleResponseError(response: HttpErrorResponse): void {
    console.error(response);
  }
}
