import { Component, Input, OnInit } from "@angular/core";
import {NgbModal, NgbActiveModal} from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: "app-confirmation-dialog",
  templateUrl: "./confirmation-dialog.component.html",
  styleUrls: ["./confirmation-dialog.component.scss"]
})

export class ConfirmationDialogComponent implements OnInit {
  @Input() confirmationBoxTitle: string;
  @Input() confirmationMessage: string;
  
  constructor(public activeModal: NgbActiveModal) { 

  }

  ngOnInit() {

  }
}
