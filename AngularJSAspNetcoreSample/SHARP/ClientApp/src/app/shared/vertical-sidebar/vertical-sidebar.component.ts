import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { RouteInfo } from './vertical-sidebar.metadata';
import { VerticalSidebarService } from './vertical-sidebar.service';


@Component({
  selector: 'app-vertical-sidebar',
  templateUrl: './vertical-sidebar.component.html',
  styleUrls: ['./vertical-sidebar.component.scss']
})
export class VerticalSidebarComponent {
  showMenu = '';
  showSubMenu = '';
  public sidebarnavItems: RouteInfo[] = [];
  path = '';
  userFullName: string;
  public activeLink: string = '';
  public activeSubLink: string = '';

  @Input() showClass: boolean = false;
  @Output() notify: EventEmitter<boolean> = new EventEmitter<boolean>();

  handleNotify() {
    this.notify.emit(!this.showClass);
  }

  constructor(private menuServise: VerticalSidebarService, private router: Router, private authService: AuthService) {
    const user = this.authService.getCurrentUser();
    this.userFullName = user.firstName + ' ' + user.lastName;

    this.setMenuActiveLink();

    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)  
    ).subscribe((val) => {
      this.setMenuActiveLink();
    });
  }

  setMenuActiveLink() {
    this.sidebarnavItems = this.menuServise.getMenuItems();

    const pathParts = this.router.url.split("/");

    const path = pathParts?.[1];

    this.sidebarnavItems.filter(m => {
      m.active = false;
      m.submenu.filter(sm => {
        sm.active = false;
      });

    });


    this.sidebarnavItems.filter(m => {
      if (m.path === this.router.url || (path === m.path.replace("/", ""))) {
        m.active = true;
      }
      else if (m.submenu && m.submenu.length > 0) {
        m.submenu.filter(sm => {
          if (sm.path === this.router.url || (path === m.path.replace("/", ""))) {
            m.expanded = true;
            sm.active = true;
          }
        });
      }
    });
  }

  expandMenu(title: any) {
    this.sidebarnavItems.filter(m => {
      if (m.title == title) {
        m.expanded = !m.expanded;
      }
    });
  }
}
