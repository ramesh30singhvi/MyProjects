import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsManagementComponent } from './forms-management.component';
import { RouterModule, Routes } from '@angular/router';
import { AgGridModule } from 'ag-grid-angular';
import { RolesEnum } from '../models/roles.model';
import { AuthGuard } from '../auth.guard';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FormsEditComponent } from './forms-edit/forms-edit.component';
import { NgSelectModule } from '@ng-select/ng-select';
import { AuditServiceApi } from '../services/audit-api.service';
import { FormServiceApi } from '../services/form-api.service';
import { FormService } from './services/form.service';
import { KeywordFormComponent } from './keyword-form/keyword-form.component';
import { TagInputModule } from 'ngx-chips';
import { CriteriaFormComponent } from './criteria-form/criteria-form.component';
import { DragulaModule } from 'ng2-dragula';
import { EditCrieriaFormModalComponent } from './edit-criteria-form-modal/edit-crieria-form-modal.component';
import { NgxSpinnerModule } from 'ngx-spinner';
import { EditSubheaderModalComponent } from './edit-subheader-modal/edit-subheader-modal.component';
import { SetSectionModalComponent } from './set-section-modal/set-section-modal.component';
import { TrackerFormComponent } from './tracker-form/tracker-form.component';
import { EditTrackerQuestionModalComponent } from './edit-tracker-question-modal/edit-tracker-question-modal.component';
import { EditFormModalComponent } from './edit-form-modal/edit-form-modal.component';
import { MdsFormComponent } from './mds-form/mds-form.component';
import { EditMdsSectionModalComponent } from './edit-mds-section-modal/edit-mds-section-modal.component';
import { EditMdsGroupModalComponent } from './edit-mds-group-modal/edit-mds-group-modal.component';
import { EditMdsQuestionModalComponent } from './edit-mds-question-modal/edit-mds-question-modal.component';
import { DuplicateFormModalComponent } from './duplicate-form-modal/duplicate-form-modal.component';
import { ReportsService } from '../services/reports.service';
import { AuthService } from '../services/auth.service';

const routes: Routes = [
  {
    path: '',
    component: FormsManagementComponent,
    data: {
      roles: [RolesEnum.Admin]
    },
    canActivate: [AuthGuard]
  },
  {
    path: 'new',
    data: {
      title: 'New Form',
      urls: [],
      roles: [RolesEnum.Admin],
      redirectTo: '/forms-management'
    },
    component: FormsEditComponent,
    canActivate: [AuthGuard],
  },
  {
    path: ':id',
    data: {
      title: 'Form',
      urls: [],
      roles: [RolesEnum.Admin],
      redirectTo: '/forms-management'
    },
    component: FormsEditComponent,
    canActivate: [AuthGuard],
  }
];

@NgModule({
  declarations: [
    FormsManagementComponent,
    FormsEditComponent,
    KeywordFormComponent,
    CriteriaFormComponent,
    TrackerFormComponent,
    EditCrieriaFormModalComponent,
    EditTrackerQuestionModalComponent,
    EditSubheaderModalComponent,
    SetSectionModalComponent,
    EditFormModalComponent,
    MdsFormComponent,
    EditMdsSectionModalComponent,
    EditMdsGroupModalComponent,
    EditMdsQuestionModalComponent,
    DuplicateFormModalComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    AgGridModule,
    FormsModule,
    NgbModule,
    NgSelectModule,
    ReactiveFormsModule,
    TagInputModule,
    DragulaModule,
    NgxSpinnerModule,
  ],
  providers: [
    FormServiceApi,
    FormService,
    AuditServiceApi,
    ReportsService,
    AuthService
  ],
})
export class FormsManagementModule { }
