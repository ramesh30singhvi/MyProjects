import { Component, Input, OnInit } from "@angular/core";
import { NgbModal, NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

@Component({
  selector: "app-simple-alert-dialog",
  templateUrl: "./simple-alert-dialog.component.html",
})
export class SimpleAlertDialogComponent implements OnInit {
  @Input() title: string;
  @Input() message: string;

  constructor(public activeModal: NgbActiveModal) {}

  ngOnInit() {}
}
