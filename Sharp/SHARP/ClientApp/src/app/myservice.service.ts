import { Injectable } from '@angular/core';

@Injectable()
export class MyserviceService {

  constructor() { }

  checkusernameandpassword(uname: string, pwd: string) {
    
  }

  logout() {
    // remove user from local storage to log user out
    alert(1);
    localStorage.removeItem('access_token');
	}
}
