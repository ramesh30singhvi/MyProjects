import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { ITableau } from "../tableau/tableau"

@Injectable({
  providedIn: "root"
})
export class ExportService {
  constructor(private http: HttpClient) {
  }

  openExportPDFDialog(tableau: ITableau): void {
    tableau.showExportPDFDialog();
  }

  openDownloadDiaglog(tableau: any): void {
    tableau.showDownloadDialog();
  }
}
