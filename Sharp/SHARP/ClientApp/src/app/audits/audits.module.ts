import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { Routes, RouterModule } from "@angular/router";
import {AutocompleteLibModule} from 'angular-ng-autocomplete';

import { AuditsComponent } from "./audits.component";
import { AgGridModule } from "ag-grid-angular";
import { BtnCellRenderer } from "../component/three-dots-menu/three-dots-menu.component";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { NgbdratingBasicComponent } from "../component/rating/rating.component";
import { SharedModule } from "../shared.module";
import { AvatarModule } from "ngx-avatar";
import { DragulaModule } from "ng2-dragula";
import { FormsComponent } from "./forms/forms.component";
import { AuditsGridButtonsRendererComponent } from "./buttons/AuditsGridButtonsRenderer.component";
import { ReasonComponent } from "./reason/reason.component";
import { DisapproveAuditModalComponent } from "./disapprove-popup/DisapproveAuditModal.component";
import { RolesEnum } from "../models/roles.model";
import { AuthGuard } from "../auth.guard";
import { KeywordInputSectionComponent } from "./keyword-input-section/keyword-input-section.component";
import { TwentyHourKeywordsComponent } from "./twenty-hour-keywords/twenty-hour-keywords.component";
import { NgSelectModule } from "@ng-select/ng-select";
import { CriteriaComponent } from "./criteria/criteria.component";
import { ProgressNotesSectionComponent } from "./progress-notes-section/progress-notes-section.component";
import { HighlightPipe } from "../common/pipes/keyword";
import { SafeHtmlPipe } from "../common/pipes/safeHtml";
import { AuditServiceApi } from "../services/audit-api.service";
import { AuditService } from "./services/audit.service";
import { TrackerComponent } from "./tracker/tracker.component";
import { CriteriaPdfFilterPopupComponent } from "./criteria-pdf-filter-popup/criteria-pdf-filter-popup.component";
import { NgxSpinnerModule } from "ngx-spinner";
import { FormServiceApi } from "../services/form-api.service";
import { ControlService } from "../services/control.service";
import { DynamicFormControlComponent } from "../shared/dynamic-form-control/dynamic-form-control.component";
import { NgxEditorModule } from "ngx-editor";
import { AuditOptionsRendererComponent } from "./options/AuditOptionsRenderer.component";
import { NgxMaterialTimepickerModule } from "ngx-material-timepicker";
import { ReportRequestServiceApi } from "../services/report-request-api.service";
import { MDSComponent } from "./mds/mds.component";
import { AuditSubmitPopupComponent } from './audit-submit-popup/audit-submit-popup.component';

const routes: Routes = [
  {
    path: "",
    data: {
      title: "Audits",
      urls: [],
      roles: [RolesEnum.Auditor, RolesEnum.Reviewer, RolesEnum.Admin, RolesEnum.Facility],
      redirectTo: "/users",
    },
    component: AuditsComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "filtered/:state",
    data: {
      title: "Audits Archived",
      urls: [],
      roles: [RolesEnum.Auditor, RolesEnum.Reviewer, RolesEnum.Admin],
      redirectTo: "/users",
    },
    component: AuditsComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "filteredByStatus/:status",
    data: {
      title: "Audits",
      urls: [],
      roles: [RolesEnum.Auditor, RolesEnum.Reviewer, RolesEnum.Admin, RolesEnum.Facility],
      redirectTo: "/users",
    },
    component: AuditsComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "new",
    data: {
      title: "Audits",
      roles: [RolesEnum.Auditor],
      redirectTo: "/audits",
    },
    component: FormsComponent,
    canActivate: [AuthGuard],
  },
  {
    path: ":id",
    data: {
      title: "Audits",
      roles: [RolesEnum.Auditor, RolesEnum.Reviewer, RolesEnum.Admin, RolesEnum.Facility],
      redirectTo: "/users",
    },
    component: FormsComponent,
    canActivate: [AuthGuard],
    runGuardsAndResolvers: "always",
  },
];

@NgModule({
  imports: [
    NgbModule,
    FormsModule,
    CommonModule,
    RouterModule.forChild(routes),
    AgGridModule.withComponents([
      BtnCellRenderer,
      AuditsGridButtonsRendererComponent,
      ReasonComponent,
    ]),
    SharedModule,
    AvatarModule,
    DragulaModule,
    ReactiveFormsModule,
    NgSelectModule,
    NgxSpinnerModule,
    NgxEditorModule,
    NgxMaterialTimepickerModule,
    AutocompleteLibModule
  ],
  declarations: [
    AuditsComponent,
    BtnCellRenderer,
    FormsComponent,
    AuditsGridButtonsRendererComponent,
    AuditOptionsRendererComponent,
    ReasonComponent,
    DisapproveAuditModalComponent,
    TwentyHourKeywordsComponent,
    KeywordInputSectionComponent,
    CriteriaComponent,
    TrackerComponent,
    ProgressNotesSectionComponent,
    HighlightPipe,
    SafeHtmlPipe,
    CriteriaPdfFilterPopupComponent,
    DynamicFormControlComponent,
    MDSComponent,
    AuditSubmitPopupComponent
  ],
  providers: [
    AuditServiceApi,
    AuditService,
    FormServiceApi,
    ControlService,
    ReportRequestServiceApi,
  ],
  bootstrap: [NgbdratingBasicComponent],
})
export class AuditsModule {}
