import { Component, OnInit, ViewChild } from '@angular/core';
import {
  TabsEnum,
  AuditorProductivityInputFiltersModel,
  AuditorProductivityAHTPerAuditTypeFiltersModel,
  AuditorProductivitySummaryPerAuditorFiltersModel,
} from '../models/dashboard/auditor-productivity-dashboard.model';
import { LocalStoreRepository } from '../services/repository/local-store-repository';
import {
  ColDef,
  ColGroupDef,
  ColumnApi,
  GridApi,
  GridOptions,
  GridReadyEvent,
  IDatasource,
  SetFilterValuesFuncParams,
  ICellRendererParams,
  ValueFormatterParams,
} from 'ag-grid-community';
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
import {
  INPUT_GRID_COLUMNS,
  AUDITOR_PRODUCTIVITY_INPUT_TABLE_FILTERS,
  AUDITOR_PRODUCTIVITY_INPUT_COLUMNS_STATE,
} from "../auditor-productivity-dashboard/common/inputGrid";
import {
  AHT_GRID_COLUMNS,
  AUDITOR_PRODUCTIVITY_AHT_TABLE_FILTERS,
  AUDITOR_PRODUCTIVITY_AHT_COLUMNS_STATE,
  AUDITOR_PRODUCTIVITY_AHT_DATE_PROCESSED,
} from "../auditor-productivity-dashboard/common/ahtGrid";
import {
  SUMMARY_GRID_COLUMNS,
  AUDITOR_PRODUCTIVITY_SUMMARY_TABLE_FILTERS,
  AUDITOR_PRODUCTIVITY_SUMMARY_COLUMNS_STATE,
  AUDITOR_PRODUCTIVITY_SUMMARY_DATE_PROCESSED,
} from "../auditor-productivity-dashboard/common/summaryGrid";
import { IDefaultFilters } from '../common/types/types';
import { IFilterOption } from '../models/audits/audit.filters.model';
import { AuditorProductivityDashboardService } from "../services/auditor-productivity-dashboard.service";
import * as moment from "moment";
import {
  MM_DD_YYYY_HH_MM_A_SLASH,
} from "../common/constants/date-constants";
import {
  NgbDate,
  NgbDateParserFormatter,
  NgbInputDatepicker,
  NgbNavChangeEvent
} from "@ng-bootstrap/ng-bootstrap";
import { transformDate } from "../common/helpers/dates-helper";
import { AgGridAngular } from 'ag-grid-angular';
import { OrganizationService } from '../services/organization.service';
import { SummaryFacilityComponent } from './summary-facility/summary-facility.component';
import { Observable } from 'rxjs';
import { IOption } from '../models/audits/audits.model';
import { UserService } from '../services/user.service';

let auditorProductivityInputFilterValues: AuditorProductivityInputFiltersModel = {};
let auditorProductivityAHTPerAuditTypeFilterValues: AuditorProductivityAHTPerAuditTypeFiltersModel = {};
let auditorProductivitySummaryPerAuditorFilterValues: AuditorProductivitySummaryPerAuditorFiltersModel = {};


const AUDITOR_PRODUCTIVITY_SELECTED_TAB = "auditor-productivity-selected-tab";

@Component({
  selector: 'app-auditor-productivity-dashboard',
  templateUrl: './auditor-productivity-dashboard.component.html',
  styleUrls: ['./auditor-productivity-dashboard.component.scss']
})

export class AuditorProductivityDashboardComponent implements OnInit {
  @ViewChild("datepicker") datepicker: NgbInputDatepicker;
  @ViewChild("datepicker2") datepicker2: NgbInputDatepicker;
  @ViewChild("datepicker3") datepicker3: NgbInputDatepicker;
  @ViewChild('inputGrid') inputGrid: AgGridAngular;
  @ViewChild('ahtGrid') ahtGrid: AgGridAngular;
  @ViewChild('summaryGrid') summaryGrid: AgGridAngular;

  @ViewChild('summaryFacility') summaryFacilityTab!: SummaryFacilityComponent;


  private gridsInitialized = {
    input: false,
    aht: false,
    summary: false
  };

  public selectedTab: string = '1';
  public tabsEnum = TabsEnum;
  public components;
  private SelectedTabRepository: LocalStoreRepository<any>;
  private AHTDateProcessedRepository: LocalStoreRepository<any>;
  private SummaryDateProcessedRepository: LocalStoreRepository<any>;
  private InputGridFiltersRepository: LocalStoreRepository<any>;
  private InputGridColumnsStateRepository: LocalStoreRepository<any>;
  private InputGridRequestColumnsState: any;
  private loadedFilters_Input: IDefaultFilters;
  teams$: Observable<IOption[]>;
  public defaultColumn_Input: ColDef = {
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
  public columns_Input: ColDef[];
  public getRowStyle = function (params) {

    if (params.data == undefined)
     return;

    if (params.data.facilityName == "") {
        return { "font-weight": "bold", "background": "#00539B14" , color: "#145196", 'font-size': "16px"  };
    }


    if (params.data.overTimeUsed != undefined && params.data.overTimeUsed != null && params.data.overTimeUsed == true) {
      return { "background": "#FFEEEE" };
    }
  }


  private AHTGridFiltersRepository: LocalStoreRepository<any>;
  private AHTGridColumnsStateRepository: LocalStoreRepository<any>;
  private AHTGridRequestColumnsState: any;
  private loadedFilters_AHT: IDefaultFilters;

  public defaultColumn_AHT: ColDef = {
    resizable: true,
    filter: true,
    sortable: true,
    menuTabs: ['filterMenuTab'],
    filterParams: {
      suppressSorting: false,
      caseSensitive: false,
      defaultToNothingSelected: true,
      suppressSelectAll: true,
    },
  };
  public columns_AHT: ColDef[];

  public fromDate_AHT: NgbDate | null;
  public toDate_AHT: NgbDate | null;
  public maxDate_AHT: NgbDate = new NgbDate(
    new Date().getFullYear(),
    new Date().getMonth() + 1,
    new Date().getDate()
  );
  public hoveredDate_AHT: NgbDate | null = null;


  private SummaryGridFiltersRepository: LocalStoreRepository<any>;
  private SummaryGridColumnsStateRepository: LocalStoreRepository<any>;
  private SummaryGridRequestColumnsState: any;
  private loadedFilters_Summary: IDefaultFilters;

  public defaultColumn_Summary: (ColDef | ColGroupDef) = {
    resizable: true,
    filter: true,
    sortable: true,
    menuTabs: ['filterMenuTab'],
    filterParams: {
      suppressSorting: false,
      caseSensitive: false,
      defaultToNothingSelected: true,
      suppressSelectAll: true,
    },
  };



  public columns_Summary: (ColDef | ColGroupDef)[
  ];

  public fromDate_Summary: NgbDate | null;
  public toDate_Summary: NgbDate | null;
  public maxDate_Summary: NgbDate = new NgbDate(
    new Date().getFullYear(),
    new Date().getMonth() + 1,
    new Date().getDate()
  );



  public hoveredDate_Summary: NgbDate | null = null;

  constructor(
    private auditorProductivityDashboardService: AuditorProductivityDashboardService, public userService: UserService,
    private formatter: NgbDateParserFormatter
  ) {


    this.components = {
      loadingCellRenderer: this.loadingCellRenderer,
    };

    this.SelectedTabRepository = new LocalStoreRepository<any>(AUDITOR_PRODUCTIVITY_SELECTED_TAB);
    var tab = this.SelectedTabRepository.load();
    console.log("sss loaf " + tab);
    if (tab != undefined) {
      this.selectedTab = tab;
 
    } 

    this.teams$ = this.userService.getTeams();

    this.AHTDateProcessedRepository = new LocalStoreRepository<any>(AUDITOR_PRODUCTIVITY_AHT_DATE_PROCESSED);
    this.AHTDateProcessedRepository.clear();
    //if (this.AHTDateProcessedRepository.load()) {
    //  this.fromDate_AHT = this.AHTDateProcessedRepository.load()?.fromDate_AHT;
    //  this.toDate_AHT = this.AHTDateProcessedRepository.load()?.toDate_AHT;
    //}

    this.SummaryDateProcessedRepository = new LocalStoreRepository<any>(AUDITOR_PRODUCTIVITY_SUMMARY_DATE_PROCESSED);
    this.SummaryDateProcessedRepository.clear();

    //if (this.SummaryDateProcessedRepository.load()) {
    //  this.fromDate_Summary = this.SummaryDateProcessedRepository.load()?.fromDate_Summary;
    //  this.toDate_Summary = this.SummaryDateProcessedRepository.load()?.toDate_Summary;
    //}


    this.InputGridFiltersRepository = new LocalStoreRepository<any>(AUDITOR_PRODUCTIVITY_INPUT_TABLE_FILTERS);
    this.InputGridColumnsStateRepository = new LocalStoreRepository<any>(AUDITOR_PRODUCTIVITY_INPUT_COLUMNS_STATE);

    this.InputGridColumnsStateRepository.clear();
    this.InputGridRequestColumnsState = this.InputGridColumnsStateRepository.load();
    this.loadedFilters_Input = this.InputGridFiltersRepository.load();

    this.columns_Input = this.getInputColumns();


    this.AHTGridFiltersRepository = new LocalStoreRepository<any>(AUDITOR_PRODUCTIVITY_AHT_TABLE_FILTERS);
    this.AHTGridColumnsStateRepository = new LocalStoreRepository<any>(AUDITOR_PRODUCTIVITY_AHT_COLUMNS_STATE);

    this.AHTGridColumnsStateRepository.clear();
    this.AHTGridRequestColumnsState = this.AHTGridColumnsStateRepository.load();
    this.loadedFilters_AHT = this.AHTGridFiltersRepository.load();

    this.columns_AHT = this.getAHTColumns();


    this.SummaryGridFiltersRepository = new LocalStoreRepository<any>(AUDITOR_PRODUCTIVITY_SUMMARY_TABLE_FILTERS);
    this.SummaryGridColumnsStateRepository = new LocalStoreRepository<any>(AUDITOR_PRODUCTIVITY_SUMMARY_COLUMNS_STATE);

    this.SummaryGridColumnsStateRepository.clear();
    this.SummaryGridRequestColumnsState = this.SummaryGridColumnsStateRepository.load();
    this.loadedFilters_Summary = this.SummaryGridFiltersRepository.load();

    this.columns_Summary = this.getSummaryColumns();

  }

  ngOnInit(): void {
    this.loadedFilters_Input = this.InputGridFiltersRepository.load();
    this.loadedFilters_AHT = this.AHTGridFiltersRepository.load();
    this.loadedFilters_Summary = this.SummaryGridFiltersRepository.load();
  
  }

 
  private resizeCurrentGrid(): void {
    switch (this.selectedTab) {
      case '1':
        if (this.gridApi_Input) {
          this.gridApi_Input.sizeColumnsToFit();
        }
        break;
      //case '2':
      //  if (this.gridApi_AHT) {
      //    this.gridApi_AHT.sizeColumnsToFit();
      //  }
      //  break;
      //case '3':
      //  if (this.gridApi_Summary) {
      //    this.gridApi_Summary.sizeColumnsToFit();
      //  }
      //  break;
      //case 3:
      //  if (this.gridApi_SummaryFacility) {
      //    this.gridApi_SummaryFacility.sizeColumnsToFit();
      //  }
      //  break;
    }
  }

  private getInputColumns(): ColDef[] {
    return [
      {
        field: INPUT_GRID_COLUMNS.ID,
        headerName: "Id",
        minWidth: 100,
        filterParams: {
          ...this.getInputDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
        },
        cellRenderer: this.loadingCellRenderer,
      },
      {
        field: INPUT_GRID_COLUMNS.START_TIME,
        headerName: 'Start time',
        minWidth: 200,
        valueFormatter: (params: any) => params.value ? this.dateFormatter(params.value) : "",
        filter: 'agDateColumnFilter',
        filterParams: {
          ...this.getInputDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          suppressAndOrCondition: true,
          filterOptions: ['equals', 'greaterThan', 'lessThan', 'inRange'],
        },
      },
      {
        field: INPUT_GRID_COLUMNS.COMPLETION_TIME,
        headerName: 'Completion time',
        minWidth: 200,
        valueFormatter: (params: any) => params.value ? this.dateFormatter(params.value) : "",
        filter: 'agDateColumnFilter',
        filterParams: {
          ...this.getInputDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          suppressAndOrCondition: true,
          filterOptions: ['equals', 'greaterThan', 'lessThan', 'inRange'],
        },
      },
      {
        field: INPUT_GRID_COLUMNS.USER_FULLNAME,
        headerName: "Name",
        minWidth: 300,
        filterParams: {
          ...this.getInputDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
        }
      },
      {
        field: INPUT_GRID_COLUMNS.FACILITY_NAME,
        headerName: "Facility Name",
        minWidth: 300,
        filterParams: {
          ...this.getInputDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
        }
      },
      {
        field: INPUT_GRID_COLUMNS.TYPE_OF_AUDIT,
        headerName: "Type of Audit",
        minWidth: 300,
        filterParams: {
          ...this.getInputDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
        }
      },
      {
        field: INPUT_GRID_COLUMNS.NO_OF_RESIDENTS,
        headerName: "No. of Residents",
        minWidth: 200,
        filter: false,
        flex: 1, cellClass: 'center-cell',
      },
      //TARGET_AHT
      {
        field: INPUT_GRID_COLUMNS.TARGET_AHT,
        headerName: "Targer AHT",
        minWidth: 200,
        filter: false,
        flex: 1, cellClass: 'center-cell',
      },
      {
        field: INPUT_GRID_COLUMNS.NO_OF_FILTERED_AUDITS_ALL_TYPE,
        headerName: "Number of filtered audits for all type of audits (Excel and Sharp)",
        minWidth: 300,
        filter: false,
        flex: 1, cellClass: 'center-cell',
      },
      {
        field: INPUT_GRID_COLUMNS.HANDLING_TIME,
        headerName: "Handling Time",
        minWidth: 300,
        filter: false,
      },
      {
        field: INPUT_GRID_COLUMNS.AHT_PER_AUDIT,
        headerName: "AHT per Audit",
        minWidth: 300,
        filter: false,
        flex: 1, cellClass: 'center-cell',
      },
      {
        field: INPUT_GRID_COLUMNS.HOUR,
        headerName: "Hour",
        minWidth: 300,
        filter:false
        
      },
      {
        field: INPUT_GRID_COLUMNS.NO_OF_FILTERED_AUDITS,
        headerName: "No. of Filtered Audits",
        minWidth: 300,
        filter: false,
        flex: 1, cellClass: 'center-cell',
      },
      {
        field: INPUT_GRID_COLUMNS.FINAL_AHT,
        headerName: "Final AHT",
        minWidth: 300,
        filter: false,
        flex: 1, cellClass: 'center-cell',
      },
      {
        field: INPUT_GRID_COLUMNS.MONTH,
        headerName: "Month",
        minWidth: 300,
        filter: false
      },

    ];
  }
  public onTeamChanged(event) {
    auditorProductivityInputFilterValues.team = event;
    this.gridApi_Input?.onFilterChanged();
  }

  public onTeamAHTChanged(team) {
    auditorProductivityAHTPerAuditTypeFilterValues.team = team;
    this.gridApi_AHT?.onFilterChanged();
  }
  private getAHTColumns(): ColDef[] {
    return [
      {
        field: AHT_GRID_COLUMNS.USER_FULLNAME,
        headerName: "Name",
        minWidth: 300,
        filterParams: {
          ...this.getAHTDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
        },
        cellRenderer: this.loadingCellRenderer,
      },
      {
        field: AHT_GRID_COLUMNS.FACILITY_NAME,
        headerName: "Facility Name",
        minWidth: 300,
        filterParams: {
          ...this.getAHTDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
        }
      },
      {
        field: AHT_GRID_COLUMNS.TYPE_OF_AUDIT,
        headerName: "Type of Audit",
        minWidth: 300,
        filterParams: {
          ...this.getAHTDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
        }
      },
      {
        field: AHT_GRID_COLUMNS.NO_OF_RESIDENTS,
        headerName: "No. of Residents",
        minWidth: 200,
        filter:false,
        flex: 1, cellClass: 'center-cell',
      },
      {
        field: AHT_GRID_COLUMNS.TARGET_AHT,
        headerName: "Targer AHT",
        minWidth: 200,
        filter: false,
        flex: 1, cellClass: 'center-cell',
      },
      {
        field: AHT_GRID_COLUMNS.FINAL_AHT,
        headerName: "AHT",
        minWidth: 200,
        valueFormatter: (params: any) => params.value ? this.formatSecondsToTime(params.value) : "",
        menuTabs: [],
        filter: false,
        sortable: false,
      },
      {
        field: AHT_GRID_COLUMNS.AHT_PER_RESIDENT,
        headerName: "AHT Per Resident",
        minWidth: 200,
        valueFormatter: (params: any) => params.value ? this.formatSecondsToTime(params.value) : "",
        menuTabs: [],
        filter: false,
        sortable: false,
      },
      {
        field: AHT_GRID_COLUMNS.NO_OF_FILTERED_AUDITS,
        headerName: "# of Filtered Audits",
        minWidth: 200,
        menuTabs: [],
        filter: false,
        sortable: false,
        flex: 1, cellClass: 'center-cell',
      },
    ];
  }

  private getSummaryColumns(): (ColDef | ColGroupDef)[] {
    return [
      {
        field: SUMMARY_GRID_COLUMNS.USER_FULLNAME,
        headerName: "Name",
        minWidth: 300,
        filterParams: {
          ...this.getSummaryDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
        },        
       },
      {
        field: SUMMARY_GRID_COLUMNS.IS_DTS,
        headerName: "Day Time Saving",
        minWidth: 200,
        filter: false,
        sortable: false,
        flex: 1, cellClass: 'center-cell',
        valueFormatter: (params: any) => params.value == null ? "" : (params.value == true ? "Yes" : "No"),
      },
      {
        field: SUMMARY_GRID_COLUMNS.FACILITY_NAME,
        headerName: "Facility Name",
        minWidth: 300,
        filterParams: {
          ...this.getSummaryDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
        },
        cellRenderer: this.loadingCellRenderer,
      },
      {
        field: SUMMARY_GRID_COLUMNS.TYPE_OF_AUDIT,
        headerName: "Type of Audit",
        minWidth: 300,
        filterParams: {
          ...this.getSummaryDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
        }
      },
      {
        field: SUMMARY_GRID_COLUMNS.NUMBER_OF_RESIDENTS,
        headerName: "No. Of Resident",
        minWidth: 200,
        filter:false,
        flex: 1, cellClass: 'center-cell',
      },
      {
        field: SUMMARY_GRID_COLUMNS.TARGET_AHT,
        headerName: "Targer AHT",
        minWidth: 200,
        filter: false,
        flex: 1, cellClass: 'center-cell',
      },
      {
        headerName: "7:00 - 10:00",
        children: [
          {
            field: SUMMARY_GRID_COLUMNS.NO_OF_FILTERED_AUDITS_8to10,
            headerName: "No. of Filtered Audit",
            minWidth: 200,
            menuTabs: [],
            filter: false,
            sortable: false,
            flex: 1, cellClass: 'center-cell',
          },
          {
            field: SUMMARY_GRID_COLUMNS.UTILIZED_TIME_8to10,
            headerName: "Utilized Time",
            minWidth: 200,
            valueFormatter: (params: any) => params.value ? this.formatSecondsToTime(params.value) : "",
            menuTabs: [],
            filter: false,
            sortable: false,
          },
        ]
      },
      {
        headerName: "10:00 - 1:00",
        children: [
          {
            field: SUMMARY_GRID_COLUMNS.NO_OF_FILTERED_AUDITS_10to1,
            headerName: "No. of Filtered Audit",
            minWidth: 200,
            menuTabs: [],
            filter: false,
            sortable: false,
            flex: 1, cellClass: 'center-cell',
          },
          {
            field: SUMMARY_GRID_COLUMNS.UTILIZED_TIME_10to1,
            headerName: "Utilized Time",
            minWidth: 200,
            valueFormatter: (params: any) => params.value ? this.formatSecondsToTime(params.value) : "",
            menuTabs: [],
            filter: false,
            sortable: false,
          },
        ]
      },
      {
        headerName: "1:00 - 3:00",
        children: [
          {
            field: SUMMARY_GRID_COLUMNS.NO_OF_FILTERED_AUDITS_1to3,
            headerName: "No. of Filtered Audit",
            minWidth: 200,
            menuTabs: [],
            filter: false,
            sortable: false,
            flex: 1, cellClass: 'center-cell',
          },
          {
            field: SUMMARY_GRID_COLUMNS.UTILIZED_TIME_1to3,
            headerName: "Utilized Time",
            minWidth: 200,
            valueFormatter: (params: any) => params.value ? this.formatSecondsToTime(params.value) : "",
            menuTabs: [],
            filter: false,
            sortable: false,
          },
        ]
      },
      {
        headerName: "3:00 - (4:00) 5:00 ",
        children: [
          {
            field: SUMMARY_GRID_COLUMNS.NO_OF_FILTERED_AUDITS_3to5,
            headerName: "No. of Filtered Audit",
            minWidth: 200,
            menuTabs: [],
            filter: false,
            sortable: false,
            flex: 1, cellClass: 'center-cell',
          },
          {
            field: SUMMARY_GRID_COLUMNS.UTILIZED_TIME_3to5,
            headerName: "Utilized Time",
            minWidth: 200,
            valueFormatter: (params: any) => params.value ? this.formatSecondsToTime(params.value) : "",
            menuTabs: [],
            filter: false,
            sortable: false,
          },
        ]
      },
      {
        headerName: "Overtime Hours",
        children: [
          {
            field: SUMMARY_GRID_COLUMNS.NO_OF_FILTERED_AUDITS_OVERTIME_HOURS,
            headerName: "No. of Filtered Audit",
            minWidth: 200,
            menuTabs: [],
            filter: false,
            sortable: false,
            flex: 1, cellClass: 'center-cell',
          },
          {
            field: SUMMARY_GRID_COLUMNS.UTILIZED_TIME_OVERTIME_HOURS,
            headerName: "Utilized Time",
            minWidth: 200,
            valueFormatter: (params: any) => params.value ? this.formatSecondsToTime(params.value) : "",
            menuTabs: [],
            filter: false,
            sortable: false,
            flex: 1, cellClass: 'center-cell',
          },
        ]
      },
      {
        headerName: "Total No. of Filtered Audit",
        children: [
          {
            field: SUMMARY_GRID_COLUMNS.TOTAL_NO_OF_FILTERED_AUDITS,
            headerName: "",
            minWidth: 200,
            menuTabs: [],
            filter: false,
            sortable: false,
            flex: 1, cellClass: 'center-cell',
          },
        ]
      },
      {
        headerName: "Total Utilized Time",
        children: [
          {
            field: SUMMARY_GRID_COLUMNS.TOTAL_UTILIZED_TIME,
            headerName: "",
            minWidth: 200,
            valueFormatter: (params: any) => params.value ? this.formatSecondsToTime(params.value) : "",
            menuTabs: [],
            filter: false,
            sortable: false,
          },
        ]
      },

    ];
  }

  public rowModelType = ROW_MODEL_TYPE;
  public serverSideStoreType = SERVER_SIDE_STORE_TYPE;
  public rowBuffer = ROW_BUFFER;
  public cacheOverflowSize = CACHE_OVERFLOW_SIZE;
  public cacheBlockSize = 25; //25
  public maxConcurrentDatasourceRequests= MAX_CONCURRENT_DATASOURCE_REQUESTS;
  public infiniteInitialRowCount = INFINITE_INITIAL_ROWCOUNT;
  public maxBlocksInCache = MAX_BLOCKS_IN_CACHE;

  

  private gridApi_Input: GridApi;
  private gridColumnApi_Input: ColumnApi;
  dataSourceInput: any = null;
  public onGridReady_Input(event: GridReadyEvent): void {

    this.gridsInitialized.input = true;
    this.gridApi_Input = event.api;
    this.gridColumnApi_Input = event.columnApi;

    this.gridApi_Input.sizeColumnsToFit();

    if (this.InputGridRequestColumnsState) {
      this.gridColumnApi_Input.applyColumnState({ state: this.InputGridRequestColumnsState, applyOrder: true, });
    } else {
      this.gridColumnApi_Input.applyColumnState({
        defaultState: { sort: null },
        state: [{ colId: INPUT_GRID_COLUMNS.START_TIME, sort: 'desc' }]
      });
    }

    if (this.loadedFilters_Input) {
      this.gridApi_Input.setFilterModel({
        ...this.loadedFilters_Input,
        [INPUT_GRID_COLUMNS.START_TIME]: {
          type: "equals",
          dateFrom: moment(new Date()).format("YYYY-MM-DD"),
          dateTo: null,
        },
      });
      //this.gridApi_Input.setFilterModel(this.loadedFilters_Input);
    } else {
      this.gridApi_Input.setFilterModel({
        [INPUT_GRID_COLUMNS.START_TIME]: {
          type: "equals",
          dateFrom: moment(new Date()).format("YYYY-MM-DD"),
          dateTo: null,
        },
      });
    }
    if (this.dataSourceInput == null) {
      this.dataSourceInput = {
        getRows: (params) =>
          this.auditorProductivityDashboardService
            .getInputData(
              params.filterModel,
              params.sortModel,
              params.startRow,
              params.endRow,
              auditorProductivityInputFilterValues
            )
            .subscribe((data) => {
              console.log(data);
              const currentLastRow = params.startRow + data.length;
              const lastRowIndex =
                currentLastRow < params.endRow ? currentLastRow : -1;
              params.successCallback(data, lastRowIndex);
            }),
      };
    }
    event.api.setDatasource(this.dataSourceInput);
  }

  public onFilterChanged_Input(grid: GridOptions) {
    var filterModel = grid.api.getFilterModel();
    this.InputGridFiltersRepository.save(filterModel);
  }

  public onColumnVisible_Input(e) {
    this.saveColumnsState_Input();
  }

  public onColumnMoved_Input(e) {
    this.saveColumnsState_Input();
  }

  private saveColumnsState_Input() {
    this.InputGridColumnsStateRepository.save(this.gridColumnApi_Input.getColumnState());
  }

  public onColumnResized_Input(e) {
    this.InputGridColumnsStateRepository.save(this.gridColumnApi_Input.getColumnState());
  }

  public onSortChanged_Input(e) {
    this.InputGridColumnsStateRepository.save(this.gridColumnApi_Input.getColumnState());
  }

  private valueFormatter(params: ValueFormatterParams): string {
    try {
      const column = params.value;

      return column === null ? 'Empty' : column;
    } catch {
      return '';
    }
  }

  private getInputDefaultSetFilterParams(
    setValues: (filters: any, params: SetFilterValuesFuncParams) => void
  ): any {
    return {
      buttons: FILTER_PARAMS_BUTTONS,
      closeOnApply: true,
      cellHeight: FILTER_HEIGHT,
      values: (params: SetFilterValuesFuncParams) => {
        this.auditorProductivityDashboardService
          .getInputfilters(params.colDef.field, this.gridApi_Input.getFilterModel(), auditorProductivityInputFilterValues)
          .subscribe((filters: IFilterOption[]) => {

            const columnFilters = this.InputGridFiltersRepository.load()?.[params.colDef.field];

            const filterValues = auditorProductivityInputFilterValues?.[params.colDef.field]?.filter((filter) => columnFilters?.values?.includes(filter.value)) ?? [];

            filters = filterValues?.concat(filters?.filter((f) =>
              f.id
                ? !filterValues?.map((fv) => fv.id)?.includes(f.id)
                : !filterValues?.map((fv) => fv.value)?.includes(f.value)))
              ?? [];

            auditorProductivityInputFilterValues = { ...auditorProductivityInputFilterValues, [params.colDef.field]: filters };

            setValues(
              filters.map((filter: IFilterOption) => filter.value),
              params
            );
          });
      },
      refreshValuesOnOpen: true,
    };
  }

  private dateFormatter(params: any): string {
    return moment(params.replace("Z", "")).format(MM_DD_YYYY_HH_MM_A_SLASH);
  }
  onNavChange(changeEvent: NgbNavChangeEvent) {
    this.SelectedTabRepository.save(changeEvent.nextId);

      switch (this.selectedTab) {
        case '1':
          if (!this.gridsInitialized.input && this.inputGrid) {
            this.inputGrid.gridReady.emit();
          }
          break;
        //case '2':
        //  if (!this.gridsInitialized.aht && this.ahtGrid) {
        //    this.ahtGrid.gridReady.emit();
        //  }
        //  break;
        //case '3':
        //  if (!this.gridsInitialized.summary && this.summaryGrid) {
        //    this.summaryGrid.gridReady.emit();
        //  }
        //  break;
        case '2':
          if (!this.summaryFacilityTab) {
            this.summaryFacilityTab.onTabActivated();
          }
          
          break;
      }

      setTimeout(() => this.resizeCurrentGrid(), 50);

  }

  private gridApi_AHT: GridApi;
  private gridColumnApi_AHT: ColumnApi;
  dataSourceAHT: any;
  public onGridReady_AHT(event: GridReadyEvent): void {

    this.gridsInitialized.aht = true;
    this.gridApi_AHT = event.api;
    this.gridColumnApi_AHT = event.columnApi;

    this.gridApi_AHT.sizeColumnsToFit();

    if (this.AHTGridRequestColumnsState) {
      this.gridColumnApi_AHT.applyColumnState({ state: this.AHTGridRequestColumnsState, applyOrder: true, });
    }

    if (this.loadedFilters_AHT) {
      this.gridApi_AHT.setFilterModel(this.loadedFilters_AHT);
    } else {
      this.gridColumnApi_AHT.applyColumnState({
        defaultState: { sort: null }
      });
    }

    auditorProductivityAHTPerAuditTypeFilterValues.dateProcessed = this.getFilterDateProcessed_AHT(); 

    var dataSourceAHT = {
        getRows: (params) =>
          this.auditorProductivityDashboardService
            .getAHTPerAuditTypeData(
              params.filterModel,
              params.sortModel,
              params.startRow,
              params.endRow,
              auditorProductivityAHTPerAuditTypeFilterValues
            )
            .subscribe((data) => {
              console.log(data);
              const currentLastRow = params.startRow + data.length;
              const lastRowIndex =
                currentLastRow < params.endRow ? currentLastRow : -1;

              // Calculate and set pinned bottom row
              if (data && data.length > 0) {
                //const totals = this.calculateAHTTotals(data);
                //this.gridApi_AHT.setPinnedTopRowData([totals]);
              }

              params.successCallback(data, 10000);
            }),
      

    }
    event.api.setDatasource(dataSourceAHT);
  }

  public onFilterChanged_AHT(grid: GridOptions) {
    var filterModel = grid.api.getFilterModel();
    this.AHTGridFiltersRepository.save(filterModel);
  }

  public onColumnVisible_AHT(e) {
    this.saveColumnsState_AHT();
  }

  public onColumnMoved_AHT(e) {
    this.saveColumnsState_AHT();
  }

  private saveColumnsState_AHT() {
    this.AHTGridColumnsStateRepository.save(this.gridColumnApi_AHT.getColumnState());
  }

  public onColumnResized_AHT(e) {
    this.AHTGridColumnsStateRepository.save(this.gridColumnApi_AHT.getColumnState());
  }

  public onSortChanged_AHT(e) {
    this.AHTGridColumnsStateRepository.save(this.gridColumnApi_AHT.getColumnState());
  }

  private getAHTDefaultSetFilterParams(
    setValues: (filters: any, params: SetFilterValuesFuncParams) => void
  ): any {
    return {
      buttons: FILTER_PARAMS_BUTTONS,
      closeOnApply: true,
      cellHeight: FILTER_HEIGHT,
      values: (params: SetFilterValuesFuncParams) => {

        auditorProductivityAHTPerAuditTypeFilterValues.dateProcessed = this.getFilterDateProcessed_AHT(); 

        this.auditorProductivityDashboardService
          .getAHTPerAuditTypefilters(params.colDef.field, this.gridApi_AHT.getFilterModel(), auditorProductivityAHTPerAuditTypeFilterValues)
          .subscribe((filters: IFilterOption[]) => {

            const columnFilters = this.AHTGridFiltersRepository.load()?.[params.colDef.field];

            const filterValues = auditorProductivityAHTPerAuditTypeFilterValues?.[params.colDef.field]?.filter((filter) => columnFilters?.values?.includes(filter.value)) ?? [];

            filters = filterValues?.concat(filters?.filter((f) =>
              f.id
                ? !filterValues?.map((fv) => fv.id)?.includes(f.id)
                : !filterValues?.map((fv) => fv.value)?.includes(f.value)))
              ?? [];

            auditorProductivityAHTPerAuditTypeFilterValues = { ...auditorProductivityAHTPerAuditTypeFilterValues, [params.colDef.field]: filters };

            setValues(
              filters.map((filter: IFilterOption) => filter.value),
              params
            );
          });
      },
      refreshValuesOnOpen: true,
    };
  }

  formatSecondsToTime(seconds: number): string {
    // Calculate hours, minutes, and remaining seconds
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;

    // Pad each part with leading zeros
    const pad = (num: number): string => num.toString().padStart(2, '0');

    // Return formatted string
    return `${pad(hours)}:${pad(minutes)}:${pad(secs)}`;
  }

  public onDateSelection_AHT(date: NgbDate) {
    if (!this.fromDate_AHT && !this.toDate_AHT) {
      this.fromDate_AHT = date;
    } else if (
      this.fromDate_AHT &&
      !this.toDate_AHT &&
      date &&
      (date.after(this.fromDate_AHT) || date == this.fromDate_AHT)
    ) {
      this.toDate_AHT = date;
    } else {
      this.toDate_AHT = null;
      this.fromDate_AHT = date;
    }
  }

  public isHovered_AHT(date: NgbDate) {
    return (
      this.fromDate_AHT &&
      !this.toDate_AHT &&
      this.hoveredDate_AHT &&
      date.after(this.fromDate_AHT) &&
      date.before(this.hoveredDate_AHT)
    );
  }

  public isInside_AHT(date: NgbDate) {
    return this.toDate_AHT && date.after(this.fromDate_AHT) && date.before(this.toDate_AHT);
  }

  public isRange_AHT(date: NgbDate) {
    return (
      date.equals(this.fromDate_AHT) ||
      (this.toDate_AHT && date.equals(this.toDate_AHT)) ||
      this.isInside_AHT(date) ||
      this.isHovered_AHT(date)
    );
  }

  public rangeFormat(dateFrom: NgbDate | null, dateTo: NgbDate | null): string {
    let dateRange: string = "";

    if (dateFrom && dateTo) {
      dateRange = `${this.formatter.format(dateFrom)} - ${this.formatter.format(
        dateTo
      )}`;
    } else {
      dateRange = this.formatter.format(dateFrom);
    }

    return dateRange;
  }

  onSearchClick_AHT() {
    this.AHTDateProcessedRepository.save({
      fromDate_AHT: this.fromDate_AHT,
      toDate_AHT: this.toDate_AHT
    });
    auditorProductivityAHTPerAuditTypeFilterValues.dateProcessed = this.getFilterDateProcessed_AHT(); 

    if (this.gridApi_AHT) {
      this.dataSourceAHT = null;
      this.gridApi_AHT.onFilterChanged();
    }
  }

  getFilterDateProcessed_AHT() {
    const today = new Date();
    today.setDate(today.getDate() + 1); // Add 1 day

    if (!this.fromDate_AHT) {
      this.fromDate_AHT = this.maxDate_AHT;
      this.toDate_AHT = new NgbDate(
        today.getFullYear(),
        today.getMonth() + 1,
        today.getDate()
      );
      this.AHTDateProcessedRepository.save({
        fromDate_AHT: this.fromDate_AHT,
        toDate_AHT: this.toDate_AHT
      });
    }

    return this.toDate_AHT
      ? JSON.stringify({
        firstCondition: transformDate({
          dateFrom: this.formatter.format(this.fromDate_AHT),
          dateTo: this.formatter.format(this.toDate_AHT),
          type: "inRange",
        }),
      })
      : JSON.stringify({
        firstCondition: transformDate({
          dateFrom: this.formatter.format(this.fromDate_AHT),
          dateTo: null,
          type: "equals",
        }),
      });
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
  public onTeamSummaryPerAuditorChanged(team) {
    auditorProductivitySummaryPerAuditorFilterValues.team = team;
    this.gridApi_Summary?.onFilterChanged();
  }
  private gridApi_Summary: GridApi;
  private gridColumnApi_Summary: ColumnApi;
  dataSourceSummary: any;
  public onGridReady_Summary(event: GridReadyEvent): void {
    if (this.gridsInitialized.summary) {
      event.api.setDatasource(this.dataSourceSummary);
      return;
    }

    this.gridsInitialized.summary = true;
    this.gridApi_Summary = event.api;
    this.gridColumnApi_Summary = event.columnApi;

    this.gridApi_Summary.sizeColumnsToFit();

    if (this.SummaryGridRequestColumnsState) {
      this.gridColumnApi_Summary.applyColumnState({ state: this.SummaryGridRequestColumnsState, applyOrder: true, });
    }

    if (this.loadedFilters_Summary) {
      this.gridApi_Summary.setFilterModel(this.loadedFilters_Summary);
    } else {
      this.gridColumnApi_Summary.applyColumnState({
        defaultState: { sort: null }
      });
    }

    auditorProductivitySummaryPerAuditorFilterValues.dateProcessed = this.getFilterDateProcessed_Summary();

    this.dataSourceSummary = {
      getRows: (params) =>
        this.auditorProductivityDashboardService
          .getSummaryPerAuditorData(
            params.filterModel,
            params.sortModel,
            params.startRow,
            params.endRow,
            auditorProductivitySummaryPerAuditorFilterValues
          )
          .subscribe((data) => {

            const currentLastRow = params.startRow + data.length;
            const lastRowIndex =
              currentLastRow < params.endRow ? currentLastRow : -1;

            // Calculate and set pinned bottom row
            if (data && data.length > 0) {
              const totals = this.calculateSummaryTotals(data);
              this.gridApi_Summary.setPinnedBottomRowData([totals]);
            }

            params.successCallback(data, lastRowIndex);
          }),
    };
    event.api.setDatasource(this.dataSourceSummary);
  }

  public onFilterChanged_Summary(grid: GridOptions) {
    var filterModel = grid.api.getFilterModel();
    this.SummaryGridFiltersRepository.save(filterModel);
  }

  public onColumnVisible_Summary(e) {
    this.saveColumnsState_Summary();
  }

  public onColumnMoved_Summary(e) {
    this.saveColumnsState_Summary();
  }

  private saveColumnsState_Summary() {
    this.SummaryGridColumnsStateRepository.save(this.gridColumnApi_Summary.getColumnState());
  }

  public onColumnResized_Summary(e) {
    this.SummaryGridColumnsStateRepository.save(this.gridColumnApi_Summary.getColumnState());
  }

  public onSortChanged_Summary(e) {
    this.SummaryGridColumnsStateRepository.save(this.gridColumnApi_Summary.getColumnState());
  }

  private getSummaryDefaultSetFilterParams(
    setValues: (filters: any, params: SetFilterValuesFuncParams) => void
  ): any {
    return {
      buttons: FILTER_PARAMS_BUTTONS,
      closeOnApply: true,
      cellHeight: FILTER_HEIGHT,
      values: (params: SetFilterValuesFuncParams) => {

        auditorProductivitySummaryPerAuditorFilterValues.dateProcessed = this.getFilterDateProcessed_Summary();

        this.auditorProductivityDashboardService
          .getSummaryPerAuditorfilters(params.colDef.field, this.gridApi_Summary.getFilterModel(), auditorProductivitySummaryPerAuditorFilterValues)
          .subscribe((filters: IFilterOption[]) => {

            const columnFilters = this.SummaryGridFiltersRepository.load()?.[params.colDef.field];

            const filterValues = auditorProductivitySummaryPerAuditorFilterValues?.[params.colDef.field]?.filter((filter) => columnFilters?.values?.includes(filter.value)) ?? [];

            filters = filterValues?.concat(filters?.filter((f) =>
              f.id
                ? !filterValues?.map((fv) => fv.id)?.includes(f.id)
                : !filterValues?.map((fv) => fv.value)?.includes(f.value)))
              ?? [];

            auditorProductivitySummaryPerAuditorFilterValues = { ...auditorProductivitySummaryPerAuditorFilterValues, [params.colDef.field]: filters };

            setValues(
              filters.map((filter: IFilterOption) => filter.value),
              params
            );
          });
      },
      refreshValuesOnOpen: true,
    };
  }

  public onDateSelection_Summary(date: NgbDate) {
    if (!this.fromDate_Summary && !this.toDate_Summary) {
      this.fromDate_Summary = date;
    } else if (
      this.fromDate_Summary &&
      !this.toDate_Summary &&
      date &&
      (date.after(this.fromDate_Summary) || date == this.fromDate_Summary)
    ) {
      this.toDate_Summary = date;
    } else {
      this.toDate_Summary = null;
      this.fromDate_Summary = date;
    }
  }

  public isHovered_Summary(date: NgbDate) {
    return (
      this.fromDate_Summary &&
      !this.toDate_Summary &&
      this.hoveredDate_Summary &&
      date.after(this.fromDate_Summary) &&
      date.before(this.hoveredDate_Summary)
    );
  }

  public isInside_Summary(date: NgbDate) {
    return this.toDate_Summary && date.after(this.fromDate_Summary) && date.before(this.toDate_Summary);
  }

  public isRange_Summary(date: NgbDate) {
    return (
      date.equals(this.fromDate_Summary) ||
      (this.toDate_Summary && date.equals(this.toDate_Summary)) ||
      this.isInside_Summary(date) ||
      this.isHovered_Summary(date)
    );
  }

  onSearchClick_Summary() {
    this.SummaryDateProcessedRepository.save({
      fromDate_Summary: this.fromDate_Summary,
      toDate_Summary: this.toDate_Summary
    });
    auditorProductivitySummaryPerAuditorFilterValues.dateProcessed = this.getFilterDateProcessed_Summary();

    if (this.gridApi_Summary) {
      this.gridApi_Summary.onFilterChanged();
    }
  }

  getFilterDateProcessed_Summary() {
    const today = new Date();
    today.setDate(today.getDate() + 1); // Add 1 day

    if (!this.fromDate_Summary) {
      this.fromDate_Summary = this.maxDate_Summary;
      this.toDate_Summary = new NgbDate(
        today.getFullYear(),
        today.getMonth() + 1,
        today.getDate()
      );
      this.SummaryDateProcessedRepository.save({
        fromDate_Summary: this.fromDate_Summary,
        toDate_Summary: this.toDate_Summary
      });
    }

    return this.toDate_Summary
      ? JSON.stringify({
        firstCondition: transformDate({
          dateFrom: this.formatter.format(this.fromDate_Summary),
          dateTo: this.formatter.format(this.toDate_Summary),
          type: "inRange",
        }),
      })
      : JSON.stringify({
        firstCondition: transformDate({
          dateFrom: this.formatter.format(this.fromDate_Summary),
          dateTo: null,
          type: "equals",
        }),
      });
  }

  private calculateAHTTotals(data: any[]): any {
    const totals = {
      [AHT_GRID_COLUMNS.USER_FULLNAME]: 'Grand Total',
      [AHT_GRID_COLUMNS.FACILITY_NAME]: '',
      [AHT_GRID_COLUMNS.TYPE_OF_AUDIT]: '',
      [AHT_GRID_COLUMNS.FINAL_AHT]: 0,
      [AHT_GRID_COLUMNS.NO_OF_FILTERED_AUDITS]: 0
    };

    const detailRows = data.filter(row => row[AHT_GRID_COLUMNS.TYPE_OF_AUDIT] !== '');

    detailRows.forEach(row => {
      totals[AHT_GRID_COLUMNS.FINAL_AHT] += row[AHT_GRID_COLUMNS.FINAL_AHT] || 0;
      totals[AHT_GRID_COLUMNS.NO_OF_FILTERED_AUDITS] += row[AHT_GRID_COLUMNS.NO_OF_FILTERED_AUDITS] || 0;
    });

    return totals;
  }

  private calculateSummaryTotals(data: any[]): any {
    const totals = {
      [SUMMARY_GRID_COLUMNS.USER_FULLNAME]: 'Grand Total',
      [SUMMARY_GRID_COLUMNS.FACILITY_NAME]: '',
      [SUMMARY_GRID_COLUMNS.TYPE_OF_AUDIT]: '',
      [SUMMARY_GRID_COLUMNS.NO_OF_FILTERED_AUDITS_8to10]: 0,
      [SUMMARY_GRID_COLUMNS.UTILIZED_TIME_8to10]: 0,
      [SUMMARY_GRID_COLUMNS.NO_OF_FILTERED_AUDITS_10to1]: 0,
      [SUMMARY_GRID_COLUMNS.UTILIZED_TIME_10to1]: 0,
      [SUMMARY_GRID_COLUMNS.NO_OF_FILTERED_AUDITS_1to3]: 0,
      [SUMMARY_GRID_COLUMNS.UTILIZED_TIME_1to3]: 0,
      [SUMMARY_GRID_COLUMNS.NO_OF_FILTERED_AUDITS_3to5]: 0,
      [SUMMARY_GRID_COLUMNS.UTILIZED_TIME_3to5]: 0,
      [SUMMARY_GRID_COLUMNS.NO_OF_FILTERED_AUDITS_OVERTIME_HOURS]: 0,
      [SUMMARY_GRID_COLUMNS.UTILIZED_TIME_OVERTIME_HOURS]: 0,
      [SUMMARY_GRID_COLUMNS.TOTAL_NO_OF_FILTERED_AUDITS]: 0,
      [SUMMARY_GRID_COLUMNS.TOTAL_UTILIZED_TIME]: 0
    };

    const detailRows = data.filter(row => row[SUMMARY_GRID_COLUMNS.TYPE_OF_AUDIT] !== '');

    detailRows.forEach(row => {
      Object.keys(totals).forEach(key => {
        if (key !== SUMMARY_GRID_COLUMNS.USER_FULLNAME && key !== SUMMARY_GRID_COLUMNS.FACILITY_NAME && key !== SUMMARY_GRID_COLUMNS.TYPE_OF_AUDIT) {
          totals[key] += row[key] || 0;
        }
      });
    });

    return totals;
  }

  onExportToExcel_Input() {
    this.gridApi_Input.exportDataAsExcel({
      fileName: "Auditor Productivity - Input",
      sheetName: "Input"
    });
  }


}
