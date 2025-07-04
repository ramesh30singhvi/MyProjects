import { RecipientModel } from "./recipient.model";

export interface PortalFeature {
  id: number;
  name: string;
  available: boolean;
}
export interface OrganizationDetailed {
  id: number;
  name: string;
  operatorEmail: string;
  operatorName: string;
  attachPortalReport: boolean;
  facilityCount: number;
  recipients: RecipientModel[];
  portalFeatures: PortalFeature[];
}
