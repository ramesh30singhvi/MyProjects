import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from "@angular/core";
import { Title } from "@angular/platform-browser";
import { ActivatedRoute } from "@angular/router";
import { first } from "rxjs";
import { Report } from "../../models/reports/reports.model";
import { ReportsService } from "../../services/reports.service";
import { ExportService } from "../tableau/exportService";
import { TableauComponent } from "../tableau/tableau.component";

declare var tableau: any;

@Component({
  selector: "app-report-details",
  templateUrl: "./report-details.component.html",
  styleUrls: ["./report-details.component.scss"]
})

export class ReportsDetails implements OnInit {
  @ViewChild("tableauViz") tableauСontainer: ElementRef;
  @ViewChild(TableauComponent, { static: false }) tableauComponent!: TableauComponent

  public tableuSrc: string = 'https://tableau.sharptests.net/';
  public reportId: string;
  constructor(
    private reportsService: ReportsService,
    private titleService: Title,
    private route: ActivatedRoute,
    private exportService: ExportService
  ) {
    this.reportId = this.route.snapshot.paramMap.get('id');
  }
  report: Report;
  ngOnInit() {
    this.titleService.setTitle("SHARP reports");

    this.reportsService.getReportDetails(this.reportId).subscribe(response => {
      this.report = response;
    });
  }

  handleExportClick() {
    this.exportService.openExportPDFDialog(this.tableauComponent.tableauViz);
  }

  handlePrintClick() {
    this.exportService.openDownloadDiaglog(this.tableauComponent.tableauViz);
  }

}
