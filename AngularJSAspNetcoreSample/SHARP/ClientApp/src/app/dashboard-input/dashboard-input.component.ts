import { ChangeDetectionStrategy, Component, OnInit, ViewChild } from "@angular/core";
import {
  NgbDate,
  NgbDateParserFormatter,
  NgbInputDatepicker,
  NgbModal,
  NgbModalRef,
} from "@ng-bootstrap/ng-bootstrap";
import * as moment from "moment";
import { ToastrService } from "ngx-toastr";
import { first } from "rxjs";
import {
  MM_DD_YYYY_HH_MM_SS_SLASH,
  YYYY_MM_DD_DASH,
} from "src/app/common/constants/date-constants";
import {
  DashboardInput,
  DashboardInputElement,
  DashboardInputGroup,
  DashboardInputSummary,
  DashboardInputTable,
  DashboardInputValue,
  DateRangeEnum,
  DateRanges,
  DashboardInputFilter,
  InputTabsEnum,
  IInputTableCounts,
  DateFromTo,
} from "src/app/models/dashboard/dashboard-input.model";
import { FacilityModel } from "src/app/models/organizations/facility.model";
import { RolesEnum } from "src/app/models/roles.model";
import { AuthService } from "src/app/services/auth.service";
import { DashboardService } from "src/app/services/dashboard.service";
import {
  AddDashboardInputComponent,
  AddType,
} from "./add/add-dashboard-input.component";
import { DeleteDashboardInputComponent } from "./delete/delete-dashboard-input.component";
import { IOption } from "src/app/models/audits/audits.model";
import { OrganizationService } from "src/app/services/organization.service";
import { transformDate } from "../common/helpers/dates-helper";
import { LocalStoreRepository } from "../services/repository/local-store-repository";

const DASHBOARD_INPUT_FILTER_STATE = "dashboardInputFilterState";

@Component({
  selector: "dashboard-input",
  templateUrl: "./dashboard-input.component.html",
  styleUrls: ["./dashboard-input.component.scss"],
})
export class DashboardInputComponent implements OnInit {
  private modalRef: NgbModalRef;

  public dashboardInput: DashboardInput[] = [];
  public dashboardInputValues: DashboardInputValue[] = [];
  public defaultDashboardInputValues: any[] = [];
  public organizations: IOption[];
  public selectedOrganization: IOption;
  public dashboardInputValuesDaily: DashboardInputValue[] = [];
  public dashboardInputValuesWeekly: DashboardInputValue[] = [];
  public dashboardInputValuesMonthly: DashboardInputValue[] = [];

  @ViewChild("datepicker") datepicker: NgbInputDatepicker;

  public dateRangeEnum = DateRangeEnum;
  public dateRanges: IOption[] = DateRanges;
  public selectedDateRange: IOption;

  public hoveredDate: NgbDate | null = null;
  public fromDate: NgbDate | null;
  public toDate: NgbDate | null;
  public selectedFromDate: NgbDate | null;
  public selectedToDate: NgbDate | null;

  public inputTabsEnum = InputTabsEnum;
  public inputTableCounts: IInputTableCounts = { daily: 0, weekly: 0, monthly: 0, auditorProductivity: 0 };
  public selectedTab: number = 0;

  public canEdit: boolean = false;

  private filtersRepository: LocalStoreRepository<any>;

  constructor(
    private dashboardServiceApi: DashboardService,
    private organizationServiceApi: OrganizationService,
    private authService: AuthService,
    public formatter: NgbDateParserFormatter,
    private toastr: ToastrService,
    private modalService: NgbModal
  ) {
    this.getOrganizations();

    this.filtersRepository = new LocalStoreRepository<any>(
      DASHBOARD_INPUT_FILTER_STATE
    );

    const dashboardInputFilterState = this.filtersRepository.load();

    if (dashboardInputFilterState) {
      this.selectedDateRange =
        dashboardInputFilterState.dateRange?.selectedDateRange;
      const fromDate = dashboardInputFilterState.dateRange?.fromDate;
      this.fromDate = fromDate
        ? new NgbDate(fromDate.year, fromDate.month, fromDate.day)
        : null;
      this.selectedFromDate = fromDate
        ? new NgbDate(fromDate.year, fromDate.month, fromDate.day)
        : null;
      const toDate = dashboardInputFilterState.dateRange?.toDate;
      this.toDate = toDate
        ? new NgbDate(toDate.year, toDate.month, toDate.day)
        : null;
      this.selectedToDate = toDate
        ? new NgbDate(toDate.year, toDate.month, toDate.day)
        : null;

      this.selectedOrganization = dashboardInputFilterState.organization;

      if (this.selectedOrganization?.id > 0) {
        this.getDashboardInput();
      }

      this.selectedTab = dashboardInputFilterState.inputTab;
    } else {
      const defaultDateRange: IOption = this.dateRanges.find(function (range) {
        return range.id === DateRangeEnum.Today;
      });
      this.selectedDateRange = defaultDateRange;
    }
  }

  ngOnInit(): void {
    let user = this.authService.getCurrentUser();
    if (user.roles.find((role) => role == RolesEnum.Admin)) {
      this.canEdit = true;
    }
  }

  ngOnDestory() {}

  private getOrganizations() {
    this.organizationServiceApi
      .getOrganizationOptions()
      .pipe(first())
      .subscribe((organizations: IOption[]) => {
        this.organizations = organizations;
      });
  }

  private getDashboardInput() {

    const filter: DashboardInputFilter = this.getDashboardInputFilter();

    if (filter?.organizationId == undefined || filter?.organizationId <= 0)
      return;

    this.dashboardServiceApi
      .getDashboardInput(filter)
      .pipe(first())
      .subscribe((data: DashboardInput[]) => {
        this.dashboardInput = data;
        this.dashboardInputValues = [];
        this.dashboardInputValuesDaily = [];
        this.dashboardInputValuesWeekly = [];
        this.dashboardInputValuesMonthly = [];

        let dailyCount: number = 0;
        let weeklyCount: number = 0;
        let monthlyCount: number = 0;
        let auditorProductivityCount: number = 0;

        this.dashboardInput.forEach((di) => {
          di.dashboardInputTables.forEach((dit) => {
            dit.dashboardInputGroups.forEach((dig) => {
              dig.dashboardInputElements.forEach((die) => {
                this.dashboardInputValues = this.dashboardInputValues.concat(
                  die.dashboardInputValues
                );

                let indexOfElement = this.defaultDashboardInputValues.findIndex(
                  (index, value) => index == die.id
                );
                if (indexOfElement < 0) {
                  this.defaultDashboardInputValues[die.id] = [];
                  di.facilities.forEach((facility) => {
                    let inputValue = die.dashboardInputValues.find(
                      (div) => div.facility?.id == facility.id
                    );
                    if (inputValue) {
                      this.defaultDashboardInputValues[die.id][facility.id] =
                        inputValue.value;
                    } else {
                      this.defaultDashboardInputValues[die.id][facility.id] =
                        0;
                    }
                  });
                }

                if (dit.name == "Daily") {
                  this.dashboardInputValuesDaily = this.dashboardInputValuesDaily.concat(
                    die.dashboardInputValues
                  );

                  die.dashboardInputValues.forEach((div) => {
                    dailyCount += div.value;
                  });
                }
                if (dit.name == "Weekly") {
                  this.dashboardInputValuesWeekly = this.dashboardInputValuesWeekly.concat(
                    die.dashboardInputValues
                  );

                  die.dashboardInputValues.forEach((div) => {
                    weeklyCount += div.value;
                  });
                }
                if (dit.name == "Monthly") {
                  this.dashboardInputValuesMonthly = this.dashboardInputValuesMonthly.concat(
                    die.dashboardInputValues
                  );

                  die.dashboardInputValues.forEach((div) => {
                    monthlyCount += div.value;
                  });
                }
                die.dashboardInputValues.forEach((div) => {
                  auditorProductivityCount += div.value;
                });

              });
            });
          });
        });

        this.inputTableCounts.daily = dailyCount;
        this.inputTableCounts.weekly = weeklyCount;
        this.inputTableCounts.monthly = monthlyCount;
        this.inputTableCounts.auditorProductivity = auditorProductivityCount;

      });
  }

  totalNumberOfElementsInTable(table: DashboardInputTable) {
    var numberOfElements = 0;
    table.dashboardInputGroups.forEach((dig) => {
      if (dig.dashboardInputElements.length == 0) {
        numberOfElements += 1;
      } else {
        numberOfElements += dig.dashboardInputElements.length;
      }
    });
    return numberOfElements;
  }

  public onOrganizationDropdownOpened(): void {
    this.getOrganizations();
  }

  public onOrganizationChanged(organization: IOption): void {
    this.getDashboardInput();
    this.saveDashboardInputFilterState();
  }

  totalNumberOfElementsInGroup(group: DashboardInputGroup) {
    return group.dashboardInputElements.length;
  }

  totalValueByElement(element: DashboardInputElement, tableName: string) {
    switch (tableName) {
      case "Daily":
        return this.dashboardInputValuesDaily
          .filter((div) => div.elementId == element.id)
          .reduce((n, { value }) => n + value, 0);

      case "Weekly":
        return this.dashboardInputValuesWeekly
          .filter((div) => div.elementId == element.id)
          .reduce((n, { value }) => n + value, 0);

      case "Monthly":
        return this.dashboardInputValuesMonthly
          .filter((div) => div.elementId == element.id)
          .reduce((n, { value }) => n + value, 0);

      default:
        return this.dashboardInputValues
          .filter((div) => div.elementId == element.id)
          .reduce((n, { value }) => n + value, 0);
    }
  }

  totalValueByFacility(facility: FacilityModel, tableName: string) {
    switch (tableName) {
      case "Daily":
        return this.dashboardInputValuesDaily
          .filter((div) => div.facilityId == facility.id)
          .reduce((n, { value }) => n + value, 0);

      case "Weekly":
        return this.dashboardInputValuesWeekly
          .filter((div) => div.facilityId == facility.id)
          .reduce((n, { value }) => n + value, 0);

      case "Monthly":
        return this.dashboardInputValuesMonthly
          .filter((div) => div.facilityId == facility.id)
          .reduce((n, { value }) => n + value, 0);

      default:
        return this.dashboardInputValues
          .filter((div) => div.facilityId == facility.id)
          .reduce((n, { value }) => n + value, 0);
    }
  }

  totalValueByAuditor(dashboardInputSummary: DashboardInputSummary) {
    return dashboardInputSummary.dashboardInputSummaryShift.reduce((n, {total}) => n + total, 0);
  }

  totalValueOfAudits(dashboardInput: DashboardInput): number {
    var total = 0;

    dashboardInput.dashboardInputSummaries.forEach(dis => {
      dis.dashboardInputSummaryShift.forEach(diss => {
        total+=diss.total
      })
    })

    return total;
  }

  totalDiscrepancyOfAudits(dashboardInput: DashboardInput) {
    var total = 0;

    dashboardInput.dashboardInputTables.forEach(dit => {
      dit.dashboardInputGroups.forEach(dig => {
        dig.dashboardInputElements.forEach(die => {
          die.dashboardInputValues.forEach(div => {
            total+=div.value;
          })
        })
      })
    })

    return total - this.totalValueOfAudits(dashboardInput);
  }

  onInputValueChanged(
    value: string,
    element: DashboardInputElement,
    facility: FacilityModel
  ) {
    let divIndex = this.dashboardInputValues.findIndex(
      (div) => div.elementId == element.id && div.facility?.id == facility.id
    );

    if (divIndex >= 0) {
      this.dashboardInputValues[divIndex].value = parseInt(value);
    } else {
      this.dashboardInputValues.push({
        id: 0,
        value: parseInt(value),
        facilityId: facility.id,
        facility: facility,
        elementId: element.id,
        date: moment().format(YYYY_MM_DD_DASH),
      });
    }
  }

  saveInputValues() {
    this.dashboardServiceApi
      .saveDashboardInputValues(this.dashboardInputValues)
      .pipe(first())
      .subscribe(() => {
        this.toastr.success("Data saved successully");
        this.getDashboardInput();
      });
  }

  openAddTable(organizationId: number) {
    this.modalRef = this.modalService.open(AddDashboardInputComponent, {
      modalDialogClass: "add-dasbhoard-input-modal",
    });
    this.modalRef.componentInstance.organizationId = organizationId;
    this.modalRef.componentInstance.type = AddType.Table;
    this.modalRef.componentInstance.name = undefined;
    this.modalRef.componentInstance.id = undefined;
    this.modalRef.componentInstance.keyword = undefined;
    this.modalRef.result.then(
      (result) => {},
      (response) => {
        if (response) {
          location.reload();
        }
      }
    );
  }

  openAddGroup(organizationId: number, table: DashboardInputTable) {
    this.modalRef = this.modalService.open(AddDashboardInputComponent, {
      modalDialogClass: "add-dasbhoard-input-modal",
    });
    this.modalRef.componentInstance.organizationId = organizationId;
    this.modalRef.componentInstance.tableId = table.id;
    this.modalRef.componentInstance.type = AddType.Header;
    this.modalRef.componentInstance.name = undefined;
    this.modalRef.componentInstance.id = undefined;
    this.modalRef.componentInstance.keyword = undefined;
    this.modalRef.result.then(
      (result) => {},
      (response) => {
        if (response) {
          location.reload();
        }
      }
    );
  }

  openAddElement(organizationId: number, group: DashboardInputGroup) {
    this.modalRef = this.modalService.open(AddDashboardInputComponent, {
      modalDialogClass: "add-dasbhoard-input-modal",
    });
    this.modalRef.componentInstance.organizationId = organizationId;
    this.modalRef.componentInstance.groupId = group.id;
    this.modalRef.componentInstance.type = AddType.Column;
    this.modalRef.componentInstance.name = undefined;
    this.modalRef.componentInstance.keyword = undefined;
    this.modalRef.componentInstance.id = undefined;
    this.modalRef.componentInstance.formId = undefined;
    this.modalRef.result.then(
      (result) => {},
      (response) => {
        if (response) {
          location.reload();
        }
      }
    );
  }

  openEditTable(organizationId: number, table: DashboardInputTable) {
    this.modalRef = this.modalService.open(AddDashboardInputComponent, {
      modalDialogClass: "add-dasbhoard-input-modal",
    });
    this.modalRef.componentInstance.organizationId = organizationId;
    this.modalRef.componentInstance.type = AddType.Table;
    this.modalRef.componentInstance.name = table.name;
    this.modalRef.componentInstance.id = table.id;
    this.modalRef.componentInstance.keyword = undefined;
    this.modalRef.result.then(
      (result) => {},
      (response) => {
        if (response) {
          location.reload();
        }
      }
    );
  }

  openEditGroup(organizationId: number, group: DashboardInputGroup) {
    this.modalRef = this.modalService.open(AddDashboardInputComponent, {
      modalDialogClass: "add-dasbhoard-input-modal",
    });
    this.modalRef.componentInstance.organizationId = organizationId;
    this.modalRef.componentInstance.type = AddType.Header;
    this.modalRef.componentInstance.name = group.name;
    this.modalRef.componentInstance.id = group.id;
    this.modalRef.componentInstance.keyword = undefined;
    this.modalRef.result.then(
      (result) => {},
      (response) => {
        if (response) {
          location.reload();
        }
      }
    );
  }

  openEditElement(organizationId: number, element: DashboardInputElement) {
    this.modalRef = this.modalService.open(AddDashboardInputComponent, {
      modalDialogClass: "add-dasbhoard-input-modal",
    });
    this.modalRef.componentInstance.organizationId = organizationId;
    this.modalRef.componentInstance.type = AddType.Column;
    this.modalRef.componentInstance.name = element.name;
    this.modalRef.componentInstance.id = element.id;
    this.modalRef.componentInstance.formId = element.formId;
    this.modalRef.componentInstance.keyword = element.keyword;
    this.modalRef.result.then(
      (result) => {},
      (response) => {
        if (response) {
          location.reload();
        }
      }
    );
  }

  openDeleteTable(organizationId: number, table: DashboardInputTable) {
    this.modalRef = this.modalService.open(DeleteDashboardInputComponent, {
      modalDialogClass: "delete-dasbhoard-input-modal",
    });
    this.modalRef.componentInstance.organizationId = organizationId;
    this.modalRef.componentInstance.type = AddType.Table;
    this.modalRef.componentInstance.name = table.name;
    this.modalRef.componentInstance.id = table.id;
    this.modalRef.componentInstance.keyword = undefined;
    this.modalRef.result.then(
      (result) => {},
      (response) => {
        if (response) {
          location.reload();
        }
      }
    );
  }

  openDeleteGroup(organizationId: number, group: DashboardInputGroup) {
    this.modalRef = this.modalService.open(DeleteDashboardInputComponent, {
      modalDialogClass: "delete-dasbhoard-input-modal",
    });
    this.modalRef.componentInstance.organizationId = organizationId;
    this.modalRef.componentInstance.type = AddType.Header;
    this.modalRef.componentInstance.name = group.name;
    this.modalRef.componentInstance.id = group.id;
    this.modalRef.componentInstance.keyword = undefined;
    this.modalRef.result.then(
      (result) => {},
      (response) => {
        if (response) {
          location.reload();
        }
      }
    );
  }

  openDeleteElement(organizationId: number, element: DashboardInputElement) {
    this.modalRef = this.modalService.open(DeleteDashboardInputComponent, {
      modalDialogClass: "delete-dasbhoard-input-modal",
    });
    this.modalRef.componentInstance.organizationId = organizationId;
    this.modalRef.componentInstance.type = AddType.Column;
    this.modalRef.componentInstance.name = element.name;
    this.modalRef.componentInstance.id = element.id;
    this.modalRef.componentInstance.keyword = undefined;
    this.modalRef.result.then(
      (result) => {},
      (response) => {
        if (response) {
          location.reload();
        }
      }
    );
  }

  onFileSelected(
    event: any,
    organization: IOption,
    element: DashboardInputElement
  ) {


    const file: File = event.target.files[0];
    if (file) {
      const formData = new FormData();
      formData.append("file", file);
      formData.append("organizationId", organization.id?.toString());
      formData.append("elementId", element.id.toString());
      this.dashboardServiceApi
        .uploadFile(formData)
        .pipe()
        .subscribe((response) => {
          if (response) {
            location.reload();
          }
        }, (error) => {
          this.toastr.error(error.error);
        });
    }
  }

  getBackgroundColor(row: number) {
    let colors = ["#c6efce", "#FFEB9C", "#FFC7CE"];
    if (row>2) return colors[0];
    return colors[row];
  }

  getTextColor(row: number) {
    let colors = ["#006100", "#9C5700", "#9C0006"];
    if (row>2) return colors[0];
    return colors[row];
  }

  getCellBackgroundColor(status: number) {
    let colors = ["#FFFFFF","#AEAAAA","#FFFF00","#92D050","#00B0F0"];
    return colors[status];
  }

  public onDateRangeChanged(dateRange: IOption): void {
    this.selectedDateRange = dateRange;
    this.fromDate = null;
    this.toDate = null;
    this.selectedFromDate = null;
    this.selectedToDate = null;

    if (this.selectedDateRange?.id === this.dateRangeEnum.CustomRange) {
      this.datepicker.toggle();
    } else {
      this.getDashboardInput();
      this.saveDashboardInputFilterState();
    }
  }

  private buildDateRangeFilter(): string {
    if (!this.selectedDateRange) {
      return null;
    }

    switch (this.selectedDateRange.id) {
      case DateRangeEnum.Today:
        return "day";

      case DateRangeEnum.ThisWeek:
        return "week";

      case DateRangeEnum.ThisMonth:
        return "month";

      case DateRangeEnum.CustomRange:
        if (!this.selectedFromDate && !this.selectedToDate) {
          return null;
        }

        return this.selectedToDate
          ? JSON.stringify({
            firstCondition: transformDate({
              dateFrom: this.formatter.format(this.selectedFromDate),
              dateTo: this.formatter.format(this.selectedToDate),
              type: "inRange",
            }),
          })
          : JSON.stringify({
            firstCondition: transformDate({
              dateFrom: this.formatter.format(this.selectedFromDate),
              dateTo: null,
              type: "equals",
            }),
          });

      default:
        return null;
    }
  }

  private buildDateRangeFromToFilter(): DateFromTo {
    if (!this.selectedDateRange) {
      return {
        dateFrom: moment().format("YYYY-MM-DD"),
        dateTo: moment().format("YYYY-MM-DD"),
      };
    }

    switch (this.selectedDateRange.id) {
      case DateRangeEnum.Today:
        return {
          dateFrom: moment().format("YYYY-MM-DD"),
          dateTo: moment().format("YYYY-MM-DD"),
        };

      case DateRangeEnum.ThisWeek:
        return {
          dateFrom: moment().add("-7", "days").format("YYYY-MM-DD"),
          dateTo: moment().format("YYYY-MM-DD"),
        };

      case DateRangeEnum.ThisMonth:
        return {
          dateFrom: moment().add("-30", "days").format("YYYY-MM-DD"),
          dateTo: moment().format("YYYY-MM-DD"),
        };

      case DateRangeEnum.CustomRange:
        if (!this.selectedFromDate && !this.selectedToDate) {
          return {
            dateFrom: moment().format("YYYY-MM-DD"),
            dateTo: moment().format("YYYY-MM-DD"),
          };
        }

        if (this.selectedToDate) {
          let dates = transformDate({
            dateFrom: this.formatter.format(this.selectedFromDate),
            dateTo: this.formatter.format(this.selectedToDate),
            type: "inRange",
          });

          return {
            dateFrom: moment(dates.from).format("YYYY-MM-DD").substring(0, 10),
            dateTo: moment(dates.to).format("YYYY-MM-DD").substring(0, 10),
          };
        } else {
          let dates = transformDate({
            dateFrom: this.formatter.format(this.selectedFromDate),
            dateTo: moment().format("YYYY-MM-DD"),
            type: "inRange",
          });

          return {
            dateFrom: moment(dates.from).format("YYYY-MM-DD").substring(0, 10),
            dateTo: moment(dates.to).format("YYYY-MM-DD").substring(0, 10),
          };
        }

      default:
        return {
          dateFrom: moment().format("YYYY-MM-DD"),
          dateTo: moment().format("YYYY-MM-DD"),
        };
    }
  }

  public onDateSelection(date: NgbDate) {
    if (!this.fromDate && !this.toDate) {
      this.fromDate = date;
    } else if (
      this.fromDate &&
      !this.toDate &&
      date &&
      date.after(this.fromDate)
    ) {
      this.toDate = date;
    } else {
      this.toDate = null;
      this.fromDate = date;
    }

    this.selectedDateRange = {
      ...this.selectedDateRange,
      name: this.rangeFormat(this.fromDate, this.toDate),
    };
  }

  public rangeFormat(dateFrom: NgbDate | null, dateTo: NgbDate | null): string {
    let dateRange: string = "";

    if (dateFrom && dateTo && !dateFrom.equals(dateTo)) {
      dateRange = `${this.formatter.format(dateFrom)} - ${this.formatter.format(
        dateTo
      )}`;
    } else {
      dateRange = dateFrom ? this.formatter.format(dateFrom) : "";
    }

    return dateRange;
  }

  public onApplyCustomRangeFilter(): void {
    this.selectedFromDate = this.fromDate;
    this.selectedToDate = this.toDate;

    if (this.selectedFromDate) {
      this.getDashboardInput();
      this.saveDashboardInputFilterState();
    } else {
      this.onDateRangeChanged(null);
    }

  }

  public isHovered(date: NgbDate) {
    return (
      this.fromDate &&
      !this.toDate &&
      this.hoveredDate &&
      date.after(this.fromDate) &&
      date.before(this.hoveredDate)
    );
  }

  public isInside(date: NgbDate) {
    return this.toDate && date.after(this.fromDate) && date.before(this.toDate);
  }

  public isRange(date: NgbDate) {
    return (
      date.equals(this.fromDate) ||
      (this.toDate && date.equals(this.toDate)) ||
      this.isInside(date) ||
      this.isHovered(date)
    );
  }

  public onTabsClick(dueDate: number): void {
    //this.getDashboardInput(this.selectedOrganization?.id);
    this.saveDashboardInputFilterState();
  }

  private saveDashboardInputFilterState(): void {
    this.filtersRepository.save({
      dateRange: {
        selectedDateRange: this.selectedDateRange,
        fromDate: this.selectedFromDate,
        toDate: this.selectedToDate,
      },
      organization: this.selectedOrganization,
      inputTab: this.selectedTab,
    });
  }

  private getDashboardInputFilter(): DashboardInputFilter {
    const filter: DashboardInputFilter = {
      organizationId: this.selectedOrganization?.id,
      dateRange: this.buildDateRangeFilter(),
      dateRangeFromTo: this.buildDateRangeFromToFilter(),
      inputTab: this.selectedTab,
    };

    return filter;
  }

  private getFilteredDashboardInputTable(tableData: DashboardInputTable[], tableName: string): DashboardInputTable[] {
    return tableData.filter((table) => table.name == tableName);
  }

  private totalCountAssign(dashboardInputTable: DashboardInputTable[]) {
    return dashboardInputTable
      .filter((t) => t.name == 'Daily').forEach((dit) => {
        dit.dashboardInputGroups.forEach((dig) => {
          dig.dashboardInputElements.forEach((die) => {
            /*.reduce((n, { value }) => n + value, 0);*/
          });
        });
      });
  }
}
