import { Component, EventEmitter, Input, Output } from "@angular/core";
import { combineLatest } from "rxjs";
import { Observable } from "rxjs";
import { ReportsService } from "../../services/reports.service";
import { ScriptService } from "../tableau/scripts.service";
//import { TokenService } from "../tableau/tokenService";
import { ITableau } from "../tableau/tableau"

declare var tableau: any;

export class VizCreateOptions {
  disableUrlActionsPopups?: boolean;
  hideTabs?: boolean;
  hideToolbar?: boolean;
  instanceIdToClone?: string;
  height?: string = '100%';
  width?: string = '100%';
  device?: string;
  onFirstInteractive?: (event: any) => void;
  onFirstVizSizeKnown?: (event: any) => void;
  toolbarPosition?: (event: any) => void;
}

@Component({
  selector: 'cider-tableau',
  template: `
      <div class="cider-tableau" id="tableauViz"></div>
    `,
})
export class TableauComponent {
  tableauViz!: ITableau;

  @Input() reportUrl: string;
  @Input() jsScriptUrl: string;

  constructor(
    private scriptService: ScriptService,
    private reportService: ReportsService) {
  }

  ngOnInit(): void {
    this.scriptService
      .load('tableau')
      .then(data => {
        this.renderTableauViz(this.reportUrl);
      })
  }

  renderTableauViz(reportUrl: string) {
    const placeholderDiv = document.getElementById('tableauViz');
    this.tableauViz = new tableau.Viz(
      placeholderDiv,
      reportUrl,
      { width: '100%', height: placeholderDiv?.offsetParent?.clientHeight ?? '820px', hideToolbar: true }
    );
  }
}
