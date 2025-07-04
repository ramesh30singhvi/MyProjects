import { Component, OnInit } from "@angular/core";
import {
  ColDef,
  GridApi,
  GridReadyEvent,
  IDatasource,
  RowClickedEvent,
  SetFilterValuesFuncParams,
} from "ag-grid-community";
import { Title } from "@angular/platform-browser";
import { ReportsService } from "../services/reports.service";
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
import { Router } from "@angular/router";

@Component({
  selector: "app-reports",
  templateUrl: "./reports.component.html",
  styleUrls: ["./reports.component.scss"],
})
export class ReportsComponent implements OnInit {
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
  public frameworkComponents;

  public defaultColumn: ColDef = {
    minWidth: 300,
    resizable: true,
    filter: true,
    filterParams: {
      buttons: FILTER_PARAMS_BUTTONS,
      closeOnApply: true,
      cellHeight: FILTER_HEIGHT,
      values: (params: SetFilterValuesFuncParams) =>
        this.reportsService
          .getReportsFilters(params.colDef.field)
          .subscribe((filters) => params.success(filters)),
      refreshValuesOnOpen: true,
      defaultToNothingSelected: true,
    },
    menuTabs: ["filterMenuTab"],
    sortable: true,
  };

  public columns: ColDef[] = [
    {
      field: "name",
      headerName: "Name",
    },
  ];

  constructor(
    private reportsService: ReportsService,
    private titleService: Title,
    private router: Router
  ) {}

  ngOnInit() {
    this.titleService.setTitle("SHARP reports");
  }

  public gridColumnApi;
  public onGridReady(event: GridReadyEvent): void {
    this.gridApi = event.api;
    this.gridApi.sizeColumnsToFit();
    this.gridColumnApi = event.columnApi;

    this.gridColumnApi.applyColumnState({
      state: [{ colId: "name", sort: "asc" }],
    });

    const dataSource: IDatasource = {
      getRows: (params) =>
        this.reportsService
          .getReports(
            this.searchTerm,
            params.filterModel,
            params.sortModel,
            params.startRow,
            params.endRow
          )
          .subscribe((reports) => {
            reports.push({
              id: 1001,
              name: "Quarterly Fall Analysis and Trend",
              tableauUrl: "",
              reportUrl: "",
            });

            reports.push({
              id: 1002,
              name: "Quarterly Wound Analysis and Trend",
              tableauUrl: "",
              reportUrl: "",
            });

            reports.push({
              id: 1003,
              name: "Criteria Reports",
              tableauUrl: "",
              reportUrl: "",
            });

            const currentLastRow = params.startRow + reports.length;
            const lastRowIndex =
              currentLastRow < params.endRow ? currentLastRow : -1;
            params.successCallback(reports, lastRowIndex);
          }),
    };
    event.api.setDatasource(dataSource);
  }

  public onSearch(): void {
    this.gridApi.onFilterChanged();
  }
  public onRowClicked(event: RowClickedEvent) {
    const {
      data: { id },
      rowIndex,
    } = event;

    if (id == 1001) {
      // Quarterly Fall Analysis
      return this.router.navigate(["reports/fall"]);
    } else if (id == 1002) {
      // Quarterly Wound Analysis
      return this.router.navigate(["reports/wound"]);
    } else if (id == 1003) {
      // Criteria Reports
      return this.router.navigate(["reports/criteria"]);
    }

    this.router.navigate(["reports/" + id]);
  }

  public onSearchClear(): void {
    this.searchTerm = null;
    this.onSearch();
  }
}
