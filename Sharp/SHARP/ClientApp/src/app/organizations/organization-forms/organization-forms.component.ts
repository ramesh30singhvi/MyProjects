import { Component, Input } from '@angular/core';
import { ColDef, GridApi, GridReadyEvent, IDatasource, SetFilterValuesFuncParams } from 'ag-grid-community';
import { ORGANIZATION_FORM_GRID_COLUMNS } from 'src/app/common/constants/formGrid';
import { CACHE_OVERFLOW_SIZE, FILTER_HEIGHT, FILTER_PARAMS_BUTTONS, INFINITE_INITIAL_ROWCOUNT, MAX_BLOCKS_IN_CACHE, MAX_CONCURRENT_DATASOURCE_REQUESTS, ROW_BUFFER, ROW_MODEL_TYPE, SERVER_SIDE_STORE_TYPE } from 'src/app/common/constants/grid';
import { IFilterOption } from 'src/app/models/audits/audit.filters.model';
import { IOption } from 'src/app/models/audits/audits.model';
import { FormFiltersModel, FormStates, IScheduleSetting, ScheduleType, ScheduleTypes, SettingType, SettingTypes, weekDays } from 'src/app/models/forms/forms.model';
import { OrganizationService } from '../../services/organization.service';
import { OrganizationFormsBtnCellRendererComponent } from './organization-forms-btn-cell-renderer/organization-forms-btn-cell-renderer.component';

let formFilterValues: FormFiltersModel = {};

@Component({
  selector: "app-organization-forms",
  templateUrl: "./organization-forms.component.html",
  styleUrls: ["./organization-forms.component.scss"],
})
export class OrganizationFormsComponent {
  private _organization: IOption;

  @Input() set organization(organization: IOption) {
    this._organization = organization;
    
    this.gridApi?.setFilterModel(null);
    this.onSearch();
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
  public frameworkComponents;

  public columns: ColDef[];

  public searchTerm: string;
  
  constructor(
    //public activeModal: NgbActiveModal,
    public organizationService: OrganizationService
  ) {
    formFilterValues = {
      settingType: SettingTypes.map((type) => {return { id : type.id, value: type.label}}),
      scheduleSetting: ScheduleTypes.map((type) => {return { id : type.id, value: type.label}}),
      isFormActive: Object.values(FormStates).map((status) => {return { id : status.id, value: status.label}}),
    };

    this.columns = this.getColumns();

    this.frameworkComponents = {
      btnCellRenderer: OrganizationFormsBtnCellRendererComponent,
    };
  }

  onAssignFormClick() {

  }

  onSearchClear() {
    this.searchTerm = null;
    this.gridApi?.onFilterChanged();
  }

  public onSearch(): void {
    this.gridApi?.onFilterChanged();
  }

  public getStatus(state: string): any {
    const statuses = Object.values(FormStates);

    return statuses.find((s) => s.label === state);
  }

  public noRecords = false;
  public defaultColumn: ColDef = {
    minWidth: 300,
    resizable: true,
    filter: true,
    filterParams: {
      buttons: FILTER_PARAMS_BUTTONS,
      closeOnApply: true,
      cellHeight: FILTER_HEIGHT,
      values: (params: SetFilterValuesFuncParams) =>
        this.organizationService
          .getFormFilters(params.colDef.field, this._organization?.id, this.gridApi?.getFilterModel(), formFilterValues)
          .subscribe((filters: IFilterOption[]) => {
            //const columnFilters = this.filtersRepository.load()?.[params.colDef.field];

            const filterValues = /*userFilterValues?.[params.colDef.field]?.filter((filter) => columnFilters?.values?.includes(filter.value)) ??*/ [];

            filters = filterValues?.concat(filters?.filter((f) => 
            f.id 
            ? !filterValues?.map((fv) => fv.id)?.includes(f.id) 
            : !filterValues?.map((fv) => fv.value)?.includes(f.value))) 
            ?? [];

            formFilterValues = {...formFilterValues, [params.colDef.field]: filters};

            params.success(filters.map((filter: IFilterOption) => filter.value));
          }),
      refreshValuesOnOpen: true,
      defaultToNothingSelected: true,
      suppressSorting: true,
      suppressSelectAll: true,
    },
    menuTabs: ["filterMenuTab"],
    sortable: true,
  };
  
  public onGridReady(event: GridReadyEvent): void {
    this.gridApi = event.api;

    this.gridApi.sizeColumnsToFit();

    const dataSource: IDatasource = {
      getRows: (params) =>
        this.organizationService
          .getOrganizationForms(
            this.searchTerm,
            params.filterModel,
            params.sortModel,
            params.startRow,
            params.endRow,
            this._organization?.id ?? 0,
            formFilterValues,
          )
          .subscribe((facilities) => {
            this.noRecords = facilities.length === 0;
            const currentLastRow = params.startRow + facilities.length;
            const lastRowIndex =
              currentLastRow < params.endRow ? currentLastRow : -1;
            params.successCallback(facilities, lastRowIndex);
          }),
    };
    event.api.setDatasource(dataSource);
  }

  private getColumns(): ColDef[] {
    return [
      {
        field: ORGANIZATION_FORM_GRID_COLUMNS.NAME,
        headerName: "Form Name",
        minWidth: 300,
        filterParams: {
          cellHeight: 50,
        }
      },
      {
        field: ORGANIZATION_FORM_GRID_COLUMNS.AUDIT_TYPE,
        headerName: "Form Type",
        minWidth: 150,
      },
      {
        field: ORGANIZATION_FORM_GRID_COLUMNS.SCHEDULE_SETTING,
        headerName: "Schedule",
        minWidth: 150,
        cellRenderer: (data) => {
          if (data.value) {
            const scheduleSetting: IScheduleSetting = data.value;

            const scheduleType: string = ScheduleTypes.find((type) => type.id === data.value?.scheduleType)?.label;

            let schedule: string = scheduleSetting.scheduleType === ScheduleType.Weekly 
            ? `${scheduleType}: ${scheduleSetting.days.map((day: number) => weekDays[day]?.smallName).join(', ')}`
            : `${scheduleType}: ${scheduleSetting.days.join(', ')}`;

            return schedule;
          }
        },
        filterParams: {
          buttons: ['reset', 'cancel', 'apply'],
          closeOnApply: true,
          cellHeight: FILTER_HEIGHT,
          values: formFilterValues?.scheduleSetting?.map((s) => s.value),
        },
      },
      {
        field: ORGANIZATION_FORM_GRID_COLUMNS.SETTING_TYPE,
        headerName: "Setting",
        minWidth: 150,
        cellRenderer: (data) => {
          return SettingTypes.find((type) => type.id === data.value)?.label;
        },
        filterParams: {
          buttons: ['reset', 'cancel', 'apply'],
          closeOnApply: true,
          cellHeight: FILTER_HEIGHT,
          values: formFilterValues?.settingType?.map((s) => s.value),
        },
      },
      {
        field: ORGANIZATION_FORM_GRID_COLUMNS.STATE,
        headerName: "State",
        maxWidth: 110,
        resizable: false,
        cellRenderer: (data) => {
          if (data.value === true) {
            return `<span style="
                    color: #3ac34a;
                    background-color: #c5f6cb;
                    padding: 3px 10px;
                    border-radius: 4px;
                    font-size: 14px;">Active</span>`;
        }
        else {
            return `<span style="
                    color: rgb(246, 45, 81);
                    background-color: rgba(246, 45, 81, 0.14);
                    padding: 3px 10px;
                    border-radius: 4px;
                    font-size: 14px;">Inactive</span>`;
          }
        },
        filterParams: {
          buttons: ['reset', 'cancel', 'apply'],
          closeOnApply: true,
          cellHeight: FILTER_HEIGHT,
          values: formFilterValues?.isFormActive?.map((s) => s.value), //['1', '0'],
          suppressMiniFilter: true,
          suppressSorting: true,
          cellRenderer: (params) => {
            if (params.value == '(Select All)') return params.value;
        
            const state = this.getStatus(params.value);

            return `<span style="color: ${state.color}">${state.label}</span>`;
          },
        },
      },
      {
        field: ORGANIZATION_FORM_GRID_COLUMNS.ACTIONS,
        headerName: "",
        minWidth: 80,
        maxWidth: 100,
        cellRenderer: 'btnCellRenderer',
        filter: false,
        menuTabs: [],
        sortable: false,
        resizable: false,
      },
    ];
  }
}
