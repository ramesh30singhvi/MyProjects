import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { SPINNER_TYPE } from '../common/constants/audit-constants';
import { CheckSecureCodeResponse } from '../models/login.response';
import { AuthService } from '../services/auth.service';
import { MyserviceService } from './../myservice.service';
import { UserService } from "../services/user.service";
import { ActionTypeEnum } from '../models/users/users.model';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  providers: [MyserviceService]
})
export class LoginComponent implements OnInit, OnDestroy {
  msg = '';
  @ViewChild('p2') password;
  @ViewChild('passwordToggler') passwordToggler;
  @ViewChild('sc') secureCode;
  loginform = true;
  recoverform = false;
  secureCodeForm = false;
  secureCodeFormMessage = false;
  username: string;
  pwd: string;

  code: string = '';

  public spinnerType: string;

  private subscription: Subscription;

  constructor(private authService: AuthService, private router: Router, private route: ActivatedRoute, private titleService: Title, private userServiceApi: UserService) {
    if (authService.isAuthenticated()) {
      this.router.navigate(['/']);
    }

    this.subscription = route.queryParams.subscribe(
      (queryParam: any) => {
          const username = queryParam['user'];
          const code = queryParam['code'];

          if(username && code){
            this.username = username;
            this.code = code;
            this.secureCodeForm = true;
            this.secureCodeFormMessage = false;
          }

          this.router.navigate(
            ['.'], 
            { relativeTo: this.route }
          );
      }
    );

    this.spinnerType = SPINNER_TYPE;
  }

  ngOnInit(): void {
    this.titleService.setTitle("SHARP");
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  togglePassword() {
    const type = this.password.nativeElement.getAttribute('type') === 'password' ? 'text' : 'password';
    this.password.nativeElement.setAttribute('type', type);
    this.passwordToggler.nativeElement.className = type === 'password' ? 'fas fa-eye' : 'fas fa-eye-slash';
  }

  showRecoverForm() {
    this.loginform = !this.loginform;
    this.recoverform = !this.recoverform;
  }

  onUserPasswordChange() {
    this.msg = '';
  }

  onSecureCodeChange() {
    this.msg = '';
  }

  showLoginForm() {
    this.loginform = true;
    this.recoverform = false;
    this.secureCodeForm = false;
    this.secureCodeFormMessage = false;
    this.username = null;
    this.pwd = null;
    this.code = null;
    this.msg = '';
  }

  public login(username: string, password: string) {
    this.authService.login(username, password).subscribe(data => this.processAfterLogin(data), ({ error }) => {
        if (error === '2FA required') {
            this.secureCodeForm = true;
            this.secureCodeFormMessage = true;
            this.username = username;
            this.pwd = password;
        }
        else {
          console.log(error);
          this.msg = error != "" ? error : 'Username and/or password is incorrect';
          this.userServiceApi.addUserActivityLog(ActionTypeEnum.FailedLogin, null, null, username).subscribe();
        }
    });
  }

  public enterSecureCodeSubmit(secureCode) {
    this.authService.checkSecureCode(this.username, this.pwd, secureCode).subscribe(data => {
      if (data && data.token) {
        this.processAfterLogin(data);
      }
      else {
        this.msg = 'Incorrect secure code';
        this.userServiceApi.addUserActivityLog(ActionTypeEnum.FailedLogin, null, null, this.username).subscribe();
      }
    }, error => {
      this.msg = 'Incorrect secure code';
      this.userServiceApi.addUserActivityLog(ActionTypeEnum.FailedLogin, null, null, this.username).subscribe();
    });
  }

  processAfterLogin(response: CheckSecureCodeResponse) {
    if (response.token) {
      this.authService.saveLoginResponse(response);
      const returnUrl = localStorage.getItem('redirectTo');
      if (returnUrl != undefined) {
        localStorage.removeItem('redirectTo');
        this.router.navigateByUrl(returnUrl);
      }
      else
        this.router.navigate(['/']);
    }
  }
}
