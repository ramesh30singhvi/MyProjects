import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import * as moment from "moment";
import { NgxSpinnerService } from "ngx-spinner";
import { map, Observable } from "rxjs";
import { environment } from "../../environments/environment";
import { YYYY_MM_DD_DASH } from "../common/constants/date-constants";
import { IEditMemo, IMemo } from "../models/memos/memos.model";

@Injectable()
export class MemoServiceApi {

  private memoUrl = `${environment.apiUrl}memos`;

  constructor(
    private httpClient: HttpClient) { }

  public getMemos(organizationIds: number[] = null): Observable<IMemo[]> {
    return this.httpClient.post<IMemo[]>(`${this.memoUrl}/get`, {
      organizationIds: organizationIds ?? [],
    })
    .pipe(
      map((data: IMemo[]) => {
        return data;
      })
    );
  }

  public addMemo(editMemo: IEditMemo): Observable<IMemo> {
    return this.httpClient
      .post<IMemo>(this.memoUrl, {
        ...editMemo
      })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public editMemo(editMemo: IEditMemo): Observable<IMemo> {
    return this.httpClient
      .put<IMemo>(this.memoUrl, {
        ...editMemo
      })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public deleteMemo(memoId: number): Observable<boolean> {
    return this.httpClient.delete<any>(`${this.memoUrl}/${memoId}`).pipe(
      map((data: boolean) => {
        return data;
      })
    );
  }
}
