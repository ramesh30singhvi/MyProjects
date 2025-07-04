import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { NgxSpinnerService } from "ngx-spinner";
import { Observable } from "rxjs";
import { environment } from "../../environments/environment";
import { IFilterOption } from "../models/audits/audit.filters.model";
import { transformDate } from "../common/helpers/dates-helper";
import {
  AuditorProductivityInputFiltersModel,
  AuditorProductivityAHTPerAuditTypeFiltersModel,
  AuditorProductivitySummaryPerAuditorFiltersModel,
  AuditorProductivitySummaryPerFacilityFiltersModel
} from "../models/dashboard/auditor-productivity-dashboard.model";


@Injectable()
export class AuditorProductivityDashboardService {

  private getInputDataURL = `${environment.apiUrl}auditorProductivityDashboard/getInputData`;
  private getInputfiltersURL = `${environment.apiUrl}auditorProductivityDashboard/getInputfilters`;

  private getAHTPerAuditTypeDataURL = `${environment.apiUrl}auditorProductivityDashboard/getAHTPerAuditTypeData`;
  private getAHTPerAuditTypefiltersURL = `${environment.apiUrl}auditorProductivityDashboard/getAHTPerAuditTypefilters`;

  private getSummaryPerAuditorDataURL = `${environment.apiUrl}auditorProductivityDashboard/getSummaryPerAuditorData`;
  private getSummaryPerAuditorfiltersURL = `${environment.apiUrl}auditorProductivityDashboard/getSummaryPerAuditorfilters`;


  private getSummaryPerFacilityDataURL = `${environment.apiUrl}auditorProductivityDashboard/getSummaryPerFacilityData`;
  private getFormTagsUrl = `${environment.apiUrl}auditorProductivityDashboard/getFormTags`;

  constructor(
    private httpClient: HttpClient,
    private spinner: NgxSpinnerService
  ) { }

  public getInputData(
    filterModel: any,
    sortModel: any[],
    startRow: number,
    endRow: number,
    auditorProductivityInputFilterValues: AuditorProductivityInputFiltersModel
  ): Observable<any[]> {
    //this.spinner.show();
    const filters = this.getInputFilterModelParams(
      filterModel,
      auditorProductivityInputFilterValues
    );

    const combinedFilter = {
      ...filters,
      ...auditorProductivityInputFilterValues
    };

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

    return this.httpClient.post<any[]>(
      this.getInputDataURL,
      {
        ...searchParams,
        ...combinedFilter,
      }
    );
  }

  public getInputfilters(
    column: string,
    filterModel: any,
    auditorProductivityInputFilterValues: AuditorProductivityInputFiltersModel
  ): Observable<IFilterOption[]> {

    const filters = this.getInputFilterModelParams(
      filterModel,
      auditorProductivityInputFilterValues
    );

    return this.httpClient.post<IFilterOption[]>(
      this.getInputfiltersURL,
      {
        column,
        auditorProductivityInputFilter: filters,
      }
    );
  }

  public getSummaryPerFacility(filterModel: any,
    sortModel: any[],
    startRow: number,
    endRow: number,
    summaryFacilityFilterValues: AuditorProductivitySummaryPerFacilityFiltersModel) {

    console.log("get summary per facility");

    console.log(summaryFacilityFilterValues);


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
    this.spinner.show();
    return this.httpClient.post<any>(
      this.getSummaryPerFacilityDataURL,
      {
        ...searchParams,
        ...summaryFacilityFilterValues,
      }
    );
  }
  public getFormTags(): Observable<any> {
    return this.httpClient.get(this.getFormTagsUrl);
  }
 
  getInputFilterModelParams(
    filterModel: any,
    auditorProductivityInputFilterValues: AuditorProductivityInputFiltersModel
  ) {
    const filterParams: any = {};

    if (!filterModel) return "";

    Object.keys(filterModel).forEach((key) => {
      const filter = filterModel[key];
      console.log(filter);
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
              auditorProductivityInputFilterValues?.[key];

            filterParams[key] = filterValues?.filter((fv: IFilterOption) =>
              filter.values.includes(fv.value)
            );
          }
      }
    });

    return filterParams;
  }


  public getAHTPerAuditTypeData(
    filterModel: any,
    sortModel: any[],
    startRow: number,
    endRow: number,
    auditorProductivityAHTPerAuditTypeFilterValues: AuditorProductivityAHTPerAuditTypeFiltersModel
  ): Observable<any[]> {
    //this.spinner.show();
    const filters = this.getAHTPerAuditTypeFilterModelParams(
      filterModel,
      auditorProductivityAHTPerAuditTypeFilterValues
    );

    if (auditorProductivityAHTPerAuditTypeFilterValues.dateProcessed) {
      filters.dateProcessed = auditorProductivityAHTPerAuditTypeFilterValues.dateProcessed;
    }
    const combinedFilter = {
      ...filters,
      ...auditorProductivityAHTPerAuditTypeFilterValues
    };
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

    return this.httpClient.post<any[]>(
      this.getAHTPerAuditTypeDataURL,
      {
        ...searchParams,
        ...combinedFilter,
      }
    );
  }

  public getAHTPerAuditTypefilters(
    column: string,
    filterModel: any,
    auditorProductivityAHTPerAuditTypeFilterValues: AuditorProductivityAHTPerAuditTypeFiltersModel
  ): Observable<IFilterOption[]> {

    const filters = this.getAHTPerAuditTypeFilterModelParams(
      filterModel,
      auditorProductivityAHTPerAuditTypeFilterValues
    );

    if (auditorProductivityAHTPerAuditTypeFilterValues.dateProcessed) {
      filters.dateProcessed = auditorProductivityAHTPerAuditTypeFilterValues.dateProcessed;
    }

    return this.httpClient.post<IFilterOption[]>(
      this.getAHTPerAuditTypefiltersURL,
      {
        column,
        auditorProductivityAHTPerAuditTypeFilter: filters,
      }
    );
  }

  getAHTPerAuditTypeFilterModelParams(
    filterModel: any,
    auditorProductivityAHTPerAuditTypeFilterValues: AuditorProductivityAHTPerAuditTypeFiltersModel
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
              auditorProductivityAHTPerAuditTypeFilterValues?.[key];

            filterParams[key] = filterValues?.filter((fv: IFilterOption) =>
              filter.values.includes(fv.value)
            );
          }
      }
    });

    return filterParams;
  }


  public getSummaryPerAuditorData(
    filterModel: any,
    sortModel: any[],
    startRow: number,
    endRow: number,
    auditorProductivitySummaryPerAuditorFilterValues: AuditorProductivitySummaryPerAuditorFiltersModel
  ): Observable<any[]> {
    //this.spinner.show();
    const filters = this.getSummaryPerAuditorFilterModelParams(
      filterModel,
      auditorProductivitySummaryPerAuditorFilterValues
    );


    const combinedFilter = {
      ...filters,
      ...auditorProductivitySummaryPerAuditorFilterValues
    };

    if (auditorProductivitySummaryPerAuditorFilterValues.dateProcessed) {
      combinedFilter.dateProcessed = auditorProductivitySummaryPerAuditorFilterValues.dateProcessed;
    }
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

    return this.httpClient.post<any[]>(
      this.getSummaryPerAuditorDataURL,
      {
        ...searchParams,
        ...combinedFilter,
      }
    );
  }

  public getSummaryPerAuditorfilters(
    column: string,
    filterModel: any,
    auditorProductivitySummaryPerAuditorFilterValues: AuditorProductivitySummaryPerAuditorFiltersModel
  ): Observable<IFilterOption[]> {

    const filters = this.getSummaryPerAuditorFilterModelParams(
      filterModel,
      auditorProductivitySummaryPerAuditorFilterValues
    );

    if (auditorProductivitySummaryPerAuditorFilterValues.dateProcessed) {
      filters.dateProcessed = auditorProductivitySummaryPerAuditorFilterValues.dateProcessed;
    }

    return this.httpClient.post<IFilterOption[]>(
      this.getSummaryPerAuditorfiltersURL,
      {
        column,
        auditorProductivitySummaryPerAuditorFilter: filters,
      }
    );
  }

  getSummaryPerAuditorFilterModelParams(
    filterModel: any,
    auditorProductivitySummaryPerAuditorFilterValues: AuditorProductivitySummaryPerAuditorFiltersModel
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
              auditorProductivitySummaryPerAuditorFilterValues?.[key];

            filterParams[key] = filterValues?.filter((fv: IFilterOption) =>
              filter.values.includes(fv.value)
            );
          }
      }
    });

    return filterParams;
  }
}
