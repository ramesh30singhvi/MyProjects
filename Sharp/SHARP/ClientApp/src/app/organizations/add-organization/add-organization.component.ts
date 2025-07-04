import { HttpErrorResponse } from '@angular/common/http';
import { Component, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { OrganizationService } from '../../services/organization.service';

@Component({
  selector: "app-add-organization",
  templateUrl: "./add-organization.component.html",
  styleUrls: ["./add-organization.component.scss"],
})
export class AddOrganizationComponent {
  public recipientForm: FormGroup;
  public errors: any[];
  public errorMessage: string;
  public organizationName: string;
  public operatorName: string;
  public operatorEmail: string;
  emailInvalid: boolean = false;
  constructor(
    public activeModal: NgbActiveModal,
    public organizationService: OrganizationService,
    private formBuilder: FormBuilder
  ) {
    this.recipientForm = formBuilder.group({
      recipient: new FormControl({ value: [], disabled: false }, Validators.email),
    });
  }

  public onOrganizationNameChange(): void {
    if (this.errors && this.errors["OrganizationName"]) {
      this.errors["OrganizationName"] = null;
    }
  }
  validateEmail() {
    this.emailInvalid = Validators.email({ value: this.operatorEmail } as AbstractControl) !== null;
  }
  public onSave(): void {
    if (this.operatorEmail != "") {
      this.validateEmail();

      if(this.emailInvalid)
        return;

    }
    var recipients = this.recipientForm.get("recipient").value.map(val => val.value);
    this.organizationService.createOrganizations(this.organizationName, recipients, this.operatorEmail, this.operatorName).subscribe(
      (response) => this.activeModal.close(response),
      (response: HttpErrorResponse) => {
        this.errors = response.error.errors;
        this.errorMessage = response.error.errorMessage;
      }
    );
  }
  
  public onClose(): void {
    this.activeModal.dismiss();
  }
}
