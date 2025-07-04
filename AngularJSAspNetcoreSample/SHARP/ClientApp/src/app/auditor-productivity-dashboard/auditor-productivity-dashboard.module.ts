import { CUSTOM_ELEMENTS_SCHEMA,NgModule } from '@angular/core';
import { RouterModule, Routes } from "@angular/router";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { NgSelectModule } from "@ng-select/ng-select";
import { AuthGuard } from "../auth.guard";
import { RolesEnum } from "../models/roles.model";
import { SharedModule } from "../shared.module";
import { NgApexchartsModule } from "ng-apexcharts";
import { NgxSpinnerModule } from "ngx-spinner";
import { ReactiveFormsModule } from "@angular/forms";
import { AgGridModule } from "ag-grid-angular";
import { AuditorProductivityDashboardComponent } from './auditor-productivity-dashboard.component';
import { AuditorProductivityDashboardService } from '../services/auditor-productivity-dashboard.service';
import { OrganizationService } from '../services/organization.service';
import { SummaryFacilityComponent } from './summary-facility/summary-facility.component';
import { CommonModule } from '@angular/common';
import { UserService } from '../services/user.service';


const routes: Routes = [
  {
    path: "",
    data: {
      title: "Auditor Productivity",
      urls: [],
      roles: [RolesEnum.Admin],
    },
    component: AuditorProductivityDashboardComponent,
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
    AgGridModule,
    NgxSpinnerModule,
    CommonModule
  ],
  declarations: [
    AuditorProductivityDashboardComponent,
    SummaryFacilityComponent
  ],
  providers: [
    AuditorProductivityDashboardService, OrganizationService,UserService
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class AuditorProductivityDashboardModule { }
