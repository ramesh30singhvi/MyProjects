import { HttpErrorResponse } from '@angular/common/http';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { OrganizationDetailed } from '../../models/organizations/organization.detailed.model';
import { RecipientModel } from '../../models/organizations/recipient.model';
import { OrganizationService } from '../../services/organization.service';
import { UserService } from '../../services/user.service';
import { FormService } from '../services/form.service';
import { Observable } from 'rxjs';
import { IOption } from 'src/app/models/audits/audits.model';
import { AuditServiceApi } from 'src/app/services/audit-api.service';

@Component({
  selector: "app-duplicate-form-modal",
  templateUrl: "./duplicate-form-modal.component.html",
  styleUrls: ["./duplicate-form-modal.component.scss"],
})
export class DuplicateFormModalComponent implements OnInit {
  public errors: any[];
  public errorMessage: string;
  @Input() formName: string;
  @Input() formId: number;
  public recipients: RecipientModel[];
  public recipientForm: FormGroup;
  public organizations$: Observable<IOption[]>;
  public organization: IOption;
  
  constructor(
    public activeModal: NgbActiveModal,
    public userService: UserService,
    public formService: FormService,
    private formBuilder: FormBuilder,
    private auditServiceApi: AuditServiceApi,
  ) {}

  ngOnInit(): void {
    this.organizations$ = this.auditServiceApi.getOrganizationOptions();
  }

  public onFormNameChange(): void {
    if (this.errors && this.errors["FormName"]) {
      this.errors["FormName"] = null;
    }
  }

  public onSave(): void {
    this.formService
      .duplicateForm(this.formId, this.organization.id)
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
