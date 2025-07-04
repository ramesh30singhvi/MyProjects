import { Component, OnInit } from "@angular/core";
import {
  ColDef,
  GridApi,
  GridReadyEvent,
  IDatasource,
  SetFilterValuesFuncParams,
} from "ag-grid-community";
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
import { UserService } from "../services/user.service";
import { Title } from "@angular/platform-browser";
import { USER_GRID_COLUMNS } from "../common/constants/userGrid";
import { NgbModal, NgbModalRef } from "@ng-bootstrap/ng-bootstrap";
import { AddUserComponent } from "./add-user/add-user.component";
import { UserActionsComponent } from "./user-actions/user-actions.component";
import { UserFiltersModel } from "../models/users/users.model";
import { IFilterOption } from "../models/audits/audit.filters.model";
import {
  UserProductivityComponent,
  UserProductivityReportType,
} from "./user-productivity/user-productivity.component";

let userFilterValues: UserFiltersModel = {};

@Component({
  selector: "app-users",
  templateUrl: "./users.component.html",
  styleUrls: ["./users.component.scss"],
})
export class UsersComponent implements OnInit {
  private modalRef: NgbModalRef;

  private UNLIMITED = "All";

  public defaultColumn: ColDef = {
    minWidth: 300,
    resizable: true,
    filter: true,
    filterParams: {
      buttons: FILTER_PARAMS_BUTTONS,
      closeOnApply: true,
      cellHeight: FILTER_HEIGHT,
      values: (params: SetFilterValuesFuncParams) =>
        this.userService
          .getUsersFilters(
            params.colDef.field,
            this.gridApi?.getFilterModel(),
            userFilterValues
          )
          .subscribe((filters: IFilterOption[]) => {
            //const columnFilters = this.filtersRepository.load()?.[params.colDef.field];

            const filterValues =
              /*userFilterValues?.[params.colDef.field]?.filter((filter) => columnFilters?.values?.includes(filter.value)) ??*/ [];

            filters =
              filterValues?.concat(
                filters?.filter((f) =>
                  f.id
                    ? !filterValues?.map((fv) => fv.id)?.includes(f.id)
                    : !filterValues?.map((fv) => fv.value)?.includes(f.value)
                )
              ) ?? [];

            userFilterValues = {
              ...userFilterValues,
              [params.colDef.field]: filters,
            };

            params.success(
              filters.map((filter: IFilterOption) => filter.value)
            );
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
      field: USER_GRID_COLUMNS.NAME,
      headerName: "Name",
    },
    {
      field: USER_GRID_COLUMNS.EMAIL,
      headerName: "Email Address",
    },
    {
      field: USER_GRID_COLUMNS.ROLES,
      headerName: "Role",
      valueFormatter: ({ value }) => value?.join(", "),
    },
    {
      field: USER_GRID_COLUMNS.ACCESS,
      headerName: "Organization Access",
      valueFormatter: ({ value }) => {
        if (!value) {
          return null;
        }

        const { unlimited, organizations } = value;
        return unlimited ? this.UNLIMITED : organizations.join(", ");
      },
      flex: 1,
      filterParams: {
        comparator: (left: string, right: string) => {
          if (left === right) {
            return 0;
          }

          if (left === this.UNLIMITED) {
            return -1;
          }

          if (right === this.UNLIMITED) {
            return 1;
          }

          left < right ? -1 : 1;
        },
      },
    },
    {
      field: USER_GRID_COLUMNS.FACILITY_ACCESS,
      headerName: "Facility Access",
      valueFormatter: ({ value }) => {
        if (!value) {
          return null;
        }

        const { facilityUnlimited, facilities } = value;
        return facilityUnlimited ? this.UNLIMITED : facilities.join(", ");
      },
      flex: 1,
      filterParams: {
        comparator: (left: string, right: string) => {
          if (left === right) {
            return 0;
          }

          if (left === this.UNLIMITED) {
            return -1;
          }

          if (right === this.UNLIMITED) {
            return 1;
          }

          left < right ? -1 : 1;
        },
      },
    },
    {
      field: USER_GRID_COLUMNS.STATUS,
      headerName: "",
      minWidth: 250,
      cellRenderer: "userActionsComponent",
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

  public noUsers = false;

  private gridApi: GridApi;

  public frameworkComponents;

  constructor(
    private userService: UserService,
    private titleService: Title,
    private modalService: NgbModal
  ) {
    this.frameworkComponents = {
      userActionsComponent: UserActionsComponent,
    };
  }

  ngOnInit(): void {
    this.titleService.setTitle("SHARP users");
  }

  public onGridReady(event: GridReadyEvent): void {
    this.gridApi = event.api;

    const dataSource: IDatasource = {
      getRows: (params) =>
        this.userService
          .getUsers(
            this.searchTerm,
            params.filterModel,
            params.sortModel,
            params.startRow,
            params.endRow,
            userFilterValues
          )
          .subscribe((users) => {
            this.noUsers = users.length === 0;

            const currentLastRow = params.startRow + users.length;
            const lastRowIndex =
              currentLastRow < params.endRow ? currentLastRow : -1;
            params.successCallback(users, lastRowIndex);
          }),
    };
    event.api.setDatasource(dataSource);
  }

  public onSearch(): void {
    this.gridApi.onFilterChanged();
  }

  public onSearchClear(): void {
    this.searchTerm = null;
    this.onSearch();
  }

  public onAddUser(): void {
    this.modalService.open(AddUserComponent).result.then(
      () => this.gridApi.onFilterChanged(),
      () => {}
    );
  }

  public openUserProductivityReportModal(): void {
    this.modalRef = this.modalService.open(UserProductivityComponent, {
      modalDialogClass: "generate-excel-modal",
    });
    this.modalRef.componentInstance.userId = undefined;
    this.modalRef.componentInstance.type = UserProductivityReportType.Multiple;
    this.modalRef.componentInstance.filterModel =
      this.gridApi?.getFilterModel();
    this.modalRef.componentInstance.userFilterValues = userFilterValues;
  }
}
