import { Component } from "@angular/core";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { ICellRendererAngularComp } from "ag-grid-angular";
import { ICellRendererParams, IAfterGuiAttachedParams, GridApi } from "ag-grid-community";
import { AddEmailsModalComponent } from "../add-emails-modal/add-emails-modal.component";
import { EditFacilityModalComponent } from "../edit-facility-modal/edit-facility-modal.component";

@Component({
  selector: 'btn-facility-cell-renderer',
  templateUrl: './faicility-three-dots-cell.component.html',
})
export class BtnFacilityCellRenderer implements ICellRendererAngularComp {
  public facilityId: number;

  private gridApi: GridApi;

  constructor(private modalService: NgbModal) { }

  refresh(params: ICellRendererParams): boolean {
    return true;
  }

  afterGuiAttached?(params?: IAfterGuiAttachedParams): void { }
  
  agInit(params: any): void {
    this.gridApi = params.api;
    
    const { data } = params;

    if (data) {
      const { id} = data;

      this.facilityId = id;
    }
  }

  public dotsClick(event: MouseEvent): void {
    event.stopPropagation();
  }

  public onHL7ReportClick(event: MouseEvent): void {
    event.stopPropagation();

  }
  public onAddEmailRecieptFacilityClick(facilityId: number): void {
    const modalRef = this.modalService.open(AddEmailsModalComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.title = 'Add Emails to Facility';
    modalRef.componentInstance.actionButtonLabel = 'Add';
    modalRef.componentInstance.facilityId = facilityId;
    modalRef.result
      .then((result: boolean) => {
 
      })
      .catch((res) => { });
  }
  public onEditFacilityClick(facilityId: number): void {
    const modalRef = this.modalService.open(EditFacilityModalComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.title = 'Edit Facility';
    modalRef.componentInstance.actionButtonLabel = 'Update';
    modalRef.componentInstance.facilityId = facilityId;
    
    modalRef.result
    .then((result: boolean) => {
      this.gridApi?.onFilterChanged();
    })
    .catch((res) => {});
  }

  public onDeleteFacilityClick(facilityId: number): void {
    
  }
}
