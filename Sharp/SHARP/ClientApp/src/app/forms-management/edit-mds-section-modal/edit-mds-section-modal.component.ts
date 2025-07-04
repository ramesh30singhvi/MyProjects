import { HttpErrorResponse } from "@angular/common/http";
import { Component, Input, OnInit } from "@angular/core";
import { FormBuilder, FormControl, FormGroup, Validators } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { NgxSpinnerService } from "ngx-spinner";
import { first } from "rxjs";
import { SPINNER_TYPE } from "src/app/common/constants/audit-constants";
import { ISection } from "src/app/models/audits/questions.model";
import { FormServiceApi } from "src/app/services/form-api.service";

@Component({
  selector: "app-edit-mds-section-modal",
  templateUrl: "./edit-mds-section-modal.component.html",
  styleUrls: ["./edit-mds-section-modal.component.scss"]
})

export class EditMdsSectionModalComponent implements OnInit {

  @Input() formVersionId: number;
  @Input() title: string;
  @Input() actionButtonLabel: string;

  @Input() editSection: ISection;

  public spinnerType: string;

  public sectionForm: FormGroup;

  public errors: any[] = [];

  constructor(private formBuilder: FormBuilder, public activeModal: NgbActiveModal, private formServiceApi: FormServiceApi, private spinner: NgxSpinnerService, ) {
    this.spinnerType = SPINNER_TYPE;
  }

  ngOnInit(): void {
    this.sectionForm = this.formBuilder.group({
      name: new FormControl({value: this.editSection?.name, disabled: false}, Validators.required)
    });
  }


  public onSaveClick(): void {
    const section: ISection = {
      ...this.sectionForm.value,
      id: this.editSection?.id,
      formVersionId: this.formVersionId
    }

    if (this.editSection?.id) {
      this.formServiceApi.editSection(section)
      .pipe(first())
      .subscribe({
        next: (formDetails: any) => {
            this.sectionForm.reset();
            this.activeModal.close(formDetails);
        },
        error: (response: HttpErrorResponse) => {
          this.spinner.hide("mdsSectionEditSpinner")
          this.errors = response.error?.errors;
          console.error(response);
        }
      })
    } else {
      this.formServiceApi.addSection(section)
      .pipe(first())
      .subscribe({
        next: (formDetails: any) => {
            this.sectionForm.reset();
            this.activeModal.close(formDetails);
        },
        error: (response: HttpErrorResponse) => {
          this.spinner.hide("mdsSectionEditSpinner")
          this.errors = response.error?.errors;
          console.error(response);
        }
      })
    }
  }
}
