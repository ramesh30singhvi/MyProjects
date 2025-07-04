import { HttpErrorResponse } from "@angular/common/http";
import { Component, Input, OnInit, ViewEncapsulation } from "@angular/core";
import { FormBuilder, FormControl, FormGroup, Validators } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { catchError, concat, distinctUntilChanged, first, Observable, of, Subject, switchMap, tap } from "rxjs";
import { SPINNER_TYPE } from "src/app/common/constants/audit-constants";
import { IOption } from "src/app/models/audits/audits.model";
import { IEditQuestion, Question } from "src/app/models/audits/questions.model";
import { FormServiceApi } from "src/app/services/form-api.service";

@Component({
  selector: "app-edit-crieria-form-modal",
  templateUrl: "./edit-crieria-form-modal.component.html",
  styleUrls: ["./edit-crieria-form-modal.component.scss"],
})

export class EditCrieriaFormModalComponent implements OnInit {
  @Input() formVersionId: number;
  @Input() title: string;
  @Input() actionButtonLabel: string;

  @Input() parentId?: number;

  @Input() editQuestion: IEditQuestion;
  
  public groups$: Observable<IOption[]>;
  public sectionLoading = false;
  public sectionInput$ = new Subject<string>();

  public questionForm : FormGroup;

  public spinnerType: string;

  public errors: any[];
  
  constructor(
    private formServiceApi: FormServiceApi,
    private formBuilder: FormBuilder,
    public activeModal: NgbActiveModal) { 
    this.spinnerType = SPINNER_TYPE;
  }

  ngOnInit() {
    this.groups$ = this.formServiceApi.getSectionOptions(this.formVersionId);

    this.questionForm = this.formBuilder.group({    
      group: new FormControl({value: this.editQuestion?.group, disabled: this.parentId ? true : false}),
      question: new FormControl({value: this.editQuestion?.question, disabled: false}, Validators.required),
      showNA: new FormControl({value: this.editQuestion?.showNA ?? false, disabled: false}),
      compliance: new FormControl({value: this.editQuestion?.compliance ?? false, disabled: false}),
      quality: new FormControl({value: this.editQuestion?.quality ?? false, disabled: false}),
      priority: new FormControl({value: this.editQuestion?.priority ?? false, disabled: false}),
    });
  }

  public onSaveClick(): void {
    const question: IEditQuestion = {
      ...this.questionForm.value,
      id: this.editQuestion?.id,
      formVersionId: this.formVersionId,
      parentId: this.parentId,
    };

    if(this.editQuestion?.id) {
      this.formServiceApi.editQuestion(question)
      .pipe(first())
      .subscribe({
        next: (formDetails: any) => {
          if(formDetails) {
            this.questionForm.reset();

            this.activeModal.close(formDetails);
          }
        },
        error: (response: HttpErrorResponse) =>
        {
          this.errors = response.error?.errors;
          console.error(response);
        }
      });
    } else {
      this.formServiceApi.addQuestion(question)
      .pipe(first())
      .subscribe({
        next: (formDetails: any) => {
          if(formDetails) {
            this.questionForm.reset();

            this.activeModal.close(formDetails);
          }
        },
        error: (response: HttpErrorResponse) =>
        {
          this.errors = response.error?.errors;
          console.error(response);
        }
      });
    }
  }
}
