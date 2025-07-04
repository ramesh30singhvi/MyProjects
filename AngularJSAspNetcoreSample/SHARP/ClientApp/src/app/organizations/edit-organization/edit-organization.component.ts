import { HttpErrorResponse } from '@angular/common/http';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { Options } from 'ngx-csv';
import { Observable } from 'rxjs';
import { IOption } from '../../models/audits/audits.model';
import { OrganizationDetailed, PortalFeature } from '../../models/organizations/organization.detailed.model';
import { RecipientModel } from '../../models/organizations/recipient.model';
import { OrganizationService } from '../../services/organization.service';
import { UserService } from '../../services/user.service';

@Component({
  selector: "app-edit-organization",
  templateUrl: "./edit-organization.component.html",
  styleUrls: ["./edit-organization.component.scss"],
})
export class EditOrganizationComponent implements OnInit {
  public errors: any[];
  public errorMessage: string;
  operatorName: string;
  operatorEmail: string;
  emailInvalid: boolean = false;
  attachReportToEmail = false;


  @Input() organizationName: string;
  @Input() detailedOrganization: OrganizationDetailed;
  public recipients: RecipientModel[];
  public organizationPortalFeatures: PortalFeature[];
  public recipientForm: FormGroup;
  @Input()  portalFeatures: IOption[];
  msg: string = "";
  constructor(
    public activeModal: NgbActiveModal,
    public userService: UserService,
    public organizationService: OrganizationService,
    private formBuilder: FormBuilder
  ) {}

  ngOnInit(): void {
    this.operatorName = this.detailedOrganization.operatorName;
    this.operatorEmail = this.detailedOrganization.operatorEmail;
    this.attachReportToEmail = this.detailedOrganization.attachPortalReport;
    this.recipients = [...this.detailedOrganization.recipients];
    this.organizationPortalFeatures = [...this.detailedOrganization.portalFeatures]
    this.recipientForm = this.formBuilder.group({
      recipient: new FormControl({ value: this.recipients.map(val => val.recipient), disabled: false }, Validators.email),

    });
  }

  public onOrganizationNameChange(): void {
    if (this.errors && this.errors["OrganizationName"]) {
      this.errors["OrganizationName"] = null;
    }
  }

  public featureAvailible(name: string) {
    if (this.organizationPortalFeatures.length == 0)
      return false;

    var feature = this.organizationPortalFeatures.filter(x => x.name == name);
    return feature.length == 0 ? false : feature[0].available;
  }

  trackById(index: any, item: any) {
    return item.id;
  }

  changeAttachReportToEmail(checked: boolean) {
    this.attachReportToEmail = checked;
  }
  public changeAvailability(f: IOption, checked: boolean) {

    var index = this.organizationPortalFeatures.findIndex(x => x.name == f.name);

    if (index >= 0) {
      this.organizationPortalFeatures[index] = {
        id: f.id, name: f.name, available: checked
      };
    } else {
      var feature = {
        id: f.id, name: f.name, available: checked
      };

      this.organizationPortalFeatures.push(feature);
    }


  }
  validateEmail() {
    this.emailInvalid = Validators.email({ value: this.operatorEmail } as AbstractControl) !== null;
  }
  public onSave(): void {
    var recipientsList = this.getRecipients();

    if (this.operatorEmail != "") {
      this.validateEmail();

      if (this.emailInvalid)
        return;

    }

    this.organizationService
      .editOrganization(this.detailedOrganization.id, this.organizationName, recipientsList,
        this.organizationPortalFeatures, this.operatorEmail, this.operatorName, this.attachReportToEmail)
      .subscribe(
        (response) => this.activeModal.close(response),
        (response: HttpErrorResponse) => {
          this.errors = response.error.errors;
          this.errorMessage = response.error.errorMessage;
        }
      );
  }
  getRecipients() {
    var recipients = this.recipientForm.get("recipient").value;
    var recipientValues = new Array<string>();
    for (var i = 0; i < recipients.length; i++) {
      if (recipients[i].value) {
        recipientValues.push(recipients[i].value);
      }
      else {
        recipientValues.push(recipients[i]);
      }
    }

    return recipientValues;
  }
  public onClose(): void {
    this.activeModal.dismiss();
  }
  public buttonTitle(featureName) {
    if (this.organizationPortalFeatures.length == 0)
      return "Add High Alert To Form";

    var feature = this.organizationPortalFeatures.filter(x => x.name == featureName);
    if (feature.length == 0)
      return "Add High Alert To Form";

    return feature[0].available ? "Add High Alert To Form" : "Remove High Alert To Form";
  }

  public addHighAlertToForm(): void {
    this.organizationService.addHighAlertToForms(this.detailedOrganization?.id).subscribe(result => {
      console.log(result);
      if (result == true)
        this.msg = "The forms were changed successfully.";
    });
  }
}
