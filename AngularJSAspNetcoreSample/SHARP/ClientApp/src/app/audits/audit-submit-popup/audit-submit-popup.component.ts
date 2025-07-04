import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

@Component({
  selector: 'app-audit-submit-popup',
  templateUrl: './audit-submit-popup.component.html',
  styleUrls: ['./audit-submit-popup.component.scss']
})
export class AuditSubmitPopupComponent implements OnInit {

  public formGroup: FormGroup;
  constructor(
    public activeModal: NgbActiveModal,
    private formBuilder: FormBuilder,
  ) {
    this.formGroup = this.formBuilder.group({
      reportType: new FormControl('2', Validators.required),
    });
  }

  ngOnInit(): void {
  }

  submit() {
    if (!this.formGroup.valid) {
      return;
    }

    this.activeModal.close(parseInt(this.formGroup.controls["reportType"].value));
  }

}
