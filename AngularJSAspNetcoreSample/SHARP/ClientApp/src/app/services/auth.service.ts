import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { first, Observable } from 'rxjs';
import { CheckSecureCodeResponse, UserData } from '../models/login.response';
import { environment } from 'src/environments/environment';
import { RolesEnum } from '../models/roles.model';
import { NgxSpinnerService } from 'ngx-spinner';
import { FORMS_MANAGER_COLUMNS_STATE, FORMS_MANAGER_SEARCH_TERM, FORMS_MANAGER_TABLE_FILTERS } from '../common/constants/formGrid';
import { UserService } from './user.service';
import { ActionTypeEnum } from '../models/users/users.model';
import { IOption } from '../models/audits/audits.model';
import { REPORT_REQUEST_TABLE_FILTERS } from '../report-request/common/report-request-grid.constants';

//import * as moment from 'moment-timezone';

@Injectable()
export class AuthService {
  loginUrl = environment.apiUrl + 'authenticate/login';
  clientPortalAccessUrl = environment.apiUrl + 'authenticate/clientPortalAccess';
  checkSecureCode2FaUrl = environment.apiUrl + 'authenticate/2fa';

  constructor(
    private httpClient: HttpClient,
    private userServiceApi: UserService,
    private spinner: NgxSpinnerService) { }

  public isAuthenticated(): boolean {
    const accessToken = localStorage.getItem('access_token');
    return accessToken !== undefined && accessToken !== null;
  }

  public isTokenExpired(): boolean {
    //const expires = localStorage.getItem('expiration');
    //const date = moment(expires).tz(timezoneName).toDate();
    //return !(date.valueOf() > moment.tz(timezoneName).toDate().valueOf());
    return false;
  }
  public hasToken(): boolean {
    return localStorage.getItem('access_token') != undefined;
  }

  public isUserInRole(role: RolesEnum) {
    let user = this.getCurrentUser();
    if (!user || !user.roles)
      return false;

    for (let i = 0; i < user.roles.length; i++) {
      if (user.roles[i] == role) {
        return true;
      }
    }
    return false;
  }

  public getCurrentUserId(): string {
    const user = this.getCurrentUser();

    return user?.userId;
  }

  public getCurrentUserName(): string {
    const user = this.getCurrentUser();

    return `${user.firstName} ${user.lastName}`;
  }

  public getCurrentUserSharpId(): number {
    const user = this.getCurrentUser();

    return user.id;
  }

  public getCurrentUserOrganizations(): IOption[] {
    const user = this.getCurrentUser();

    return user.organizations;
  }

  login(username: string, password: string): Observable<CheckSecureCodeResponse> {
    this.spinner.show();
    return this.httpClient.post<CheckSecureCodeResponse>(this.loginUrl, { username:username, password:password });
  }
  checkSecureCode(username: string, password: string, secureCode: string): Observable<CheckSecureCodeResponse> {
    this.spinner.show();
    return this.httpClient.post<CheckSecureCodeResponse>(this.checkSecureCode2FaUrl, { username: username, password: password, twoFactorCode: secureCode });
  }
  saveLoginResponse(response: CheckSecureCodeResponse) {
    localStorage.setItem('access_token', response.token);
    localStorage.setItem('expiration', response.expiration.toString());
    localStorage.setItem('userData', JSON.stringify(response.userData));
  }
  getCurrentUser(): UserData {
    return JSON.parse(localStorage.getItem('userData'));
  }
  getCurrentUserEmail(): string {
    const user = this.getCurrentUser();

    return user?.email;
  }
  logout(actionType: ActionTypeEnum = null) {

    if(actionType) {
      this.userServiceApi.addUserActivity(actionType).pipe(first()).subscribe();
    }

    // remove user from local storage to log user out
    localStorage.removeItem('access_token');
    localStorage.removeItem('audit-table-filters');

    localStorage.removeItem(FORMS_MANAGER_TABLE_FILTERS);
    localStorage.removeItem(FORMS_MANAGER_COLUMNS_STATE);
    localStorage.removeItem(FORMS_MANAGER_SEARCH_TERM);

    localStorage.removeItem(REPORT_REQUEST_TABLE_FILTERS);
  }

  clientPortalAccess(email: string, orgId: number, fName: string, lName: string,
    facilityName:string,
    code:string): Observable<CheckSecureCodeResponse> {
    this.spinner.show();
    return this.httpClient.post<CheckSecureCodeResponse>(this.clientPortalAccessUrl,
      {
        firstName: fName, lastName: lName,
        facilityName: facilityName, organizationId: orgId,
        emailToken: code, email: email
      });
  }
}
