import { RolesEnum } from "../../models/roles.model";

// Sidebar route metadata
export interface RouteInfo {
  path: string;
  title: string;
  icon: string;
  class: string;
  extralink: boolean;
  submenu: RouteInfo[];  
  roles?: RolesEnum[];
  expanded?: boolean;
  active?: boolean;
}
