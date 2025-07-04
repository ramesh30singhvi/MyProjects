import { NgModule } from '@angular/core';
import { RouterModule, Routes } from "@angular/router";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { NgSelectModule } from "@ng-select/ng-select";
import { AuthGuard } from "../auth.guard";
import { RolesEnum } from "../models/roles.model";
import { DashboardService } from "../services/dashboard.service";
import { SharedModule } from "../shared.module";
import { NgApexchartsModule } from "ng-apexcharts";
import { OrganizationService } from "../services/organization.service";
import { FormServiceApi } from "../services/form-api.service";
import { NgxSpinnerModule } from "ngx-spinner";
import { ReactiveFormsModule } from "@angular/forms";
import { DashboardInputComponent } from "./dashboard-input.component";
import { AddDashboardInputComponent } from "./add/add-dashboard-input.component";
import { DeleteDashboardInputComponent } from "./delete/delete-dashboard-input.component";

const routes: Routes = [
  {
    path: "",
    data: {
      title: "Dashboard Input",
      urls: [],
      roles: [RolesEnum.Auditor, RolesEnum.Reviewer, RolesEnum.Admin, RolesEnum.Facility],
    },
    component: DashboardInputComponent,
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
    DashboardInputComponent,
    AddDashboardInputComponent,
    DeleteDashboardInputComponent,
  ],
  providers: [
    DashboardService,
    OrganizationService,
    FormServiceApi,
  ],
})
export class DashboardInputModule { }
