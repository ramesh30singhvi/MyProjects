import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { NgxSpinnerService } from "ngx-spinner";
import { Observable, first } from "rxjs";
import { environment } from "../../environments/environment";
import { transformDate, transformNumber } from "../common/helpers/dates-helper";
import { IFilterOption, IPdfFilterModel } from "../models/audits/audit.filters.model";
import { IMessageResponse } from "../models/common.model";
import { ReportRequestFiltersModel } from "../models/report-requests/report-requests.model";
import { REPORT_REQUEST_GRID_COLUMNS } from "../report-request/common/report-request-grid.constants";

@Injectable()
export class ReportRequestServiceApi {
  private reportRequestUrl = `${environment.apiUrl}report/request`;
  private reportRequestFiltersUrl = `${this.reportRequestUrl}/filters`;

  constructor(
    private httpClient: HttpClient,
    private spinner: NgxSpinnerService) { }

  public getReportRequests(
    startRow: number,
    endRow: number,
    sortModel,
    filterModel,
    reportRequestFilterValues,
  ): Observable<any[]> {
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

    const filters = this.getFilterModelParams(filterModel, reportRequestFilterValues);

    return this.httpClient.post<any[]>(`${this.reportRequestUrl}/get`, { 
      ...searchParams, 
      ...filters,
     });
  }

  public getGridFilters(field: string, filterModel: any, reportRequestFilterValues: ReportRequestFiltersModel) {
    const filters = this.getFilterModelParams(filterModel, reportRequestFilterValues);

      return this.httpClient.post<IFilterOption[]>(this.reportRequestFiltersUrl, {
        column: field,
        reportRequestFilter: filters,
      });
  }

  public addReportRequest(filter: IPdfFilterModel): Observable<IMessageResponse> {
    this.spinner.show("criteriaPdfSpinner");
    
    return this.httpClient
    .post<IMessageResponse>(this.reportRequestUrl, {
      auditType: filter.auditType,
      organizationId: filter.organization.id,
      facilityId: filter.facility ? filter.facility.id : null,
      formId: filter.form.id,
      fromDate: filter.dateFrom,
      toDate: filter.dateTo,
    });
  }

  downloadPdf(report: string): void {
    this.spinner.show();

    this.httpClient
    .get(`${this.reportRequestUrl}/${report}`, {
      headers:{Accept : 'application/pdf'},
      responseType: 'blob'
    })
    .pipe(first())
    .subscribe((data: Blob) => {
        this.downloadOpenPdf(data, report);
    });
    /*.pipe(
      map((data: Blob) => {
        return data;
      })
    )*/;
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

  getFilterModelParams(filterModel: any, reportRequestFilterValues: ReportRequestFiltersModel) {
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
            //const values: string[] = filter.values.map((value) => value);

            const filterValues: IFilterOption[] = reportRequestFilterValues?.[key];

            filterParams[key] = filterValues
              ?.filter((fv: IFilterOption) => filter.values.includes(fv.value));
          }
      }
    });

    return filterParams;
  }
}
