import { HttpErrorResponse } from "@angular/common/http";
import { Component, Input, OnInit, ViewEncapsulation } from "@angular/core";
import { FormBuilder, FormControl, FormGroup, Validators } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { NgxSpinnerService } from "ngx-spinner";
import { first, Observable, of, pipe } from "rxjs";
import { SPINNER_TYPE } from "src/app/common/constants/audit-constants";
import { IFacilityTimeZoneOption, IOption } from "src/app/models/audits/audits.model";
import { IEmailRecipient, IFacilityDetails } from "src/app/models/organizations/facility.model";
import { FacilityService } from "src/app/services/facility.service";
import { OrganizationService } from "src/app/services/organization.service";

@Component({
  selector: "app-edit-facility-modal",
  templateUrl: "./edit-facility-modal.component.html",
  styleUrls: ["./edit-facility-modal.component.scss"],
  encapsulation: ViewEncapsulation.None
})

export class EditFacilityModalComponent implements OnInit {
  @Input() organization: IOption;
  @Input() title: string;
  @Input() actionButtonLabel: string;
  @Input() facilityId: number;

  public editFacility: IFacilityDetails;

  public spinnerType: string;

  public facilityForm : FormGroup;

  public organizations$: Observable<IOption[]>;
  public timeZones$: Observable<IFacilityTimeZoneOption[]>;

  public statuses: IOption[] = [{id: 1, name: 'Active'}, {id: 2, name: 'Inactive'}];

  public asyncErrorMessages = {
    invalidEmail: 'Email address is not valid.'
  };

  public asyncValidators = [this.validateEmailAsync];

  public errors: any[] = [];
  
  constructor(
    public facilityServiceApi: FacilityService,
    public organizationServiceApi: OrganizationService,
    private formBuilder: FormBuilder,
    public activeModal: NgbActiveModal,
    private spinner: NgxSpinnerService,
    ) { 
    this.spinnerType = SPINNER_TYPE;

    this.facilityForm = this.formBuilder.group({
      facilityName: new FormControl(null, Validators.required),
      organization: new FormControl(null, Validators.required),
      timeZone: new FormControl(null, Validators.required),
      emailRecipients: new FormControl([]),
      status: new FormControl(this.statuses[0], Validators.required),
      facilityLegalName: new FormControl(null),
    });
  }

  ngOnInit() {
    this.organizations$ = this.organizationServiceApi.getOrganizationOptions();
    this.timeZones$ = this.facilityServiceApi.getTimeZoneOptions();

    if(this.facilityId) {
      this.facilityServiceApi.getFacilityDetails(this.facilityId)
      .pipe(first())
      .subscribe((facility: IFacilityDetails) => {
        this.editFacility = facility;

        this.facilityForm.setValue({
          facilityName: facility.name,
          organization: facility.organization,
          timeZone: facility.timeZone,
          emailRecipients: facility.recipients?.map((recipient: IEmailRecipient) => {return {id: recipient.id, email: recipient.email}}) ?? [],
          status: facility?.isActive === false ? this.statuses[1] : this.statuses[0],
          facilityLegalName: facility.legalName,
        });

        if(this.facilityForm.value['organization']) {
          this.facilityForm.controls['organization'].disable();
        }
      });
    } else {
      if (this.organization){
        this.facilityForm.patchValue({
          organization: this.organization
        });

        this.facilityForm.controls['organization'].disable();
      }      
    }
  }

  public onSaveClick(): void {
    if(this.facilityForm.invalid) {
      return;
    }

    const facility: IFacilityDetails = {
      id: this.editFacility?.id,
      name: this.facilityForm.controls['facilityName'].value,
      organization: this.facilityForm.controls['organization'].value,
      timeZone: this.facilityForm.controls['timeZone'].value,
      recipients: this.facilityForm.controls['emailRecipients'].value?.map((recipient) => {
        return{
          id: Number.isInteger(recipient.id) ? recipient.id : null, 
          email: recipient.email
        }
      }),
      isActive: this.facilityForm.controls['status'].value?.id === 1,
      legalName: this.facilityForm.controls['facilityLegalName'].value,
    };

    if(this.editFacility?.id) {
      this.facilityServiceApi.editFacility(facility)
      .pipe(first())
      .subscribe({
        next: (formDetails: any) => {
          if(formDetails) {
            this.facilityForm.reset();

            this.activeModal.close(formDetails);
          }
        },
        error: (response: HttpErrorResponse) =>
        {
          this.spinner.hide('facilityEditSpinner');
          this.errors = response.error?.errors;
          console.error(response);
        }
      });
    } else {
      this.facilityServiceApi.addFacility(facility)
      .pipe(first())
      .subscribe({
        next: (formDetails: any) => {
          if(formDetails) {
            this.facilityForm.reset();

            this.activeModal.close(formDetails);
          }
        },
        error: (response: HttpErrorResponse) =>
        {
          this.spinner.hide('facilityEditSpinner');
          this.errors = response.error?.errors;
          console.error(response);
        }
      });
    }
  }

  public onNameChanged(): void {
    this.errors['Name'] = null;
  }

  public onNamefocusout(event: FocusEvent) {
    let legalName: string = this.facilityForm.controls['facilityLegalName'].value;

    if (!legalName || legalName.length == 0) {
      this.facilityForm.patchValue({
        facilityLegalName: this.facilityForm.controls['facilityName'].value
      });
    }
  }

  public onLegalNameChanged(): void {
    this.errors['LegalName'] = null;
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
}
