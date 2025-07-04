import { HttpErrorResponse } from '@angular/common/http';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { OrganizationDetailed } from '../../models/organizations/organization.detailed.model';
import { RecipientModel } from '../../models/organizations/recipient.model';
import { OrganizationService } from '../../services/organization.service';
import { UserService } from '../../services/user.service';
import { FormService } from '../services/form.service';

@Component({
  selector: "app-edit-form-modal",
  templateUrl: "./edit-form-modal.component.html",
  styleUrls: ["./edit-form-modal.component.scss"],
})
export class EditFormModalComponent implements OnInit {
  public errors: any[];
  public errorMessage: string;
  @Input() formName: string;
  @Input() formId: number;
  public recipients: RecipientModel[];
  public recipientForm: FormGroup;

  constructor(
    public activeModal: NgbActiveModal,
    public userService: UserService,
    public formService: FormService,
    private formBuilder: FormBuilder
  ) {}

  ngOnInit(): void {

  }

  public onFormNameChange(): void {
    if (this.errors && this.errors["FormName"]) {
      this.errors["FormName"] = null;
    }
  }

  public onSave(): void {
    this.formService
      .editFormName(this.formId, this.formName)
      .subscribe(
        (response) => {
          this.activeModal.close(response);
        },
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
