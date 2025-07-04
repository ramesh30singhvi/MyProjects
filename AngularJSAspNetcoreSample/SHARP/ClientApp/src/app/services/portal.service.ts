import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { NgxSpinnerService } from "ngx-spinner";
import { first } from "rxjs";
import { environment } from "../../environments/environment";
import { transformDate, transformNumber } from "../common/helpers/dates-helper";
import { IFilterOption, ReportFiltersModel } from "../models/audits/audit.filters.model";
import { IFacilityOption, IOption } from "../models/audits/audits.model";
import { IFacilityDetails } from "../models/organizations/facility.model";
import { EditUser, PortalEditUser } from "../models/users/users.model";

@Injectable()
export class PortalService {

  clientHasAccessFromEmailURL = environment.apiUrl + "clientportal/hasAccess"

  public hasAccess(facilityName: string, code: string) {
 
    this.spinner.show();
    return this.httpClient.post<boolean>(this.clientHasAccessFromEmailURL, {
      facilityname: facilityName,
      password: code,
    });
  }
  getHighAlertStatuses() {
    return this.httpClient
      .get<IOption[]>(`https://localhost:5001/api/clientportal/highAlertStatuses`);
  }

  getFacilityInfo() {
    return this.httpClient
      .post<IFacilityDetails>(`https://localhost:5001/api/clientportal/facilityInfo`, { id: 0, name: "United HealthCare" });
  }
  public getReportCategories() {

    return this.httpClient
      .get<IOption[]>(`https://localhost:5001/clientportal/reportCategories`);

  }

  private highAlertURL = `${environment.apiUrl}highalert`;
  public downloadReportHighAlert(highAlertId: number, reportName: string) {
    this.spinner.show();
    this.httpClient
      .post(
        `${this.highAlertURL}/downloadPDFReport`,
        { id: 10384, name: "'" },
        {
          headers: { Accept: 'application/pdf' },
          responseType: 'blob',
        },
      )
      .subscribe((data: any) => {
        if (reportName == null)
          reportName = "TestHighAlert";
        this.downloadOpenPdf(data, reportName);
      });
  }

  public downloadPdfReport(reportId: string, name: string) {

    this.httpClient.post(

      `${this.clientportaURL}downloadPDFReport`,

      { "selectedIds": [reportId] },

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
 
  public getHighAlertStatistics(facilityId: number) {
    this.spinner?.show();
    return this.httpClient.post(this.HighAlerttStatisticsURL, {
      "id": facilityId, "name": ""
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
  private portalCreateUserURL = `${environment.apiUrl}portalusers`;
  private portalRolesURL = `${environment.apiUrl}portalusers/roles`;
  private auditMigrateToReport = `${environment.apiUrl}reportManagement/admin/migrate`;
  private reportManagementURL = `${environment.apiUrl}reportManagement/admin`;
  private reportManagementSendReportsURL = `${environment.apiUrl}reportManagement/admin/sendReports`;
  private reportManagementExportFacilityEmailsURL = `${environment.apiUrl}reportManagement/admin/exportRecipients`;
  private reportRangesUrl = `${environment.apiUrl}reports/reportRanges`;
  private clientportalReportsURL = `${environment.apiUrl}clientportal/getReports`;
  private clientportaURL = `${environment.apiUrl}clientportal/`;
  private HighAlerttURL = `${environment.apiUrl}highAlert`;
  private HighAlerttStatisticsURL = `${environment.apiUrl}highAlert/statistics`;

  private portalUsersUrl = `${environment.apiUrl}portalusers`;


  constructor(
    private httpClient: HttpClient,
    private spinner: NgxSpinnerService
  ) { }
  editUser(editUser: PortalEditUser) {
    return this.httpClient.put(this.portalUsersUrl, editUser);
  }
  public getPortalRoles() {
    return this.httpClient.get(this.portalCreateUserURL + "/details/789");
    //return this.httpClient.post(this.portalCreateUserURL + "/changePassword", {
    //  "email": "inessabarkan+155@gmail.com", "password": "Fef335#Ac8cVIN8Nq1GSa2!3",
    //  "token": "CfDJ8BjSOa/3QtdHhw1STfJqZo+MUB8EaA2K0PB2v7xcDIDNVOox+62fOxpOPGjZkrghPogVYfHnOayTCTTIVnDIPxe7mA9SnWzUBBISeTEUUd6l/RJhJTA/mlIGVTT8RO/snk+shxi+pBR/dWrAZYsXl/KEkskX6A+4tTbppWV7jQgPdylvPMx7OyZTthqau4Kt96mkRn6T+kXCNXI6HuvOhl8shwKHCsKY47nV5QkQJnN5"
    //})
   // return this.httpClient.get(this.portalRolesURL);
  }
  public createUser(email: string, pass: string, orgId: number, facId: number) {
    return this.httpClient.post(this.portalCreateUserURL + "/moveRecip", {
      email: email,
      password: pass,
      organization: { id: orgId, name :"" },
      role: { id: 5, name: "" },

      facilityUnlimited:true,
      facilities: []
    });
  }
  public resetPassword() {
    this.spinner?.show();
    return this.httpClient.post(this.portalCreateUserURL + "/resetPassword", {
      "email": "inessabarkan+155@gmail.com"
    });
  }
  public migrateToReport() {
    this.spinner?.show();
    return this.httpClient.post(this.auditMigrateToReport, {});
  }

  public sendReports(ids: any[], useids: any[]) {
    this.spinner?.show();
    return this.httpClient.post(this.reportManagementSendReportsURL, {
      "selectedIds": ids, "userEmails": useids, "message": "Hello my friends"
    });
  }

  public getReportForUser(userEmail, password) {

    this.spinner.show();
    return this.httpClient.post<{ totalCount: number; items: any[] }>(this.clientportalReportsURL, { username: userEmail, password: password });

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
    //let f = [];
    //f.push({ "id": 771, "value": "Bonifay Nursing and Rehab Center" })
    var filters = { "search": "", "skipCount": 25, "takeCount": 25, "orderBy": "Date", "sortOrder": "desc", "organization": { "id": 52, "name": "Southern Healthcare Management", "keyword": null, "formId": null, "hidden": null }, "facilities": [], "facility": {} }

    //{ "organization": { "id": 52, "value": "Southern Healthcare Management" }, "facility": {  } };// "reportCategory": [{ "id": 1, "value": "Clinical" }] }; //"date": "{\"firstCondition\":{\"from\":\"2024-09-28\",\"to\":\"\",\"type\":\"equals\"},\"secondCondition\":null}" };

    

    return this.httpClient.post<{ totalCount: number; items: any[] }>(
      `api/clientportal/facilityReportsByPage`,
      {
        ...searchParams,
        ...filters,
      }
    );
    //return this.httpClient.post<{ totalCount: number; items: any[] }>(
    //  `${this.reportManagementURL}/getByPage`,
    //  {
    //    ...searchParams,
    //    ...filters,
    //  }
    //);

  }
  getPortalFilterModelParams(filterModel: any, reportFilterValues: ReportFiltersModel) {
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

            const filterValues: IFilterOption[] = reportFilterValues?.[key];

            filterParams[key] = filterValues?.filter((fv: IFilterOption) =>
              filter.values.includes(fv.value)
            );
          }
      }
    });

    return filterParams;
  }
}
