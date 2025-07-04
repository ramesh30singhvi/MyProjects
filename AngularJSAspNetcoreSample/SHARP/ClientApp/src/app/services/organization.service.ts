import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { NgxSpinnerService } from "ngx-spinner";
import { map, Observable } from "rxjs";
import { environment } from "../../environments/environment";
import { getFilterModelParams } from "../common/helpers/grid-helper";
import { IFilterOption } from "../models/audits/audit.filters.model";
import { IOption } from "../models/audits/audits.model";
import { FormFiltersModel, IFormSetting, OrganizationFormGridItem } from "../models/forms/forms.model";
import { OrganizationDetailed } from "../models/organizations/organization.detailed.model";
import { RecipientModel } from "../models/organizations/recipient.model";

@Injectable()
export class OrganizationService {

  private organizationAdminUrl = `${environment.apiUrl}organizations/admin`;
  private organizationUrl = `${environment.apiUrl}organizations`;
  private organizationAddHighAlertdUrl = `${this.organizationAdminUrl}/addHighAlert`;
  private organizationDetailedUrl = `${this.organizationAdminUrl}/detailed`;
  private organizationPortalFeatureUrl = `${this.organizationAdminUrl}/portalFeatures`;
  constructor(
    private httpClient: HttpClient,
    private spinner: NgxSpinnerService) { }

  public getOrganizations(): Observable<OrganizationDetailed[]> {
    this.spinner.show();
    return this.httpClient.get<any[]>(this.organizationDetailedUrl);
  }

  public createOrganizations(organizationName: string, recipients: string[], operatorEmail:string, operatorName:string): Observable<OrganizationDetailed> {
    this.spinner.show();
    return this.httpClient.post<OrganizationDetailed>(
      this.organizationDetailedUrl,
      { Name: organizationName, Recipients: recipients, operatorEmail: operatorEmail, operatorName: operatorName });
  }

  public editOrganization(id: number, name: string, recipients: any[], orgPortalFeatures:any, operatorEmail:string, operatorName:string,attacheReport:boolean): Observable<boolean> {
    this.spinner.show();
    return this.httpClient.put<boolean>(`${this.organizationDetailedUrl}/${id}`,
      { name: name, recipients: recipients, portalFeatures: orgPortalFeatures, operatorName: operatorName, operatorEmail: operatorEmail, attachPortalReport: attacheReport });
  }

  public getOrganizationAdminOptions(): Observable<IOption[]> {
    return this.httpClient
      .get<IOption[]>(`${this.organizationAdminUrl}`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }
  public getPortalFeatures(): Observable<IOption[]> {
    return this.httpClient
      .get<IOption[]>(`${this.organizationUrl}/portalFeatures`);
  }
  public getOrganizationOptions(): Observable<IOption[]> {
    return this.httpClient
      .get<IOption[]>(`${this.organizationUrl}/options`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public addHighAlertToForms(id) {
    this.spinner.show();
    return this.httpClient.put<boolean>(`${this.organizationAddHighAlertdUrl}/${id}`, { });
  }

  public getOrganizationForms(search: string,
    filterModel: any,
    sortModel: any[],
    startRow: number,
    endRow: number,
    organizationId: number,
    formFilterValues: FormFiltersModel): Observable<OrganizationFormGridItem[]> {
    this.spinner.show();

    const filters = getFilterModelParams(filterModel, formFilterValues);

    var searchParams = {
      search,
      skipCount: startRow,
      takeCount: endRow - startRow,
      orderBy: "",
      sortOrder: "",
      organizationId: organizationId
    };

    if (sortModel?.length) {
      const { colId, sort } = sortModel[0];
      searchParams.orderBy = colId;
      searchParams.sortOrder = sort;
    }

    return this.httpClient.post<OrganizationFormGridItem[]>(`${this.organizationAdminUrl}/forms/get`, { 
      ...searchParams,
      ...filters,
     });
  }

  public getFormFilters(column: string, organizationId: number, filterModel: any, formFilterValues: FormFiltersModel): Observable<IFilterOption[]> {
    const filters = getFilterModelParams(filterModel, formFilterValues);

    filters["organizationId"] = organizationId;

    return this.httpClient.post<IFilterOption[]>(`${this.organizationAdminUrl}/forms/filters`, { 
      column, 
      formFilter: filters
     });
  }

  public setFormScheduleSetting(formSetting: IFormSetting): Observable<boolean> {
    this.spinner.show('scheduleFormSpinner');
    return this.httpClient.put<boolean>(`${this.organizationAdminUrl}/forms/settings`, { 
      id: formSetting.id,
      settingType: formSetting.settingType,
      scheduleSetting: formSetting.scheduleSetting,
     });
  }
}
