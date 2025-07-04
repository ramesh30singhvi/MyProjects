import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { UsersComponent } from "./users.component";
import { RouterModule, Routes } from "@angular/router";
import { AgGridModule } from "ag-grid-angular";
import { RolesEnum } from "../models/roles.model";
import { AuthGuard } from "../auth.guard";
import { FormsModule } from "@angular/forms";
import { AddUserComponent } from "./add-user/add-user.component";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { UserActionsComponent } from "./user-actions/user-actions.component";
import { EditUserComponent } from "./edit-user/edit-user.component";
import { NgSelectModule } from "@ng-select/ng-select";
import { UserProductivityComponent } from "./user-productivity/user-productivity.component";
import { AuditsModule } from "../audits/audits.module";
import { AuditServiceApi } from "../services/audit-api.service";

const routes: Routes = [
  {
    path: "",
    component: UsersComponent,
    data: {
      roles: [RolesEnum.Admin],
    },
    canActivate: [AuthGuard],
  },
];

@NgModule({
  declarations: [
    UsersComponent,
    AddUserComponent,
    UserActionsComponent,
    EditUserComponent,
    UserProductivityComponent,
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    AgGridModule,
    FormsModule,
    NgbModule,
    NgSelectModule,
  ],
  providers: [AuditServiceApi],
})
export class UsersModule {}
