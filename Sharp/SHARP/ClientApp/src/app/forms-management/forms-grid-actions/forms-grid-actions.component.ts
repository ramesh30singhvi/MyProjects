import { HttpErrorResponse } from "@angular/common/http";
import { Component } from "@angular/core";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { ICellRendererAngularComp } from "ag-grid-angular";
import { ICellRendererParams, IAfterGuiAttachedParams, GridApi } from "ag-grid-community";
import { first } from "rxjs";
import { FormStatuses } from "src/app/models/forms/forms.model";
import { FormServiceApi } from "src/app/services/form-api.service";
import { RolesEnum } from "../../models/roles.model";
import { AuthService } from "../../services/auth.service";
import { EditFormModalComponent } from "../edit-form-modal/edit-form-modal.component";
import { DuplicateFormModalComponent } from "../duplicate-form-modal/duplicate-form-modal.component";

@Component({
  selector: 'forms-grid-actions-renderer',
  templateUrl: './forms-grid-actions.component.html',
})
export class FormsGridActionsRenderer implements ICellRendererAngularComp {
  public formVersionId: number;
  public isFormActive: boolean;
  public formId: number;
  public name: string;
  public auditType: string;

  public formStatuses = FormStatuses;

  private gridApi: GridApi;

  constructor(
    private formServiceApi: FormServiceApi,
    private modalService: NgbModal,
    private authService: AuthService
  ) { }

  refresh(params: ICellRendererParams): boolean {
    return true;
  }

  afterGuiAttached?(params?: IAfterGuiAttachedParams): void { }
  
  agInit(params: any): void {
    this.gridApi = params.api;
    
    const { data } = params;
    console.log("Data", data);

    if (data) {
      const { id, formId, isFormActive, name, auditType } = data;

      this.formVersionId = id;
      this.formId = formId;
      this.isFormActive = isFormActive;
      this.name = name;
      this.auditType = auditType
    }
  }

  public dotsClick(event: MouseEvent): void {
    event.stopPropagation();
  }

  public onArchiveFormClick(formId: number): void {
    this.setFormStatus(false);
  }
  public onEditFormClick(formId: number): void {
    const modalRef = this.modalService.open(EditFormModalComponent);
    modalRef.componentInstance.formId = formId;
    modalRef.componentInstance.formName = this.name;
    
    modalRef.result.then(
      (r) => {
        this.gridApi.forEachNode(rowNode => {
          if (rowNode.data.formId === this.formId) {
            var updated = rowNode.data;
            updated.name = r.formName;
            rowNode.setData(updated);
          }
        });
      },
      () => { }
    );
  }

  public onDuplicateFormClick(formId: number): void {
    const modalRef = this.modalService.open(DuplicateFormModalComponent);
    modalRef.componentInstance.formId = formId;
    modalRef.componentInstance.formName = this.name;
    
    modalRef.result.then(
      (r) => {
        this.gridApi.forEachNode(rowNode => {
          if (rowNode.data.formId === this.formId) {
            var updated = rowNode.data;
            updated.name = r.formName;
            rowNode.setData(updated);
          }
        });
      },
      () => { }
    );
  }

  public onUnarchiveFormClick(formId: number): void {
    this.setFormStatus(true);
  }

  private setFormStatus(state: boolean): void {
    this.formServiceApi.setFormState(this.formId, state)
    .pipe(first())
    .subscribe({
      next: (result: boolean) => {
        if(result) {
          this.gridApi.forEachNode(rowNode => {
            if (rowNode.data.formId === this.formId) {
                var updated = rowNode.data;
                updated.isFormActive = state;
        
                rowNode.setData(updated);
            }
          });
        }
      },
      error: (response: HttpErrorResponse) =>
      {
        console.error(response);
      }
    });    
  }
}
