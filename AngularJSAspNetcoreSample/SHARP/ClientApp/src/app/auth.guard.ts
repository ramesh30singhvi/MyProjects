import { Injectable } from '@angular/core';
import {
  CanActivate,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  Router
} from '@angular/router';
import { RolesEnum } from './models/roles.model';
import { AuthService } from './services/auth.service';

@Injectable({
  providedIn: 'root'
})

export class AuthGuard implements CanActivate {

  constructor(private authService: AuthService, private routes: Router) { }

  canActivate(
    { data: { roles, redirectTo } }: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): boolean {
    if (!this.authService.hasToken()) {
      this.authService.logout();
      localStorage.setItem('redirectTo', state.url);
      this.routes.navigate(['login'], { queryParams: { returnUrl: state.url } });
      return false;
    }

    if (!roles) {
      return true;
    }

    const isAuthorized = roles.some((role: RolesEnum) => this.authService.isUserInRole(role));

    if (isAuthorized) {
      localStorage.setItem('redirectTo', state.url);
      return true;
    }

    this.routes.navigate([redirectTo || '']);
    return false;
  }
}
