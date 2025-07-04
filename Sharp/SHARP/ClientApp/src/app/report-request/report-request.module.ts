import { CommonModule } from '@angular/common';
import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { NgSelectModule } from '@ng-select/ng-select';
import { AgGridModule } from 'ag-grid-angular';
import { DragulaModule } from 'ng2-dragula';
import { NgxSpinnerModule } from 'ngx-spinner';
import { AuditService } from '../audits/services/audit.service';
import { AuthGuard } from '../auth.guard';
import { RolesEnum } from '../models/roles.model';
import { AuditServiceApi } from '../services/audit-api.service';
import { FormServiceApi } from '../services/form-api.service';
import { ReportRequestServiceApi } from '../services/report-request-api.service';
import { SharedModule } from '../shared.module';
import { DownloadGridButtonComponent } from './download-grid-button/download-grid-button.component';
import { ReportRequestComponent } from './report-request.component';
import { StatusRequestComponent } from './status-request-component/status-request-component';

const routes: Routes = [
    {
        path: '',
        data: {
        title: 'Report Requests',
        urls: [],
        roles: [RolesEnum.Reviewer, RolesEnum.Admin, RolesEnum.Facility],
        redirectTo: '/'
        },
        component: ReportRequestComponent,
        canActivate: [AuthGuard],
    },
];

@NgModule({
    imports: [
        NgbModule,
        RouterModule.forChild(routes),
        CommonModule,
        SharedModule,
        AgGridModule,
        ReactiveFormsModule,
        NgSelectModule,
        NgxSpinnerModule,
    ],
    declarations: [
        ReportRequestComponent,
        DownloadGridButtonComponent,
        StatusRequestComponent,
    ],
    providers: [
        ReportRequestServiceApi,
        AuditServiceApi,
        AuditService,
        FormServiceApi,
    ],
    schemas: [ 
        CUSTOM_ELEMENTS_SCHEMA 
    ],
})
export class ReportRequestModule {}