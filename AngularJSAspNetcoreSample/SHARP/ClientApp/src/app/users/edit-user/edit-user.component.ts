import { HttpErrorResponse } from "@angular/common/http";
import { Component, Input, OnInit, ViewEncapsulation } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { first } from "rxjs/operators";
import { ActionTypeEnum, CreateUser, ITimeZone, IUserDetails } from "src/app/models/users/users.model";
import { OrganizationService } from "src/app/services/organization.service";
import { UserService } from "../../services/user.service";
import { AddEditUserBase } from "../add-edit-user-base";
import { FacilityService } from "src/app/services/facility.service";

@Component({
  selector: "app-edit-user",
  templateUrl: "./edit-user.component.html",
  styleUrls: ["./edit-user.component.scss"],
  encapsulation: ViewEncapsulation.None,
})
export class EditUserComponent extends AddEditUserBase implements OnInit {
  @Input() userId: number;
  initialRoles: any[] = [];
  initialOrganizations: any[] = [];

  constructor(
    public activeModal: NgbActiveModal,
    public organizationService: OrganizationService,
    public userService: UserService,
    public facilityService: FacilityService,
  ) {
    super(userService, organizationService, facilityService, activeModal);
  }

  ngOnInit(): void {
    this.userService
      .getUserDetails(this.userId)
      .pipe(first())
      .subscribe((user: IUserDetails) => {
        this.firstName = user.firstName;
        this.lastName = user.lastName;
        this.email = user.email;

        this.selectedTimeZone = user.timeZone;

        this.selectedRoles = user.roles;
        this.initialRoles = this.selectedRoles;

        this.selectedOrganizations = user.organizations?.length > 0 ? user.organizations : [{ id: 0, name: 'All', unlimited: true }];
        this.initialOrganizations = this.selectedOrganizations;
        this.selectedTeams = user.teams?.length > 0 ? user.teams: [] ;
        if (this.selectedOrganizations.filter(organization => organization.id != 0).length == 1) {
          this.facilityService.getFacilityOptions(
            this.selectedOrganizations.find(organization => organization.id != 0).id
          ).subscribe((options) => {
            this.facilities = options;
            this.selectedFacilities = user.facilities?.length > 0 ? user.facilities : [{ id:0, name: 'All', unlimited: true }];
          });
        }


        this.selectedStatus = this.statuses.find(
          ({ id }) => id === user.status
        );
      });
  }

  public onSave(): void {
    let userData: CreateUser = this.mapUserData();
    debugger;
    this.userService
      .editUser({
        id: this.userId,
        ...this.mapUserData(),
      })
      .subscribe(
        () => {
          try {
            if (!this.equalArray(this.initialRoles.map(m => m.id), userData.roles)) {
              this.userService.addUserActivityLog(ActionTypeEnum.RoleChange, null, this.userId).subscribe();
            }
            if (!this.equalArray(this.initialOrganizations.map(m => m.id), userData.organizations)) {
              this.userService.addUserActivityLog(ActionTypeEnum.OrganizationChange, null, this.userId).subscribe();
            }
            if (userData.status == 2) {
              this.userService.addUserActivityLog(ActionTypeEnum.InactiveChange, null, this.userId).subscribe();
            }
          } catch (error) {
            console.log(error);
          }
          this.activeModal.close();
        },
        (response: HttpErrorResponse) => {
          this.errors = response.error.errors;
          this.errorMessage = response.error.errorMessage;
        }
      );
  }

  equalArray(a, b) {
    a = a.map(String);
    b = b.map(String);
    if (a.length === b.length) {
      for (var i = 0; i < a.length; i++) {
        if (!b.includes(a[i].toString())) {
          return false;
        }
      }
      return true;
    } else {
      return false;
    }
  }
}
