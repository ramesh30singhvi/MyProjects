import { HttpErrorResponse } from '@angular/common/http';
import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { NgxSpinnerService } from 'ngx-spinner';
import { first, Observable } from 'rxjs';
import { SPINNER_TYPE } from '../../../common/constants/audit-constants';
import { IOption } from '../../../models/audits/audits.model';
import { IFacilityDetails } from '../../../models/organizations/facility.model';
import { FacilityService } from '../../../services/facility.service';
import { OrganizationService } from '../../../services/organization.service';

@Component({
  selector: 'app-add-emails-modal',
  templateUrl: './add-emails-modal.component.html',
  styleUrls: ['./add-emails-modal.component.scss']
})
export class AddEmailsModalComponent implements OnInit {
  @Input() organization: IOption;
  @Input() title: string;
  @Input() actionButtonLabel: string;
  @Input() facilityId: number;
  public editFacility: IFacilityDetails;
  public organizations$: Observable<IOption[]>;
  public spinnerType: string;

  public facilityForm: FormGroup;
  public asyncErrorMessages = {
    invalidEmail: 'Email address is not valid.'
  };

  public asyncValidators = [this.validateEmailAsync];

  public errors: any[] = [];
  constructor(public facilityServiceApi: FacilityService,
    public organizationServiceApi: OrganizationService,
    private formBuilder: FormBuilder,
    public activeModal: NgbActiveModal,
    private spinner: NgxSpinnerService) { }

  ngOnInit(): void {
    this.spinnerType =  SPINNER_TYPE;

    this.facilityForm = this.formBuilder.group({
      organization: new FormControl(null, Validators.required),
      facilityName: new FormControl(null, Validators.required),
      emailRecipients: new FormControl([]),

    });

    if (this.facilityId) {
      this.facilityServiceApi.getFacilityDetails(this.facilityId)
        .pipe(first())
        .subscribe((facility: IFacilityDetails) => {
          this.editFacility = facility;

          this.facilityForm.setValue({
            facilityName: facility.name,
            organization: facility.organization,

            emailRecipients:""
          });

          if (this.facilityForm.value['organization']) {
            this.facilityForm.controls['organization'].disable();
          }
        });
    } else {
      if (this.organization) {
        this.facilityForm.patchValue({
          organization: this.organization
        });

        this.facilityForm.controls['organization'].disable();
      }
    }
  }

  public hasEmailError(): boolean {
    return this.errors && Object.keys(this.errors).some((key) => key.includes('Email'));
  }

  public getEmailError(): string {
    return Object.entries(this.errors)?.find((error) => error[0].includes('Email'))?.[1]?.[0];
  }

  private validateEmailAsync(control: FormControl): Promise<any> {
    return new Promise(resolve => {
      const value = control.value;


      var regExp = new RegExp("[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$");
      const result: any = value && !regExp.test(value) ? {
        invalidEmail: true
      } : null;

      setTimeout(() => {
        resolve(result);
      }, 400);
    });
  }
  public onSaveClick(): void {
    if (this.facilityForm.invalid) {
      return;
    }
    console.log(this.facilityForm.controls['emailRecipients'].value[0].email);

    let cleanEmail = this.facilityForm.controls['emailRecipients'].value[0].email.replace(/(\r\n|\n|\r)/gm, " ");
    let emails: string[] = cleanEmail.split(" ");
    console.log(emails);
    var regExp = new RegExp(/^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/);
    let validEmails: string[] = [];
    emails.forEach(email => {
      if (email != "" && regExp.test(email.trim()))
        validEmails.push(email);
      else
        console.log(email);
    })
    if (validEmails.length == 0)
      return;
    console.log(validEmails);
    if (this.editFacility?.id) {
      this.facilityServiceApi.addEmailRecipientsFacility(this.facilityId, validEmails)
        .pipe(first())
        .subscribe({
          next: (formDetails: any) => {
            if (formDetails) {

              this.activeModal.close(formDetails);
            }
          },
          error: (response: HttpErrorResponse) => {
            this.spinner.hide('facilityEditSpinner');
            this.errors = response.error?.errors;
            console.error(response);
          }
        });
    }


  }
}
