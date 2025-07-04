import { IFilterOption } from "../audits/audit.filters.model";
import { IOption } from "../audits/audits.model";
import { PortalFeature } from "./organization.detailed.model";

export interface FacilityModel {
  id: number;
  name: string;
  organizationName:string,
  facilityCount: number;
}

export interface IFacilityDetails {
  id: number;
  name: string;
  organization: IOption,
  timeZone: IOption;
  recipients: IEmailRecipient[];
  isActive: boolean;
  legalName: string;
}


export interface IEmailRecipient {
  id: number;
  email: string;
}

export const FacilityStatuses = {
  active: {id: 1, label: 'Active', color: '#0b860b'},
  inactive: {id: 0, label: 'Inactive', color: '#ff3737'},
}

export interface FacilityFiltersModel {
  name?: Array<IFilterOption>;
  organizationName?: Array<IFilterOption>;
  timeZoneName?: Array<IFilterOption>;
  active?: Array<IFilterOption>;
}
