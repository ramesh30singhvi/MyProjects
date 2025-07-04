import { Component, OnInit, ViewChild } from '@angular/core';
import { ColDef, ColumnApi, ColumnState, GridApi, GridOptions, GridReadyEvent, IDatasource, RowClickedEvent, SetFilterValuesFuncParams, ICellRendererParams, ValueFormatterParams } from 'ag-grid-community';
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
import { Title } from "@angular/platform-browser";
import * as moment from 'moment';
import { MM_DD_YYYY_HH_MM_A_SLASH, MM_DD_YYYY_SLASH } from '../../common/constants/date-constants';
import { LocalStoreRepository } from '../../services/repository/local-store-repository';
import { IDefaultFilters } from '../../common/types/types';
import { IFilterOption } from '../../models/audits/audit.filters.model';
import { AuthService } from "../../services/auth.service";
import { KEYWORD_AI_REPORT_GRID_COLUMNS, KEYWORD_AI_REPORT_TABLE_FILTERS, KEYWORD_AI_REPORT_COLUMNS_STATE } from "./common/keywordAIReportGrid";
import { KeywordAIReportService } from './services/keyword-ai-report.service';
import { ReportAIService } from '../../services/reportAI.service';
import { KeywordAIReportFiltersModel, ReportAIStatuses, AIAuditStateEnum } from '../../models/reports/reports.model';
import { StatusAiReportComponent } from '../../reports/keyword-ai-report/status-ai-report/status-ai-report.component';
import { AiAuditGridActionsComponent } from '../../reports/keyword-ai-report/ai-audit-grid-actions/ai-audit-grid-actions.component';
import { ActivatedRoute, Router } from '@angular/router';
import { formatGridDate, formatGridDateTimeLocal } from "../../common/helpers/dates-helper";
import { RolesEnum } from '../../models/roles.model';

let keywordAIReportFilterValues: KeywordAIReportFiltersModel = {};

@Component({
  selector: 'app-keyword-ai-report',
  templateUrl: './keyword-ai-report.component.html',
  styleUrls: ['./keyword-ai-report.component.scss']
})
export class KeywordAiReportComponent implements OnInit {
  public gridColumns: any[];
  @ViewChild("columnsDropdown") columnsDropdown;

  private keywordAIReportFiltersRepository: LocalStoreRepository<any>;
  private keywordAIReportColumnsStateRepository: LocalStoreRepository<any>;

  private reportRequestColumnsState: any;

  private loadedFilters: IDefaultFilters;
  private columnsState: ColumnState[];
  public isAdmin: boolean;
  public isAuditor: boolean;
  public isReviewer: boolean;
  public columns: ColDef[];

  public frameworkComponents;
  public components;

  public sharpUserId: number;
  public statuses = ReportAIStatuses;

  public defaultColumn: ColDef = {
    resizable: true,
    filter: true,
    sortable: true,
    menuTabs: ['filterMenuTab'],
    filterParams: {
      suppressSorting: false,
      caseSensitive: false,
      defaultToNothingSelected: true,
      suppressSelectAll: true,
    }
  };

  public state = AIAuditStateEnum.Active;

  constructor(
    private titleService: Title,
    private authService: AuthService,
    private keywordAIReportService: KeywordAIReportService,
    private reportAIService: ReportAIService,
    private router: Router,
    private activatedRoute: ActivatedRoute,
  ) {
    
    this.activatedRoute.params.subscribe((params) => {
      let state = params["state"];

      if (state == "archived") {
        this.state = AIAuditStateEnum.Archived;
      } else if (state == "deleted") {
        this.state = AIAuditStateEnum.Deleted;
      } else {
        this.state = AIAuditStateEnum.Active;
      }
    });

    this.sharpUserId = this.authService.getCurrentUserSharpId();

    this.keywordAIReportFiltersRepository = new LocalStoreRepository<any>(KEYWORD_AI_REPORT_TABLE_FILTERS);
    this.keywordAIReportColumnsStateRepository = new LocalStoreRepository<any>(KEYWORD_AI_REPORT_COLUMNS_STATE);

    this.keywordAIReportColumnsStateRepository.clear();
    this.reportRequestColumnsState = this.keywordAIReportColumnsStateRepository.load();
    this.loadedFilters = this.keywordAIReportFiltersRepository.load();

    keywordAIReportFilterValues = {
      status: Object.values(ReportAIStatuses).map((status) => { return { id: status.id, value: status.label } }),
    };
    this.isAdmin = this.authService.isUserInRole(RolesEnum.Admin);
    this.isAuditor = this.authService.isUserInRole(RolesEnum.Auditor);
    this.isReviewer = this.authService.isUserInRole(RolesEnum.Reviewer);
   

    this.columns = this.getColumns();

    this.frameworkComponents = {
      statusAiReportComponent: StatusAiReportComponent,
      btnCellRenderer: AiAuditGridActionsComponent
    };

    this.components = {
      loadingCellRenderer: this.loadingCellRenderer,
    };
  }

  ngOnInit(): void {
    this.loadedFilters = this.keywordAIReportFiltersRepository.load();
    this.columnsState = this.keywordAIReportColumnsStateRepository.load();
  }

  private getColumns(): ColDef[] {
    return [
      {
        field: KEYWORD_AI_REPORT_GRID_COLUMNS.ORGANIZATION_NAME,
        headerName: "Organization",
        minWidth: 300,
        filterParams: {
          ...this.getDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
        },
        cellRenderer: this.loadingCellRenderer,
      },
      {
        field: KEYWORD_AI_REPORT_GRID_COLUMNS.FACILITY_NAME,
        headerName: "Facility",
        minWidth: 300,
        filterParams: {
          ...this.getDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
        },
      },
      {
        field: KEYWORD_AI_REPORT_GRID_COLUMNS.AUDITOR_NAME,
        headerName: "Audited by",
        minWidth: 250,
        filterParams: {
          ...this.getDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
        },
      },
      {
        field: KEYWORD_AI_REPORT_GRID_COLUMNS.AUDIT_DATE,
        headerName: 'Audit Date',
        minWidth: 200,
        valueFormatter: (params: any) => formatGridDate(params.value),
        filter: 'agDateColumnFilter',
        filterParams: {
          ...this.getDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          suppressAndOrCondition: true,
          filterOptions: ['equals', 'greaterThan', 'lessThan', 'inRange'],
        },
      },
      {
        field: KEYWORD_AI_REPORT_GRID_COLUMNS.CREATED_AT,
        headerName: 'Create At',
        minWidth: 200,
        valueFormatter: (params: any) => formatGridDate(params.value),
        filter: 'agDateColumnFilter',
        filterParams: {
          ...this.getDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          suppressAndOrCondition: true,
          filterOptions: ['equals', 'greaterThan', 'lessThan', 'inRange'],
        },
      },
      {
        field: KEYWORD_AI_REPORT_GRID_COLUMNS.SUBMITTED_DATE,
        headerName: 'Submitted Date',
        minWidth: 200,
        valueFormatter: (params: any) => formatGridDateTimeLocal(params.value),
        filter: 'agDateColumnFilter',
        filterParams: {
          ...this.getDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          suppressAndOrCondition: true,
          filterOptions: ['equals', 'greaterThan', 'lessThan', 'inRange'],
        },
      },
      {
        field: KEYWORD_AI_REPORT_GRID_COLUMNS.SENT_FOR_APPROVAL_DATE,
        headerName: 'Auditor\'s Time',
        minWidth: 320,
        valueFormatter: (params: any) => {
          let cellText: string = "";
          if (params && params.value && params.data && params.data.createdAt) {
            let date1 = new Date(params.value.replace('Z', ''));
            let date2 = new Date(params.data.createdAt.replace('Z', ''));
            let dif = Math.abs(Number(date1) - Number(date2));
            dif = Math.round((dif / 1000) / 60);
            cellText = this.minToDays(dif);
          }
          return cellText;
        },
        filter: false,
        sortable: false,
        suppressMenu: true,
      },
      {
        field: KEYWORD_AI_REPORT_GRID_COLUMNS.SUBMITTED_DATE,
        headerName: 'Duration',
        minWidth: 320,
        valueFormatter: (params: any) => {
          let cellText: string = "";
          if (params && params.value && params.data && params.data.createdAt) {
            let date1 = new Date(params.value.replace('Z', ''));
            let date2 = new Date(params.data.createdAt.replace('Z', ''));
            let dif = Math.abs(Number(date1) - Number(date2));
            dif = Math.round((dif / 1000) / 60);
            cellText = this.minToDays(dif);
          }
          return cellText;
        },
        filter: false,
        sortable: false,
        suppressMenu: true,
      },
      {
        field: KEYWORD_AI_REPORT_GRID_COLUMNS.STATUS,
        headerName: "Status",
        minWidth: 170,
        cellRenderer: 'statusAiReportComponent',
        sortable: true,
        filter: true,
        resizable: true,
        filterParams: {
          buttons: ['reset', 'cancel', 'apply'],
          closeOnApply: true,
          cellHeight: FILTER_HEIGHT,
          values: keywordAIReportFilterValues?.status?.map((status) => status.value),
          cellRenderer: (params) => {
            if (params.value == '(Select All)') return params.value;

            const status = Object.values(ReportAIStatuses).find((status) => status.label === params.value);

            return `<span style="color: ${status.color}">` + status.label + '</span>';
          },
          suppressSorting: true,
          suppressSelectAll: false,
        },
      },
      {
        field: KEYWORD_AI_REPORT_GRID_COLUMNS.ACTIONS,
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
  private gridColumnApi: ColumnApi;

  public onGridReady(event: GridReadyEvent): void {
    this.gridApi = event.api;
    this.gridColumnApi = event.columnApi;

    this.gridApi.sizeColumnsToFit();

    if (this.reportRequestColumnsState) {
      this.gridColumnApi.applyColumnState({ state: this.reportRequestColumnsState, applyOrder: true, });
    } else {
      this.gridColumnApi.applyColumnState({
        defaultState: { sort: null },
        state: [{ colId: KEYWORD_AI_REPORT_GRID_COLUMNS.AUDIT_DATE, sort: 'desc' }]
      });
    }

    if (this.loadedFilters) {
      this.gridApi.setFilterModel(this.loadedFilters);
    } else {
      this.gridColumnApi.applyColumnState({
        defaultState: { sort: null },
        state: [{ colId: KEYWORD_AI_REPORT_GRID_COLUMNS.AUDIT_DATE, sort: 'desc' }]
      });
    }
    var state = this.state;

    const dataSource: IDatasource = {
      getRows: (params) =>
        this.reportAIService
          .getKeywordAIReport(
            params.filterModel,
            params.sortModel,
            params.startRow,
            params.endRow,
            keywordAIReportFilterValues,
            state
          )
          .subscribe((reports) => {
            console.log(reports);
            const currentLastRow = params.startRow + reports.length;
            const lastRowIndex =
              currentLastRow < params.endRow ? currentLastRow : -1;
            params.successCallback(reports, lastRowIndex);
          }),
    };
    event.api.setDatasource(dataSource);
  }

  public onFilterChanged(grid: GridOptions) {
    var filterModel = grid.api.getFilterModel();

    this.keywordAIReportFiltersRepository.save(filterModel);
  }

  public onColumnVisible(e) {
    this.saveColumnsState();
  }

  public onColumnMoved(e) {
    this.saveColumnsState();
  }

  private saveColumnsState() {
    this.keywordAIReportColumnsStateRepository.save(this.gridColumnApi.getColumnState());
  }

  public onColumnResized(e) {
    this.keywordAIReportColumnsStateRepository.save(this.gridColumnApi.getColumnState());
  }

  public onSortChanged(e) {
    this.keywordAIReportColumnsStateRepository.save(this.gridColumnApi.getColumnState());
  }

  private autoSizeAll(skipHeader: boolean) {
    const allColumnIds: string[] = [];
    this.gridColumnApi.getAllColumns()!.forEach((column) => {
      allColumnIds.push(column.getId());
    });
    this.gridColumnApi.autoSizeColumns(allColumnIds, skipHeader);
  }

  private getDefaultSetFilterParams(
    setValues: (filters: any, params: SetFilterValuesFuncParams) => void
  ): any {
    return {
      buttons: FILTER_PARAMS_BUTTONS,
      closeOnApply: true,
      cellHeight: FILTER_HEIGHT,
      values: (params: SetFilterValuesFuncParams) => {
        this.reportAIService
          .getKeywordAIReportFilters(params.colDef.field, this.gridApi.getFilterModel(), keywordAIReportFilterValues, this.state)
          .subscribe((filters: IFilterOption[]) => {

            const columnFilters = this.keywordAIReportFiltersRepository.load()?.[params.colDef.field];

            const filterValues = keywordAIReportFilterValues?.[params.colDef.field]?.filter((filter) => columnFilters?.values?.includes(filter.value)) ?? [];

            filters = filterValues?.concat(filters?.filter((f) =>
              f.id
                ? !filterValues?.map((fv) => fv.id)?.includes(f.id)
                : !filterValues?.map((fv) => fv.value)?.includes(f.value)))
              ?? [];

            keywordAIReportFilterValues = { ...keywordAIReportFilterValues, [params.colDef.field]: filters };

            setValues(
              filters.map((filter: IFilterOption) => filter.value),
              params
            );
          });
      },
      refreshValuesOnOpen: true,
    };
  }

  private valueFormatter(params: ValueFormatterParams): string {
    try {
      const column = params.value;

      return column === null ? 'Empty' : column;
    } catch {
      return '';
    }
  }

  loadingCellRenderer = (params: ICellRendererParams) => {
    if (params.value !== undefined) {
      return params.value;
    } else {
      return `<div class="la-ball-pulse la-dark">
                  <div></div>
                  <div></div>
                  <div></div>
              </div>`;
    }
  };
  onColumnsStateReset() {
    this.gridColumnApi.resetColumnState();

    this.keywordAIReportColumnsStateRepository.save(this.gridColumnApi.getColumnState());

    this.onColumnsStateCancel();
  }

  onColumnsStateCancel() {
    this.getGridColumns();
    this.columnsDropdown.close();
  }


  public getGridColumns() {
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
          (colState) => colState.colId === column.getColId()
        ),
      }));
  }
  public onRowClicked(event: RowClickedEvent) {
    if (this.state != AIAuditStateEnum.Active) return;

    const { data: { id, status}, rowIndex } = event;

    this.router.navigate(['reports/editAIAudit/' + id]);
  }

  public onCreateAiReport() {
    this.router.navigate(['reports/processAIReport']);
  }

  minToDays(min: number) {
    let days = Math.floor(min / 1440);
    let hours = Math.floor((min % 1440) / 60);
    let remainingMin = Math.floor(min % 60);

    if (days == 0 && hours == 0 && remainingMin == 0) {
      return `0 minute(s)`;
    }
    else if (days == 0 && hours == 0 && remainingMin != 0) {
      return `${remainingMin} minute(s)`;
    }
    else if (days == 0 && hours != 0) {

      if (remainingMin == 0) {
        return `${hours} hour(s)`;
      }
      return `${hours} hour(s) and ${remainingMin} minute(s)`;

    }
    else if (days != 0) {

      if (hours == 0 && remainingMin == 0) {
        return `${days} day(s)`;
      } else if (hours != 0 && remainingMin == 0) {
        return `${days} day(s) and ${hours} hour(s)`;
      } else if (hours == 0 && remainingMin != 0) {
        return `${days} day(s) and ${remainingMin} minute(s)`;
      }
      return `${days} day(s), ${hours} hour(s) and ${remainingMin} minute(s)`;

    }
    else {
      return '';
    }
  }
}
