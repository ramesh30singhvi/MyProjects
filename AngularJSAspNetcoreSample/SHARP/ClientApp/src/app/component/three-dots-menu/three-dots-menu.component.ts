import { Component } from "@angular/core";
import { ICellRendererAngularComp } from "ag-grid-angular";
import { ICellRendererParams, IAfterGuiAttachedParams } from "ag-grid-community";
import { StudyService } from "../../services/study.service";
import { Status } from "../../shared/status";

@Component({
  selector: 'btn-cell-renderer',
  templateUrl: './three-dots-menu.component.html',
})
export class BtnCellRenderer implements ICellRendererAngularComp {
  public hide: boolean;

  private studyId: number;

  constructor(private studyService: StudyService) { }

  refresh(params: ICellRendererParams): boolean {
    return true;
  }

  afterGuiAttached?(params?: IAfterGuiAttachedParams): void { }
  
  agInit(params: any): void {
    const { data } = params;
    if (data) {
      const { id, status } = data;

      this.studyId = id;
      this.hide = status === Status.Active;
    }
  }

  public dotsClick(event: MouseEvent): void {
    event.stopPropagation();
  }

  public onReportClick(event: MouseEvent): void {
    event.stopPropagation();
    this.studyService.getReport(this.studyId).subscribe(response => {
      const url = URL.createObjectURL(response);
      window.open(url, '_blank');
    });
  }
}
