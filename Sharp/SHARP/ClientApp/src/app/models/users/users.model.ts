import { UserProductivityReportType } from "src/app/users/user-productivity/user-productivity.component";
import { IFilterOption } from "../audits/audit.filters.model";
import { IOption } from "../audits/audits.model";
import { RolesEnum } from "../roles.model";

export interface User {
  name: string;
  email: string;
  roles: RolesEnum[];
  access: { unlimited: boolean; organizations: string[] };
}

export interface CreateUser {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  timeZone: string;
  roles: number[];
  unlimited: boolean;
  facilityUnlimited: boolean;
  organizations: number[];
  teams: number[];
  facilities: number[];
  status: number;
}
export interface CreateSharpUser {
  firstName: string;
  lastName: string;
  email: string;
  timeZone: string;
  roles: number[];
  unlimited: boolean;
  facilityUnlimited: boolean;
  organizations: number[];
  facilities: number[];
  status: number;
  position: string
}
export interface EditUser extends CreateUser {
  id: number;
}
export interface PortalEditUser extends CreateSharpUser {
  id: number;
}
export const UserStatuses = {
  Active: {
    id: 1,
    label: "Active",
    color: "#3AC34A",
    backgroundColor: "#C5F6CB",
  },
  Inactive: {
    id: 2,
    label: "Inactive",
    color: "#F62D51",
    backgroundColor: "rgba(246,45,81,0.14)",
  },
};

export interface IUserDetails {
  firstName: string;
  lastName: string;
  email: string;
  timeZone: ITimeZone;
  roles: IOption[];
  unlimited: boolean;
  organizations: IOption[];
  teams: IOption[];
  facilityUnlimited: boolean;
  facilities: IOption[];
  status: number;
}

export enum ActionTypeEnum {
  Login = 1,
  Logout,
  IdleLogout,
  InitAudit,
  CreateAudit,
  SaveAudit,
  DeleteAudit,
  DuplicateAudit,
  ArchiveAudit,
  SendForApprovalAudit,
  ReopenAudit,
  DissapproveAudit,
  SubmitAudit,
  DownloadPDFAudit,
  UndeleteAudit,
  UnarchiveAudit,
  ApproveAudit,
  NewAccount,
  RoleChange,
  OrganizationChange,
  InactiveChange,
  FailedLogin,
}

export interface IUserOrganizations {
  organizations: IOption[];
  filteredByUserId?: number;
}

export interface IUserFacilities {
  facilities: IOption[];
  filteredByUserId?: number;
}

export interface ITimeZone {
  id: string;
  displayName: string;
}

export interface UserFiltersModel {
  name?: Array<IFilterOption>;
  email?: Array<IFilterOption>;
  roles?: Array<IFilterOption>;
  access?: Array<IFilterOption>;
  facilityAccess?: Array<IFilterOption>;
  status?: Array<IFilterOption>;
}

export interface IUserProductivityFilter {
  userId: number | undefined;
  fromDate: string;
  toDate: string;
  type: UserProductivityReportType;
  filterModel: any | undefined;
  userFilterValues: UserFiltersModel;
}
