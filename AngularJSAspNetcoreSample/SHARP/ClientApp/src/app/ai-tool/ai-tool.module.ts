import { CommonModule } from "@angular/common";
import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";
import { NgbAccordionModule, NgbModule, NgbNavModule } from "@ng-bootstrap/ng-bootstrap";
import { NgSelectModule } from "@ng-select/ng-select";
import { AgGridModule } from "ag-grid-angular";
import { NgApexchartsModule } from "ng-apexcharts";
import { NgxSpinnerModule } from "ngx-spinner";
import { AuthGuard } from "../auth.guard";
import { RolesEnum } from "../models/roles.model";
import { FacilityService } from "../services/facility.service";
import { FormServiceApi } from "../services/form-api.service";
import { OrganizationService } from "../services/organization.service";
import { ReportAIService } from "../services/reportAI.service";
import { ReportsService } from "../services/reports.service";
import { AiReportsComponent } from './ai-reports/ai-reports.component';
import { EditAiReportComponent } from './edit-ai-report/edit-ai-report.component';
import { CreateAiReportComponent } from './create-ai-report/create-ai-report.component';
import { KeywordAIReportService } from "../reports/keyword-ai-report/services/keyword-ai-report.service";
import { AuthService } from "../services/auth.service";


const routes: Routes = [
  {
    path: "",
    data: {
      title: "AI TOOL V.2",
      urls: [],
      roles: [RolesEnum.Auditor, RolesEnum.Reviewer, RolesEnum.Admin, RolesEnum.Facility],
      redirectTo: "/",
    },
    component: AiReportsComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "createAIAudit",
    data: {
      title: "Reports using AI",
      urls: [],
      roles: [RolesEnum.Admin, RolesEnum.Auditor, RolesEnum.Reviewer],
      redirectTo: "/",
    },
    component: CreateAiReportComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "editAIAudit/:id",
    data: {
      title: "Edit AI Audits",
      urls: [],
      roles: [RolesEnum.Admin, RolesEnum.Auditor, RolesEnum.Reviewer],
      redirectTo: "",
    },
    component: EditAiReportComponent,
    canActivate: [AuthGuard],
  },
]

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    NgApexchartsModule,
    AgGridModule,
    FormsModule,
    NgbModule,
    NgSelectModule,
    ReactiveFormsModule,
    NgbAccordionModule,
    NgxSpinnerModule,
    NgbNavModule,
  ],
  declarations: [
    AiReportsComponent,
    EditAiReportComponent,
    CreateAiReportComponent,
  ],
  providers: [ReportsService, FormServiceApi, OrganizationService, FacilityService, ReportAIService, KeywordAIReportService, AuthService],

  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class AiToolModule { }
