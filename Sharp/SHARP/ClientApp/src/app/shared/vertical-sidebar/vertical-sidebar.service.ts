import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { RouteInfo } from './vertical-sidebar.metadata';
import { ROUTES } from './vertical-menu-items';
import { AuthService } from '../../services/auth.service';
import { RolesEnum } from 'src/app/models/roles.model';


@Injectable({
    providedIn: 'root'
})
export class VerticalSidebarService {

    public screenWidth: any;
    public collapseSidebar: boolean = false;
    public fullScreen: boolean = false;

    items: BehaviorSubject<RouteInfo[]>;

    constructor(private authService: AuthService) {
      const routes = this.getMenuItems();
      
      this.items = new BehaviorSubject<RouteInfo[]>(routes);
    }

    public getMenuItems(): RouteInfo[] {
     var menuItems = ROUTES.filter(({ roles }) => {
        if (!roles) {
          return true;
        }

        return roles.some((role: RolesEnum) => this.authService.isUserInRole(role));
     });
      menuItems.forEach(menuItem => {
        menuItem.submenu = menuItem.submenu.filter(({ roles }) => {
          return roles.some((role: RolesEnum) => this.authService.isUserInRole(role));
        });
      })

      return menuItems;

    }
}
