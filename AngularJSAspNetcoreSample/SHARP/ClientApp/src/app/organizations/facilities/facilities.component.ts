import { HttpErrorResponse } from '@angular/common/http';
import { Component, Input } from '@angular/core';
import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ColDef, GridApi, GridReadyEvent, IDatasource, SetFilterValuesFuncParams } from 'ag-grid-community';
import {
  CACHE_OVERFLOW_SIZE,
  FILTER_HEIGHT,
  FILTER_PARAMS_BUTTONS,
  INFINITE_INITIAL_ROWCOUNT,
  MAX_BLOCKS_IN_CACHE,
  MAX_CONCURRENT_DATASOURCE_REQUESTS,
  ROW_BUFFER,
  ROW_MODEL_TYPE,
  SERVER_SIDE_STORE_TYPE
} from '../../common/constants/grid';
import { FACILITY_GRID_COLUMNS } from '../../common/constants/facilityGrid';
import { FacilityService } from '../../services/facility.service';
import { BtnFacilityCellRenderer } from './three-dot-cell-render/faicility-three-dots-cell.component';
import { EditFacilityModalComponent } from './edit-facility-modal/edit-facility-modal.component';
import { IOption } from 'src/app/models/audits/audits.model';
import { FacilityFiltersModel, FacilityStatuses } from 'src/app/models/organizations/facility.model';
import { IFilterOption } from 'src/app/models/audits/audit.filters.model';

let facilityFilterValues: FacilityFiltersModel = {};

@Component({
  selector: "app-facilities",
  templateUrl: "./facilities.component.html",
  styleUrls: ["./facilities.component.scss"],
})
export class FacilitiesComponent {
  private _organization: IOption;

  @Input() set organization(organization: IOption) {
    this._organization = organization;

    this.gridApi?.setFilterModel(null);
    this.onSearch();
  }

  public columns: ColDef[];

  constructor(
    public facilityService: FacilityService,
    private modalService: NgbModal,
  ) {
    facilityFilterValues = {
      active: Object.values(FacilityStatuses).map((status) => {return { id : status.id, value: status.label}}),
    };

    this.columns = this.getColumns();

    this.frameworkComponents = {
      btnCellRenderer: BtnFacilityCellRenderer,
    };
  }
  public searchTerm: string;

  public onAddFacilityClick(): void {
    const modalRef = this.modalService.open(EditFacilityModalComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.organization = this._organization;
    modalRef.componentInstance.title = 'Add Facility';
    modalRef.componentInstance.actionButtonLabel = 'Create';
    
    modalRef.result
    .then((result: boolean) => {      
      this.gridApi?.onFilterChanged();
    })
    .catch((res) => {});
  }

  onSearchClear() {
    this.searchTerm = null;
    this.gridApi?.onFilterChanged();
  }

  public onSearch(): void {
    this.gridApi?.onFilterChanged();
  }

  private UNLIMITED = "All";
  public noRecords = false;
  public defaultColumn: ColDef = {
    minWidth: 300,
    resizable: true,
    filter: true,
    filterParams: {
      buttons: FILTER_PARAMS_BUTTONS,
      closeOnApply: true,
      cellHeight: FILTER_HEIGHT,
      values: (params: SetFilterValuesFuncParams) =>
        this.facilityService
          .getFacilityFilters(params.colDef.field, this._organization?.id, this.gridApi?.getFilterModel(), facilityFilterValues)
          .subscribe((filters: IFilterOption[]) => {
            //const columnFilters = this.filtersRepository.load()?.[params.colDef.field];

            const filterValues = /*userFilterValues?.[params.colDef.field]?.filter((filter) => columnFilters?.values?.includes(filter.value)) ??*/ [];

            filters = filterValues?.concat(filters?.filter((f) => 
            f.id 
            ? !filterValues?.map((fv) => fv.id)?.includes(f.id) 
            : !filterValues?.map((fv) => fv.value)?.includes(f.value))) 
            ?? [];

            facilityFilterValues = {...facilityFilterValues, [params.colDef.field]: filters};

            params.success(filters.map((filter: IFilterOption) => filter.value));
          }),
      refreshValuesOnOpen: true,
      defaultToNothingSelected: true,
      suppressSorting: true,
      suppressSelectAll: true,
    },
    menuTabs: ["filterMenuTab"],
    sortable: true,
  };

  public onGridReady(event: GridReadyEvent): void {
    this.gridApi = event.api;

    this.gridApi.sizeColumnsToFit();

    const dataSource: IDatasource = {
      getRows: (params) =>
        this.facilityService
          .getFacilities(
            this.searchTerm,
            params.filterModel,
            params.sortModel,
            params.startRow,
            params.endRow,
            this._organization?.id ?? 0,
            facilityFilterValues,
          )
          .subscribe((facilities) => {
            this.noRecords = facilities.length === 0;
            const currentLastRow = params.startRow + facilities.length;
            const lastRowIndex =
              currentLastRow < params.endRow ? currentLastRow : -1;
            params.successCallback(facilities, lastRowIndex);
          }),
    };
    event.api.setDatasource(dataSource);
  }

  statusFiltertRenderer = (params) => {
    if (params.value == '(Select All)') return params.value;

    const column = params.value;

    const status = this.getStatus(column);

    return `<span style="color: ${status.color}">` + status.label + '</span>';
  };

  private getColumns(): ColDef[] {
    return [
      {
        field: FACILITY_GRID_COLUMNS.NAME,
        headerName: "Name",
        minWidth: 300,
        filterParams: {
          cellHeight: 45,
        }
      },
      {
        field: FACILITY_GRID_COLUMNS.TIMEZONE,
        headerName: "Timezone",
        minWidth: 300,
        maxWidth: 400,
      },
      {
        field: FACILITY_GRID_COLUMNS.EMAILRECIPIENTS,
        headerName: "Email Recipients",
        maxWidth: 165,
        filter: false,
        menuTabs: [],
        resizable: false,
      },
      {
        field: FACILITY_GRID_COLUMNS.STATUS,
        headerName: "Status",
        maxWidth: 110,
        resizable: false,
        cellRenderer: (data) => {
          if (data.value === true) {
            return `<span style="
                    color: #3ac34a;
                    background-color: #c5f6cb;
                    padding: 3px 10px;
                    border-radius: 4px;
                    font-size: 14px;">` + 'Active' + '</span>';
        }
        else {
            return `<span style="
                    color: rgb(246, 45, 81);
                    background-color: rgba(246, 45, 81, 0.14);
                    padding: 3px 10px;
                    border-radius: 4px;
                    font-size: 14px;">` + 'Inactive' + '</span>';
          }
        },
        filterParams: {
          buttons: ['reset', 'cancel', 'apply'],
          closeOnApply: true,
          cellHeight: FILTER_HEIGHT,
          values:  facilityFilterValues?.active?.map((status) => status.value),
          cellRenderer: this.statusFiltertRenderer,
          suppressMiniFilter: true,
          suppressSorting: true,
        },
      },
      {
        field: FACILITY_GRID_COLUMNS.ACTIONS,
        headerName: "",
        maxWidth: 80,
        cellRenderer: 'btnCellRenderer',
        filter: false,
        menuTabs: [],
        sortable: false,
        resizable: false,
      },
    ];
  }

  statusValueCellRenderer = (params) => {
    if (!params.value) {
      return;
    }
    console.log(params);
    

    return `<span style="color: green">` + 'Active' + '</span>';
  };

  public getStatus(status: string): any {
    const statuses = Object.values(FacilityStatuses);

    return statuses.find((i) => i.label === status);
  }

  public dotsClick(event: MouseEvent): void {
    event.stopPropagation();
  }
  public rowModelType = ROW_MODEL_TYPE;
  public serverSideStoreType = SERVER_SIDE_STORE_TYPE;
  public rowBuffer = ROW_BUFFER;
  public cacheOverflowSize = CACHE_OVERFLOW_SIZE;
  public cacheBlockSize = 25;
  public maxConcurrentDatasourceRequests = MAX_CONCURRENT_DATASOURCE_REQUESTS;
  public infiniteInitialRowCount = INFINITE_INITIAL_ROWCOUNT;
  public maxBlocksInCache = MAX_BLOCKS_IN_CACHE;

  private gridApi: GridApi;
  public frameworkComponents;
}
