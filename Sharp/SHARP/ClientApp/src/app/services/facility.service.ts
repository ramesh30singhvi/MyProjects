import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { NgxSpinnerService } from "ngx-spinner";
import { map, Observable } from "rxjs";
import { environment } from "../../environments/environment";
import { transformDate } from "../common/helpers/dates-helper";
import { getFilterModelParams } from "../common/helpers/grid-helper";
import { IFilterOption } from "../models/audits/audit.filters.model";
import { IFacilityTimeZoneOption, IOption } from "../models/audits/audits.model";
import { FacilityFiltersModel, FacilityModel, IFacilityDetails } from "../models/organizations/facility.model";

@Injectable()
export class FacilityService {

  private facilitiesUrl = `${environment.apiUrl}facilities`;
  private facilityOptionsUrl = `${environment.apiUrl}facility/options`;
  private facilitiesAddRecipietsUrl = `${environment.apiUrl}facilities/addemailsfacility`;
  constructor(
    private httpClient: HttpClient,
    private spinner: NgxSpinnerService) { }

  public getFacilities(
    search: string,
    filterModel: any,
    sortModel: any[],
    startRow: number,
    endRow: number,
    organizationId: number,
    facilityFilterValues: FacilityFiltersModel
    ): Observable<FacilityModel[]> {
    this.spinner.show();

    const filters = getFilterModelParams(filterModel, facilityFilterValues);

    var searchParams = {
      search,
      skipCount: startRow,
      takeCount: endRow - startRow,
      orderBy: "",
      sortOrder: "",
      organizationId: organizationId,
    };

    if (sortModel?.length) {
      const { colId, sort } = sortModel[0];
      searchParams.orderBy = colId;
      searchParams.sortOrder = sort;
    }

    return this.httpClient.post<FacilityModel[]>(`${this.facilitiesUrl}/get`, { 
      ...searchParams,
      ...filters,
    });
  }

  public getFacilityFilters(column: string, organizationId: number, filterModel: any, facilityFilterValues: FacilityFiltersModel): Observable<IFilterOption[]> {
    const filters = getFilterModelParams(filterModel, facilityFilterValues);

    filters["organizationId"] = organizationId;

    return this.httpClient.post<IFilterOption[]>(`${this.facilitiesUrl}/filters`, { 
      column,
      facilityFilter: filters,
     });
  }

  public getFacilityOptions(organizationId: number): Observable<IOption[]> {
    return this.httpClient
      .get<IOption[]>(`${this.facilityOptionsUrl}/organization/${organizationId}`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public getFacilityFilteredOptions(
    search: string,
    skipCount: number,
    takeCount: number,
    organizationIds: number[] | null): Observable<IOption[]> {

    return this.httpClient.post<IOption[]>(this.facilityOptionsUrl, { 
      search,
      skipCount,
      takeCount,
      organizationIds
     });
  }

  public getTimeZoneOptions(): Observable<IFacilityTimeZoneOption[]> {
    return this.httpClient
      .get<IFacilityTimeZoneOption[]>(`${this.facilitiesUrl}/timezones`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public getFacilityDetails(facilityId: number): Observable<IFacilityDetails> {
    return this.httpClient
      .get<IFacilityDetails>(`${this.facilitiesUrl}/${facilityId}/details`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public addFacility(facility: IFacilityDetails): Observable<IFacilityDetails> {
    this.spinner.show('facilityEditSpinner');

    return this.httpClient
      .post<IFacilityDetails>(`${this.facilitiesUrl}`, {
          name: facility.name,
          organizationId: facility.organization?.id,
          timeZoneId: facility.timeZone?.id,
          recipients: facility.recipients,
          isActive: facility.isActive,
          legalName: facility.legalName,
        })
      .pipe(
        map((data: any) => {
          this.spinner.hide("facilityEditSpinner");
          return data;
        })
      );
  }
  public addEmailRecipientsFacility(ficilityid: number, emails:any) :Observable<IFacilityDetails>{
    this.spinner.show('facilityEditSpinner');
    return this.httpClient
      .post<IFacilityDetails>(`${this.facilitiesUrl}/addemailsfacility`, {
        id: ficilityid,
        emails: emails
      })
      .pipe(
        map((data: any) => {
          this.spinner.hide("facilityEditSpinner");
          return data;
        })
      );
  }
  public editFacility(facility: IFacilityDetails): Observable<IFacilityDetails> {
    this.spinner.show('facilityEditSpinner');

    return this.httpClient
      .put<IFacilityDetails>(`${this.facilitiesUrl}`, {
          id: facility.id,
          name: facility.name,
          organizationId: facility.organization?.id,
          timeZoneId: facility.timeZone?.id,
          recipients: facility.recipients,
          isActive: facility.isActive,
          legalName: facility.legalName,
        })
      .pipe(
        map((data: any) => {
          this.spinner.hide("facilityEditSpinner");
          return data;
        })
      );
  }

  /*private getFilterModelParams(filterModel: any, facilityFilterValues: FacilityFiltersModel): any {
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
            const filterValues: IFilterOption[] = facilityFilterValues?.[key];

            filterParams[key] = filterValues
              ?.filter((fv: IFilterOption) => filter.values.includes(fv.value));
          }
      }
    });

    return filterParams;
  }*/
}
