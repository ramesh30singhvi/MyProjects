import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ReportsService } from '../../services/reports.service';
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
} from "ag-grid-community";

import {
  MM_DD_YYYY_HH_MM_A_SLASH,
  MM_DD_YYYY_SLASH,
} from "../../common/constants/date-constants";
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
} from "../../common/constants/grid";
import { IDefaultFilters } from "../../common/types/types";
import { BtnCellRenderer } from '../../component/three-dots-menu/three-dots-menu.component';
import { ReportFiltersModel } from '../../models/audits/audit.filters.model';
import { AuthService } from '../../services/auth.service';
import { Observable } from 'rxjs';
import { IOption } from '../../models/audits/audits.model';
import { PortalService } from '../../services/portal.service';
import { CreateSharpUser } from '../../models/users/users.model';

let portalReportFilterValues: ReportFiltersModel = {};


@Component({
  selector: 'app-portal-report',
  templateUrl: './portal-report.component.html',
  styleUrls: ['./portal-report.component.scss']
})
export class PortalReportComponent implements OnInit {

  @ViewChild("passElement") pass: ElementRef; 
  public gridColumns: any[];
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
  public gridApi;
  public gridColumnApi;

  public totalRecordsCount: number = 0;

  public rowClassRules: RowClassRules = {
    "unready-study": (params) => params.data?.imagesCount <= 0,
  };
  routeState: any;
  filterModel: any;
  sortModel: any;

  categories$: Observable<IOption[]>;
  constructor(private reportService: ReportsService, private portalService: PortalService, private authService: AuthService,) {
    this.columnDefs = this.getColumns();

    this.frameworkComponents = {
      btnCellRenderer: BtnCellRenderer
    };
    console.log("get categories");
    this.categories$ = this.reportService.getReportCategories();

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



  public onRowClicked(event: RowClickedEvent) {
 
  }
  ngOnInit(): void {


  }
 public  migrateAuditToReport() {
   this.portalService.migrateToReport().subscribe((result: any) => {
      console.log(result);

    });
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
          (colState) => colState.colId === column.colId
        ),
      }));
  }
  public pivotPanelShow: "always" | "onlyWhenPivoting" | "never" = "always";
  public onGridReady(params) {
    this.gridApi = params.api;
    this.gridColumnApi = params.columnApi;

    this.getGridColumns();



    //if (this.loadedFilters) {
    //  this.gridApi.setFilterModel(this.loadedFilters);
    //} else {
    //  this.setDatePeriodFilter(this.datePeriodFilterRepository.load());
    //  this.gridColumnApi.applyColumnState({
    //    defaultState: { sort: null },
    //    state: [{ colId: AUDIT_GRID_COLUMNS.AUDIT_DATE, sort: "desc" }],
    //  });
    //  this.setDefaultStatusFilter();
    //  this.setDefaultUserFilter();
    //}
    this.gridColumnApi.applyColumnState({
      defaultState: { sort: null },
      state: [{ colId: "Date", sort: "desc" }],
    });
    var reportServiceApi = this.portalService;

    let filter = this.gridApi.getFilterModel();
    console.log(params);
    console.log(filter);

    var dataSource = {
      rowCount: null,
      getRows: function (params) {
        reportServiceApi.getPortalReports(
          params.startRow,
          params.endRow,
          params.sortModel,
          params.filterModel,
          portalReportFilterValues
        ).subscribe(
            (data) => {
              const currentLastRow = params.startRow + data.items.length;
            console.log(data);
              let lastRowIndex =
                currentLastRow < params.endRow ? currentLastRow : -1;

            // auditService.setTotalRecordsCount(data.totalCount);
              params.successCallback(data.items, lastRowIndex);
            },
            (error) => {
              console.log(error);
              params.failCallback();
            }
          );
        //  .getAudits(
        //    params.startRow,
        //    params.endRow,
        //    params.sortModel,
        //    params.filterModel,
        //    auditFilterValues,
        //    state
        //  )
        //  .subscribe(
        //    (data) => {
        //      const currentLastRow = params.startRow + data.items.length;

        //      let lastRowIndex =
        //        currentLastRow < params.endRow ? currentLastRow : -1;

        //      auditService.setTotalRecordsCount(data.totalCount);
        //      params.successCallback(data.items, lastRowIndex);
        //    },
        //    (error) => {
        //      console.log(error);
        //      params.failCallback();
        //    }
        //  );
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
    //const organization: ColDef = {
    //  field: "organizationName",
    //  headerName: "Company",
    //  minWidth: 300,
    //  sortable: true,
    //  filter: false,
    //  resizable: true,
    //  suppressMenu: true,
    //  //checkboxSelection: true ,
    //  //cellStyle: { color: '#263237', 'font-size': '16px', 'font-weight': '300', 'font-family': 'HelveticaRegular' },
    // cellRenderer: this.loadingCellRenderer,
    //};

    const name: ColDef = {
      field: "reportName",
      headerName: "Report name",
      minWidth: 400,
      sortable: true,
      filter: false,
      resizable: true,
      suppressMenu: true,
      checkboxSelection: true,
      wrapText: true,     // <-- HERE
     // cellRenderer: this.loadingCellRenderer,
    };
    const compliance: ColDef = {
      field: "compliance",
      headerName: "Compliance %",
      minWidth: 170,
      sortable: true,
      filter: false,
      resizable: true,
      suppressMenu: true,
      wrapText: true,     // <-- HERE
      valueFormatter: (params: any) => (params.value > 0 ? `${params.value}%` : "")
      // cellRenderer: this.loadingCellRenderer,
    };
    const documentType: ColDef = {
      field: "highAlertCategoryPotentialAreas",
      headerName: "Document type",
      minWidth: 100,
      sortable: true,
      filter: false,
      resizable: true,
      suppressMenu: true
      // cellRenderer: this.loadingCellRenderer,
    };

    const date: ColDef = {
      field: "createdDate",
      headerName: "Date",
      minWidth: 200,
      sortable: true,
      sort: "desc",
      resizable: true,
      filter: false,
      suppressMenu: true,
      cellRenderer: (data) => {
        if (!data.value) {
          return;
        }

        return moment(data.value).format(MM_DD_YYYY_HH_MM_A_SLASH);
      },
    };

    return [
        name,
        compliance,
        date,
     
        documentType
  
      ];
   
  }
  public sendReportsByEmail() {
    const selectedData = this.gridApi.getSelectedRows();
    console.log(selectedData);
    if (selectedData == undefined)
      return;

    if (selectedData.length == 0)
      return;

    var ids = selectedData.map(x => x.id);
    console.log(selectedData);
    var useremails = [];
    useremails.push("inessabarkan+68@gmail.com");
    
   
    this.portalService.sendReports(ids, useremails).subscribe(
      (response) => {
        console.log(response);
      },
      (error) => {
     
      }
    );
  }
  public deleteReport() {

    this.reportService.deleteReport(16868).subscribe(
      (response) => {
        this.gridApi?.onFilterChanged();
      },
      (error) => {
        console.log(error)
      }
    );
  }

  public getReportsByPass() {

    console.log(this.pass.nativeElement.value);
    if (this.pass.nativeElement.value == undefined)
      return;

    this.portalService.getReportForUser("Acspro Facility - Training", this.pass.nativeElement.value).subscribe(rs => {
      console.log(rs);
    });
  }
  private valueFormatter(params: ValueFormatterParams): string {
    try {
      const column = params.value;

      return column === null ? "Empty" : column;
    } catch {
      return "";
    }
  }

  public downloadReport() {
 //   this.portalService.resetPassword().subscribe(() => { });
    this.portalService.downloadPdfReport("1199807", "ss");
  }

  protected mapUserData(): CreateSharpUser {
    return {
      firstName: 'c',
      lastName: '',
      timeZone: '',
      email: "inessabarkan+13@gmail.com",
      roles: [6],
      organizations: [52],
      unlimited: false,
      facilities: [],
      facilityUnlimited: false,
      status: 1,
      position: ""
    };
  }
  public exportFacilityRecipients() {
   
   // this.portalService.getPortalRoles().subscribe(() => { });

    //this.portalService.editUser({
    //  id: 812,
    //  ...this.mapUserData() 
    //}).subscribe(() => { });

    this.portalService.createUser("inessabarkan+13@gmail.com","SS",52,33434).subscribe(() => { });

   //   "roles": [6], "organizations": [52], "unlimited": false, "facilities": [], "facilityUnlimited": false, "status": 1, "position": "" }).subscribe(() => { });
  //this.authService.clientPortalAccess("inessabarkan@gmail.com",37, "InessaTest","lastName",
  //    "Acspro Facility - Training",
  //   "$2a$11$KLwqWiALCmBtOoKIg323sOOvDdx72y9GQY54zAmQrURT1YZeUlvI6").subscribe(() => { });
   // this.portalService.getHighAlertStatistics(760).subscribe(() => {});
    //this.reportService.exportFacilityRecipients().subscribe(() => {
    //});;
  }
}
