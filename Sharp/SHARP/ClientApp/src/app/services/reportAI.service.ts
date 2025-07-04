import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import * as moment from "moment";
import { NgxSpinnerService } from "ngx-spinner";
import { first, Observable, timeout } from "rxjs";
import { environment } from "../../environments/environment";
import { AIResult } from "../models/reports/file-upload.model";
import { AIAuditStateEnum, IReportAIContent, KeywordAIReportFiltersModel, KeywordAIReportGridItem } from '../models/reports/reports.model';
import { IFilterOption } from "../models/audits/audit.filters.model";
import { transformDate } from "../common/helpers/dates-helper";
import { map } from "rxjs/operators";
import { AIAudit, AIKeywordSummary, AIProgressNotes, AIServiceRespond } from "../models/reports/reportAI.model";



@Injectable()
export class ReportAIService {

  private reportsUploadReportsForAnalysisUrl = `${environment.apiUrl}reportAI/uploadForAnalysis`;
  private reportsUploadReportsForAnalysisV2Url = `${environment.apiUrl}reportAI/uploadForAnalysisV2`;
  private reportAIURL = `${environment.apiUrl}reportAI`;
  private reportAIV2List = `${environment.apiUrl}reportAI/getReports`;


  private reportsProgresNotesAIUrl = `${environment.apiUrl}reportAI/progressNotesFromAI`;
  private reportsUploadReportsUrl = `${environment.apiUrl}reportAI/uploadForProcess`;
  private reportsDownloadReportbyAI = `${environment.apiUrl}reportAI/downloadReportbyAI`;

  private reportAIContentList = `${environment.apiUrl}reportAIContent/getReport`;
 
  private reportAIContentFilters = `${environment.apiUrl}reportAIContent/filters`;
  private reportDownloadUpdatedURL = `${environment.apiUrl}reportAIContent/downloadUpdatedReport`;
  private reportAIContentURL = `${environment.apiUrl}reportAIContent`;
  private reportUpdateAIAuditURL = `${environment.apiUrl}reportAIContent/updateAIAuditReport`;
  private reportSetAIAuditStatusURL = `${environment.apiUrl}reportAIContent/setAIAuditStatus`;
  private reportsSaveReportAIData = `${environment.apiUrl}reportAIContent/saveReportAIData`;

  private getAppSettingsValueURL = `${environment.apiUrl}reportAIContent/getAppSettingsValue`;
  private checkOnlineStatusURL = `${environment.apiUrl}reportAIContent/checkOnlineStatus`;

  constructor(
    private httpClient: HttpClient,
    private spinner: NgxSpinnerService
  ) { }


  public downloadManualReport(report: AIResult) {
    console.log(report);
    this.spinner?.show();
    this.httpClient
      .post(
        this.reportsDownloadReportbyAI,
        {
          "error": report.error,
          "jsonResult": report.jsonResult,
          "organization": report.organization,
          "facility": report.facility == undefined ? null : report.facility,
          "date": report.date,
          "time": report.time,
          "user": report.user,
          "keywords": report.keywords,
          "containerName": report.containerName,
          "reportFileName": report.reportFileName
        },
        {
          headers: { Accept: "application/pdf" },
          responseType: "blob",
        }
      )
      .pipe(first())
      .subscribe((data: Blob) => {
        let name = report.facility ? report.facility.name : report.organization.name;
        name = name + " 24 Hour Keyword ";
        let prefDate = moment(report.date, "MMM DD, YYYY").format("MM.DD.YYYY");
        this.downloadOpenPdf(data, name + prefDate);
      });
  }
  public uploadReportForAnalyse(pdfFile: FormData): Observable<AIResult> {
    this.spinner?.show();
    var options = {
      headers: {
        ContexType: "mulipart/form-data"
      },
      reportProgress: true, observe: 'events'
    };
    return this.httpClient.post<AIResult>(this.reportsUploadReportsForAnalysisUrl, pdfFile).pipe(timeout(1200000));
  }
  public uploadReportForAnalyseV2(pdfFile: FormData): Observable<any> {
    this.spinner?.show();
    return this.httpClient.post<any>(this.reportsUploadReportsForAnalysisV2Url, pdfFile).pipe();
  }

  getProgressNotes(patientDataId: number): Observable<AIServiceRespond> {
   // if (showSpinned)
      this.spinner.show();
    console.log("ask for " + patientDataId);
 

    return this.httpClient.post<AIServiceRespond>(this.reportsProgresNotesAIUrl, { patientNotesId: patientDataId });
  }
  public uploadReportForProcess(pdfFile: FormData): Observable<AIResult> {
    this.spinner.show();
    return this.httpClient.post<AIResult>(this.reportsUploadReportsUrl, pdfFile);
  }

  public getAIAuditV2(id: number) {
    this.spinner?.show();


    return this.httpClient.get<any>(`${this.reportAIURL}/${id}/getById`).pipe(
      map((data: any) => {
        return data;
      })
    );
  }
  public getAIReportV2(
   filterModel: any,
   sortModel: any[],
   startRow: number,
   endRow: number,
   keywordAIReportFilterValues: KeywordAIReportFiltersModel,
   state: number

 ): Observable<KeywordAIReportGridItem[]> {
    this.spinner.show();
    const filters = this.getFilterModelParams(
      filterModel,
      keywordAIReportFilterValues
    );
    filters["state"] = state;

    var searchParams = {
      skipCount: startRow,
      takeCount: endRow - startRow,
      orderBy: "",
      sortOrder: "",
    };

    if (sortModel?.length) {
      const { colId, sort } = sortModel[0];
      searchParams.orderBy = colId;
      searchParams.sortOrder = sort;
    }

    return this.httpClient.post<KeywordAIReportGridItem[]>(
      this.reportAIV2List,
      {
        ...searchParams,
        ...filters,
      }
    );
  }


  public downloadOpenPdf(data: Blob, fileName: string): void {
    const blob = new Blob([data], { type: data.type });
    const url = window.URL.createObjectURL(blob);
    var a = document.createElement("a");
    a.href = url;
    a.download = `${fileName}.pdf`;
    document.body.appendChild(a);
    a.click();
    URL.revokeObjectURL(url);
  }

  public getKeywordAIReportFilters(
    column: string,
    filterModel: any,
    keywordAIReportFilterValues: KeywordAIReportFiltersModel,
    state: number
  ): Observable<IFilterOption[]> {
    const filters = this.getFilterModelParams(
      filterModel,
      keywordAIReportFilterValues
    );
    filters["state"] = state;
    return this.httpClient.post<IFilterOption[]>(
      this.reportAIContentFilters,
      {
        column,
        reportAIContentFilter: filters,
      }
    );
  }

  getFilterModelParams(
    filterModel: any,
    keywordAIReportFilterValues: KeywordAIReportFiltersModel
  ) {
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
        default:
          if (filter.values && filter.values.length > 0) {
            const filterValues: IFilterOption[] =
              keywordAIReportFilterValues?.[key];

            filterParams[key] = filterValues?.filter((fv: IFilterOption) =>
              filter.values.includes(fv.value)
            );
          }
      }
    });

    return filterParams;
  }

  public getKeywordAIReport(
    filterModel: any,
    sortModel: any[],
    startRow: number,
    endRow: number,
    keywordAIReportFilterValues: KeywordAIReportFiltersModel,
    state: number

  ): Observable<KeywordAIReportGridItem[]> {
    this.spinner.show();
    const filters = this.getFilterModelParams(
      filterModel,
      keywordAIReportFilterValues
    );
    filters["state"] = state;

    var searchParams = {
      skipCount: startRow,
      takeCount: endRow - startRow,
      orderBy: "",
      sortOrder: "",
    };

    if (sortModel?.length) {
      const { colId, sort } = sortModel[0];
      searchParams.orderBy = colId;
      searchParams.sortOrder = sort;
    }

    return this.httpClient.post<KeywordAIReportGridItem[]>(
      this.reportAIContentList,
      {
        ...searchParams,
        ...filters,
      }
    );
  }

  public getAIAudit(paramId: number, showSpinner: boolean = true): Observable<any> {
    
    this.spinner?.show();
    

    return this.httpClient.get<any>(`${this.reportAIContentURL}/${paramId}/getById`).pipe(
      map((data: any) => {
        return data;
      })
    );
  }

  public downloadUpdatedManualReport(result: AIResult,id:number) {

    this.spinner?.show();
    console.log(result.organization);
    console.log(result.facility);
    console.log(result.date);
    this.httpClient
      .post(
        this.reportDownloadUpdatedURL,
        {
          "ReportAIContentId": id 
        },
        {
          headers: { Accept: "application/pdf" },
          responseType: "blob",
        }
      )
      .pipe(first())
      .subscribe((data: Blob) => {
        let name = result.facility != null ? result.facility?.name : result.organization?.name;
        name = name + " 24 Hour Keyword ";
       
        let prefDate =  moment(result.auditDate, "MMM DD, YYYY").format("MM.DD.YYYY");
        this.downloadOpenPdf(data, name + result.date);
      });
  }

  public updateAIAuditReport(report: AIResult, Id: number, showSpinner: boolean): Observable<IReportAIContent> {
    if (showSpinner == true) {
      this.spinner?.show();
    }

    console.log(report.organization);
    console.log(report.facility);

    return this.httpClient
      .post<IReportAIContent>(
        this.reportUpdateAIAuditURL,
        {
          "error": report.error,
          "jsonResult": report.jsonResult,
          "organization": report.organization,
          "facility": report.facility == undefined ? null : report.facility,
          "date": report.date,
          "time": report.time,
          "user": report.user,
          "status": report.status,
          "keywords": report.keywords,
          "containerName": report.containerName,
          "reportFileName": report.reportFileName,
          "reportAIContentId": Id
        }
      )
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public setAIAuditStatus(reportAIContentId: number, statusId: number): Observable<any> {
    this.spinner.show();

    return this.httpClient.put<any>(this.reportSetAIAuditStatusURL, {
      "reportAIContentId": reportAIContentId,
      "status": statusId,
    }).pipe(
      map((data: any) => {
        return data;
      })
    );
  }

  public setAIAuditV2Status(auditId: number, statusId: number): Observable<any> {
    this.spinner.show();

    return this.httpClient.put<any>(`${this.reportAIURL}/updateAIAuditV2Status`, {
      "reportAIContentId": auditId,
      "status": statusId,
    }).pipe(
      map((data: any) => {
        return data;
      })
    );
  }
  public saveReportAIData(report: AIResult, showSpinner: boolean = true): Observable<IReportAIContent> {
    if (showSpinner == true) {
      this.spinner?.show();
    }
    return this.httpClient.post<IReportAIContent>(this.reportsSaveReportAIData, {
      "error": report.error,
      "jsonResult": report.jsonResult,
      "organization": report.organization,
      "facility": report.facility == undefined ? null : report.facility,
      "date": report.date,
      "time": report.time,
      "user": report.user,
      "keywords": report.keywords,
      "containerName": report.containerName,
      "reportFileName": report.reportFileName
    }).pipe(
      map((data: any) => {
        return data;
      })
    );
  }

  public getAppSettingsValue(keyName: string): Observable<string> {
    return this.httpClient.get<any>(`${this.getAppSettingsValueURL}/${keyName}`).pipe(
      map((data: any) => {
        return data;
      })
    );
  }


  public updateAIAudit(reportAIContentId: number, state: AIAuditStateEnum): Observable<boolean> {
    this.spinner.show();

    return this.httpClient
      .post<boolean>(`${this.reportAIContentURL}/${reportAIContentId}/${state}/updateState`, {})
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }


  //update state Active /archive /deleted
  public updateStateAIAuditV2(auditV2Id: number, state: AIAuditStateEnum): Observable<boolean> {
    this.spinner.show();

    return this.httpClient
      .post<boolean>(`${this.reportAIURL}/${auditV2Id}/${state}/updateState`, {})
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public updateKeywordSummary(summary: AIKeywordSummary) {
    this.spinner.show();
    console.log(summary);

    return this.httpClient
      .post<AIKeywordSummary>(`${this.reportAIURL}/updateKeyWordSummary`, {
        id:summary.id,
        auditAIPatientPdfNotesID: summary.auditAIPatientPdfNotesID,
        keyword: summary.keyword,
        summary: summary.summary,
        accept: summary.accept
      })
      .pipe(
        map((data: AIKeywordSummary) => {
          return data;
        })
      );

  }
  public addKeyWordSummary(summary: AIKeywordSummary) {
    this.spinner.show();
    console.log(summary);

    return this.httpClient
      .post<AIKeywordSummary>(`${this.reportAIURL}/addKeyWordSummary`, {

        auditAIPatientPdfNotesID: summary.auditAIPatientPdfNotesID,
        keyword: summary.keyword,
        summary: summary.summary,
        accept: summary.accept
      })
      .pipe(
        map((data: AIKeywordSummary) => {
          return data;
        })
      );
  }
  //addPatientNoteWithSummary
  public addPatientNoteWithSummary(notes: AIProgressNotes) {
    this.spinner.show();
    console.log(notes);
    return this.httpClient
      .post<AIProgressNotes>(`${this.reportAIURL}/addPatientKeySummary`, {

        summaries: notes.summaries,
        patientName: notes.patientName,
        dateTime: notes.dateTime,
        reportId: notes.reportId

      })
      .pipe(
        map((data: AIProgressNotes) => {
          return data;
        })
      );
  }

  public updateAuditAIV2(audit: AIAudit) {
    this.spinner.show();
    return this.httpClient
      .post<AIAudit>(`${this.reportAIURL}/updateAuditAIV2`, {
        id: audit.id,
        values: audit.values,
        organization: audit.organization,
        facility: audit.facility,
        auditorName: audit.auditorName,
        auditTime: audit.auditTime,
        auditDate: audit.auditDate,
        filteredDate: audit.filteredDate,
        status: audit.status,
        createdAt: audit.createdAt,
        submittedDate: audit.submittedDate,
        sentForApprovalDate: audit.sentForApprovalDate,
        state: audit.state
      })
      .pipe(
        map((data: AIAudit) => {
          return data;
        })
      );
  }

  public updatePatientNoteWithSummary(notes: AIProgressNotes) {
    this.spinner.show();
    
    return this.httpClient
      .post<AIProgressNotes>(`${this.reportAIURL}/updatePatientKeySummary`, {
        id:notes.id,
        summaries: notes.summaries,
        patientName: notes.patientName,
        patientId: notes.patientId,
        dateTime: notes.dateTime,
        reportId: notes.reportId

      })
      .pipe(
        map((data: AIProgressNotes) => {
          return data;
        })
      );
  }

  public downloadAIAuditV2Report(audit: AIAudit ) {

    this.spinner?.show();
    var optionModel = { id: audit.id, name: "" };
    this.httpClient
      .post(
        `${this.reportAIURL}/downloadReportbyAIV2`,
        {
          id: audit.id, name: ""
        },
        {
          headers: { Accept: "application/pdf" },
          responseType: "blob",
        }
      )
      .pipe(first())
      .subscribe((data: Blob) => {
        let name = audit.facility != null ? audit.facility?.name : audit.organization?.name;
        name = name + " 24 Hour Keyword ";

        let prefDate = moment(audit.auditDate, "MMM DD, YYYY").format("MM.DD.YYYY");
        this.downloadOpenPdf(data, name + audit.auditDate);
      });
  }

}
