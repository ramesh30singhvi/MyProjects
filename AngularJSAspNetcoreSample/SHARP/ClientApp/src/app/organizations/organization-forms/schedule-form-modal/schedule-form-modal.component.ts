import { WeekDay } from "@angular/common";
import { HttpErrorResponse } from "@angular/common/http";
import { Component, Input, OnInit } from "@angular/core";
import { FormBuilder, FormControl, FormGroup, Validators } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { NgxSpinnerService } from "ngx-spinner";
import { first, Subscription } from "rxjs";
import { SPINNER_TYPE } from "src/app/common/constants/audit-constants";
import { IFormSetting, IScheduleSetting, ScheduleType, ScheduleTypes, SettingType, weekDays } from "src/app/models/forms/forms.model";
import { OrganizationService } from "src/app/services/organization.service";

@Component({
  selector: "app-schedule-form-modal",
  templateUrl: "./schedule-form-modal.component.html",
  styleUrls: ["./schedule-form-modal.component.scss"]
})

export class ScheduleFormModalComponent implements OnInit {
  @Input() formOrganizationId: number;
  @Input() formName: string;
  @Input() settingType: SettingType;
  @Input() scheduleSetting: IScheduleSetting;

  public scheduleForm : FormGroup;

  public spinnerType: string;

  public errors: any[] = [];

  public settingTypeEnum = SettingType;
  public scheduleTypeEnum = ScheduleType;

  public scheduleTypes = ScheduleTypes;
  public weekDays = weekDays.filter((day) => day.id !== WeekDay.Saturday);

  private subscription: Subscription;
  
  constructor(
    public organizationServiceApi: OrganizationService,
    private formBuilder: FormBuilder,
    public activeModal: NgbActiveModal,
    private spinner: NgxSpinnerService,) { 
    this.spinnerType = SPINNER_TYPE;

    this.subscription = new Subscription();

    this.scheduleForm = this.formBuilder.group({
      scheduleType: new FormControl(null),
      days: new FormControl(null),
    });

    this.subscription.add(this.scheduleForm.controls['scheduleType'].valueChanges.subscribe(value  => {
      if(value?.id === ScheduleType.Monthly) {
        this.scheduleForm.controls['days'].setValidators([Validators.required, Validators.min(1), Validators.max(31)]);
      } else {
        this.scheduleForm.controls['days'].clearValidators();
        this.scheduleForm.controls['days'].setValidators(Validators.required);
      }

      this.scheduleForm.updateValueAndValidity();
      this.scheduleForm.controls['days'].reset();
    }));
  }

  ngOnInit() {
    if(this.settingType === SettingType.Scheduled) {
      this.scheduleForm.setValue({
        scheduleType: ScheduleTypes.find((scheduleType) => scheduleType.id === this.scheduleSetting?.scheduleType),
        days: this.scheduleSetting?.scheduleType === ScheduleType.Weekly ? weekDays[this.scheduleSetting?.days?.[0]] : this.scheduleSetting?.days?.[0]
      });
    }
  }

  public setSettingType(settingType: SettingType): void {
    this.settingType = settingType;

    if(this.settingType === SettingType.Scheduled) {
      this.scheduleForm.controls['scheduleType'].setValidators(Validators.required);
      this.scheduleForm.controls['days'].setValidators(Validators.required);
    } else 
    {
      this.scheduleForm.clearValidators();
    }

    this.scheduleForm.updateValueAndValidity();
    this.scheduleForm.reset();
  }

  public onSaveClick(): void {
    if (this.settingType === SettingType.Scheduled && this.scheduleForm.invalid){
      return;
    }

    const formSetting: IFormSetting = {
      id: this.formOrganizationId,
      settingType: this.settingType,

      scheduleSetting: this.settingType === SettingType.Scheduled ? 
      {
        id: this.formOrganizationId,
        scheduleType: this.scheduleForm.controls['scheduleType'].value?.id,
        days: [this.scheduleForm.controls['days'].value?.id ?? this.scheduleForm.controls['days'].value],
      } : null
    };

    this.organizationServiceApi.setFormScheduleSetting(formSetting)
    .pipe(first())
    .subscribe({
      next: (result: boolean) => {
        if(result) {
          this.scheduleForm.reset();
          this.activeModal.close(true);
        }
      },
      error: (response: HttpErrorResponse) =>
      {
        this.spinner.hide('scheduleFormSpinner');
        this.errors = response.error?.errors;
        console.error(response);
      }
    });
  }
}
