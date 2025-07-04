import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import * as moment from "moment";
import { NgxSpinnerService } from "ngx-spinner";
import { concatMap, first, map, Observable, timeout } from "rxjs";
import { environment } from "../../environments/environment";
import { transformDate, transformNumber } from "../common/helpers/dates-helper";
import { IFilterOption, ReportFiltersModel } from "../models/audits/audit.filters.model";
import { IFacilityOption, IOption } from "../models/audits/audits.model";
import { FallReport } from "../models/reports/fall-reports.model";
import { AIResult } from "../models/reports/file-upload.model";
import { Report } from "../models/reports/reports.model";
import { WoundReport } from "../models/reports/wound-reports.model";

@Injectable()
export class ReportsService {
  getHighAlertStatuses() {
    return this.httpClient
      .get<IOption[]>(`https://localhost:5001/api/clientportal/highAlertStatuses`);
  }

  getFacilityInfo() {
    return this.httpClient
      .post<IFacilityOption>(`https://localhost:5001/api/clientportal/facilityInfo`, {id: 0, name : "Bonifay Nursing and Rehab Center" } );
  }
  public getReportCategories() {
   
    return this.httpClient
      .get<IOption[]>(`https://localhost:5001/api/clientportal/potentialAreas`);
 
  }
  public changeHighAlertStatus() {

    return this.httpClient
      .post<any>(`https://localhost:5001/api/highAlert/statusChange`, {
        "highAlertId": 18,
        "status": {id:2,name:"Resolve"},
        "userNotes": "welcome",
        "changeBy": "Inessa "
      });

  }
  public getHighAlerts() {

    return this.httpClient
      .post<any>(`https://localhost:5001/api/highAlert/get`, {
        "highAlertId": 7,
        "status": { id: 2, name: "Resolve" },
        "userNotes": "welcome"
      });

  }
  private reportsUrl = `${environment.apiUrl}reports`;
  private reportsFallUrl = `${environment.apiUrl}reports/fall`;
  private reportsWoundUrl = `${environment.apiUrl}reports/wound`;
  private reportsDownloadFallUrl = `${environment.apiUrl}reports/downloadFall`;
  private reportsDownloadWoundUrl = `${environment.apiUrl}reports/downloadWound`;
  private reportsDownloadCriteriaUrl = `${environment.apiUrl}reports/downloadCriteria`;
  private reportsFiltersUrl = `${environment.apiUrl}reports/filters`;
  private reportsTableauUrl = `${environment.apiUrl}reports/url?reportId=`;
  private reportsDetailsUrl = `${environment.apiUrl}reports?id=`;

  private auditMigrateToReport = `${environment.apiUrl}reportManagement/admin/migrate`;
  private reportManagementURL = `${environment.apiUrl}reportManagement/admin`;
  private reportManagementSendReportsURL = `${environment.apiUrl}reportManagement/admin/sendReports`;
  private reportManagementExportFacilityEmailsURL = `${environment.apiUrl}reportManagement/admin/exportRecipients`;
  private reportRangesUrl = `${environment.apiUrl}reports/reportRanges`;
  private clientportalReportsURL = `${environment.apiUrl}clientportal/getReports`;
  private clientportalReportsFacilityURL = `${environment.apiUrl}clientportal/facilityReportsByPage`;
  private clientportaURL = `${environment.apiUrl}clientportal/`;
  private HighAlerttURL = `${environment.apiUrl}highAlert`;
 
  constructor(
    private httpClient: HttpClient,
    private spinner: NgxSpinnerService
  ) {}


  public deleteReport(id: number) {
    this.spinner.show();
    return this.httpClient
      .post<number>(`${this.reportManagementURL}/${id}/delete`, {})
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }
  public getReportsUrls(): Observable<string> {
    this.spinner.show();

    return this.httpClient.get<string>(this.reportsUrl);
  }
  public getReportUrl(id: string): Observable<{ reportUrl: string }> {
    this.spinner.show();

    return this.httpClient.get<{ reportUrl: string }>(
      this.reportsTableauUrl + id
    );
  }


  public getReportDetails(id: string): Observable<Report> {
    this.spinner.show();

    return this.httpClient.get<Report>(this.reportsDetailsUrl + id.toString());
  }
  public getReportsFilters(column: string): Observable<string[]> {
    return this.httpClient.get<string[]>(this.reportsFiltersUrl, {
      params: { column },
    });
  }


  downloadReport(reportId: string, name: string, reportType: any): void {

    this.spinner.show();

    if (reportType == 'PDF') return this.downloadPdf(reportId, name);

    if (reportType == 'Excel') return this.downloadExcel(reportId, name);

  }

  private downloadExcel(reportId: string, name: string) {

    this.httpClient

      .post(

        `${this.clientportaURL}downloadExcelReport`,

        { "selectedIds": [reportId] },

        {

          headers: {

            Accept: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
          },

          responseType: 'blob',

        },

      )

      .pipe(first())

      .subscribe((data: Blob) => {

        this.downloadOpenExcel(data, name);

      });

  }

  private downloadPdf(reportId: string, name: string) {

    this.httpClient.post(

      `${this.clientportaURL}downloadPDFReport`,

      { "selectedIds":[ reportId ]},

        {

          headers: { Accept: 'application/pdf' },

          responseType: 'blob',

        },

      )

      .pipe(first())

      .subscribe((data: Blob) => {

        this.downloadOpenPdf(data, name);

      });

  }


  public migrateToReport() {
    this.spinner?.show();
    return this.httpClient.post(this.auditMigrateToReport, {});
  }

  public sendReports(ids: any[],useids:any[]) {
    this.spinner?.show();
    return this.httpClient.post(this.reportManagementSendReportsURL, {
      "selectedIds": ids, "userEmails": useids,"message":"Hello my friends"
    });
  }

  public getReportForUser(facility) {
    
    this.spinner.show();


    var searchParams = {
      search: "",
      skipCount: 0,
      takeCount: 25,
      orderBy: "",
      sortOrder: "",
    };
    var colId = "";
    var sortOrder = "";

    //if (colId && sortOrder) {
    searchParams.orderBy = "Date";
    searchParams.sortOrder = "desc";
    //}

    //  const filters = this.getPortalFilterModelParams(filterModel, filterValues);
    //  filters["state"] = state; [{"timeZoneOffset":-5,"organizationId":36,"id":619,"name":"United HealthCare","hidden":null}]

    var filters = { "facilityName": facility };//, "date": "{\"firstCondition\":{\"from\":\"2024-06-12\",\"to\":\"\",\"type\":\"equals\"},\"secondCondition\":null}" };

    return this.httpClient.post<{ totalCount: number; items: any[] }>(this.clientportalReportsFacilityURL,
      {
        ...searchParams,
        ...filters
      });
    
  }


  public getPortalReports(startRow,
    endRow,
    sortModel,
    filterModel,
    filterValues) {
    this.spinner.show();



    var searchParams = {
      search: "",
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
    //if (colId && sortOrder) {
      searchParams.orderBy = "Date";
      searchParams.sortOrder = "desc";
    //}

  //  const filters = this.getPortalFilterModelParams(filterModel, filterValues);
    //  filters["state"] = state; [{"timeZoneOffset":-5,"organizationId":36,"id":619,"name":"United HealthCare","hidden":null}]
    let f = [];
    f.push({ "id": 771, "value": "Bonifay Nursing and Rehab Center" })
    var filters = { "organization": { "id": 52, "value": "Southern Healthcare Management" }, "facilities": f, "reportCategory": { "id": 1, "value": "Clinical" },"date": "{\"firstCondition\":{\"from\":\"2024-06-12\",\"to\":\"\",\"type\":\"equals\"},\"secondCondition\":null}" };


    return this.httpClient.post<{ totalCount: number; items: any[] }>(
      `${this.reportManagementURL}/getByPage`,
      {
        ...searchParams,
        ...filters,
      }
    );

  }


  public getReports(
    search: string,
    filterModel: any,
    sortModel: any[],
    startRow: number,
    endRow: number
  ): Observable<Report[]> {
    this.spinner.show();
    const filters = this.getFilterModelParams(filterModel);

    var searchParams = {
      search,
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

    return this.httpClient.post<Report[]>(this.reportsUrl, {
      ...searchParams,
      ...filters,
    });
  }

  public getFallReport(
    organizationId: number,
    facilityId: number,
    year: number,
    months: number[]
  ): Observable<FallReport> {
    return this.httpClient.post<FallReport>(this.reportsFallUrl, {
      organizationId,
      facilityId,
      year,
      months,
    });
  }

  public getDownloadCriteria(
    organizationId: number,
    facilityIds: number[],
    fromAuditDate: string,
    toAuditDate: string,
    fromDate: string,
    toDate: string,
    formVersionIds: number[],
    questionsIds: number[],
    compliantType: number // 1 - compliance, 2 - non compliance, 3 - n/a
  ) {
    this.spinner.show();
    this.httpClient
      .post(
        this.reportsDownloadCriteriaUrl,
        {
          organizationId,
          facilityIds,
          fromAuditDate,
          toAuditDate,
          fromDate,
          toDate,
          formVersionIds,
          questionsIds,
          compliantType,
        },
        {
          headers: {
            Accept:
              "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
          },
          responseType: "blob",
        }
      )
      .pipe(first())
      .subscribe((data: Blob) => {
        this.downloadOpenExcel(data, "CriteriaReport");
      });
  }

  public getDownloadFallReport(
    organizationId: number,
    facilityId: number,
    year: number,
    months: number[]
  ) {
    this.spinner.show();

    this.httpClient
      .post(
        this.reportsDownloadFallUrl,
        {
          organizationId,
          facilityId,
          year,
          months,
        },
        {
          headers: { Accept: "application/pdf" },
          responseType: "blob",
        }
      )
      .pipe(first())
      .subscribe((data: Blob) => {
        this.downloadOpenPdf(data, "FallReport");
      });
  }

  public getDownloadWoundReport(
    organizationId: number,
    facilityId: number,
    year: number,
    months: number[]
  ) {
    this.spinner.show();

    this.httpClient
      .post(
        this.reportsDownloadWoundUrl,
        {
          organizationId,
          facilityId,
          year,
          months,
        },
        {
          headers: { Accept: "application/pdf" },
          responseType: "blob",
        }
      )
      .pipe(first())
      .subscribe((data: Blob) => {
        this.downloadOpenPdf(data, "WoundReport");
      });
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

  public downloadOpenExcel(data: Blob, fileName: string): void {
    const blob = new Blob([data], { type: data.type });
    const url = window.URL.createObjectURL(blob);
    var a = document.createElement("a");
    a.href = url;
    a.download = `${fileName}.xls`;
    document.body.appendChild(a);
    a.click();
    URL.revokeObjectURL(url);
  }

  public getWoundReport(
    organizationId: number,
    facilityId: number,
    year: number,
    months: number[]
  ): Observable<WoundReport> {
    return this.httpClient.post<WoundReport>(this.reportsWoundUrl, {
      organizationId,
      facilityId,
      year,
      months,
    });
  }
 
  private getFilterModelParams(filterModel: any): any {
    const filterParams = {};

    for (const key in filterModel) {
      filterParams[key] = filterModel[key].values;
    }

    return filterParams;
  }


}
