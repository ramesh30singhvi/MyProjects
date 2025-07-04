import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import * as moment from "moment";
import { NgxSpinnerService } from "ngx-spinner";
import { map, Observable } from "rxjs";
import { environment } from "src/environments/environment";
import { YYYY_MM_DD_DASH } from "../common/constants/date-constants";
import {
  DashboardFilter,
  IAuditKPI,
  IAuditKPIApi,
  IAuditsDueDateCounts,
} from "../models/dashboard/dashboard.model";
import {
  DashboardInput,
  DashboardInputValue,
  DashboardInputFilter,
} from "../models/dashboard/dashboard-input.model";
import {
  AddElementDto,
  AddGroupDto,
  AddTableDto,
  EditDashboardInputDto,
} from "../models/dashboard/dashboard-input.dto";

/**
 * @description
 * @class
 */
@Injectable()
export class DashboardService {
  private dashboardUrl = environment.apiUrl + "dashboard";

  constructor(
    private httpClient: HttpClient,
    private spinner: NgxSpinnerService
  ) {}

  public getOrganizationForms(organizationId: any): Observable<any[]> {
    return this.httpClient.post<any[]>(`${environment.apiUrl}organizations/admin/forms/get`, {
      orderBy:"",
      organizationId: organizationId,
      skipCount: 0,
      sortOrder: "",
      takeCount: 1000
    }).pipe(
      map((data: any[]) => {
        return data;
      })
    );
  }

  public getAuditKPIs(filter: DashboardFilter): Observable<IAuditKPIApi[]> {
    return this.httpClient
      .post<IAuditKPIApi[]>(this.dashboardUrl, {
        organizations: filter.organizationIds,
        facilities: filter.facilityIds,
        forms: filter.formIds,
        timeFrame: filter.timeFrame,
        dueDateType: filter.dueDateType,
      })
      .pipe(
        map((data: IAuditKPIApi[]) => {
          return data;
        })
      );
  }

  public getAuditsDueDateCount(
    filter: DashboardFilter
  ): Observable<IAuditsDueDateCounts> {
    return this.httpClient
      .post(`${this.dashboardUrl}/duedates`, {
        organizations: filter.organizationIds,
        facilities: filter.facilityIds,
        forms: filter.formIds,
        timeFrame: filter.timeFrame,
      })
      .pipe(
        map((data: IAuditsDueDateCounts) => {
          return data;
        })
      );
  }

  public getDashboardInput(filter: DashboardInputFilter): Observable<DashboardInput[]> {
    this.spinner.show();
    return this.httpClient.post(`${this.dashboardUrl}/input/data`, {
      organizationId: filter.organizationId,
      dateFrom: filter.dateRangeFromTo.dateFrom,
      dateTo: filter.dateRangeFromTo.dateTo,
    }).pipe(
      map((data: any) => {
        return data;
      })
    );
  }

  public addDashboardTable(
    addTableDto: AddTableDto
  ): Observable<DashboardInput> {
    this.spinner.show();
    return this.httpClient
      .post(`${this.dashboardUrl}/input/table`, addTableDto)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public addDashboardGroup(
    addGroupDto: AddGroupDto
  ): Observable<DashboardInput> {
    this.spinner.show();
    return this.httpClient
      .post(`${this.dashboardUrl}/input/group`, addGroupDto)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public addDashboardElement(
    addElementDto: AddElementDto
  ): Observable<DashboardInput> {
    this.spinner.show();
    return this.httpClient
      .post(`${this.dashboardUrl}/input/element`, addElementDto)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public editDashboardTable(
    editDashboardInputDto: EditDashboardInputDto
  ): Observable<DashboardInput> {
    this.spinner.show();
    return this.httpClient
      .put(`${this.dashboardUrl}/input/table`, editDashboardInputDto)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public editDashboardGroup(
    editDashboardInputDto: EditDashboardInputDto
  ): Observable<DashboardInput> {
    this.spinner.show();
    return this.httpClient
      .put(`${this.dashboardUrl}/input/group`, editDashboardInputDto)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public editDashboardElement(
    aeditDashboardInputDto: EditDashboardInputDto
  ): Observable<DashboardInput> {
    this.spinner.show();
    return this.httpClient
      .put(`${this.dashboardUrl}/input/element`, aeditDashboardInputDto)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public deleteDashboardElement(
    editDashboardInputDto: EditDashboardInputDto
  ): Observable<DashboardInput> {
    this.spinner.show();
    return this.httpClient
      .delete(
        `${this.dashboardUrl}/input/element/${editDashboardInputDto.id}/${editDashboardInputDto.organizationId}`
      )
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public deleteDashboardGroup(
    editDashboardInputDto: EditDashboardInputDto
  ): Observable<DashboardInput> {
    this.spinner.show();
    return this.httpClient
      .delete(
        `${this.dashboardUrl}/input/group/${editDashboardInputDto.id}/${editDashboardInputDto.organizationId}`
      )
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public deleteDashboardTable(
    editDashboardInputDto: EditDashboardInputDto
  ): Observable<DashboardInput> {
    this.spinner.show();
    return this.httpClient
      .delete(
        `${this.dashboardUrl}/input/table/${editDashboardInputDto.id}/${editDashboardInputDto.organizationId}`
      )
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public saveDashboardInputValues(
    values: DashboardInputValue[]
  ): Observable<any> {
    this.spinner.show();
    return this.httpClient.post(`${this.dashboardUrl}/input`, values).pipe(
      map((data: any) => {
        return data;
      })
    );
  }

  public uploadFile(formData: FormData): Observable<DashboardInput> {
    this.spinner.show();
    return this.httpClient
      .post(`${this.dashboardUrl}/input/upload`, formData)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }
}
