import { Component, OnInit, ViewChild } from '@angular/core';
import { NgbDate, NgbDateParserFormatter } from '@ng-bootstrap/ng-bootstrap';
import { AgGridAngular } from 'ag-grid-angular';
import { ColDef, ColGroupDef, ColumnApi, GridApi, GridOptions, GridReadyEvent, ICellRendererParams, SetFilterValuesFuncParams, ValueFormatterParams } from 'ag-grid-community';
import { Observable } from 'rxjs';
import { CACHE_OVERFLOW_SIZE, FILTER_HEIGHT, FILTER_PARAMS_BUTTONS, INFINITE_INITIAL_ROWCOUNT, MAX_BLOCKS_IN_CACHE, MAX_CONCURRENT_DATASOURCE_REQUESTS, ROW_BUFFER, ROW_MODEL_TYPE, SERVER_SIDE_STORE_TYPE } from '../../common/constants/grid';
import { transformDate } from '../../common/helpers/dates-helper';
import { IDefaultFilters } from '../../common/types/types';
import { IFilterOption } from '../../models/audits/audit.filters.model';
import { IOption } from '../../models/audits/audits.model';
import { AuditorProductivitySummaryPerFacilityFiltersModel } from '../../models/dashboard/auditor-productivity-dashboard.model';
import { AuditorProductivityDashboardService } from '../../services/auditor-productivity-dashboard.service';
import { OrganizationService } from '../../services/organization.service';
import { LocalStoreRepository } from '../../services/repository/local-store-repository';
import { AUDITOR_PRODUCTIVITY_SUMMARYFACILITY_COLUMNS_STATE, AUDITOR_PRODUCTIVITY_SUMMARYFACILITY_DATE_PROCESSED, AUDITOR_PRODUCTIVITY_SUMMARYFACILITY_TABLE_FILTERS, SUMMARYFACILITY_GRID_COLUMNS } from '../common/sumarryFacilityGrid';
import { SUMMARY_GRID_COLUMNS } from '../common/summaryGrid';



let auditorProductivitySummaryPerFacilityFilterValues: AuditorProductivitySummaryPerFacilityFiltersModel = {};


@Component({
  selector: 'app-summary-facility',
  templateUrl: './summary-facility.component.html',
  styleUrls: ['./summary-facility.component.scss']
})
export class SummaryFacilityComponent implements OnInit {
  organizations$: Observable<IOption[]>;
  @ViewChild('summaryFacilityGrid') summaryFacilityGrid: AgGridAngular;
  public gridApi_SummaryFacility;
  public gridColumnApi_SummaryFacility;
  dataSourceSummaryFacility: any;
  private SummaryFacilityGridColumnsStateRepository: LocalStoreRepository<any>;
  private SummaryFacilityGridFiltersRepository: LocalStoreRepository<any>;
  private SummaryFacilityDateProcessedRepository: LocalStoreRepository<any>;

  private loadedFilters_SummaryFacility: IDefaultFilters;
  public components;
  isEditable: boolean = true;

  public rowModelType_SummaryFacility = ROW_MODEL_TYPE;
  public serverSideStoreType_SummaryFacility = SERVER_SIDE_STORE_TYPE;
  public rowBuffer_SummaryFacility = ROW_BUFFER;
  public cacheOverflowSize_SummaryFacility = CACHE_OVERFLOW_SIZE;
  public cacheBlockSize_Summary = 10;
  public maxConcurrentDatasourceRequests_SummaryFacility = MAX_CONCURRENT_DATASOURCE_REQUESTS;
  public infiniteInitialRowCount_SummaryFacility = INFINITE_INITIAL_ROWCOUNT;
  public maxBlocksInCache_SummaryFacility = MAX_BLOCKS_IN_CACHE;
  selectedTag:string ;
  formTags: string[] = [];
  public defaultColumn_SummaryFacility: (ColDef | ColGroupDef) = {
    resizable: true,
    filter: true,
    sortable: false,
    menuTabs: ['filterMenuTab'],
    filterParams: {
      suppressSorting: false,
      caseSensitive: false,
      defaultToNothingSelected: true,
      suppressSelectAll: true,
    },
  };

  public fromDate_SummaryFacility: NgbDate | null;
  public toDate_SummaryFacility: NgbDate | null;
  public maxDate_SummaryFacility: NgbDate = new NgbDate(
    new Date().getFullYear(),
    new Date().getMonth() + 1,
    new Date().getDate()
  );
  public hoveredDate_SummaryFacility: NgbDate | null = null;
  public basicColumn: (ColDef | ColGroupDef)[
  ];
  public columns_SummaryFacility: (ColDef | ColGroupDef)[
  ];
  constructor(private auditorProductivityDashboardService: AuditorProductivityDashboardService,
    private formatter: NgbDateParserFormatter, private organizationServiceApi: OrganizationService) {

    this.organizations$ = this.organizationServiceApi.getOrganizationOptions();

    this.auditorProductivityDashboardService.getFormTags().subscribe(x => {
      if (x != undefined)
        this.formTags = x;
    });
    this.components = {
      loadingCellRenderer: this.loadingCellRenderer,
    };

    this.SummaryFacilityDateProcessedRepository = new LocalStoreRepository<any>(AUDITOR_PRODUCTIVITY_SUMMARYFACILITY_DATE_PROCESSED);
    this.SummaryFacilityDateProcessedRepository.clear();
    this.SummaryFacilityGridFiltersRepository = new LocalStoreRepository<any>(AUDITOR_PRODUCTIVITY_SUMMARYFACILITY_TABLE_FILTERS);
    this.SummaryFacilityGridColumnsStateRepository = new LocalStoreRepository<any>(AUDITOR_PRODUCTIVITY_SUMMARYFACILITY_COLUMNS_STATE);
    this.basicColumn = this.getSummaryFacilityColumns();
  }

  showAllColumns() {
    const allColumns = this.gridColumnApi_SummaryFacility.getAllColumns();
    if (allColumns) {
      const allColIds = allColumns.map(col => col.getColId());
      this.gridColumnApi_SummaryFacility.setColumnsVisible(allColIds, true);
    }
  }
  //public get noColumn() {

  //  if (this.gridColumnApi_SummaryFacility == undefined)
  //    return false;
  //  console.log(this.gridColumnApi_SummaryFacility?.getAllGridColumns().length);
  //  return false;
  //}
  public onFormTagsChanged(tag: string) {

    const allColumns = this.gridColumnApi_SummaryFacility.getAllGridColumns()
      .filter(col => {
        const headerName = (col.getColDef().headerName || '').trim();
        return headerName.toLowerCase() !== "Facility Name".toLowerCase()
          && headerName.toLowerCase() !== "Total".toLowerCase();
      });

    if (allColumns.length == 0)
      return;

    this.selectedTag = tag;

    if (this.selectedTag.length == 0) {
      this.showAllColumns();
      return;
    }

    allColumns.forEach(col => {
      const headerName = col.getColDef().headerName;
      const shouldShow = tag === headerName;
      this.gridColumnApi_SummaryFacility.setColumnVisible(col.getColId(), shouldShow);
    });
  }
  ngOnInit(): void {
    this.loadedFilters_SummaryFacility = this.SummaryFacilityGridFiltersRepository.load();
  }
  onTabActivated() {
    console.log("Tab One Activated!");
    // Do something here
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

  onOrganizationChanged(organization: IOption): void {
    console.log(organization);
    if (!organization) {
      return;
    }
    var orgFilter = {
      id: organization.id, value: organization.name
    };

    console.log(orgFilter);

    auditorProductivitySummaryPerFacilityFilterValues.organization = orgFilter;

    this.gridApi_SummaryFacility?.onFilterChanged();

  }
  public onDateSelection_SummaryFacility(date: NgbDate) {
    if (!this.fromDate_SummaryFacility && !this.toDate_SummaryFacility) {
      this.fromDate_SummaryFacility = date;
    } else if (
      this.fromDate_SummaryFacility &&
      !this.toDate_SummaryFacility &&
      date &&
      (date.after(this.fromDate_SummaryFacility) || date == this.fromDate_SummaryFacility)
    ) {
      this.toDate_SummaryFacility = date;
    } else {
      this.toDate_SummaryFacility = null;
      this.fromDate_SummaryFacility = date;
    }
  }

  public isHovered_SummaryFacility(date: NgbDate) {
    return (
      this.fromDate_SummaryFacility &&
      !this.toDate_SummaryFacility &&
      this.hoveredDate_SummaryFacility &&
      date.after(this.fromDate_SummaryFacility) &&
      date.before(this.hoveredDate_SummaryFacility)
    );
  }

  public isInside_SummaryFacility(date: NgbDate) {
    return this.toDate_SummaryFacility && date.after(this.fromDate_SummaryFacility) && date.before(this.toDate_SummaryFacility);
  }

  public isRange_SummaryFacility(date: NgbDate) {
    return (
      date.equals(this.fromDate_SummaryFacility) ||
      (this.toDate_SummaryFacility && date.equals(this.toDate_SummaryFacility)) ||
      this.isInside_SummaryFacility(date) ||
      this.isHovered_SummaryFacility(date)
    );
  }

  onSearchClick_SummaryFacility() {
    this.SummaryFacilityDateProcessedRepository.save({
      fromDate_SummaryFacility: this.fromDate_SummaryFacility,
      toDate_SummaryFacility: this.toDate_SummaryFacility
    });
    auditorProductivitySummaryPerFacilityFilterValues.dateProcessed = this.getFilterDateProcessed_SummaryFacility();

    if (this.gridApi_SummaryFacility) {
      this.gridApi_SummaryFacility.onFilterChanged();
    }
  }

  getFilterDateProcessed_SummaryFacility() {
    const today = new Date();
    today.setDate(today.getDate() + 1); // Add 1 day

    if (!this.fromDate_SummaryFacility) {
      this.fromDate_SummaryFacility = this.maxDate_SummaryFacility;
      this.toDate_SummaryFacility = new NgbDate(
        today.getFullYear(),
        today.getMonth() + 1,
        today.getDate()
      );
      this.SummaryFacilityDateProcessedRepository.save({
        fromDate_SummaryFacility: this.fromDate_SummaryFacility,
        toDate_SummaryFacility: this.toDate_SummaryFacility
      });
    }

    return this.toDate_SummaryFacility
      ? JSON.stringify({
        firstCondition: transformDate({
          dateFrom: this.formatter.format(this.fromDate_SummaryFacility),
          dateTo: this.formatter.format(this.toDate_SummaryFacility),
          type: "inRange",
        }),
      })
      : JSON.stringify({
        firstCondition: transformDate({
          dateFrom: this.formatter.format(this.fromDate_SummaryFacility),
          dateTo: null,
          type: "equals",
        }),
      });
  }

  private valueFormatter(params: ValueFormatterParams): string {
    try {
      const column = params.value;

      return column === null ? 'Empty' : column;
    } catch {
      return '';
    }
  }


  private getSummaryFacilityColumns(): (ColDef | ColGroupDef)[] {
    return [

      {
        field: SUMMARYFACILITY_GRID_COLUMNS.FACILITY_NAME,
        headerName: "Facility Name",
        minWidth: 500,
        filterParams: {
          ...this.getSummaryFacilityDefaultSetFilterParams((filters, params) => {
            params.success(filters);
          }),
          valueFormatter: this.valueFormatter,
        },
        cellRenderer: this.loadingCellRenderer,
      },
      {
        headerName: "Total",
        field: SUMMARYFACILITY_GRID_COLUMNS.TOTAL,
        sortable: false,
        filter: false,
        cellClass: 'center-cell'
      }

    ];
  }
  result: any[] = [];

  public onGridReady_SummaryFacility(event: GridReadyEvent): void {

    this.gridApi_SummaryFacility = event.api;
    this.gridColumnApi_SummaryFacility = event.columnApi;
    this.gridApi_SummaryFacility.doLayout();
    this.gridApi_SummaryFacility.sizeColumnsToFit();


    if (this.loadedFilters_SummaryFacility) {
      this.gridApi_SummaryFacility.setFilterModel(this.loadedFilters_SummaryFacility);
    } else {
      this.gridColumnApi_SummaryFacility.applyColumnState({
        defaultState: { sort: null }
      });
    }
    if (auditorProductivitySummaryPerFacilityFilterValues.facilities == undefined)
      auditorProductivitySummaryPerFacilityFilterValues.facilities = [];
    auditorProductivitySummaryPerFacilityFilterValues.organization = { id: 0, value: "" };
    auditorProductivitySummaryPerFacilityFilterValues.dateProcessed = this.getFilterDateProcessed_SummaryFacility();

    this.dataSourceSummaryFacility = {
      getRows: (params) =>
        this.auditorProductivityDashboardService
          .getSummaryPerFacility(
            params.filterModel,
            params.sortModel,
            params.startRow,
            params.endRow,
            auditorProductivitySummaryPerFacilityFilterValues
          )
          .subscribe((data) => {
            const currentLastRow = params.startRow + data?.summaryPerFacilities?.length;
            const lastRowIndex =
              currentLastRow < params.endRow ? currentLastRow : -1;

            this.result = data?.summaryPerFacilities;
            console.log(this.result);
            let rowsThisPage = [];
            if (this.result != null && this.result.length > 0) {
              const allColumns = this.result[0].formProductivityResult.map(item => ({
                headerName: item.item1,
                field: item.item1,
                sortable: false,
                filter: false,
                suppressMenu: true,
                cellClass: 'center-cell'
              }));

              this.columns_SummaryFacility = [...this.basicColumn, ...allColumns];

              rowsThisPage = this.result.map(x => {
                const row: any = {};
                row[SUMMARYFACILITY_GRID_COLUMNS.FACILITY_NAME] = x.facility.name;

                x.formProductivityResult.forEach(item => {
                  row[item.item1] = item.item2 == 0 ? "" : item.item2;
                });

                row[SUMMARYFACILITY_GRID_COLUMNS.TOTAL] = x.totalCount;

                return row;
              });


            }

            params.successCallback(rowsThisPage, lastRowIndex);
          }),
    };


    event.api.setDatasource(this.dataSourceSummaryFacility);
  }

  public onFilterChanged_SummaryFacility(grid: GridOptions) {
    
    var filterModel = grid.api.getFilterModel();
 //   console.log(filterModel[SUMMARYFACILITY_GRID_COLUMNS.FACILITY_NAME]?.values);
    if (this.result != undefined) {
      const selectedFacilities = this.result
        .map(entry => entry.facility)
        .filter(facility => filterModel[SUMMARYFACILITY_GRID_COLUMNS.FACILITY_NAME]?.values.includes(facility?.name)).map(facility => ({
          id: facility.id,
          value: facility.name
        }));;

      console.log(selectedFacilities);
      auditorProductivitySummaryPerFacilityFilterValues.facilities = selectedFacilities;
    }
    
    
    this.SummaryFacilityGridFiltersRepository.save(filterModel);
  }

  public onColumnVisible_SummaryFacility(e) {
    this.saveColumnsState_SummaryFacility();
  }

  public onColumnMoved_SummaryFacility(e) {
    this.saveColumnsState_SummaryFacility();
  }

  private saveColumnsState_SummaryFacility() {
    this.SummaryFacilityGridColumnsStateRepository.save(this.gridColumnApi_SummaryFacility.getColumnState());
  }

  public onColumnResized_SummaryFacility(e) {
    this.SummaryFacilityGridColumnsStateRepository.save(this.gridColumnApi_SummaryFacility.getColumnState());
  }

  public onSortChanged_SummaryFacility(e) {
    this.SummaryFacilityGridColumnsStateRepository.save(this.gridColumnApi_SummaryFacility.getColumnState());
  }
  private getSummaryFacilityDefaultSetFilterParams(
    setValues: (filters: any, params: SetFilterValuesFuncParams) => void
  ): any {
    return {
      buttons: FILTER_PARAMS_BUTTONS,
      closeOnApply: true,
      cellHeight: FILTER_HEIGHT,
      values: (params: SetFilterValuesFuncParams) => {
        const field = params.colDef.field;

        if (field === SUMMARYFACILITY_GRID_COLUMNS.FACILITY_NAME && this.result) {
          const uniqueValues = Array.from(new Set(
            this.result.map(entry => entry.facility?.name).filter(name => !!name)
          ));

          setValues(uniqueValues, params);
        } else {
          setValues([], params); // fallback if no data
        }
      },
      refreshValuesOnOpen: true,
    };
  }


}
//etFilterParams(
//    setValues: (filters: any, params: SetFilterValuesFuncParams) => void
//  ): any {
//    return {
//      buttons: FILTER_PARAMS_BUTTONS,
//      closeOnApply: true,
//      cellHeight: FILTER_HEIGHT,
//      values: (params: SetFilterValuesFuncParams) => {

//        auditorProductivitySummaryPerFacilityFilterValues.dateProcessed = this.getFilterDateProcessed_SummaryFacility();

//        this.auditorProductivityDashboardService
//          .getSummaryPerFacilityfilters(params.colDef.field, this.gridApi_SummaryFacility.getFilterModel(), auditorProductivitySummaryPerFacilityFilterValues)
//          .subscribe((filters: IFilterOption[]) => {

//            const columnFilters = this.SummaryFacilityGridFiltersRepository.load()?.[params.colDef.field];

//            const filterValues = auditorProductivitySummaryPerFacilityFilterValues?.[params.colDef.field]?.filter((filter) => columnFilters?.values?.includes(filter.value)) ?? [];

//            filters = filterValues?.concat(filters?.filter((f) =>
//              f.id
//                ? !filterValues?.map((fv) => fv.id)?.includes(f.id)
//                : !filterValues?.map((fv) => fv.value)?.includes(f.value)))
//              ?? [];

//            auditorProductivitySummaryPerFacilityFilterValues = { ...auditorProductivitySummaryPerFacilityFilterValues, [params.colDef.field]: filters };

//            setValues(
//              filters.map((filter: IFilterOption) => filter.value),
//              params
//            );
//          });
//      },
//      refreshValuesOnOpen: true,
//    };
//  }
//}
