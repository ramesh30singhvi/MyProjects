import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { NgbDateAdapter, NgbDateParserFormatter, NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { NotifierModule } from 'angular-notifier';
import { DragulaModule } from 'ng2-dragula';
import { AuditStatusButtonsComponent } from './shared/audit-status-buttons/audit-status-buttons.component';

import { BreadcrumbComponent } from './shared/breadcrumb/breadcrumb.component';
import { CustomDateParserFormatter } from './shared/datepicker-adapters';
import { GridColumnsComponent } from './shared/grid-columns/grid-columns.component';

@NgModule({
  imports: [
    NgbModule,
    CommonModule,
    RouterModule,
    NotifierModule,
    DragulaModule,
  ],
  declarations: [
    BreadcrumbComponent,
    AuditStatusButtonsComponent,
    GridColumnsComponent,
  ],
  exports: [
    BreadcrumbComponent,
    CommonModule,
    FormsModule,
    NotifierModule,
    AuditStatusButtonsComponent,
    GridColumnsComponent,
  ],
  providers: [
    { 
      provide: NgbDateParserFormatter, 
      useClass: CustomDateParserFormatter 
    }
  ]
})
export class SharedModule { }
