import { Component, Input, OnInit, ViewEncapsulation } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { ToastrService } from "ngx-toastr";
import { first } from "rxjs";
import { DashboardService } from "src/app/services/dashboard.service";

export enum AddType {
  Table = "Table",
  Header = "Header",
  Column = "Column",
}

@Component({
  selector: "add-dashboard-input",
  templateUrl: "./add-dashboard-input.component.html",
  styleUrls: ["./add-dashboard-input.component.scss"],
  encapsulation: ViewEncapsulation.None,
})
export class AddDashboardInputComponent implements OnInit {
  @Input() organizationId: number;
  @Input() tableId: number | undefined;
  @Input() groupId: number | undefined;
  @Input() id: number | undefined;
  @Input() formId: number | undefined;
  @Input() type: AddType = AddType.Table;
  @Input() name: string | undefined;
  @Input() keyword: string | undefined;

  public errorMessage: string;
  public showKeyword: boolean = false;

  public selectedForm: any | undefined;
  public availableForms: any[] = [];

  constructor(
    public dashboardService: DashboardService,
    public activeModal: NgbActiveModal,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    if (this.type == AddType.Column) {
      this.showKeyword = true;
    }
    this.fetchForms();
  }

  private fetchForms() {
    this.dashboardService.getOrganizationForms(this.organizationId).pipe(first()).subscribe((forms) => {
        this.availableForms = forms
        if (this.formId) {
          let form = this.availableForms.find(f => f.formId == this.formId);
          if (form) {
            this.selectedForm = form
          }
        }
    })
  }

  public onClose(): void {
    this.activeModal.dismiss();
  }

  public onNameChanged(): void {
    if (this.errorMessage) {
      this.errorMessage = null;
    }
  }

  public onKeywordChanged(): void {
    if (this.errorMessage) {
      this.errorMessage = null;
    }
  }

  public save(): void {
    console.log("Selected form", this.selectedForm);
    if (!this.name) {
      this.errorMessage = "Please enter the name";
    } else if (this.type == AddType.Column && !this.keyword) {
      this.errorMessage = "Please enter the keyword to extract data from PDF";
    } else if (this.type == AddType.Column && !this.selectedForm) {
      this.errorMessage = "Please select the form";
    } else {
      switch (this.type) {
        case AddType.Table: {
          if (this.id) {
            this.dashboardService
              .editDashboardTable({
                id: this.id,
                organizationId: this.organizationId,
                name: this.name,
                keyword: this.keyword,
              })
              .pipe()
              .subscribe((response) => {
                this.activeModal.dismiss(response);
                this.toastr.success("Table updated successully");
              });
          } else {
            this.dashboardService
              .addDashboardTable({
                organizationId: this.organizationId,
                name: this.name,
              })
              .pipe()
              .subscribe((response) => {
                this.activeModal.dismiss(response);
                this.toastr.success("Table added successully");
              });
          }
          return;
        }
        case AddType.Header: {
          if (this.id) {
            this.dashboardService
              .editDashboardGroup({
                id: this.id,
                organizationId: this.organizationId,
                name: this.name,
                keyword: this.keyword,
              })
              .pipe()
              .subscribe((response) => {
                this.activeModal.dismiss(response);
                this.toastr.success("Group updated successully");
              });
          } else {
            this.dashboardService
              .addDashboardGroup({
                organizationId: this.organizationId,
                tableId: this.tableId,
                name: this.name,
              })
              .pipe()
              .subscribe((response) => {
                this.activeModal.dismiss(response);
                this.toastr.success("Group added successully");
              });
          }
          return;
        }
        case AddType.Column: {
          if (this.id) {
            this.dashboardService
              .editDashboardElement({
                id: this.id,
                organizationId: this.organizationId,
                name: this.name,
                keyword: this.keyword,
                formId: this.selectedForm.formId
              })
              .pipe()
              .subscribe((response) => {
                this.activeModal.dismiss(response);
                this.toastr.success("Column updated successully");
              });
          } else {
            this.dashboardService
              .addDashboardElement({
                organizationId: this.organizationId,
                groupId: this.groupId,
                name: this.name,
                keyword: this.keyword,
                formId: this.selectedForm.formId
              })
              .pipe()
              .subscribe((response) => {
                this.activeModal.dismiss(response);
                this.toastr.success("Column added successully");
              });
          }
        }
      }
    }
  }
}
