import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { NgxSpinnerService } from 'ngx-spinner';


@Injectable()
export class SpinnerInterceptorService implements HttpInterceptor {

  constructor(
    private spinner: NgxSpinnerService
  ) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    //this.spinner.show();
    return next
      .handle(request)
      .pipe(
        finalize(this.finalize.bind(this)),
        //catchError(this.catchError.bind(this))
      );
  }

  finalize = (): void => {this.spinner.hide()};

  /*catchError = ((err, caught): HttpErrorResponse => {
    this.spinner.hide();
    return err;
  });*/
}
