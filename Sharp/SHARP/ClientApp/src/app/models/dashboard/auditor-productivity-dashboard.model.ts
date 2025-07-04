import { IFilterOption } from "../audits/audit.filters.model";
import { IOption } from "../audits/audits.model";

export enum TabsEnum {
  Input,
  AHT_per_AuditType,
  Summary_per_Auditor,
}

export interface AuditorProductivityInputFiltersModel {
  id?: Array<IFilterOption>;
  startTime?: string;
  CompletionTime?: string;
  userFullName?: Array<IFilterOption>;
  facilityName?: Array<IFilterOption>;
  typeOfAudit?: Array<IFilterOption>;
  noOfFilteredAuditsAllType?: Array<IFilterOption>;
  handlingTime?: Array<IFilterOption>;
  aHTPerAudit?: Array<IFilterOption>;
 // hour?: Array<IFilterOption>;
  noOfFilteredAudits?: Array<IFilterOption>;
  finalAHT?: Array<IFilterOption>;
  month?: Array<IFilterOption>;
  team?: IFilterOption;
};

export interface AuditorProductivityAHTPerAuditTypeFiltersModel {
  dateProcessed?: string;
  userFullName?: Array<IFilterOption>;
  facilityName?: Array<IFilterOption>;
  typeOfAudit?: Array<IFilterOption>;
  team?: IFilterOption;
};

export interface AuditorProductivitySummaryPerAuditorFiltersModel {
  dateProcessed?: string;
  userFullName?: Array<IFilterOption>;
  facilityName?: Array<IFilterOption>;
  typeOfAudit?: Array<IFilterOption>;
  team?: IFilterOption;
};
export interface AuditorProductivitySummaryPerFacilityFiltersModel {
  dateProcessed?: string;
  facilities?: Array<IFilterOption>;
  organization?: IFilterOption;

};
