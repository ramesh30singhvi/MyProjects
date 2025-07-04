import { Component, Input, OnDestroy, OnInit, ViewChild } from "@angular/core";
import { DragulaService } from "ng2-dragula";

@Component({
  selector: "app-grid-columns",
  templateUrl: "./grid-columns.component.html",
  styleUrls: ["./grid-columns.component.scss"]
})

export class GridColumnsComponent implements OnInit, OnDestroy {
  @ViewChild('columnsDropdown') columnsDropdown;
  
  @Input() gridApi: any;
  @Input() gridColumnApi: any;

  public gridColumns: any[];

  public readonly COLUMNS: string = 'COLUMNS';

  public timer: any;

  private elementToScroll:HTMLElement;
  
  constructor(
    private dragulaService: DragulaService,
  ) { 
    dragulaService.createGroup(this.COLUMNS, {
      removeOnSpill: false,
    });
  }

  ngOnInit() {

  }

  ngOnDestroy(): void {
    this.dragulaService.destroy(this.COLUMNS);
  }

  public getGridColumns(): void {
    if (!this.gridColumnApi) {
      return;
    }

    const gridColumnsState: any[] = this.gridColumnApi.getColumnState();

    this.gridColumns = this.gridColumnApi
      .getAllGridColumns()
      .filter((column) => !column.isPinned() && column.getColDef().headerName)
      .map((column, index) => ({
        colId: column.getColId(),
        name: column.getColDef().headerName ?? column.getColDef().field,
        state: gridColumnsState.find(
          (colState) => colState.colId === column.colId
        ),
      }));
  }

  public onColumnsStateReset(): void {
    this.gridColumnApi.resetColumnState();
    
    //this.reportRequestColumnsStateRepository.save(this.gridColumnApi.getColumnState());

    this.onColumnsStateCancel();
  }

  public onColumnsStateCancel(): void {
    this.getGridColumns();
    this.columnsDropdown.close();
  }

  public onColumnsStateSave(): void {
    this.saveColumnsState();
    this.columnsDropdown.close();
  }

  public openColumnsDropdown(isOpen: boolean): void {
    if(!isOpen) {
      return;
    }

    this.getGridColumns();
  }

  public onColumnVisibleStateChange(column): void {
    const columnIndex = this.gridColumns.findIndex(
      (gridColumn) => gridColumn.colId === column.colId
    );
    if (columnIndex === -1) {
      return;
    }

    this.gridColumns[columnIndex].state.hide =
      !this.gridColumns[columnIndex].state.hide;
  }

  public scrollDiv(elementToScroll:HTMLElement, depl) {
    this.elementToScroll = elementToScroll;

    if(this.elementToScroll) {
      this.elementToScroll.scrollTop -= depl;
    } else {
      return;
    }

    if(!this.canScroll()) {
      this.stopScroll();
      return;
    }
    
    this.timer = setTimeout(()=>{
      this.scrollDiv(this.elementToScroll, depl)
    }, 30);
  }

  public stopScroll() {
    this.elementToScroll = null;
    clearTimeout(this.timer);
  }

  private canScroll(): boolean {
    if(!this.elementToScroll || 
      this.elementToScroll.scrollTop === 0 || 
      (this.elementToScroll.scrollHeight - this.elementToScroll.offsetHeight) === this.elementToScroll.scrollTop) {
      return false;
    }

    return true;
  }

  private saveColumnsState(): void {
    const columnsState = this.gridColumns.map((column) => column.state);

    this.gridColumnApi.applyColumnState({
      state: columnsState,
      applyOrder: true,
    });

    //this.reportRequestColumnsStateRepository.save(this.gridColumnApi.getColumnState());
  }
}
