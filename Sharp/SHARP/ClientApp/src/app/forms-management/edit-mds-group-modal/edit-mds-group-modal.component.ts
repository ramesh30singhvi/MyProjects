import { HttpErrorResponse } from "@angular/common/http";
import { Component, Input, OnInit } from "@angular/core";
import { FormBuilder, FormControl, FormGroup, Validators } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { NgxSpinnerService } from "ngx-spinner";
import { Observable, Subject, first } from "rxjs";
import { SPINNER_TYPE } from "src/app/common/constants/audit-constants";
import { IOption } from "src/app/models/audits/audits.model";
import { IGroup, ISection } from "src/app/models/audits/questions.model";
import { FormServiceApi } from "src/app/services/form-api.service";

@Component({
  selector: "app-edit-mds-group-modal",
  templateUrl: "./edit-mds-group-modal.component.html",
  styleUrls: ["./edit-mds-group-modal.component.scss"]
})

export class EditMdsGroupModalComponent implements OnInit {

  @Input() formVersionId: number;
  @Input() formSectionId: number;
  @Input() title: string;
  @Input() actionButtonLabel: string;

  @Input() editGroup: IGroup;

  public spinnerType: string;

  public groupForm: FormGroup;

  public errors: any[] = [];

  constructor(
    private formBuilder: FormBuilder,
    public activeModal: NgbActiveModal,
    private formServiceApi: FormServiceApi,
    private spinner: NgxSpinnerService) {
    this.spinnerType = SPINNER_TYPE;
  }

  ngOnInit(): void {
    this.groupForm = this.formBuilder.group({
      name: new FormControl({value: this.editGroup?.name, disabled: false}, Validators.required)
    });
  }


  public onSaveClick(): void {
    const group: IGroup = {
      ...this.groupForm.value,
      id: this.editGroup?.id,
      formVersionId: this.formVersionId,
      formSectionId: this.formSectionId
    }

    if (this.editGroup?.id) {
      this.formServiceApi.editGroup(group)
        .pipe(first())
        .subscribe({
          next: (formDetails: any) => {
              this.groupForm.reset();
              this.activeModal.close(formDetails);
          },
          error: (response: HttpErrorResponse) => {
            this.spinner.hide("mdsGroupEditSpinner")
            this.errors = response.error?.errors;
            console.error(response);
          }
        })
    } else {
      this.formServiceApi.addGroup(group)
        .pipe(first())
        .subscribe({
          next: (formDetails: any) => {
              this.groupForm.reset();
              this.activeModal.close(formDetails);
          },
          error: (response: HttpErrorResponse) => {
            this.spinner.hide("mdsGroupEditSpinner")
            this.errors = response.error?.errors;
            console.error(response);
          }
        })
    }
  }
}
