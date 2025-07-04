import { Component, OnInit } from '@angular/core';
import { ColDef, ColumnApi, ColumnState, GridApi, GridOptions, GridReadyEvent, IDatasource, RowClickedEvent, SetFilterValuesFuncParams } from 'ag-grid-community';
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
} from '../common/constants/grid';
import { Title } from "@angular/platform-browser";
import { FORMS_MANAGER_COLUMNS_STATE, FORMS_MANAGER_SEARCH_TERM, FORMS_MANAGER_TABLE_FILTERS, FORM_GRID_COLUMNS } from '../common/constants/formGrid';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { FormServiceApi } from '../services/form-api.service';
import * as moment from 'moment';
import { MM_DD_YYYY_HH_MM_A_SLASH } from '../common/constants/date-constants';
import { FormService } from './services/form.service';
import { Router } from '@angular/router';
import { FormsGridActionsRenderer } from './forms-grid-actions/forms-grid-actions.component';
import { LocalStoreRepository } from '../services/repository/local-store-repository';
import { IDefaultFilters } from '../common/types/types';
import { FormManagementFiltersModel } from '../models/forms/forms.model';
import { IFilterOption } from '../models/audits/audit.filters.model';

let formManagementFilterValues: FormManagementFiltersModel = {};

@Component({
  selector: "app-forms",
  templateUrl: "./forms-management.component.html",
  styleUrls: ["./forms-management.component.scss"],
})
export class FormsManagementComponent implements OnInit {
  private UNLIMITED = "All";
  
  private filtersRepository: LocalStoreRepository<any>;
  private columnStatesRepository: LocalStoreRepository<any>;
  private searchTermRepository: LocalStoreRepository<any>;

  private filterModel: IDefaultFilters;
  private columnsState: ColumnState[];

  public defaultColumn: ColDef = {
    resizable: true,
    filter: true,
    filterParams: {
      buttons: FILTER_PARAMS_BUTTONS,
      closeOnApply: true,
      cellHeight: FILTER_HEIGHT,
      values: (params: SetFilterValuesFuncParams) =>
        this.formServiceApi
          .getFormsFilters(params.colDef.field, this.gridApi.getFilterModel(), formManagementFilterValues)
          .subscribe((filters: IFilterOption[]) => {
            const columnFilters = this.filtersRepository.load()?.[params.colDef.field];

            const filterValues = formManagementFilterValues?.[params.colDef.field]?.filter((filter) => columnFilters?.values?.includes(filter.value)) ?? [];

            filters = filterValues?.concat(filters?.filter((f) => 
            f.id 
            ? !filterValues?.map((fv) => fv.id)?.includes(f.id) 
            : !filterValues?.map((fv) => fv.value)?.includes(f.value))) 
            ?? [];

            formManagementFilterValues = {...formManagementFilterValues, [params.colDef.field]: filters};

            params.success(filters.map((filter: IFilterOption) => filter.value));
          }),
      refreshValuesOnOpen: true,
      defaultToNothingSelected: true,
      suppressSelectAll: true,
      suppressSorting: true,
    },
    menuTabs: ["filterMenuTab"],
    sortable: true,
  };

  public columns: ColDef[] = [
    {
      field: FORM_GRID_COLUMNS.ORGANIZATIONS,
      headerName: "Organization",
      minWidth: 250,
      valueFormatter: ({ value }) => {
        if (!value) {
          return null;
        }

        return value.join(", ");
      },
      filterParams: {
        comparator: (left: string, right: string) => {
          if (left === right) {
            return 0;
          }

          left < right ? -1 : 1;
        }
      }
    },
    {
      field: FORM_GRID_COLUMNS.NAME,
      headerName: "Form Name",
      minWidth: 300,
      filterParams: {
        cellHeight: 50
      }
    },
    {
      field: FORM_GRID_COLUMNS.AUDITTYPE,
      headerName: "Audit Type",
      width: 200,
      minWidth: 150,
    },
    {
      field: FORM_GRID_COLUMNS.CREATEDATE,
      headerName: "Create Date",
      maxWidth: 220,
      minWidth: 200,
      filter: 'agDateColumnFilter',
      filterParams: {
        valueFormatter: (params: any) => params.value.text,
        suppressAndOrCondition: true,
        filterOptions: ['equals', 'greaterThan', 'lessThan', 'inRange'],
      },
      cellRenderer: (data) => {
        if(!data.value){
          return;
        }
        
        return moment(data.value).format(MM_DD_YYYY_HH_MM_A_SLASH);
      },
    },
    {
      field: FORM_GRID_COLUMNS.STATUS,
      headerName: '',
      minWidth: 100,
      width: 100,
      sortable: false,
      filter: false,
      menuTabs: [],
      resizable: false,
      suppressMovable: true,      
      cellClass: 'button-cell',      
      cellRenderer: (params) => {
        if(!params.value){
          return;
        }
    
        const status = this.formService.getStatus(params.value);
    
        return `<span style="
          color: ${status.txtColor}; 
          background-color: ${status.bgColor}; 
          padding: 3px 10px;
          border-radius: 4px;
          font-size: 14px;">` + status.label + '</span>';
      },
    },
    {
      field: FORM_GRID_COLUMNS.FORM_STATE,
      headerName: '',
      minWidth: 80,
      width: 80,
      sortable: false,
      filter: false,
      menuTabs: [],
      resizable: false,
      suppressMovable: true,      
      cellClass: 'button-cell',      
      cellRenderer: (params) => {
        const state: boolean = params.value;

        if(state) {
          return `<span style="
          color: #3ac34a; 
          background-color: #c5f6cb; 
          padding: 3px 10px;
          border-radius: 4px;
          font-size: 14px;">Active</span>`;
        } else {
          return `<span style="
          color: #f62d51; 
          background-color: #f62d5124; 
          padding: 3px 10px;
          border-radius: 4px;
          font-size: 14px;">Inactive</span>`;
        }
      },
    },
    {
      field: FORM_GRID_COLUMNS.ACTIONS,
      headerName: "",
      minWidth: 100,
      width: 100,
      cellRenderer: 'btnCellRenderer',
      filter: false,
      menuTabs: [],
      sortable: false,
      resizable: false,
    },
  ];

  public rowModelType = ROW_MODEL_TYPE;
  public serverSideStoreType = SERVER_SIDE_STORE_TYPE;
  public rowBuffer = ROW_BUFFER;
  public cacheOverflowSize = CACHE_OVERFLOW_SIZE;
  public cacheBlockSize = 25;
  public maxConcurrentDatasourceRequests = MAX_CONCURRENT_DATASOURCE_REQUESTS;
  public infiniteInitialRowCount = INFINITE_INITIAL_ROWCOUNT;
  public maxBlocksInCache = MAX_BLOCKS_IN_CACHE;

  public searchTerm: string;

  private gridApi: GridApi;
  private gridColumnApi: ColumnApi;

  public frameworkComponents;

  constructor(
    private formServiceApi: FormServiceApi,
    private formService: FormService,
    private titleService: Title,
    private modalService: NgbModal,
    private router: Router
  ) {
    this.frameworkComponents = {
      btnCellRenderer: FormsGridActionsRenderer,
    };

    this.filtersRepository = new LocalStoreRepository<any>(FORMS_MANAGER_TABLE_FILTERS);
    this.columnStatesRepository = new LocalStoreRepository<any>(FORMS_MANAGER_COLUMNS_STATE);
    this.searchTermRepository = new LocalStoreRepository<any>(FORMS_MANAGER_SEARCH_TERM);
  }

  ngOnInit(): void {
    this.titleService.setTitle("SHARP forms");

    this.filterModel = this.filtersRepository.load();
    this.columnsState = this.columnStatesRepository.load();
    this.searchTerm = this.searchTermRepository.load();
  }

  public onRowClicked(event: RowClickedEvent) {
    const { data: { id }, rowIndex } = event;

    this.router.navigate(['forms-management/' + id]);
  }

  public onGridReady(event: GridReadyEvent): void {
    this.gridApi = event.api;
    this.gridColumnApi = event.columnApi;

    this.gridApi.sizeColumnsToFit();

    if(this.columnsState) {
      this.gridColumnApi.applyColumnState({ state: this.columnsState, applyOrder: true,});
    }

    if(this.filterModel) {
      this.gridApi.setFilterModel(this.filterModel);
    }

   ;
    const dataSource: IDatasource = {
      getRows: (params) =>
        this.formServiceApi
          .getForms(
            this.searchTerm,
            params.filterModel,
            params.sortModel,
            params.startRow,
            params.endRow,
            formManagementFilterValues,
          )
          .subscribe((forms) => {
            const currentLastRow = params.startRow + forms.length;
            const lastRowIndex =
              currentLastRow < params.endRow ? currentLastRow : -1;
            params.successCallback(forms, lastRowIndex);
          }),
    };
    event.api.setDatasource(dataSource);
  }

  public onSearch(): void {
    this.gridApi.onFilterChanged();
    this.searchTermRepository.save(this.searchTerm);
  }

  public onSearchClear(): void {
    this.searchTerm = null;
    this.onSearch();
    this.searchTermRepository.clear();
  }

  public onColumnVisible(e) {
    this.saveColumnsState();
  }

  public onColumnMoved(e) {    
    this.saveColumnsState();
  }

  public onSortChanged(e) {
    this.columnStatesRepository.save(this.gridColumnApi.getColumnState());
  }

  public onColumnResized(e) {
    this.columnStatesRepository.save(this.gridColumnApi.getColumnState());
  }

  public onFilterChanged(grid: GridOptions) {
    var filterModel = grid.api.getFilterModel(); 
    
    this.filtersRepository.save(filterModel);
  }

  private saveColumnsState() {
    this.columnStatesRepository.save(this.gridColumnApi.getColumnState());
  }

  private autoSizeAll(skipHeader: boolean) {
    const allColumnIds: string[] = [];
    this.gridColumnApi.getAllColumns()!.forEach((column) => {
      allColumnIds.push(column.getId());
    });
    this.gridColumnApi.autoSizeColumns(allColumnIds, skipHeader);
  }
}
