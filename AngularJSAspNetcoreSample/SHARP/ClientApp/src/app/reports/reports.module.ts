import { CommonModule } from "@angular/common";
import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";
import { NgbAccordionModule, NgbModule, NgbNavModule } from "@ng-bootstrap/ng-bootstrap";
import { NgSelectModule } from "@ng-select/ng-select";
import { AgGridModule } from "ag-grid-angular";
import { NgApexchartsModule } from "ng-apexcharts";
import { AuthGuard } from "../auth.guard";
import { RolesEnum } from "../models/roles.model";
import { ReportsService } from "../services/reports.service";
import { FallReportComponent } from "./customreports/fall-report.component";
import { ReportsDetails } from "./report-details/report-details.component";
import { ReportsComponent } from "./reports.component";
import { TableauComponent } from "./tableau/tableau.component";
import { WoundReportComponent } from "./customreports/wound-report.component";
import { CriteriaReportComponent } from "./customreports/criteria-report.component";
import { FormServiceApi } from "../services/form-api.service";
import { ReportAiComponent } from './report-ai/report-ai.component';
import { OrganizationService } from "../services/organization.service";
import { FacilityService } from "../services/facility.service";
import { PortalReportComponent } from './portal-report/portal-report.component';
import { AddKeywordReportComponent } from './add-keyword-report/add-keyword-report.component';
import { NgxSpinnerModule } from "ngx-spinner";
import { ReportAIService } from "../services/reportAI.service";
import { KeywordAiReportComponent } from './keyword-ai-report/keyword-ai-report.component';
import { KeywordAIReportService } from "./keyword-ai-report/services/keyword-ai-report.service";
import { StatusAiReportComponent } from './keyword-ai-report/status-ai-report/status-ai-report.component';
import { EditAiAuditComponent } from './keyword-ai-report/edit-ai-audit/edit-ai-audit.component';
import { AiAuditGridActionsComponent } from './keyword-ai-report/ai-audit-grid-actions/ai-audit-grid-actions.component';
import { PortalService } from "../services/portal.service";


const routes: Routes = [
  {
    path: "criteria",
    data: {
      title: "Reports",
      urls: [],
      roles: [RolesEnum.Auditor, RolesEnum.Reviewer, RolesEnum.Admin, RolesEnum.Facility],
      redirectTo: "/",
    },
    component: CriteriaReportComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "fall",
    data: {
      title: "Reports",
      urls: [],
      roles: [RolesEnum.Auditor, RolesEnum.Reviewer, RolesEnum.Admin, RolesEnum.Facility],
      redirectTo: "/",
    },
    component: FallReportComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "wound",
    data: {
      title: "Reports",
      urls: [],
      roles: [RolesEnum.Auditor, RolesEnum.Reviewer, RolesEnum.Admin, RolesEnum.Facility],
      redirectTo: "/",
    },
    component: WoundReportComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "",
    data: {
      title: "Reports",
      urls: [],
      roles: [RolesEnum.Auditor, RolesEnum.Reviewer, RolesEnum.Admin, RolesEnum.Facility],
      redirectTo: "/",
    },
    component: ReportsComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "portalReports",
    data: {
      title: "Test Protal Reports",
      urls: [],
      roles: [RolesEnum.Admin],
      redirectTo: "/",
    },
    component: PortalReportComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "processAIReport",
    data: {
      title: "Reports using AI",
      urls: [],
      roles: [RolesEnum.Admin, RolesEnum.Auditor, RolesEnum.Reviewer],
      redirectTo: "/",
    },
    component: ReportAiComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "aiAudits",
    data: {
      title: "AI Audits",
      urls: [],
      roles: [RolesEnum.Admin, RolesEnum.Auditor, RolesEnum.Reviewer],
      redirectTo: "/",
    },
    component: KeywordAiReportComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "aiAudits/filtered/:state",
    data: {
      title: "AI Audits",
      urls: [],
      roles: [RolesEnum.Admin, RolesEnum.Auditor, RolesEnum.Reviewer],
      redirectTo: "/",
    },
    component: KeywordAiReportComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "editAIAudit/:id",
    data: {
      title: "Edit AI Audits",
      urls: [],
      roles: [RolesEnum.Admin, RolesEnum.Auditor, RolesEnum.Reviewer],
      redirectTo: "/aiAudits",
    },
    component: EditAiAuditComponent,
    canActivate: [AuthGuard],
  },
  {
    path: ":id",
    data: {
      title: "Reports",
      urls: [],
      roles: [RolesEnum.Auditor, RolesEnum.Reviewer, RolesEnum.Admin, RolesEnum.Facility],
      redirectTo: "/",
    },
    component: ReportsDetails,
    canActivate: [AuthGuard],
  },
];

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
    ReportsComponent,
    ReportsDetails,
    TableauComponent,
    FallReportComponent,
    WoundReportComponent,
    CriteriaReportComponent,
    ReportAiComponent,
    PortalReportComponent,
    AddKeywordReportComponent,
    KeywordAiReportComponent,
    StatusAiReportComponent,
    EditAiAuditComponent,
    AiAuditGridActionsComponent,
  ],
  exports: [AddKeywordReportComponent] ,
  providers: [ReportsService, FormServiceApi, OrganizationService, FacilityService, ReportAIService, KeywordAIReportService, PortalService],

  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class ReportsModule {}
