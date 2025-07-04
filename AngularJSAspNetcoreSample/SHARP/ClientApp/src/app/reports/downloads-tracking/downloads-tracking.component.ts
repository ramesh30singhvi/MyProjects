import { Router } from '@angular/router';
import { Component, OnInit, ViewChild } from '@angular/core';
import {
  ICellRendererParams,
  IDatasource,
  IGetRowsParams,
} from 'ag-grid-community';
import "ag-grid-enterprise";
import { UserService } from '../../services/user.service';
import { AuthService } from '../../services/auth.service';
import { transformDate } from "../../common/helpers/dates-helper";
import "ag-grid-angular";
import {
  NgbDate,
  NgbDateParserFormatter,
  NgbInputDatepicker,
  NgbModal,
} from "@ng-bootstrap/ng-bootstrap";
import * as moment from "moment";

@Component({
  selector: 'app-downloads-tracking',
  templateUrl: './downloads-tracking.component.html',
  styleUrls: ['./downloads-tracking.component.scss'],
})
export class DownloadsTrackingComponent implements OnInit {
  @ViewChild("datepicker") datepicker: NgbInputDatepicker;
  public showClearFilters: boolean = false;

  private userFacilityFilter: string = '';

  public headerHeight: number | undefined;
  public rowHeight: number | undefined;
  private filter: any = {
  };
  public columnDefs: any[] | undefined;
  public rowData: any[] = [];
  public filteredData: any[] = [];
  private gridApi: any;
  private gridColumnApi: any;
  public gridOptions: any;
  public components: any;
  private usersTotal: number = 0;
  public searchTerm: string = '';
  constructor(
    private modalService: NgbModal,
    private router: Router,
    private userServiceApi: UserService,
    private authService: AuthService,
  ) {}
  ngOnInit(): void {
    this.initGrid();

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

  private initGrid() {
    this.columnDefs = [
      {
        headerName: 'Name',
        field: 'name',
        flex: 1,
        cellRenderer: this.loadingCellRenderer,
        filter: false,
        suppressMenu: true
      },
      {
        headerName: 'Report date',
        field: 'reportDate',
        sortable: true,
        sort: 'desc',
        cellRenderer: this.dateFormatter,
        flex: 1,
        filter: false,
        suppressMenu: true
      },
      {
        headerName: 'Number of downloads', field: 'numberOfDownloads', flex: 1.3, filter: false, suppressMenu: true },
      {
        headerName: 'Date & Time',
        field: 'dateAndTime',
        sortable: true,
        flex: 1,
        filter: false,
        suppressMenu: true
      },
    ];

    this.rowHeight = 65;
    this.headerHeight = 50;
    this.components = {
      loadingCellRenderer: this.loadingCellRenderer,
    };
    this.gridOptions = {
      enableColResize: true,
      suppressRowClickSelection: true,
      rowModelType: 'infinite',
      cacheBlockSize: 25,
      rowSelection: 'multiple',
      maxConcurrentDatasourceRequests: 1,
      infiniteInitialRowCount: 100,
      maxBlocksInCache: 10,
      defaultColDef: {
        sortable: true,
        filter: false,
        suppressMovable: true,
        resizable: true,
      },
    };
  }

  ngAfterViewInit() {}


  public onSelectionChanged(): void {

  }
  get noSelectedRow(){
    return this.gridApi?.getSelectedRows().length == 0;
  }
  public enableEditUser(){
    return this.gridApi?.getSelectedRows().length == 1;
  }

  public onGridReady(params: any): void {
    this.gridApi = params.api;
    this.gridColumnApi = params.columnApi;
    let that = this;

    const dataSource: IDatasource = {
      rowCount: undefined, // behave as infinite scroll

      getRows: (params: IGetRowsParams) => {
        that.userServiceApi
          .getDownloadsTrackingData(
            that.searchTerm,
            params.sortModel,
            params.startRow,
            params.endRow,
            that.filter,
          )
          .subscribe(
            (data: any) => {
              const currentLastRow = params.startRow + data.items.length;

              let lastRowIndex =
                currentLastRow < params.endRow ? currentLastRow : -1;

              that.usersTotal = data.items.totalCount;
              params.successCallback(data.items, lastRowIndex);
            },
            (error) => {
              console.log(error);
              params.failCallback();
            },
          );
      },
    };

    this.gridApi?.setDatasource(dataSource);
    //this.gridApi!.setGridOption('datasource', dataSource);
    this.gridApi?.addEventListener(
      'selectionChanged',
      this.onSelectionChanged.bind(this),
    );

  }

  public onSearchInputChanged(value: any): void {
    this.searchTerm = value.target.value;
    this.gridApi.onFilterChanged();
  }

  onDateRangeFilterChanged(event: any): void {
    const format = "YYYY-MM-DD";
    if(event.start == null)
      return;
    if(event.end == null)
      return;

    if(event.start.format(format) == event.end.format(format))
    {
      this.filter.date = JSON.stringify({
        firstCondition: transformDate({dateFrom: event.start.format(format), dateTo:undefined,type: "equals"},true),
        secondCondition: null
      })

    }else{
      this.filter.date = JSON.stringify({
        firstCondition: transformDate({dateFrom: event.start.format(format), dateTo: event.end.format(format),type: "inRange"},true),
        secondCondition: null,

      })
    }

    this.gridApi?.onFilterChanged();
  }

  private dateFormatter(params: any): string {
    return moment(params.value).format("MMM DD, YYYY h:mma");
  }

}
