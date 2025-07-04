import { HttpErrorResponse } from "@angular/common/http";
import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";
import { NgbModal, NgbModalRef } from "@ng-bootstrap/ng-bootstrap";
import { first } from "rxjs/operators";
import { DisapproveAuditModalComponent } from "src/app/audits/disapprove-popup/DisapproveAuditModal.component";
import { AuditService } from "src/app/audits/services/audit.service";
import {
  Actions,
  AuditStatuses,
  IAuditAction,
  IStatus,
} from "src/app/models/audits/audits.model";
import { RolesEnum } from "src/app/models/roles.model";
import { AuthService } from "src/app/services/auth.service";
import { AuditServiceApi } from "../../services/audit-api.service";
import {
  TRACKER_KEYWORD,
  CRITERIA_KEYWORD
} from "src/app/common/constants/audit-constants";
import { AuditSubmitPopupComponent } from "src/app/audits/audit-submit-popup/audit-submit-popup.component";

@Component({
  selector: "app-audit-status-buttons",
  templateUrl: "./audit-status-buttons.component.html",
  styleUrls: ["./audit-status-buttons.component.scss"],
})
export class AuditStatusButtonsComponent implements OnInit {
  @Input() auditId: number;
  @Input() auditUserId: string;
  @Input() currentStatus: number = undefined;
  @Input() auditTypeName: string;
  @Input() disabled: boolean;

  @Output() onStatusChanged = new EventEmitter<IStatus>();

  public actions: Array<IAuditAction> = [];

  public isAuditor: boolean;
  public isReviewer: boolean;

  private userId: string;

  private modalRef: NgbModalRef;

  constructor(
    private authService: AuthService,
    public auditServiceApi: AuditServiceApi,
    public auditService: AuditService,
    private modalService: NgbModal
  ) {
    this.isAuditor = this.authService.isUserInRole(RolesEnum.Auditor);
    this.isReviewer = this.authService.isUserInRole(RolesEnum.Reviewer);

    this.userId = this.authService.getCurrentUserId();
  }

  ngOnInit() {
    this.setActions(this.currentStatus);
  }

  setActions(status: number | undefined) {
    if (!this.auditId || status < 0) {
      return;
    }

    const isAuditOwner =  this.userId && this.auditUserId && this.userId === this.auditUserId;

    let actions: Array<IAuditAction> = [];

    switch (status) {
      case AuditStatuses.InProgress.id:
        if (this.isAuditor && isAuditOwner) {
          actions.push({
            id: Actions.SendForApproval.id,
            label: Actions.SendForApproval.label,
            classes: "blue",
          });
        }
        break;
      case AuditStatuses.WaitingForApproval.id:
        if (this.isAuditor && isAuditOwner) {
          actions.push({
            id: Actions.ReopenToInProgress.id,
            label: Actions.ReopenToInProgress.label,
            classes: "white",
          });
        }

        if (this.isReviewer) {
          actions.push({
            id: Actions.Approve.id,
            label: Actions.Approve.label,
            classes: "green",
          });
          actions.push({
            id: Actions.Disapprove.id,
            label: Actions.Disapprove.label,
            classes: "red",
          });
        }
        break;
      case AuditStatuses.Approved.id:
        if (this.isReviewer) {
          actions.push({
            id: Actions.ReopenToReopened.id,
            label: Actions.ReopenToReopened.label,
            classes: "white",
          });
          actions.push({
            id: Actions.Submit.id,
            label: Actions.Submit.label,
            classes: "blue",
          });
        }
        break;
      case AuditStatuses.Reopened.id:
        if (this.isReviewer) {
          actions.push({
            id: Actions.Approve.id,
            label: Actions.Approve.label,
            classes: "blue",
          });
        }
        break;
      case AuditStatuses.Disapproved.id:
        if (this.isAuditor && isAuditOwner) {
          actions.push({
            id: Actions.SendForApproval.id,
            label: Actions.SendForApproval.label,
            classes: "blue",
          });
        }
        break;
      default:
        break;
    }

    this.actions = actions;
  }

  onActionClick(actionId) {
    if (!this.auditId || this.disabled) {
      return;
    }

    let statusId: number;

    switch (actionId) {
      case Actions.SendForApproval.id:
          statusId = AuditStatuses.WaitingForApproval.id;
        break;

      case Actions.ReopenToInProgress.id:
        statusId = AuditStatuses.InProgress.id;
        break;

      case Actions.ReopenToReopened.id:
        statusId = AuditStatuses.Reopened.id;
        break;

      case Actions.Approve.id:
        statusId = AuditStatuses.Approved.id;
        break;

      case Actions.Disapprove.id:
        this.openDisapproveModal();

        break;

      case Actions.Submit.id:
        statusId = AuditStatuses.Submitted.id;
        break;

      default:
        break;
    }

    if(!statusId || statusId < 1) {
      return;
    }

    if ((this.auditTypeName == TRACKER_KEYWORD || this.auditTypeName == CRITERIA_KEYWORD) && statusId == AuditStatuses.Submitted.id) {
      this.openSubmitReportTypeModal(statusId);
      return;
    }

    this.auditServiceApi
          .setAuditStatus(this.auditId, statusId)
          .pipe(first())
          .subscribe(
            (response) => {
              this.onStatusChanged.emit(response);
            },
            (error: HttpErrorResponse) => {
              console.log(error);
            }
          );

  }

  openDisapproveModal() {
    this.modalService
      .open(DisapproveAuditModalComponent)
      .result.then((reason) => {
        this.auditServiceApi
          .setAuditStatus(this.auditId, AuditStatuses.Disapproved.id, reason)
          .pipe(first())
          .subscribe(
            (response) => {
              this.onStatusChanged.emit(response/*{statusId: AuditStatuses.Disapproved.id, reason: reason }*/);
            },
            (error: HttpErrorResponse) => {
              console.log(error);
            }
          );
      })
      .catch((res) => {});
  }

  openSubmitReportTypeModal(statusId: number) {
    this.modalService
      .open(AuditSubmitPopupComponent)
      .result.then((result) => {
        this.auditServiceApi
          .setAuditStatus(this.auditId, statusId, null, result)
          .pipe(first())
          .subscribe(
            (response) => {
              this.onStatusChanged.emit(response);
            },
            (error: HttpErrorResponse) => {
              console.log(error);
            }
          );
      })
      .catch((res) => { console.log(res); });
  }
}
