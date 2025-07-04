import { Component, Input, ViewEncapsulation } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { DashboardService } from "src/app/services/dashboard.service";
import { AddType } from "../add/add-dashboard-input.component";
import { first } from "rxjs-compat/operator/first";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "add-dashboard-input",
  templateUrl: "./delete-dashboard-input.component.html",
  styleUrls: ["./delete-dashboard-input.component.scss"],
  encapsulation: ViewEncapsulation.None,
})
export class DeleteDashboardInputComponent {
  @Input() organizationId: number;
  @Input() id: number | undefined;
  @Input() type: AddType = AddType.Table;
  @Input() name: string | undefined;

  constructor(
    public dashboardService: DashboardService,
    public activeModal: NgbActiveModal,
    private toastr: ToastrService
  ) {}

  public onClose(): void {
    this.activeModal.dismiss();
  }

  public delete(): void {
    switch (this.type) {
      case AddType.Table: {
        this.dashboardService
          .deleteDashboardTable({
            id: this.id,
            organizationId: this.organizationId,
            name: this.name,
            keyword: undefined,
          })
          .pipe()
          .subscribe((response) => {
            this.activeModal.dismiss(response);
            this.toastr.success("Table removed successully");
          });
        return;
      }
      case AddType.Header: {
        this.dashboardService
          .deleteDashboardGroup({
            id: this.id,
            organizationId: this.organizationId,
            name: this.name,
            keyword: undefined,
          })
          .pipe()
          .subscribe((response) => {
            this.activeModal.dismiss(response);
            this.toastr.success("Group removed successully");
          });
        return;
      }
      case AddType.Column: {
        this.dashboardService
          .deleteDashboardElement({
            id: this.id,
            organizationId: this.organizationId,
            name: this.name,
            keyword: undefined,
          })
          .pipe()
          .subscribe((response) => {
            this.activeModal.dismiss(response);
            this.toastr.success("Column removed successully");
          });
      }
    }
  }
}
