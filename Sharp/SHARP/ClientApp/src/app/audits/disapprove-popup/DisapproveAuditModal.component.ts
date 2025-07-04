import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";
import { FormBuilder, FormControl, FormGroup, Validators } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

@Component({
  selector: "app-DisapproveAuditModal",
  templateUrl: "./DisapproveAuditModal.component.html",
  styleUrls: ["./DisapproveAuditModal.component.scss"]
})

export class DisapproveAuditModalComponent implements OnInit {
  reason: string;

  public reasonForm : FormGroup;

  constructor(
    public activeModal: NgbActiveModal,
    private formBuilder: FormBuilder,
  ) {
    this.reasonForm = this.formBuilder.group({    
      reason: new FormControl('', Validators.required),
    });
  }

  ngOnInit() {

  }

  save() {
    if(!this.reasonForm.valid) {
      return;
    }

    this.activeModal.close(this.reasonForm.controls["reason"].value);
  }
}
