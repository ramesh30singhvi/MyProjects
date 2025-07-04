import { Component, OnInit, Input } from '@angular/core';
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import {
  ICellRendererParams,
  IDatasource,
  IGetRowsParams,
} from 'ag-grid-community';
import "ag-grid-enterprise";
import "ag-grid-angular";
import * as moment from "moment";

@Component({
  selector: 'app-downloads-detail-popup',
  templateUrl: './downloads-detail-popup.component.html',
  styleUrls: ['./downloads-detail-popup.component.scss']
})
export class DownloadsDetailPopupComponent implements OnInit {
  @Input() detailsData: any;

  constructor(
    public activeModal: NgbActiveModal,
  ) { }

  ngOnInit(): void {
  }

  private dateFormatter(value: any): string {
    return moment(value).format("MMM DD, YYYY h:mma");
  }

}
