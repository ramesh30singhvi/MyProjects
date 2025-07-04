import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { NgSelectModule } from "@ng-select/ng-select";
import { AuthGuard } from "../auth.guard";
import { RolesEnum } from "../models/roles.model";
import { DashboardService } from "../services/dashboard.service";
import { SharedModule } from "../shared.module";
import { DashboardComponent } from "./dashboard.component";
import { NgApexchartsModule } from "ng-apexcharts";
import { OrganizationService } from "../services/organization.service";
import { FormServiceApi } from "../services/form-api.service";
import { TrimPipe } from "../common/pipes/trim.pipe";
import { RawInfoComponent } from "./raw-info/raw-info.component";
import { MemoComponent } from "./memo/memo.component";
import { MemoServiceApi } from "../services/memo-api.service";
import { MemoEditComponent } from "./memo/memo-edit/memo-edit.component";
import { NgxSpinnerModule } from "ngx-spinner";
import { ReactiveFormsModule } from "@angular/forms";

const routes: Routes = [
  {
    path: "",
    data: {
      title: "Dahboard",
      urls: [],
      roles: [RolesEnum.Auditor, RolesEnum.Reviewer, RolesEnum.Admin, RolesEnum.Facility],
      redirectTo: "/users",
    },
    component: DashboardComponent,
    canActivate: [AuthGuard],
  },
];

@NgModule({
  imports: [
    RouterModule.forChild(routes),
    NgbModule,
    SharedModule,
    NgSelectModule,
    NgApexchartsModule,
    NgxSpinnerModule,
    ReactiveFormsModule,
  ],
  declarations: [
    DashboardComponent,
    RawInfoComponent,
    MemoComponent,
    MemoEditComponent,
  ],
  providers: [
    DashboardService,
    OrganizationService,
    FormServiceApi,
    MemoServiceApi,
  ],
})
export class DashboardModule {}
