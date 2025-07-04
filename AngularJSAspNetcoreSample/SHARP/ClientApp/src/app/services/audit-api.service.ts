import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "src/environments/environment";
import { catchError, finalize, map } from "rxjs/operators";
import {
  AuditFiltersModel,
  IFilterOption,
  IPdfFilterModel,
} from "../models/audits/audit.filters.model";
import {
  Audit,
  AuditStateEnum,
  AuditStatusEnum,
  IAuditDetails,
  IAuditKeyword,
  IAuditTrackerAnswer,
  IAuditTrackerAnswerGroup,
  IOption,
  IProgressNoteDetails,
  IResidents,
  ITracker,
} from "../models/audits/audits.model";
import { transformDate, transformNumber } from "../common/helpers/dates-helper";
import * as moment from "moment";
import { YYYY_MM_DD_DASH } from "../common/constants/date-constants";
import {
  ISortModel,
  ITrackerQuestion,
  QuestionGroup,
} from "../models/audits/questions.model";
import { AnswerGroup } from "../models/audits/answers.model";
import { NgxSpinnerService } from "ngx-spinner";
import { AUDIT_GRID_COLUMNS } from "../common/constants/audit-constants";
import { UserService } from "./user.service";
import { ActionTypeEnum } from "../models/users/users.model";
import { IHighAlert } from "../models/forms/forms.model";

@Injectable()
export class AuditServiceApi {
  auditsUrl = environment.apiUrl + "audits";
  getAuditsFiltersUrl = environment.apiUrl + "audits/filters";
  getAuditUrl = environment.apiUrl + "audits/{id}";
  questionUrl = environment.apiUrl + "audits/form/{formVersionId}/questions";
  answerUrl = environment.apiUrl + "audits/{id}/answers";
  deleteAuditsUrl = environment.apiUrl + "audits/deleteAudits";
  archiveAuditsUrl = environment.apiUrl + "audits/archiveAudits";
  highAlertCategoriessUrl = environment.apiUrl + "audits/getHighAlertCategories";
  constructor(
    private httpClient: HttpClient,
    private userServiceApi: UserService,
    private spinner: NgxSpinnerService
  ) {}


  getAudits(
    startRow,
    endRow,
    sortModel,
    filterModel,
    auditFilterValues,
    state
  ): Observable<{ totalCount: number; items: Audit[] }> {
    this.spinner.show();

    var searchParams = {
      skipCount: startRow,
      takeCount: endRow - startRow,
      orderBy: "",
      sortOrder: "",
    };
    var colId = "";
    var sortOrder = "";
    if (sortModel && sortModel.length > 0) {
      colId = sortModel[0].colId;
      sortOrder = sortModel[0].sort;
    }
    if (colId && sortOrder) {
      searchParams.orderBy = colId;
      searchParams.sortOrder = sortOrder;
    }

    const filters = this.getFilterModelParams(filterModel, auditFilterValues);
    filters["state"] = state;
    return this.httpClient.post<{ totalCount: number; items: Audit[] }>(
      `${this.auditsUrl}/get`,
      {
        ...searchParams,
        ...filters,
      }
    );
  }

  getAuditsFilters(
    field: string,
    filterModel: any,
    auditFilterValues: AuditFiltersModel,
    state
  ): Observable<IFilterOption[]> {
    const filters = this.getFilterModelParams(filterModel, auditFilterValues);
    filters["state"] = state;
    return this.httpClient.post<IFilterOption[]>(this.getAuditsFiltersUrl, {
      column: field,
      auditFilter: filters,
    });
  }

  protected cachedFilter: AuditFiltersModel;
  clearCachedFilters() {
    this.cachedFilter = null;
  }
  public getHighAlertCategories(): Observable<IOption[]> {
    return this.httpClient.get<IOption[]>(this.highAlertCategoriessUrl);
  }
  setAuditStatus(
    auditId: number,
    statusId: number,
    comment: string = null,
    reportType: number = null
  ): Observable<any> {
    this.spinner.show();

    let actionType = ActionTypeEnum.SaveAudit;

    switch (statusId) {
      case AuditStatusEnum.Submitted:
        actionType = ActionTypeEnum.SubmitAudit;
        break;
      case AuditStatusEnum.Reopened:
        actionType = ActionTypeEnum.ReopenAudit;
        break;
      case AuditStatusEnum.Approved:
        actionType = ActionTypeEnum.ApproveAudit;
        break;
      case AuditStatusEnum.Disapproved:
        actionType = ActionTypeEnum.DissapproveAudit;
        break;
      case AuditStatusEnum.WaitingForApproval:
        actionType = ActionTypeEnum.SendForApprovalAudit;
        break;
    }

    this.userServiceApi.addUserActivity(actionType, auditId).subscribe();

    return this.httpClient.put(`${this.auditsUrl}/${auditId}`, {
      status: statusId,
      comment: comment,
      reportType: reportType,
    });
  }
  isAllowToDeleteTriggerAudit(auditId: number): any {
    return this.httpClient
      .get<boolean>(`${this.auditsUrl}/${auditId}/isAuditTriggedByKeyword`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }
  getAuditDetails(auditId: number): Observable<IAuditDetails> {
    this.spinner.show();

    return this.httpClient
      .get<IAuditDetails>(`${this.auditsUrl}/${auditId}`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  getResidents(auditId: number): Observable<IResidents[]> {

    return this.httpClient
      .get<IResidents[]>(`${this.auditsUrl}/${auditId}/residents`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  downloadAuditPdf(auditId: number): Observable<Blob> {
    this.spinner.show();

    this.userServiceApi
      .addUserActivity(ActionTypeEnum.DownloadPDFAudit, auditId)
      .subscribe();

    return this.httpClient
      .get(`${this.auditsUrl}/${auditId}/pdf/get`, {
        headers: { Accept: "application/pdf" },
        responseType: "blob",
      })
      .pipe(
        map((data: Blob) => {
          return data;
        })
      );
  }

  getKeywords(formVersionId: number): Observable<IOption[]> {
    return this.httpClient
      .get<IOption[]>(`${this.auditsUrl}/form/${formVersionId}/keywords`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  getProgressNotes(
    auditId: number,
    facilityId: number,
    keyword: IOption,
    dateFrom: Date,
    dateTo: Date,
    timeZoneOffset: number
  ): Observable<IProgressNoteDetails> {
    this.spinner.show();

    return this.httpClient
      .get<IOption[]>(
        `${this.auditsUrl}/${auditId}/facility/${facilityId}/progressNotes`,
        {
          params: {
            keywordId: keyword.id,
            keywordName: keyword.name,
            dateFrom: dateFrom ? moment(dateFrom).format(YYYY_MM_DD_DASH) : "",
            dateTo: dateTo ? moment(dateTo).format(YYYY_MM_DD_DASH) : "",
            timeZoneOffset,
          },
        }
      )
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  getOrganizationOptions(): Observable<IOption[]> {
    return this.httpClient
      .get<IOption[]>(`${this.auditsUrl}/organizations`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  addAudit(audit: Audit): Observable<Audit> {
    this.spinner.show();

    return this.httpClient
      .post<Audit>(this.auditsUrl, {
        facilityId: audit.facility?.id,
        formVersionId: audit.form?.id,
        incidentDateFrom: audit.incidentDateFrom
          ? moment(audit.incidentDateFrom).format(YYYY_MM_DD_DASH)
          : null,
        incidentDateTo: audit.incidentDateTo
          ? moment(audit.incidentDateTo).format(YYYY_MM_DD_DASH)
          : null,
        room: audit.room,
        resident: audit.resident,
        values: audit.values,
        subHeaderValues: audit.subHeaderValues,
        totalYes: audit.totalYES,
        totalNo: audit.totalNO,
        totalNa: audit.totalNA,
        totalCompliance: audit.totalCompliance
      })
      .pipe(
        map((data: any) => {
          if (data?.audit) {
            this.userServiceApi
              .addUserActivity(ActionTypeEnum.CreateAudit, data?.audit?.id)
              .subscribe();
          }
          return data;
        })
      );
  }

  editAudit(audit: Audit): Observable<Audit> {
    this.spinner.show();

    return this.httpClient
      .put<Audit>(this.auditsUrl, {
        id: audit.id,
        facilityId: audit.facility?.id,
        formVersionId: audit.form?.id,
        incidentDateFrom: audit.incidentDateFrom
          ? moment(audit.incidentDateFrom).format(YYYY_MM_DD_DASH)
          : null,
        incidentDateTo: audit.incidentDateTo
          ? moment(audit.incidentDateTo).format(YYYY_MM_DD_DASH)
          : null,
        room: audit.room,
        resident: audit.resident,
        values: audit.values,
        subHeaderValues: audit.subHeaderValues,
        totalYes: audit.totalYES,
        totalNo: audit.totalNO,
        totalNa: audit.totalNA,
        totalCompliance: audit.totalCompliance,
        highAlertCategory: audit.highAlertCategory,
        highAlertDescription: audit.highAlertDescription,
        highAlertNotes: audit.highAlertNotes
      })
      .pipe(
        map((data: any) => {
          this.userServiceApi
            .addUserActivity(ActionTypeEnum.SaveAudit, audit.id)
            .subscribe();

          return data;
        })
      );
  }

  addAuditKeyword(auditKeyword: IAuditKeyword): Observable<IAuditKeyword> {
    this.spinner.show();

    return this.httpClient
      .post<IAuditKeyword>(`${this.auditsUrl}/keyword`, auditKeyword)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  editAuditKeyword(auditKeyword: IAuditKeyword): Observable<IAuditKeyword> {
    this.spinner.show();

    return this.httpClient
      .put<IAuditKeyword>(`${this.auditsUrl}/keyword`, auditKeyword)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  deleteKeyword(keywordValueId: number): Observable<boolean> {
    return this.httpClient
      .delete<boolean>(`${this.auditsUrl}/keyword/${keywordValueId}`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  addAuditTrackerAnswer(
    auditId: number,
    auditAnswers: IAuditTrackerAnswer[], highAlert: IHighAlert
  ): Observable<ITracker> {
    this.spinner.show();

    return this.httpClient
      .post<ITracker>(`${this.auditsUrl}/${auditId}/tracker`, {
        answers: auditAnswers,
        highAlertCategory: highAlert?.highAlertCategory,
        highAlertDescription: highAlert?.highAlertDescription,
        highAlertNotes: highAlert?.highAlertNotes
      })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  editAuditTrackerAnswer(
    auditId: number,
    groupId: string,
    auditAnswers: IAuditTrackerAnswer[], highAlert: IHighAlert
  ): Observable<ITracker> {
    this.spinner.show();

    return this.httpClient
      .put<ITracker>(`${this.auditsUrl}/${auditId}/tracker/${groupId}`, {
        answers: auditAnswers,
        highAlertCategory: highAlert?.highAlertCategory,
        highAlertDescription: highAlert?.highAlertDescription,
        highAlertNotes: highAlert?.highAlertNotes
      })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  deleteAuditTrackerAnswers(
    auditId: number,
    answersGroupId: string
  ): Observable<boolean> {
    return this.httpClient
      .delete<boolean>(`${this.auditsUrl}/${auditId}/tracker/${answersGroupId}`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public saveAuditSortModel(
    auditId: number,
    sortModel: ISortModel
  ): Observable<ITracker> {
    this.spinner.show();

    return this.httpClient
      .post<ITracker>(`${this.auditsUrl}/${auditId}/tracker/sort`, {
        ...sortModel,
      })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  getFilterModelParams(filterModel: any, auditFilterValues: AuditFiltersModel) {
    const filterParams: any = {};

    if (!filterModel) return "";

    Object.keys(filterModel).forEach((key) => {
      const filter = filterModel[key];
      switch (filter.filterType) {
        case "date":
          filterParams[key] = JSON.stringify({
            firstCondition: transformDate(filter.condition1 || filter),
            secondCondition: transformDate(filter.condition2),
            operator: filter.operator,
          });
          break;
        case "number":
          filterParams[key] = JSON.stringify({
            firstCondition: transformNumber(filter.condition1 || filter),
            secondCondition: transformNumber(filter.condition2),
            operator: filter.operator,
          });
          break;
        default:
          if (filter.values && filter.values.length > 0) {
            //const values: string[] = filter.values.map((value) => value);

            const filterValues: IFilterOption[] = auditFilterValues?.[key];

            filterParams[key] = filterValues?.filter((fv: IFilterOption) =>
              filter.values.includes(fv.value)
            );
          }
      }
    });

    return filterParams;
  }

  getQuestions(formVersionId: number): Observable<QuestionGroup[]> {
    const url = this.questionUrl.replace(
      "{formVersionId}",
      formVersionId.toString()
    );
    return this.httpClient.get<QuestionGroup[]>(url);
  }

  getAnswers(id: number): Observable<AnswerGroup[]> {
    const url = this.answerUrl.replace("{id}", id.toString());
    return this.httpClient.get<AnswerGroup[]>(url);
  }

  public duplicateAudit(auditId: number): Observable<number> {
    this.spinner.show();

    this.userServiceApi
      .addUserActivity(ActionTypeEnum.DuplicateAudit, auditId)
      .subscribe();

    return this.httpClient
      .post<number>(`${this.auditsUrl}/${auditId}/duplicate`, {
        currentClientDate: moment()
          .clone()
          .startOf("day")
          .format(YYYY_MM_DD_DASH),
      })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }
  public archiveAudits(ids: any): Observable<any> {
    this.spinner.show();

    return this.httpClient
      .post(this.archiveAuditsUrl, { AuditIds: ids })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }
  public deleteAudits(ids:any): Observable<any> {
    this.spinner.show();

    return this.httpClient
      .post(this.deleteAuditsUrl, { AuditIds : ids})
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public archiveAudit(auditId: number): Observable<number> {
    this.spinner.show();

    this.userServiceApi
      .addUserActivity(ActionTypeEnum.ArchiveAudit, auditId)
      .subscribe();

    return this.httpClient
      .post<number>(`${this.auditsUrl}/${auditId}/archive`, {})
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public unArchiveAudit(auditId: number): Observable<number> {
    this.spinner.show();

    this.userServiceApi
      .addUserActivity(ActionTypeEnum.UnarchiveAudit, auditId)
      .subscribe();

    return this.httpClient
      .post<number>(`${this.auditsUrl}/${auditId}/unarchive`, {})
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public deleteAudit(auditId: number): Observable<number> {
    this.spinner.show();

    this.userServiceApi
      .addUserActivity(ActionTypeEnum.DeleteAudit, auditId)
      .subscribe();

    return this.httpClient
      .post<number>(`${this.auditsUrl}/${auditId}/delete`, {})
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public unDeleteAudit(auditId: number): Observable<number> {
    this.spinner.show();

    this.userServiceApi
      .addUserActivity(ActionTypeEnum.UndeleteAudit, auditId)
      .subscribe();

    return this.httpClient
      .post<number>(`${this.auditsUrl}/${auditId}/undelete`, {})
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public updateDuplicatedAudit(audit: Audit): Observable<Audit> {
    this.spinner.show();

    this.userServiceApi
      .addUserActivity(ActionTypeEnum.SaveAudit, audit.id)
      .subscribe();

    return this.httpClient
      .put<Audit>(`${this.auditsUrl}/${audit.id}/duplicate`, {
        id: audit.id,
        facilityId: audit.facility?.id,
        formVersionId: audit.form?.id,
        incidentDateFrom: audit.incidentDateFrom
          ? moment(audit.incidentDateFrom).format(YYYY_MM_DD_DASH)
          : null,
        incidentDateTo: audit.incidentDateTo
          ? moment(audit.incidentDateTo).format(YYYY_MM_DD_DASH)
          : null,
        room: audit.room,
        resident: audit.resident,
        values: audit.values,
        subHeaderValues: audit.subHeaderValues,
        totalYes: audit.totalYES,
        totalNo: audit.totalNO,
        totalNa: audit.totalNA,
        totalCompliance: audit.totalCompliance,
      })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }


  downloadAuditExcel(auditId: number): Observable<Blob> {
    this.spinner.show();

    this.userServiceApi
      .addUserActivity(ActionTypeEnum.DownloadPDFAudit, auditId)
      .subscribe();

    return this.httpClient
      .get(`${this.auditsUrl}/${auditId}/excel/get`, {
        headers: { Accept: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        responseType: "blob",
      })
      .pipe(
        map((data: Blob) => {
          return data;
        })
      );
  }
}
