import { Component, OnInit, ViewChild } from "@angular/core";
import * as moment from "moment";
import {
  ColDef,
  FilterChangedEvent,
  FilterModifiedEvent,
  GridOptions,
  ICellRendererParams,
  RowClassRules,
  RowClickedEvent,
  SetFilterValuesFuncParams,
  ValueFormatterParams,
  SelectionChangedEvent,
} from "ag-grid-community";
import { ActivatedRoute, Router } from "@angular/router";
import { BtnCellRenderer } from "../component/three-dots-menu/three-dots-menu.component";

import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import "ag-grid-enterprise";
import { AuthService } from "../services/auth.service";
import { RolesEnum } from "../models/roles.model";
import {
  AuditFiltersModel,
  DatePeriodEnum,
  IFilterOption,
} from "../models/audits/audit.filters.model";
import { AuditServiceApi } from "../services/audit-api.service";
import { DragulaService } from "ng2-dragula";
import { Subscription } from "rxjs";
import { AuditsGridButtonsRendererComponent } from "./buttons/AuditsGridButtonsRenderer.component";
import {
  AuditStatuses,
  AuditStateEnum,
  IUserTimeZone,
  AuditStatusEnum,
} from "../models/audits/audits.model";
import { ReasonComponent } from "./reason/reason.component";
import { LocalStorageService } from "../services/local-storage.service";
import {
  CACHE_OVERFLOW_SIZE,
  FILTER_HEIGHT,
  FILTER_PARAMS_BUTTONS,
  INFINITE_INITIAL_ROWCOUNT,
  MAX_BLOCKS_IN_CACHE,
  MAX_CONCURRENT_DATASOURCE_REQUESTS,
  ROW_BUFFER,
  ROW_MODEL_TYPE,
  SERVER_SIDE_STORE_TYPE,
} from "../common/constants/grid";
import { AuditService } from "./services/audit.service";
import {
  MM_DD_YYYY_HH_MM_A_SLASH,
  MM_DD_YYYY_SLASH,
} from "../common/constants/date-constants";
import { LocalStoreRepository } from "../services/repository/local-store-repository";
import { IDefaultFilters } from "../common/types/types";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { CriteriaPdfFilterPopupComponent } from "./criteria-pdf-filter-popup/criteria-pdf-filter-popup.component";
import { first } from "rxjs/operators";
import { AuditOptionsRendererComponent } from "./options/AuditOptionsRenderer.component";
import { Title } from "@angular/platform-browser";
import { ToastrService } from "ngx-toastr";
import { IMessageResponse, MessageStatusEnum } from "../models/common.model";
import {
  AUDITS_COLUMNS_STATE,
  AUDITS_COLUMNS_STATE_ARCHIVED,
  AUDITS_COLUMNS_STATE_DELETED,
  AUDIT_GRID_COLUMNS,
  AUDIT_TABLE_FILTERS,
  AUDIT_TABLE_FILTERS_ARCHIVED,
  AUDIT_TABLE_FILTERS_DELETED,
  DATE_PERIOD_FILTER,
  DATE_PERIOD_FILTER_ARCHIVED,
  DATE_PERIOD_FILTER_DELETED,
} from "../common/constants/audit-constants";
import { UserService } from "../services/user.service";
import { getUserLocalDateTime } from "../common/helpers/dates-helper";
import { ConfirmationDialogComponent } from "../shared/confirmation-dialog/confirmation-dialog.component";

let auditFilterValues: AuditFiltersModel = {};

@Component({
  templateUrl: "./audits.component.html",
  styleUrls: ["./audits.component.scss"],
})
export class AuditsComponent implements OnInit {
  public gridColumns: any[];
  public subscriptions = new Subscription();

  @ViewChild("columnsDropdown") columnsDropdown;

  public gridApi;
  public gridColumnApi;

  public frameworkComponents;
  public columnDefs;
  public defaultColDef: ColDef = {
    filter: true,
    sortable: true,
    resizable: true,
    menuTabs: ["filterMenuTab"],
    filterParams: {
      suppressSorting: false,
      caseSensitive: false,
      defaultToNothingSelected: true,
      suppressSelectAll: true,
    },
  };
  public rowSelection;
  public rowModelType;
  public serverSideStoreType;
  public paginationPageSize;
  public rowBuffer;
  public cacheBlockSize;
  public cacheOverflowSize;
  public maxConcurrentDatasourceRequests;
  public infiniteInitialRowCount;
  public maxBlocksInCache;
  public getRowNodeId;
  public components;
  public rowData: [];
  public datePeriodFilter: number;

  public isAdmin: boolean;
  public isAuditor: boolean;
  public isReviewer: boolean;
  public isFacility: boolean;
  public isAuditSelected: boolean = false;

  public haveRightToViewPdf: boolean;

  public userId: string;
  public sharpUserId: number;
  public userName: string;
  private userTimeZone: IUserTimeZone;
  public totalRecordsCount: number = 0;

  public rowClassRules: RowClassRules = {
    "unready-study": (params) => params.data?.imagesCount <= 0,
  };
  routeState: any;
  filterModel: any;
  sortModel: any;
  auditsColumnsStateModel: any;

  private filtersRepository: any;
  private loadedFilters: IDefaultFilters;

  private auditsColumnsStateRepository: any;
  private datePeriodFilterRepository: LocalStoreRepository<number>;
  public state = AuditStateEnum.Active;
  public filteredStatus: AuditStatusEnum | undefined = undefined;

  constructor(
    private http: HttpClient,
    private auditServiceApi: AuditServiceApi,
    private auditService: AuditService,
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private authService: AuthService,
    private userService: UserService,
    //private dragulaService: DragulaService,
    private localStorageService: LocalStorageService,
    private modalService: NgbModal,
    private titleService: Title,
    private toastr: ToastrService,
  ) {
    this.router.routeReuseStrategy.shouldReuseRoute = () => false;

    this.activatedRoute.params.subscribe((params) => {
      let state = params["state"];
      let filteredByStatus = params["filteredByStatus"];

      if (state == "archived") {
        this.state = AuditStateEnum.Archived;
      } else if (state == "deleted") {
        this.state = AuditStateEnum.Deleted;
      } else {
        this.state = AuditStateEnum.Active;
      }

      if (filteredByStatus) {
        this.filteredStatus = filteredByStatus;
      }
    });

    if (this.state == AuditStateEnum.Active) {
      this.filtersRepository = new LocalStoreRepository<any>(
        AUDIT_TABLE_FILTERS
      );
      this.auditsColumnsStateRepository = new LocalStoreRepository<any>(
        AUDITS_COLUMNS_STATE
      );
      this.datePeriodFilterRepository = new LocalStoreRepository<number>(
        DATE_PERIOD_FILTER
      );
    } else if (this.state == AuditStateEnum.Archived) {
      this.filtersRepository = new LocalStoreRepository<any>(
        AUDIT_TABLE_FILTERS_ARCHIVED
      );
      this.auditsColumnsStateRepository = new LocalStoreRepository<any>(
        AUDITS_COLUMNS_STATE_ARCHIVED
      );
      this.datePeriodFilterRepository = new LocalStoreRepository<number>(
        DATE_PERIOD_FILTER_ARCHIVED
      );

      this.filtersRepository.clear();
      this.auditsColumnsStateRepository.clear();
      this.datePeriodFilterRepository.clear();

    } else if (this.state == AuditStateEnum.Deleted) {
      this.filtersRepository = new LocalStoreRepository<any>(
        AUDIT_TABLE_FILTERS_DELETED
      );
      this.auditsColumnsStateRepository = new LocalStoreRepository<any>(
        AUDITS_COLUMNS_STATE_DELETED
      );
      this.datePeriodFilterRepository = new LocalStoreRepository<number>(
        DATE_PERIOD_FILTER_DELETED
      );

      this.filtersRepository.clear();
      this.auditsColumnsStateRepository.clear();
      this.datePeriodFilterRepository.clear();
    }

    this.userId = this.authService.getCurrentUserId();
    this.sharpUserId = this.authService.getCurrentUserSharpId();
    this.userName = this.authService.getCurrentUserName();

    if (!this.filteredStatus) {
      this.loadedFilters = this.filtersRepository.load();
    }

    if (!this.loadedFilters) {
      this.datePeriodFilter = this.datePeriodFilterRepository.load();
    } /*else {
      this.setDatePeriod(this.loadedFilters)
    }*/

    this.getUserTimeZone(() => {
      if (this.loadedFilters) {
        this.setDatePeriod(this.loadedFilters);
      }
    });

    this.isAdmin = this.authService.isUserInRole(RolesEnum.Admin);
    this.isAuditor = this.authService.isUserInRole(RolesEnum.Auditor);
    this.isReviewer = this.authService.isUserInRole(RolesEnum.Reviewer);
    this.isFacility = this.authService.isUserInRole(RolesEnum.Facility);


    auditFilterValues = {
      status: Object.values(AuditStatuses).filter(status => {
        if (this.isFacility) {
          if (status.id == AuditStatuses.Submitted.id) {
            return status;
          } else {
            return null;
          }
        } else {
          return status;
        }
        
      }).map((status) => {
        return { id: status.id, value: status.label };
      }),
      auditorName: this.auditService.isCurrentUserOnlyAuditor()
        ? [{ id: this.sharpUserId, value: this.userName }]
        : null,
    };

    this.auditsColumnsStateModel = this.auditsColumnsStateRepository.load();

   
    this.haveRightToViewPdf = this.auditService.haveRightToViewPdf();

    this.columnDefs = this.getColumns();

    this.frameworkComponents = {
      btnCellRenderer: BtnCellRenderer,
      auditsGridButtonsRendererComponent: AuditsGridButtonsRendererComponent,
      auditOptionsRendererComponent: AuditOptionsRendererComponent,
      reasonComponent: ReasonComponent,
    };

    this.rowBuffer = ROW_BUFFER;
    //this.rowSelection = "single";
   
    this.rowModelType = ROW_MODEL_TYPE;
    this.serverSideStoreType = SERVER_SIDE_STORE_TYPE;
    this.cacheBlockSize = 20;
    this.cacheOverflowSize = CACHE_OVERFLOW_SIZE;
    this.maxConcurrentDatasourceRequests = MAX_CONCURRENT_DATASOURCE_REQUESTS;
    this.infiniteInitialRowCount = INFINITE_INITIAL_ROWCOUNT;
    this.maxBlocksInCache = MAX_BLOCKS_IN_CACHE;

    this.getRowNodeId = function (item) {
      return item.id;
    };
    this.components = {
      loadingCellRenderer: this.loadingCellRenderer,
    };

    this.auditService.recordsCount$.subscribe(
      (totalCount) => (this.totalRecordsCount = totalCount)
    );
  }

  ngOnInit(): void {
    this.titleService.setTitle("SHARP audits");
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  public onGeneratePdfClick() {
    const modalRef = this.modalService.open(CriteriaPdfFilterPopupComponent, {
      modalDialogClass: "generate-pdf-modal",
    });
  }

  public OnDeleteSelectedAudits() {
    const selectedData = this.gridApi.getSelectedRows();
    console.log(selectedData);
    if (selectedData == undefined)
      return;

    if (selectedData.length == 0)
      return;

    var ids = selectedData.map(x => x.id);
    console.log(selectedData);

    const modalRef = this.modalService.open(ConfirmationDialogComponent, {
      modalDialogClass: "custom-modal",
    });
    modalRef.componentInstance.confirmationBoxTitle = "Confirmation?";
    modalRef.componentInstance.confirmationMessage = `Are you sure you want to delete this audit(s)?`;

    modalRef.result.then(
      (userResponse) => {
        if (userResponse) {
          this.auditServiceApi.deleteAudits(ids).subscribe(
            (response) => {
              this.gridApi?.onFilterChanged();
            },
            (error: HttpErrorResponse) => {
              if (error.error?.errorMessage) {
                this.toastr.error("Error while deleting Audit data. Error: " + error.error?.errorMessage);
              } else {
                this.toastr.error("Error while deleting Audit data.");
              }
            }
          );
        }
      },
      () => {
        /*user closed the message box*/
      }
    );

  }
  public OnArchiveSelectedAudits() {
    const selectedData = this.gridApi.getSelectedRows();

    if (selectedData == undefined)
      return;

    if (selectedData.length == 0)
      return;

    var ids = selectedData.map(x => x.id);

    const modalRef = this.modalService.open(ConfirmationDialogComponent, {
      modalDialogClass: "custom-modal",
    });
    modalRef.componentInstance.confirmationBoxTitle = "Confirmation?";
    modalRef.componentInstance.confirmationMessage = `Are you sure you want to archive this audit(s)?`;

    modalRef.result.then(
      (userResponse) => {
        if (userResponse) {
          this.auditServiceApi.archiveAudits(ids).subscribe(
            (response) => {
              this.gridApi?.onFilterChanged();
            },
            (error: HttpErrorResponse) => {
              if (error.error?.errorMessage) {
                this.toastr.error("Error while archiving Audit data. Error: " + error.error?.errorMessage);
              } else {
                this.toastr.error("Error while archiving Audit data.");
              }
            }
          );
        }
      },
      () => {
        /*user closed the message box*/
      }
    );
  }
  onColumnVisible(e) {
    this.getGridColumns();
    this.saveColumnsState();
  }

  onColumnMoved(e) {
    this.getGridColumns();
    this.saveColumnsState();
  }

  onSortChanged(e) {
    this.getGridColumns();
    this.saveColumnsState();
  }
 
  onColumnsStateSave() {
    this.saveColumnsState();
    this.columnsDropdown.close();
  }

  saveColumnsState() {
    const columnsState = this.gridColumns.map((column) => column.state);

    this.gridColumnApi.applyColumnState({
      state: columnsState,
      applyOrder: true,
    });

    this.auditsColumnsStateRepository.save(this.gridColumnApi.getColumnState());
  }

  onColumnsStateReset() {
    this.gridColumnApi.resetColumnState();

    this.auditsColumnsStateRepository.save(this.gridColumnApi.getColumnState());

    this.onColumnsStateCancel();
  }

  onColumnsStateCancel() {
    this.getGridColumns();
    this.columnsDropdown.close();
  }

  onColumnVisibleStateChange(column) {
    const columnIndex = this.gridColumns.findIndex(
      (gridColumn) => gridColumn.colId === column.colId
    );
    if (columnIndex === -1) {
      return;
    }

    this.gridColumns[columnIndex].state.hide =
      !this.gridColumns[columnIndex].state.hide;
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

  statusFiltertRenderer = (params) => {
    if (params.value == "(Select All)") return params.value;

    const column = params.value;

    const status = this.auditService.getStatusByLabel(column);

    return `<span style="color: ${status.color}">` + status.label + "</span>";
  };

  public resetFilters() {
    this.datePeriodFilter = DatePeriodEnum.Today;

    this.auditServiceApi.clearCachedFilters();

    this.gridApi.purgeServerSideCache(null);
    this.gridApi.setFilterModel(null);
    this.gridApi.onFilterChanged();
  }

  public onTodayClick() {
    this.setDatePeriodFilter(DatePeriodEnum.Today);
  }

  public onLastSevenDaysClick() {
    this.setDatePeriodFilter(DatePeriodEnum.LastSevenDays);
  }

  private setDatePeriodFilter(datePeriod: DatePeriodEnum) {
    this.getUserTimeZone(() => {
      switch (datePeriod) {
        case DatePeriodEnum.LastSevenDays:
          this.setLastSevenDaysFilter();
          break;
        case DatePeriodEnum.Today:
          this.setTodayFilter();
          break;
        default:
          if (this.state == AuditStateEnum.Active) {
            this.setTodayFilter();
          } else {
            this.resetFilters();
          }
          break;
      }
    });
  }

  setTodayFilter() {
    const format = "YYYY-MM-DD";

    let filter = this.gridApi.getFilterModel();

    //const userLocalDateTime = getUserLocalDateTime(this.userTimeZone);

    this.gridApi.setFilterModel({
      ...filter,
      [AUDIT_GRID_COLUMNS.AUDIT_DATE]: {
        type: "equals",
        dateFrom: moment(this.userTimeZone?.userTimeZoneDateTime).format(
          format
        ), //moment(userLocalDateTime, MM_DD_YYYY_HH_MM_A_SLASH).format(format),
        dateTo: null,
      },
    });

    this.datePeriodFilter = DatePeriodEnum.Today;
    this.datePeriodFilterRepository.save(this.datePeriodFilter);
  }

  setLastSevenDaysFilter() {
    const format = "YYYY-MM-DD";

    let filter = this.gridApi.getFilterModel();

    //const userLocalDateTime = getUserLocalDateTime(this.userTimeZone);

    this.gridApi.setFilterModel({
      ...filter,
      [AUDIT_GRID_COLUMNS.AUDIT_DATE]: {
        type: "inRange",
        dateFrom: moment(this.userTimeZone?.userTimeZoneDateTime)
          .subtract(7, "d")
          .format(format), //moment(userLocalDateTime, MM_DD_YYYY_HH_MM_A_SLASH).subtract(7, 'd').format(format),
        dateTo: moment(this.userTimeZone?.userTimeZoneDateTime).format(format), //moment(userLocalDateTime, MM_DD_YYYY_HH_MM_A_SLASH).format(format),
      },
    });

    this.datePeriodFilter = DatePeriodEnum.LastSevenDays;
    this.datePeriodFilterRepository.save(this.datePeriodFilter);
  }

  setDefaultStatusFilter() {
    let filter = this.gridApi.getFilterModel();

    let defaultStatuses = [];

    if (this.isAuditor) {
      defaultStatuses = [
        AuditStatuses.InProgress.label,
        AuditStatuses.Disapproved.label,
      ];
    }

    if (this.isReviewer) {
      defaultStatuses.push(AuditStatuses.WaitingForApproval.label);
      defaultStatuses.push(AuditStatuses.Reopened.label);
      defaultStatuses.push(AuditStatuses.Approved.label);
    }
    if (this.isFacility) {
      defaultStatuses.push(AuditStatuses.Submitted.label);
    }


    if (this.filteredStatus) {
      defaultStatuses = [];

      if (this.filteredStatus == AuditStatuses.Triggered.id) {
        defaultStatuses = [AuditStatuses.Triggered.label];
      } else if (this.filteredStatus == AuditStatuses.InProgress.id) {
        defaultStatuses = [AuditStatuses.InProgress.label];
      } else if (this.filteredStatus == AuditStatuses.WaitingForApproval.id) {
        defaultStatuses = [AuditStatuses.WaitingForApproval.label];
      } else if (this.filteredStatus == AuditStatuses.Disapproved.id) {
        defaultStatuses = [AuditStatuses.Disapproved.label];
      } else if (this.filteredStatus == AuditStatuses.Approved.id) {
        defaultStatuses = [AuditStatuses.Approved.label];
      } else if (this.filteredStatus == AuditStatuses.Reopened.id) {
        defaultStatuses = [AuditStatuses.Reopened.label];
      } else if (this.filteredStatus == AuditStatuses.Submitted.id) {
        defaultStatuses = [AuditStatuses.Submitted.label];
      } else if (this.filteredStatus == AuditStatuses.Duplicated.id) {
        defaultStatuses = [AuditStatuses.Duplicated.label];
      }
    }

    if (!defaultStatuses || defaultStatuses.length === 0) {
      return;
    }

    this.gridApi.setFilterModel({
      ...filter,
      [AUDIT_GRID_COLUMNS.STATUS]: {
        filterType: "set",
        values: defaultStatuses,
      },
    });
  }

  setDefaultUserFilter() {
    if (this.auditService.isCurrentUserOnlyAuditor()) {
      let filter = this.gridApi.getFilterModel();
      this.gridApi.setFilterModel({
        ...filter,
        auditorName: { filterType: "set", values: [this.userName] },
      });
    }
  }

  public onRowClicked(event: RowClickedEvent) {
    if (this.state != AuditStateEnum.Active) return;

    const {
      data: { id, auditorUserId },
      rowIndex,
    } = event;

    if (!this.auditService.haveRightToViewAudit(auditorUserId)) {
      return;
    }

    this.localStorageService.setRowIndex(rowIndex);

    this.router.navigate(["audits/" + id]);
  }

  filterButtonVisible: boolean = false;

  public onFilterChanged(event: FilterChangedEvent) {
    var filterModel = event.api.getFilterModel();

    this.setDatePeriod(filterModel);

    this.filtersRepository.save(filterModel);

    if (this.datePeriodFilter !== DatePeriodEnum.Undefined) {
      this.datePeriodFilterRepository.save(this.datePeriodFilter);
    }
  }

  public onFilterModified(event: FilterModifiedEvent) {}

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
          (colState) => colState.colId === column.colId
        ),
      }));
  }

  public onGridReady(params) {
    this.gridApi = params.api;
    this.gridColumnApi = params.columnApi;

    this.getGridColumns();

    if (this.auditsColumnsStateModel) {
      this.gridColumnApi.applyColumnState({
        state: this.auditsColumnsStateModel,
        applyOrder: true,
      });
    }

    if (this.loadedFilters) {
      this.gridApi.setFilterModel(this.loadedFilters);
    } else {
      this.setDatePeriodFilter(this.datePeriodFilterRepository.load());
      this.gridColumnApi.applyColumnState({
        defaultState: { sort: null },
        state: [{ colId: AUDIT_GRID_COLUMNS.AUDIT_DATE, sort: "desc" }],
      });
      this.setDefaultStatusFilter();
      this.setDefaultUserFilter();
    }

    var auditServiceApi = this.auditServiceApi;
    var auditService = this.auditService;
    var state = this.state;
    var dataSource = {
      rowCount: null,
      getRows: function (params) {
        auditServiceApi
          .getAudits(
            params.startRow,
            params.endRow,
            params.sortModel,
            params.filterModel,
            auditFilterValues,
            state
          )
          .subscribe(
            (data) => {
              const currentLastRow = params.startRow + data.items.length;

              let lastRowIndex =
                currentLastRow < params.endRow ? currentLastRow : -1;

              auditService.setTotalRecordsCount(data.totalCount);
              params.successCallback(data.items, lastRowIndex);
            },
            (error) => {
              console.log(error);
              params.failCallback();
            }
          );
      },
    };

    params.api.setDatasource(dataSource);
  }

  public onFirstDataRendered(params: any): void {
    params.api.sizeColumnsToFit();
  }

  public getContextMenuItems(params) {
    var result = [
      {
        name: "Clear selection",
        api: params.api,
        action: function () {
          this.api.deselectAll();
        },
      },
    ];
    return result;
  }

  private getColumns(): ColDef[] {
    const organization: ColDef = {
      field: "organizationName",
      headerName: "Organization",
      minWidth: 300,
      sortable: true,
      filter: true,
      resizable: true,
      checkboxSelection: this.state == 1 ? true : false,
      cellStyle: { color: '#263237', 'font-size': '16px', 'font-weight': '300', 'font-family':'HelveticaRegular'} ,
      filterParams: {
        ...this.getDefaultSetFilterParams((filters, params) => {
          params.success(filters);
        }),
        valueFormatter: this.valueFormatter,
      },
      cellRenderer: this.loadingCellRenderer,
    };

    const facility: ColDef = {
      field: "facilityName",
      headerName: "Facility",
      minWidth: 200,
      sortable: true,
      filter: true,
      resizable: true,
      filterParams: {
        ...this.getDefaultSetFilterParams((filters, params) => {
          params.success(filters);
        }),
        valueFormatter: this.valueFormatter,
        cellHeight: 50,
      },
    };

    const form: ColDef = {
      field: "formName",
      headerName: "Form",
      minWidth: 300,
      sortable: true,
      filter: true,
      resizable: true,
      filterParams: {
        ...this.getDefaultSetFilterParams((filters, params) => {
          params.success(filters);
        }),
        valueFormatter: this.valueFormatter,
        cellHeight: 50,
      },
    };

    const auditType: ColDef = {
      field: "auditType",
      headerName: "Audit Type",
      minWidth: 150,
      sortable: true,
      filter: true,
      resizable: true,
      filterParams: {
        ...this.getDefaultSetFilterParams((filters, params) => {
          params.success(filters);
        }),
        valueFormatter: this.valueFormatter,
      },
    };

    const auditDate: ColDef = {
      field: "auditDate",
      headerName: "Audit Date",
      minWidth: 200,
      sortable: true,
      sort: "desc",
      resizable: true,
      filter: "agDateColumnFilter",
      filterParams: {
        ...this.getDefaultSetFilterParams((filters, params) => {
          params.success(filters);
        }),
        suppressAndOrCondition: true,
        filterOptions: ["equals", "greaterThan", "lessThan", "inRange"],
      },
      cellRenderer: (data) => {
        if (!data.value) {
          return;
        }

        return moment(data.value).format(MM_DD_YYYY_HH_MM_A_SLASH);
      },
    };

    const auditorName: ColDef = {
      field: AUDIT_GRID_COLUMNS.AUDITOR_NAME,
      headerName: "Auditor Name",
      minWidth: 200,
      sortable: true,
      filter: true,
      resizable: true,
      filterParams: {
        ...this.getDefaultSetFilterParams((filters, params) => {
          params.success(filters);
        }),
        valueFormatter: this.valueFormatter,
      },
    };

    /*const unit: ColDef = {
      field: 'unit',
      headerName: 'Unit/Floor',
      minWidth: 150,
      sortable: true,
      filter: true,
      resizable: true,
      filterParams: {
        ...this.getDefaultSetFilterParams((filters, params) => {
          params.success(filters);
        }),
        valueFormatter: this.valueFormatter,
      },
    };*/

    const room: ColDef = {
      field: "room",
      headerName: "Room #",
      minWidth: 150,
      sortable: true,
      filter: true,
      resizable: true,
      filterParams: {
        ...this.getDefaultSetFilterParams((filters, params) => {
          params.success(filters);
        }),
        valueFormatter: this.valueFormatter,
        suppressSelectAll: true,
      },
    };

    const resident: ColDef = {
      field: "resident",
      headerName: "Resident",
      minWidth: 200,
      sortable: true,
      filter: true,
      resizable: true,
      filterParams: {
        ...this.getDefaultSetFilterParams((filters, params) => {
          params.success(filters);
        }),
        valueFormatter: this.valueFormatter,
        suppressSelectAll: true,
      },
    };

    const incidentDateFrom: ColDef = {
      field: "incidentDateFrom",
      headerName: "Filter Start Date",
      minWidth: 200,
      sortable: true,
      resizable: true,
      filter: "agDateColumnFilter",
      filterParams: {
        ...this.getDefaultSetFilterParams((filters, params) => {
          params.success(filters);
        }),
        suppressAndOrCondition: true,
        filterOptions: ["equals", "greaterThan", "lessThan", "inRange"],
      },
      cellRenderer: this.dateValueFormatter,
    };

    const incidentDateTo: ColDef = {
      field: "incidentDateTo",
      headerName: "Filter End Date",
      minWidth: 200,
      sortable: true,
      resizable: true,
      filter: "agDateColumnFilter",
      filterParams: {
        ...this.getDefaultSetFilterParams((filters, params) => {
          params.success(filters);
        }),
        suppressAndOrCondition: true,
        filterOptions: ["equals", "greaterThan", "lessThan", "inRange"],
      },
      cellRenderer: this.dateValueFormatter,
    };

    const lastDeletedDate: ColDef = {
      field: "lastDeletedDate",
      headerName: "Delete Date",
      minWidth: 200,
      sortable: true,
      sort: "desc",
      resizable: true,
      filter: "agDateColumnFilter",
      filterParams: {
        ...this.getDefaultSetFilterParams((filters, params) => {
          params.success(filters);
        }),
        suppressAndOrCondition: true,
        filterOptions: ["equals", "greaterThan", "lessThan", "inRange"],
      },
      cellRenderer: (data) => {
        if (!data.value) {
          return;
        }

        return moment(data.value).format(MM_DD_YYYY_HH_MM_A_SLASH);
      },
    };

    const deletedByUser: ColDef = {
      field: "deletedByUser",
      headerName: "Deleted By User",
      minWidth: 200,
      sortable: true,
      filter: true,
      resizable: true,
      filterParams: {
        ...this.getDefaultSetFilterParams((filters, params) => {
          params.success(filters);
        }),
        valueFormatter: this.valueFormatter,
      },
    };

    const compliance: ColDef = {
      field: "compliance",
      headerName: "Compliance %",
      minWidth: 170,
      sortable: true,
      filter: "agNumberColumnFilter",
      resizable: true,
      filterParams: {
        ...this.getDefaultSetFilterParams((filters, params) => {
          params.success(filters);
        }),
        suppressAndOrCondition: true,
        filterOptions: ["equals", "greaterThan", "lessThan", "inRange"],
      },
      valueFormatter: (params: any) => (params.value ? `${params.value}%` : ""),
    };

    const auditorsTime: ColDef = {
      field: "sentForApprovalDate",
      headerName: "Auditor\'s Time",
      minWidth: 320,
      sortable: false,
      resizable: true,
      filter: false,
      suppressMenu: true,
      cellRenderer: (params) => {
        let cellText: string = "";
        if (params && params.value && params.data && params.data.auditDate) {
          let date1 = new Date(params.value.replace('Z', ''));
          let date2 = new Date(params.data.auditDate.replace('Z', ''));
          let dif = Math.abs(Number(date1) - Number(date2));
          dif = Math.round((dif / 1000) / 60);
          cellText = this.minToDays(dif);
        }
        return cellText;
      },
    };

    const durationText: ColDef = {
      field: "auditCompletedDate",
      headerName: "Duration",
      minWidth: 320,
      sortable: false,
      resizable: true,
      filter: false,
      suppressMenu: true,
      cellRenderer: (params) => {
        let cellText: string = "";
        if (params && params.value && params.data && params.data.auditDate) {
          let date1 = new Date(params.value.replace('Z', ''));
          let date2 = new Date(params.data.auditDate.replace('Z', ''));
          let dif = Math.abs(Number(date1) - Number(date2));
          dif = Math.round((dif / 1000) / 60);
          cellText = this.minToDays(dif);
        }
        return cellText;
      },
    };

    const status: ColDef = {
      field: "status",
      headerName: "Status",
      minWidth: 240,
      sortable: true,
      filter: true,
      resizable: true,
      cellClass: "button-cell",
      filterParams: {
        buttons: ["reset", "cancel", "apply"],
        closeOnApply: true,
        cellHeight: FILTER_HEIGHT,
        values: auditFilterValues?.status?.map((status) => status.value),
        cellRenderer: this.statusFiltertRenderer,
        suppressSorting: true,
        suppressSelectAll: false,
      },
      cellRenderer: "reasonComponent",
    };

    const button: ColDef = {
      field: "button",
      headerName: "Actions",
      minWidth: 270,
      cellRenderer: "auditsGridButtonsRendererComponent",
      cellClass: "button-cell",
      filter: false,
      sortable: false,
      menuTabs: [],
    };

    const options: ColDef = {
      field: "options",
      headerName: "Opt-s",
      minWidth: 80,
      maxWidth: 80,
      cellRenderer: "auditOptionsRendererComponent",
      cellClass: "button-cell",
      filter: false,
      sortable: false,
      menuTabs: [],
    };

    if (this.state == AuditStateEnum.Active) {
      return [
        organization,
        facility,
        form,
        auditType,
        auditDate,
        auditorName,
        //unit,
        room,
        resident,
        incidentDateFrom,
        incidentDateTo,
        compliance,
        auditorsTime,
        durationText,
        status,
        button,
        options,
      ];
    } else if (this.state == AuditStateEnum.Archived) {
      return [
        organization,
        facility,
        form,
        auditType,
        auditDate,
        auditorsTime,
        durationText,
        resident,
        options,
      ];
    } else if (this.state == AuditStateEnum.Deleted) {
      return [
        organization,
        facility,
        form,
        auditType,
        auditDate,
        auditorsTime,
        durationText,
        resident,
        lastDeletedDate,
        deletedByUser,
        options,
      ];
    }
  }

  private valueFormatter(params: ValueFormatterParams): string {
    try {
      const column = params.value;

      return column === null ? "Empty" : column;
    } catch {
      return "";
    }
  }

  private dateValueFormatter(params: ValueFormatterParams): string {
    if (!params.value) {
      return;
    }

    return moment(params.value).format(MM_DD_YYYY_SLASH);
  }

  private setDatePeriod(filterModel: any) {
    const format = "YYYY-MM-DD";

    const userLocalDateTime = getUserLocalDateTime(this.userTimeZone);

    if (
      filterModel.auditDate &&
      filterModel.auditDate.type === "equals" &&
      moment(filterModel.auditDate.dateFrom, format).isSame(
        moment(userLocalDateTime, MM_DD_YYYY_HH_MM_A_SLASH).format(format)
      )
    ) {
      this.datePeriodFilter = DatePeriodEnum.Today;
    } else if (
      filterModel.auditDate &&
      filterModel.auditDate.type === "inRange" &&
      moment(filterModel.auditDate.dateFrom, format).isSame(
        moment(userLocalDateTime, MM_DD_YYYY_HH_MM_A_SLASH)
          .subtract(7, "d")
          .format(format)
      ) &&
      moment(filterModel.auditDate.dateTo, format).isSame(
        moment(userLocalDateTime, MM_DD_YYYY_HH_MM_A_SLASH).format(format)
      )
    ) {
      this.datePeriodFilter = DatePeriodEnum.LastSevenDays;
    } else {
      this.datePeriodFilter = DatePeriodEnum.Undefined;
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
        this.auditServiceApi
          .getAuditsFilters(
            params.colDef.field,
            this.gridApi.getFilterModel(),
            auditFilterValues,
            this.state
          )
          .subscribe((filters: IFilterOption[]) => {
            if (
              params.colDef.field == AUDIT_GRID_COLUMNS.AUDITOR_NAME &&
              this.auditService.isCurrentUserOnlyAuditor()
            ) {
              if (
                !filters.map((filter) => filter.id).includes(this.sharpUserId)
              ) {
                filters.unshift({ id: this.sharpUserId, value: this.userName });
              }
            }

            const columnFilters =
              this.filtersRepository.load()?.[params.colDef.field];

            const filterValues =
              auditFilterValues?.[params.colDef.field]?.filter((filter) =>
                columnFilters?.values?.includes(filter.value)
              ) ?? [];

            filters =
              filterValues?.concat(
                filters?.filter((f) =>
                  f.id
                    ? !filterValues?.map((fv) => fv.id)?.includes(f.id)
                    : !filterValues?.map((fv) => fv.value)?.includes(f.value)
                )
              ) ?? [];

            auditFilterValues = {
              ...auditFilterValues,
              [params.colDef.field]: filters,
            };

            setValues(
              filters.map((filter: IFilterOption) => filter.value),
              params
            );
          });
      },
      refreshValuesOnOpen: true,
    };
  }

  private getUserTimeZone(setDatePeriod: () => void) {
    this.userService
      .getUserTimeZone(this.sharpUserId)
      .pipe(first())
      .subscribe({
        next: (timeZone: IUserTimeZone) => {
          this.userTimeZone = timeZone;

          setDatePeriod();
        },
      });
  }

  public onSelectionChanged(event: SelectionChangedEvent) {
    let selectedData = event.api.getSelectedRows();

    if (selectedData != undefined && selectedData.length > 0) {
      this.isAuditSelected = true;
    } else {
      this.isAuditSelected = false;
    }
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
