import { HttpErrorResponse } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { ICellRendererAngularComp } from "ag-grid-angular";
import { ICellRendererParams, IAfterGuiAttachedParams, GridApi } from "ag-grid-community";
import { first } from "rxjs";
import { IScheduleSetting, ScheduleType, SettingType } from "src/app/models/forms/forms.model";
import { FormServiceApi } from "src/app/services/form-api.service";
import { ScheduleFormModalComponent } from "../schedule-form-modal/schedule-form-modal.component";

@Component({
  selector: "app-organization-forms-btn-cell-renderer",
  templateUrl: "./organization-forms-btn-cell-renderer.component.html",
  styleUrls: ["./organization-forms-btn-cell-renderer.component.scss"]
})

export class OrganizationFormsBtnCellRendererComponent implements ICellRendererAngularComp {
  public formOrganizationId: number;
  public formId: number;
  private formName: string;
  private formType: string;
  private settingType: SettingType;
  private scheduleSetting: IScheduleSetting;

  private gridApi: GridApi;

  constructor(
    private modalService: NgbModal,
    private formServiceApi: FormServiceApi,
    private router: Router,
    ) { 

  }

  refresh(params: ICellRendererParams): boolean {
    return true;
  }

  agInit(params: ICellRendererParams): void {
    this.gridApi = params.api;
    
    const { data } = params;

    if (data) {
      const { formOrganizationId, formId, name, auditType, settingType, scheduleSetting } = data;

      this.formOrganizationId = formOrganizationId;
      this.formId = formId;
      this.formName = name;
      this.formType = auditType;
      this.settingType = settingType;
      this.scheduleSetting = scheduleSetting;
    }
  }

  afterGuiAttached?(params?: IAfterGuiAttachedParams): void { }

  public onSetScheduleClick(organizationFormId: number): void {
    const modalRef = this.modalService.open(ScheduleFormModalComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.formOrganizationId = this.formOrganizationId;
    modalRef.componentInstance.formName = `${this.formName}, ${this.formType}`;
    modalRef.componentInstance.settingType = this.settingType;
    modalRef.componentInstance.scheduleSetting = this.scheduleSetting;
    
    modalRef.result
    .then((result: boolean) => {
      this.gridApi?.onFilterChanged();
    })
    .catch((res) => {});
  }

  public onViewFormClick(formId: number): void {
    this.formServiceApi.getLastActiveFormVersion(formId)
    .pipe(first())
    .subscribe({
      next: (formVersionId: number) => {
        if(formVersionId) {
          this.router.navigate(['forms-management/' + formVersionId]);
        }
      },
      error: (response: HttpErrorResponse) =>
      {
      }
    });
  }
}
