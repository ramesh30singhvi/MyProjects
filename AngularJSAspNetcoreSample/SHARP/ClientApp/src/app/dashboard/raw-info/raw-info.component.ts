import { Component, Input, OnInit } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { AuditStatuses, IOption } from "src/app/models/audits/audits.model";
import { IAuditKPIApi } from "src/app/models/dashboard/dashboard.model";
import { ngxCsv } from "ngx-csv/ngx-csv";

@Component({
  selector: "app-raw-info",
  templateUrl: "./raw-info.component.html",
  styleUrls: ["./raw-info.component.scss"],
})
export class RawInfoComponent implements OnInit {
  @Input() auditOrganizationKPIs: IAuditKPIApi[];

  @Input() organizations: IOption[];

  public statuses = Object.values(AuditStatuses).filter(
    (status) =>
      status.id != AuditStatuses.Disapproved.id &&
      status.id != AuditStatuses.Reopened.id
  );

  constructor(public activeModal: NgbActiveModal) {}

  ngOnInit() {}

  public getAuditCount(organizationId: number, statusId: number): number {
    const kpi = this.auditOrganizationKPIs.find(
      (kpi: IAuditKPIApi) =>
        kpi.organization.id === organizationId && kpi.auditStatus === statusId
    );
    return kpi?.auditCount ?? 0;
  }

  public onExportCSVClick(): void {
    const options = this.getCsvOptions();
    const data = this.getCsvData();

    new ngxCsv(data, "Audits KPI", options);
  }

  private getCsvData(): any[] {
    return this.organizations.map((organization: IOption) => {
      let organizationAuditKpi = { organzation: organization.name };
      this.statuses
        .filter(
          (status) =>
            status.id != AuditStatuses.Disapproved.id &&
            status.id != AuditStatuses.Reopened.id
        )
        .map((status) => {
          organizationAuditKpi[`status_${status.id}`] = this.getAuditCount(
            organization.id,
            status.id
          );
        });

      return organizationAuditKpi;
    });
  }

  private getCsvOptions(): any {
    let headers: string[] = ["Organization"];
    const statusLabels: string[] = this.statuses
      .filter(
        (status) =>
          status.id != AuditStatuses.Disapproved.id &&
          status.id != AuditStatuses.Reopened.id
      )
      .map((status) => status.label);

    headers = [...headers, ...statusLabels];

    return {
      fieldSeparator: ",",
      quoteStrings: '"',
      decimalseparator: ".",
      showLabels: true,
      //showTitle: true,
      //title: 'Your title',
      useBom: true,
      //noDownload: true,
      headers: headers,
    };
  }
}
