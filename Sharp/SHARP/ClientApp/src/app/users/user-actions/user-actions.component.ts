import { Component, OnInit } from "@angular/core";
import { NgbModal, NgbModalRef } from "@ng-bootstrap/ng-bootstrap";
import { AgRendererComponent } from "ag-grid-angular";
import {
  ICellRendererParams,
  IAfterGuiAttachedParams,
  GridApi,
} from "ag-grid-community";
import { UserStatuses } from "src/app/models/users/users.model";
import { AddUserComponent } from "../add-user/add-user.component";
import { EditUserComponent } from "../edit-user/edit-user.component";
import {
  UserProductivityComponent,
  UserProductivityReportType,
} from "../user-productivity/user-productivity.component";

@Component({
  selector: "app-user-actions",
  templateUrl: "./user-actions.component.html",
  styleUrls: ["./user-actions.component.scss"],
})
export class UserActionsComponent implements AgRendererComponent {
  public userId: number;
  public status: any;

  private gridApi: GridApi;

  private modalRef: NgbModalRef;

  constructor(private modalService: NgbModal) {}

  refresh(params: ICellRendererParams): boolean {
    return true;
  }

  agInit(params: ICellRendererParams): void {
    this.gridApi = params.api;
    const data = params.data;

    if (data && data.id) {
      this.userId = data.id;
      this.status = this.getStatus(data.status);
    } else {
      return;
    }
  }

  afterGuiAttached?(params?: IAfterGuiAttachedParams): void {}

  public onUserViewClick(userId: number): void {
    this.modalRef = this.modalService.open(EditUserComponent, {
      modalDialogClass: "user-modal",
    });

    this.modalRef.componentInstance.userId = this.userId;

    this.modalRef.result.then(
      () => this.gridApi.onFilterChanged(),
      () => {}
    );
  }

  public onProductivityClicked(userId: number): void {
    this.modalRef = this.modalService.open(UserProductivityComponent, {
      modalDialogClass: "generate-excel-modal",
    });

    this.modalRef.componentInstance.userId = userId;
    this.modalRef.componentInstance.type = UserProductivityReportType.Single;
    this.modalRef.componentInstance.filterModel = undefined;
  }

  private getStatus(statusId: number) {
    const statuses = Object.values(UserStatuses);

    return statuses.find((status) => status.id === statusId);
  }
}
