import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { BehaviorSubject } from "rxjs";
import { first } from "rxjs/operators";
import { Audit, AuditStatuses, IAuditKeyword, IAuditStatus, ICriteria, IHourKeyword, IMds, IOption, IProgressNoteKeyword, ITracker } from "src/app/models/audits/audits.model";
import { RolesEnum } from "src/app/models/roles.model";
import { AuditServiceApi } from "src/app/services/audit-api.service";
import { AuthService } from "src/app/services/auth.service";

@Injectable()
export class AuditService {
    private isEditableSubject = new BehaviorSubject<boolean>(true);
    private auditSubject = new BehaviorSubject<Audit>(null);
    private keywordSubject = new BehaviorSubject<IOption>(null);
    private progressNoteKeywordSubject = new BehaviorSubject<IProgressNoteKeyword>(null);
    private hourKeywordSubject = new BehaviorSubject<IHourKeyword>(null);
    private auditCriteriaSubject = new BehaviorSubject<ICriteria>(null);
    private auditTrackerSubject = new BehaviorSubject<ITracker>(null);
    private auditMdsSubject = new BehaviorSubject<IMds>(null);
    private totalCountSubject = new BehaviorSubject<number>(null);

    public isEditable$ = this.isEditableSubject.asObservable();
    public audit$ = this.auditSubject.asObservable();
    public keyword$ = this.keywordSubject.asObservable();
    public progressNoteKeyword$ = this.progressNoteKeywordSubject.asObservable();
    public hourKeyword$ = this.hourKeywordSubject.asObservable();
    public criteria$ = this.auditCriteriaSubject.asObservable();
    public tracker$ = this.auditTrackerSubject.asObservable();
    public mds$ = this.auditMdsSubject.asObservable();
    public recordsCount$ = this.totalCountSubject.asObservable();
    constructor(
        private auditServiceApi: AuditServiceApi,
        private authService: AuthService,
        private router: Router,
        ) {
    }

    public setEditable(isEditable: boolean) {
        this.isEditableSubject.next(isEditable);
    }

    public setAudit(audit: Audit) {
        this.auditSubject.next(audit);
  }

  public setTotalRecordsCount(count: number) {
    this.totalCountSubject.next(count);
  }

    public setKeyword(keyword: IOption) {
        this.keywordSubject.next(keyword);
    }

    public setProgressNoteKeyword(progressNoteKeyword: IProgressNoteKeyword) {
        this.progressNoteKeywordSubject.next(progressNoteKeyword);
    }

    public setHourKeyword(hourKeyword: IHourKeyword) {
        this.hourKeywordSubject.next(hourKeyword);
    }

    public setCriteria(criteria: ICriteria) {
        this.auditCriteriaSubject.next(criteria);
    }

    public setTracker(tracker: ITracker) {
        this.auditTrackerSubject.next(tracker);
    }

    public setMds(mds: IMds) {
        this.auditMdsSubject.next(mds);
    }

    public getStatus(statusId: number): IAuditStatus {
        const statuses = Object.values(AuditStatuses);

        return statuses.find((status) => status.id === statusId);
    }

    public getStatusByLabel(label: string): IAuditStatus {
        const statuses = Object.values(AuditStatuses);

        return statuses.find((status) => status.label === label);
    }

    public isEditable(statusId: number, auditorUserId: string, state: number): void {
      if (state != 1) {
        this.setEditable(false);
        return;
      }

      const isAuditor = this.authService.isUserInRole(RolesEnum.Auditor);
      const isReviewer = this.authService.isUserInRole(RolesEnum.Reviewer);

      const userId = this.authService.getCurrentUserId();

      switch(statusId) {
          case AuditStatuses.InProgress.id:
              this.setEditable(isAuditor && auditorUserId === userId);
              break;

          case AuditStatuses.WaitingForApproval.id:
          case AuditStatuses.Reopened.id:
              this.setEditable(isReviewer /*&& auditorUserId === userId*/);
              break;

          case AuditStatuses.Disapproved.id:
              this.setEditable(isAuditor && auditorUserId === userId);
              break;

          default:
              this.setEditable(false);
          break;
      }
    }

    public haveRightToViewAudit(auditorUserId: string): boolean {
        const isAuditor = this.authService.isUserInRole(RolesEnum.Auditor);
        const isReviewer = this.authService.isUserInRole(RolesEnum.Reviewer);
        const isAdmin = this.authService.isUserInRole(RolesEnum.Admin);
        const isFacility = this.authService.isUserInRole(RolesEnum.Facility);

        const userId = this.authService.getCurrentUserId();

        return isReviewer || (isAuditor && auditorUserId === userId) || isAdmin || isFacility;
    }

    public haveRightToViewPdf(): boolean {
        const isReviewer = this.authService.isUserInRole(RolesEnum.Reviewer);
        const isAdmin = this.authService.isUserInRole(RolesEnum.Admin);
        const isFacility = this.authService.isUserInRole(RolesEnum.Facility);

        return isReviewer || isAdmin || isFacility;
    }

    public isAuditorOwnerOfAudit(auditorUserId: string): boolean {
        const isAuditor = this.authService.isUserInRole(RolesEnum.Auditor);
        const userId = this.authService.getCurrentUserId();

        return isAuditor && auditorUserId === userId;
    }

    public isCurrentUserOnlyAuditor(): boolean {
        const isAuditor = this.authService.isUserInRole(RolesEnum.Auditor);
        const isReviewer = this.authService.isUserInRole(RolesEnum.Reviewer);
        const isAudmin = this.authService.isUserInRole(RolesEnum.Admin);

        return isAuditor && !isReviewer && !isAudmin;
    }
   public isAllowToDeleteTriggerAudit(auditId: number): any {
      return this.auditServiceApi.isAllowToDeleteTriggerAudit(auditId);
    }
    public downloadPdf(auditId: number): void {
        this.auditServiceApi.downloadAuditPdf(auditId)
        .pipe(first())
        .subscribe((data: Blob) => {
            this.downloadOpenPdf(data, `Audit_${auditId}`);
        });
    }

    public downloadOpenPdf(data: Blob, fileName: string): void {
          const blob = new Blob([data], { type: data.type });
          const url = window.URL.createObjectURL(blob);

          window.open(url);

          var a = document.createElement('a');
          a.href = url;
          a.download = `${fileName}.pdf`;
          document.body.appendChild(a);
          a.click();
          URL.revokeObjectURL(url);
    }

    public redirectToAudit(auditId: number): void {
        this.router.navigate([`audits/${auditId}`]);
      }

  public downloadExcel(auditId: number): void {
    this.auditServiceApi.downloadAuditExcel(auditId)
      .pipe(first())
      .subscribe((data: Blob) => {
        this.downloadOpenExcel(data, `Audit_${auditId}`);
      });
  }

  public downloadOpenExcel(data: Blob, fileName: string): void {
    const blob = new Blob([data], { type: data.type });
    const url = window.URL.createObjectURL(blob);

    var a = document.createElement('a');
    a.href = url;
    a.download = `${fileName}.xlsx`;
    document.body.appendChild(a);
    a.click();
    URL.revokeObjectURL(url);
  }
}
