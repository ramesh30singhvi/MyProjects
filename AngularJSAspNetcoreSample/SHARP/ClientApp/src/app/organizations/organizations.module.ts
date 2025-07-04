import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrganizationsComponent } from './organizations.component';
import { RouterModule, Routes } from '@angular/router';
import { AgGridModule } from 'ag-grid-angular';
import { RolesEnum } from '../models/roles.model';
import { AuthGuard } from '../auth.guard';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { AddOrganizationComponent } from './add-organization/add-organization.component';
import { EditOrganizationComponent } from './edit-organization/edit-organization.component';
import { FacilitiesComponent } from './facilities/facilities.component';
import { OrganizationFormsComponent } from './organization-forms/organization-forms.component';
import { NgSelectModule } from '@ng-select/ng-select';
import { BtnFacilityCellRenderer } from './facilities/three-dot-cell-render/faicility-three-dots-cell.component';
import { RecipientsComponent } from './recipients/recipients.component';
import { TagInputModule } from 'ngx-chips';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { EditFacilityModalComponent } from './facilities/edit-facility-modal/edit-facility-modal.component';
import { NgxSpinnerModule } from 'ngx-spinner';
import { FormsGridActionsRenderer } from '../forms-management/forms-grid-actions/forms-grid-actions.component';
import { OrganizationFormsBtnCellRendererComponent } from './organization-forms/organization-forms-btn-cell-renderer/organization-forms-btn-cell-renderer.component';
import { ScheduleFormModalComponent } from './organization-forms/schedule-form-modal/schedule-form-modal.component';
import { FormServiceApi } from '../services/form-api.service';
import { AddEmailsModalComponent } from './facilities/add-emails-modal/add-emails-modal.component';

const routes: Routes = [
  {
    path: '',
    component: OrganizationsComponent,
    data: {
      roles: [RolesEnum.Admin]
    },
    canActivate: [AuthGuard]
  }
];

@NgModule({
  declarations: [
    OrganizationsComponent,
    AddOrganizationComponent,
    EditOrganizationComponent,
    FacilitiesComponent,
    OrganizationFormsComponent,
    BtnFacilityCellRenderer,
    RecipientsComponent,
    EditFacilityModalComponent,
    FormsGridActionsRenderer,
    OrganizationFormsBtnCellRendererComponent,
    ScheduleFormModalComponent,
    AddEmailsModalComponent,
    RecipientsComponent
  ],
  imports: [
    RouterModule.forChild(routes),
    AgGridModule.withComponents([BtnFacilityCellRenderer]),
    FormsModule,
    NgbModule,
    NgSelectModule,
    CommonModule,
    TagInputModule,
    ReactiveFormsModule,
    NgxSpinnerModule,
  ],
  providers: [
    FormServiceApi,
  ]
})
export class OrganizationsModule {}
