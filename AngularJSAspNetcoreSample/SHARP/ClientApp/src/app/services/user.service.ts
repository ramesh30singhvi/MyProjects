import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { NgxSpinnerService } from "ngx-spinner";
import { first, map, Observable } from "rxjs";
import { fileURLToPath } from "url";
import { environment } from "../../environments/environment";
import { USER_GRID_COLUMNS } from "../common/constants/userGrid";
import { transformDate } from "../common/helpers/dates-helper";
import { IFilterOption } from "../models/audits/audit.filters.model";
import { IUserOption, IUserTimeZone } from "../models/audits/audits.model";
import {
  ActionTypeEnum,
  CreateUser,
  EditUser,
  ITimeZone,
  IUserDetails,
  IUserFacilities,
  IUserOrganizations,
  IUserProductivityFilter,
  User,
  UserFiltersModel,
} from "../models/users/users.model";

@Injectable()
export class UserService {
  private usersUrl = `${environment.apiUrl}users`;
  private usersFiltersUrl = `${environment.apiUrl}users/filters`;

  private roleUrl = `${environment.apiUrl}roles`;
  private teamUrl = `${environment.apiUrl}users/teams`;
  private usersPerOrganization = `${environment.apiUrl}users/organization`;
   
  constructor(
    private httpClient: HttpClient,
    private spinner: NgxSpinnerService
  ) {}

  public getUsers(
    search: string,
    filterModel: any,
    sortModel: any[],
    startRow: number,
    endRow: number,
    userFilterValues: UserFiltersModel
  ): Observable<User[]> {
    this.spinner.show();

    const filters = this.getFilterModelParams(filterModel, userFilterValues);

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

    return this.httpClient.post<User[]>(`${this.usersUrl}/get`, {
      ...searchParams,
      ...filters,
    });
  }


  public getUserOptions(organizationId: number): Observable<IUserOption[]> {
    return this.httpClient
      .get<IUserOption[]>(`${this.usersPerOrganization}/${organizationId}`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }
  public getUsersFilters(
    column: string,
    filterModel: any,
    userFilterValues: UserFiltersModel
  ): Observable<IFilterOption[]> {
    const filters = this.getFilterModelParams(filterModel, userFilterValues);
    return this.httpClient.post<IFilterOption[]>(this.usersFiltersUrl, {
      column,
      userFilter: filters,
    });
  }

  public createUser(user: CreateUser): Observable<any> {
    return this.httpClient.post(this.usersUrl, user);
  }

  public editUser(user: EditUser): Observable<any> {
    return this.httpClient.put(this.usersUrl, user);
  }

  public getUserDetails(userId: number): Observable<any> {
    return this.httpClient.get<IUserDetails>(
      `${this.usersUrl}/details/${userId}`
    );
  }

  public getRoles(): Observable<any[]> {
    return this.httpClient.get<any[]>(this.roleUrl);
  }

  public getTeams(): Observable<any[]> {
    return this.httpClient.get<any[]>(this.teamUrl);
  }

  public addUserActivity(
    actionType: ActionTypeEnum,
    auditId: number | null = null
  ): Observable<boolean> {
    return this.httpClient.post<boolean>(`${this.usersUrl}/activities`, {
      actionType,
      auditId,
    });
  }

  public addUserActivityLog(
    actionType: ActionTypeEnum,
    auditId: number | null = null,
    updatedUserId: number | null = null,
    loginUsername: string | null = null,
  ): Observable<boolean> {
    return this.httpClient.post<boolean>(`${this.usersUrl}/activities`, {
      actionType,
      auditId,
      updatedUserId,
      loginUsername
    });
  }

  public getOrganizationOptions(): Observable<IUserOrganizations> {
    return this.httpClient.get<IUserOrganizations>(
      `${this.usersUrl}/organizations/options`
    );
  }

  public getFacilityOptions(): Observable<IUserFacilities> {
    return this.httpClient.get<IUserFacilities>(
      `${this.usersUrl}/facilities/options`
    );
  }
  

  public getTimeZones(): Observable<ITimeZone[]> {
    return this.httpClient.get<ITimeZone[]>(`${this.usersUrl}/timezones`);
  }

  public getUserTimeZone(userId: number): Observable<IUserTimeZone> {
    return this.httpClient.get<IUserTimeZone>(
      `${this.usersUrl}/${userId}/timezone`
    );
  }

  public downloadReport(filter: IUserProductivityFilter) {
    this.spinner.show();
    const filters = this.getFilterModelParams(
      filter.filterModel,
      filter.userFilterValues
    );
    this.httpClient
      .post(
        `${this.usersUrl}/activities/download-logs`,
        {
          userId: filter.userId,
          fromDate: filter.fromDate,
          toDate: filter.toDate,
          type: filter.type,
          filters: filters,
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
        this.downloadOpenExcel(data, `ProductivityLog`);
      });
  }

  public downloadOpenExcel(data: Blob, fileName: string): void {
    const blob = new Blob([data], { type: data.type });
    const url = window.URL.createObjectURL(blob);

    //window.open(url);

    var a = document.createElement("a");
    a.href = url;
    a.download = `${fileName}.xlsx`;
    document.body.appendChild(a);
    a.click();
    URL.revokeObjectURL(url);
  }

  private getFilterModelParams(
    filterModel: any,
    userFilterValues: UserFiltersModel
  ): any {
    const filterParams: any = {};

    if (!filterModel) return null;

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
            const filterValues: IFilterOption[] = userFilterValues?.[key];

            filterParams[key] = filterValues?.filter((fv: IFilterOption) =>
              filter.values.includes(fv.value)
            );
          }
      }
    });

    return filterParams;
  }
}
