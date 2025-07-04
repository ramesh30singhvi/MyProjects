import { Component, OnInit } from "@angular/core";
import { Title } from "@angular/platform-browser";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { ColDef, ColumnApi, GridApi, GridOptions, GridReadyEvent, ICellRendererParams, IDatasource, SetFilterValuesFuncParams, ValueFormatterParams } from "ag-grid-community";
import * as moment from "moment";
import { first } from "rxjs";
import { CriteriaPdfFilterPopupComponent } from "../audits/criteria-pdf-filter-popup/criteria-pdf-filter-popup.component";
import { AuditService } from "../audits/services/audit.service";
import { CACHE_OVERFLOW_SIZE, FILTER_HEIGHT, FILTER_PARAMS_BUTTONS, INFINITE_INITIAL_ROWCOUNT, MAX_BLOCKS_IN_CACHE, MAX_CONCURRENT_DATASOURCE_REQUESTS, ROW_BUFFER, ROW_MODEL_TYPE, SERVER_SIDE_STORE_TYPE } from "../common/constants/grid";
import { formatGridDate, formatGridDateTimeLocal } from "../common/helpers/dates-helper";
import { IDefaultFilters } from "../common/types/types";
import { IFilterOption } from "../models/audits/audit.filters.model";
import { IUserTimeZone } from "../models/audits/audits.model";
import { ReportRequestFiltersModel, ReportRequestStatuses } from "../models/report-requests/report-requests.model";
import { RolesEnum } from "../models/roles.model";
import { AuthService } from "../services/auth.service";
import { ReportRequestServiceApi } from "../services/report-request-api.service";
import { LocalStoreRepository } from "../services/repository/local-store-repository";
import { UserService } from "../services/user.service";
import { REPORT_REQUEST_COLUMNS_STATE, REPORT_REQUEST_GRID_COLUMNS, REPORT_REQUEST_TABLE_FILTERS } from "./common/report-request-grid.constants";
import { DownloadGridButtonComponent } from "./download-grid-button/download-grid-button.component";
import { StatusRequestComponent } from "./status-request-component/status-request-component";

let reportRequestFilterValues: ReportRequestFiltersModel = {};
@Component({
  selector: "app-report-request",
  templateUrl: "./report-request.component.html",
  styleUrls: ["./report-request.component.scss"]
})
export class ReportRequestComponent implements OnInit {
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

  public columns: ColDef[];

  public rowModelType = ROW_MODEL_TYPE;
  public serverSideStoreType = SERVER_SIDE_STORE_TYPE;
  public rowBuffer = ROW_BUFFER;
  public cacheOverflowSize = CACHE_OVERFLOW_SIZE;
  public cacheBlockSize = 25;
  public maxConcurrentDatasourceRequests = MAX_CONCURRENT_DATASOURCE_REQUESTS;
  public infiniteInitialRowCount = INFINITE_INITIAL_ROWCOUNT;
  public maxBlocksInCache = MAX_BLOCKS_IN_CACHE;

  public frameworkComponents;

  public gridApi: GridApi;
  public gridColumnApi: ColumnApi;

  public isAuditor: boolean;
  public sharpUserId: number;
  public haveRightToViewPdf: boolean;
  private userTimeZone: IUserTimeZone;

  private reportRequestFiltersRepository: any;
  private reportRequestColumnsStateRepository: any;

  private loadedFilters: IDefaultFilters;
  private reportRequestColumnsState: any;

  constructor(
    private reportRequestServiceApi: ReportRequestServiceApi,
    private authService: AuthService,
    private auditService: AuditService,
    private userService: UserService,
    private modalService: NgbModal,
    private titleService: Title,
  ) { 
    this.isAuditor = this.authService.isUserInRole(RolesEnum.Auditor);
    this.sharpUserId = this.authService.getCurrentUserSharpId();
    this.haveRightToViewPdf = this.auditService.haveRightToViewPdf();

    this.reportRequestFiltersRepository = new LocalStoreRepository<any>(REPORT_REQUEST_TABLE_FILTERS);
    this.reportRequestColumnsStateRepository = new LocalStoreRepository<any>(REPORT_REQUEST_COLUMNS_STATE);

    this.reportRequestColumnsState = this.reportRequestColumnsStateRepository.load();
    this.loadedFilters = this.reportRequestFiltersRepository.load();

    reportRequestFilterValues = {
      status: Object.values(ReportRequestStatuses).map((status) => {return { id : status.id, value: status.label}}),
    };

    this.columns = this.getColumns();
    
    this.frameworkComponents = {
      downloadGridButtonComponent: DownloadGridButtonComponent,
      statusRequestComponent: StatusRequestComponent,
    };
  }

  ngOnInit() {
    this.titleService.setTitle("SHARP report requests");
  }

  onColumnVisible(e) {
    this.saveColumnsState();
  }

  onColumnMoved(e) {
    this.saveColumnsState();
  }

  onSortChanged(e) {
    this.saveColumnsState();
  }

  saveColumnsState() {
    this.reportRequestColumnsStateRepository.save(this.gridColumnApi.getColumnState());
  }

  public onFilterChanged(grid: GridOptions) {
    var filterModel = grid.api.getFilterModel();

    this.reportRequestFiltersRepository.save(filterModel);
  }

  public onGridReady(event: GridReadyEvent): void {
    this.gridApi = event.api;
    this.gridColumnApi = event.columnApi;

    if (this.reportRequestColumnsState) {
      this.gridColumnApi.applyColumnState({
        state: this.reportRequestColumnsState,
        applyOrder: true,
      });
    }

    if(this.loadedFilters) {
      this.gridApi.setFilterModel(this.loadedFilters);
    } else {
      this.setDefaultRequestReportFilter();
      this.gridColumnApi.applyColumnState({
        defaultState: { sort: null }, 
        state: [{ colId: REPORT_REQUEST_GRID_COLUMNS.REQUESTED_TIME, sort: 'desc' }]});
    }

    const dataSource: IDatasource = {
      getRows: (params) =>
        this.reportRequestServiceApi
          .getReportRequests(
            params.startRow,
            params.endRow,
            params.sortModel,
            params.filterModel,
            reportRequestFilterValues,
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

  public onRefreshClick(): void {
    this.gridApi.onFilterChanged();
  }

  public onGeneratePdfClick() {
    const modalRef = this.modalService
      .open(CriteriaPdfFilterPopupComponent, { modalDialogClass: 'generate-pdf-modal' });

    modalRef.result.then((result: boolean) => {
      if(result) {        
        this.gridApi.onFilterChanged();
      }
    })
    .catch((res) => {});
  }

  private getColumns(): ColDef[] {
    return [
      {
        field: REPORT_REQUEST_GRID_COLUMNS.AUDIT_TYPE,
        headerName: "Audit Type",
        minWidth: 200,
        filterParams: {
          ...this.getDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
        },
      },
      {
        field: REPORT_REQUEST_GRID_COLUMNS.ORGANIZATION_NAME,
        headerName: "Organization",
        minWidth: 250,
        filterParams: {
          ...this.getDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
        },
      },
      {
        field: REPORT_REQUEST_GRID_COLUMNS.FACILITY_NAME,
        headerName: "Facility",
        minWidth: 300,
        filterParams: {
          ...this.getDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
          cellHeight: 50,
        },
      },
      {
        field: REPORT_REQUEST_GRID_COLUMNS.FORM_NAME,
        headerName: 'Form',
        minWidth: 300,
        filterParams: {
          ...this.getDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
          cellHeight: 50,
        },
      },
      {
        field: REPORT_REQUEST_GRID_COLUMNS.FROM_DATE,
        headerName: 'Filter Start Date',
        minWidth: 150,
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
        field: REPORT_REQUEST_GRID_COLUMNS.TO_DATE,
        headerName: 'Filter End Date',
        minWidth: 150,
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
        field: REPORT_REQUEST_GRID_COLUMNS.REQUESTED_TIME,
        headerName: 'Requested Time',
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
        field: REPORT_REQUEST_GRID_COLUMNS.GENERATED_TIME,
        headerName: 'Generated Time',
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
        field: REPORT_REQUEST_GRID_COLUMNS.USER_FULL_NAME,
        headerName: 'Generator ID',
        minWidth: 200,
        filterParams: {
          ...this.getDefaultSetFilterParams((filters, params) => { params.success(filters) }),
          valueFormatter: this.valueFormatter,
        },
      },
      {
        field: REPORT_REQUEST_GRID_COLUMNS.STATUS,
        headerName: "Status",
        minWidth: 150,
        cellRenderer: 'statusRequestComponent',
        sortable: true,
        filter: true,
        resizable: true,
        filterParams: {
          buttons: ['reset', 'cancel', 'apply'],
          closeOnApply: true,
          cellHeight: FILTER_HEIGHT,
          values:  reportRequestFilterValues?.status?.map((status) => status.value),
          cellRenderer: (params) => {
            if (params.value == '(Select All)') return params.value;
        
            const status = Object.values(ReportRequestStatuses).find((status) => status.label === params.value);
        
            return `<span style="color: ${status.color}">` + status.label + '</span>';
          },
          suppressSorting: true,
          //suppressSelectAll: false,
        },
      },
      {
        field: REPORT_REQUEST_GRID_COLUMNS.REPORT,
        headerName: "Report",
        width: 100,
        cellRenderer: 'downloadGridButtonComponent',
        filter: false,
        sortable: false,
        menuTabs: [],
      },
    ];
  }

  private setDefaultRequestReportFilter() {
    this.getUserTimeZone(() => {
      let filter = this.gridApi.getFilterModel();

      this.gridApi.setFilterModel({
      ...filter, 
      [REPORT_REQUEST_GRID_COLUMNS.REQUESTED_TIME]: {
        type: 'equals',
        dateFrom: moment(this.userTimeZone?.userTimeZoneDateTime).format('YYYY-MM-DD'),
        dateTo: null,
      }});
    });
  }

  private valueFormatter(params: ValueFormatterParams): string {
    try {
      const column =  params.value;

      return column === null ? 'Empty' : column;
    } catch {
      return '';
    }    
  }

  private getDefaultSetFilterParams(
    setValues: (filters: any, params: SetFilterValuesFuncParams) => void
  ): any {
    return {
      buttons: FILTER_PARAMS_BUTTONS,
      closeOnApply: true,
      cellHeight: FILTER_HEIGHT,
      values: (params: SetFilterValuesFuncParams) => {
        this.reportRequestServiceApi
          .getGridFilters(params.colDef.field,  this.gridApi.getFilterModel(), reportRequestFilterValues)
          .subscribe((filters: IFilterOption[]) => {

            const columnFilters = this.reportRequestFiltersRepository.load()?.[params.colDef.field];

            const filterValues = reportRequestFilterValues?.[params.colDef.field]?.filter((filter) => columnFilters?.values?.includes(filter.value)) ?? [];

            filters = filterValues?.concat(filters?.filter((f) => 
            f.id 
            ? !filterValues?.map((fv) => fv.id)?.includes(f.id) 
            : !filterValues?.map((fv) => fv.value)?.includes(f.value))) 
            ?? [];

            reportRequestFilterValues = {...reportRequestFilterValues, [params.colDef.field]: filters};

            setValues(
              filters.map((filter: IFilterOption) => filter.value),
              params
            );
          });
      },
      refreshValuesOnOpen: true,
    };
  }

  private getUserTimeZone(setDatePeriod:() => void) {
    this.userService
    .getUserTimeZone(this.sharpUserId)
    .pipe(first())
    .subscribe({
      next: (timeZone: IUserTimeZone) => {
        this.userTimeZone = timeZone;

        setDatePeriod();
      }
    });
  }
}
