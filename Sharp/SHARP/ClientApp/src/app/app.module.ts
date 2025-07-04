import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import {
  HttpClientModule,
  HttpClient,
  HTTP_INTERCEPTORS,
} from "@angular/common/http";
import { RouterModule } from "@angular/router";

import { NgbModule } from "@ng-bootstrap/ng-bootstrap";

import { FeatherModule } from "angular-feather";
import { allIcons } from "angular-feather/icons";

import { FullComponent } from "./layouts/full/full.component";
// import { BlankComponent } from './layouts/blank/blank.component';

import { VerticalNavigationComponent } from "./shared/vertical-header/vertical-navigation.component";
import { VerticalSidebarComponent } from "./shared/vertical-sidebar/vertical-sidebar.component";
import { HorizontalNavigationComponent } from "./shared/horizontal-header/horizontal-navigation.component";
import { HorizontalSidebarComponent } from "./shared/horizontal-sidebar/horizontal-sidebar.component";

import { Approutes } from "./app-routing.module";
import { AppComponent } from "./app.component";
import { SpinnerComponent } from "./shared/spinner.component";
import { LoginComponent } from "./login/login.component";
import { AuthGuard } from "./auth.guard";

import { PerfectScrollbarModule } from "ngx-perfect-scrollbar";
import { PERFECT_SCROLLBAR_CONFIG } from "ngx-perfect-scrollbar";
import { PerfectScrollbarConfigInterface } from "ngx-perfect-scrollbar";

import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";
import { AuthService } from "./services/auth.service";
import { TokenInterceptor } from "./interceptors/token.interceptor";
import { StudyService } from "./services/study.service";
import { AvatarModule } from "ngx-avatar";
import "ag-grid-enterprise";

import { NgIdleKeepaliveModule } from "@ng-idle/keepalive"; // this includes the core NgIdleModule but includes keepalive providers for easy wireup

import { MomentModule } from "angular2-moment"; // optional, provides moment-style pipes for date formatting
import { UserService } from "./services/user.service";
import { DragulaModule } from "ng2-dragula";
import { LocalStorageService } from "./services/local-storage.service";
import { NgxSpinnerModule } from "ngx-spinner";
import { SpinnerInterceptorService } from "./interceptors/spinner.interceptor";
import { CodeValidationComponent } from "./shared/code-validation.component";
import { ConfirmationDialogComponent } from "./shared/confirmation-dialog/confirmation-dialog.component";
import { OrganizationService } from "./services/organization.service";
import { FacilityService } from "./services/facility.service";
import { ToastrModule } from "ngx-toastr";
import { SimpleAlertDialogComponent } from "./shared/simple-alert-dialog/simple-alert-dialog.component";


export function HttpLoaderFactory(http: HttpClient) {
  return new TranslateHttpLoader(http, "./assets/i18n/", ".json");
}

const DEFAULT_PERFECT_SCROLLBAR_CONFIG: PerfectScrollbarConfigInterface = {
  suppressScrollX: true,
  wheelSpeed: 1,
  wheelPropagation: true,
  minScrollbarLength: 20,
};

@NgModule({
  declarations: [
    AppComponent,
    SpinnerComponent,
    FullComponent,
    VerticalNavigationComponent,
    VerticalSidebarComponent,
    HorizontalNavigationComponent,
    HorizontalSidebarComponent,
    LoginComponent,
    CodeValidationComponent,
    ConfirmationDialogComponent,
    SimpleAlertDialogComponent,
  ],
  imports: [
    CommonModule,
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    NgbModule,
    FeatherModule.pick(allIcons),
    FeatherModule,
    RouterModule.forRoot(Approutes),
    PerfectScrollbarModule,
    HttpClientModule,
    AvatarModule,
    MomentModule,
    NgIdleKeepaliveModule.forRoot(),
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient],
      },
    }),
    DragulaModule.forRoot(),
    NgxSpinnerModule,
    ToastrModule.forRoot({
      timeOut: 8000,
      positionClass: "toast-top-center",
      //preventDuplicates: true,
    }),
  ],
  providers: [
    AuthService,
    StudyService,
    AuthGuard,
    UserService,
    {
      provide: PERFECT_SCROLLBAR_CONFIG,
      useValue: DEFAULT_PERFECT_SCROLLBAR_CONFIG,
    },
    LocalStorageService,
    { provide: HTTP_INTERCEPTORS, useClass: TokenInterceptor, multi: true },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: SpinnerInterceptorService,
      multi: true,
    },
    OrganizationService,
    FacilityService,
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
