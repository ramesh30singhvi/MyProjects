import { ChangeDetectionStrategy, Component, OnInit } from "@angular/core";
import {
  NgbDate,
  NgbDateParserFormatter,
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
  public selectedOrganizations: IOption[];

  public canEdit: boolean = false;

  constructor(
    private dashboardServiceApi: DashboardService,
    private organizationServiceApi: OrganizationService,
    private authService: AuthService,
    public formatter: NgbDateParserFormatter,
    private toastr: ToastrService,
    private modalService: NgbModal
  ) {
    this.getOrganizations();
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

  private getDashboardInput(id: number) {
    this.dashboardServiceApi
    .getDashboardInput(id)
    .pipe(first())
    .subscribe((data: DashboardInput[]) => {
      this.dashboardInput = data;
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
            });
          });
        });
      });
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
    this.getDashboardInput(organization.id);
  }

  totalNumberOfElementsInGroup(group: DashboardInputGroup) {
    return group.dashboardInputElements.length;
  }

  totalValueByElement(element: DashboardInputElement) {
    return this.dashboardInputValues
      .filter((div) => div.elementId == element.id)
      .reduce((n, { value }) => n + value, 0);
  }

  totalValueByFacility(facility: FacilityModel) {
    return this.dashboardInputValues
      .filter((div) => div.facilityId == facility.id)
      .reduce((n, { value }) => n + value, 0);
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
}
