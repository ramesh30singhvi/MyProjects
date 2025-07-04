import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { UserStatus, UserStatusOptions } from "../common/constants/userStatus";
import { CreateUser, ITimeZone } from "../models/users/users.model";
import { OrganizationService } from "../services/organization.service";
import { UserService } from "../services/user.service";
import { RolesEnum } from "../models/roles.model";
import { FacilityService } from "../services/facility.service";

export abstract class AddEditUserBase {
  public firstName: string;
  public lastName: string;
  public email: string;
  public password: string;
  public showPassword: boolean;
  public timeZones: ITimeZone[] = [];
  public selectedTimeZone: ITimeZone;
  public roles: any[] = [];
  public selectedRoles: any[] = [];
  public organizations: any[] = [];
  public teams:any[] = []
  public selectedOrganizations: any[] = [];
  public selectedTeams: any[] = [];
  public facilities: any[] = [];
  public selectedFacilities: any[] = [];
  public statuses = [
    UserStatusOptions[UserStatus.Active],
    UserStatusOptions[UserStatus.Inactive]
  ];
  public selectedStatus = UserStatusOptions[UserStatus.Active];

  public errors: any[];
  public errorMessage: string;

  constructor(
    public userService: UserService,
    public organizationService: OrganizationService,
    public facilityService: FacilityService,
    public activeModal: NgbActiveModal
  ) {
    this.userService
    .getTimeZones()
    .subscribe((timeZones) => (this.timeZones = timeZones));

    this.userService.getTeams()
      .subscribe((teams) => (this.teams = teams));

    this.userService
      .getRoles()
      .subscribe((roles) => (this.roles = roles));

    this.organizationService
      .getOrganizationAdminOptions()
      .subscribe((organizations) => (this.organizations = organizations));
  }

  public onClose(): void {
    this.activeModal.dismiss();
  }

  abstract onSave(): void;

  public onToggleShowPassword(): void {
    this.showPassword = !this.showPassword;
  }

  public onTimeZoneSelected(timeZone: ITimeZone): void {
    if (this.errors && this.errors["TimeZone"]) {
      this.errors["TimeZone"] = null;
    }
  }

  public onRoleSelected(role: any): void {
    if (this.errors && this.errors["Roles"]) {
      this.errors["Roles"] = null;
    }
  }
  public onTeamSelected(team: any): void {

  }
  public onOrganizationSelected(organization: any): void {
    if (this.errors && this.errors["Organizations"]) {
      this.errors["Organizations"] = null;
    }
    if (this.selectedOrganizations.filter(organization => organization.unlimited != 0).length == 1) {
      this.facilityService.getFacilityOptions(
        this.selectedOrganizations.find(organization => organization.unlimited != 0).id
      ).subscribe((options) => {
        this.facilities = options;
      });
    }

  }

  public onFacilitySelected(facility: any): void {
    if (this.errors && this.errors["Facilities"]) {
      this.errors["Facilities"] = null;
    }
  }

  public onStatusSelected(status: any): void {
    this.selectedStatus = status;
  }

  public onFirstNameChange(): void {
    if (this.errors && this.errors["FirstName"]) {
      this.errors["FirstName"] = null;
    }
  }

  public onLastNameChange(): void {
    if (this.errors && this.errors["LastName"]) {
      this.errors["LastName"] = null;
    }
  }

  public onEmailChange(): void {
    if (this.errors && this.errors["Email"]) {
      this.errors["Email"] = null;
    }
  }

  public onPasswordChange(): void {
    if (this.errors && this.errors["Password"]) {
      this.errors["Password"] = null;
    }
  }

  protected mapUserData(): CreateUser {
    return {
      firstName: this.firstName,
      lastName: this.lastName,
      email: this.email,
      password: this.password,
      timeZone: this.selectedTimeZone?.id,
      roles: this.selectedRoles.map(({ id }) => id),
      organizations: this.selectedOrganizations.map(({ id }) => id),
      teams: this.selectedTeams.map(({ id }) => id),
      unlimited: this.selectedOrganizations
        .some(({ unlimited }) => unlimited),
      facilities: this.selectedFacilities.filter(fac => fac.id!=0).map(({ id }) => id),
      facilityUnlimited: this.selectedFacilities
        .some(({ facilityUnlimited }) => facilityUnlimited),
      status: this.selectedStatus.id,
    };
  }

  public showFacilityAccess(): boolean {
    return (
      this.selectedRoles.filter(role => role.name == RolesEnum.Facility).length>0 &&
      this.selectedOrganizations.filter(organization => organization.id!=0).length == 1
      );
    return false;
  }
}
