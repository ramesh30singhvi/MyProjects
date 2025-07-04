import { HttpErrorResponse } from "@angular/common/http";
import { Component, ElementRef, Input, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormControl, FormGroup, Validators } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { DragulaService } from "ng2-dragula";
import { NgxSpinnerService } from "ngx-spinner";
import { first, Observable } from "rxjs";
import { find } from "rxjs-compat/operator/find";
import { SPINNER_TYPE } from "src/app/common/constants/audit-constants";
import { IOption } from "src/app/models/audits/audits.model";
import { IEditTrackerQuestion, ITrackerOption, ITrackerQuestion } from "src/app/models/audits/questions.model";
import { FormFieldTypes, IFormFieldItem } from "src/app/models/forms/forms.model";
import { FormServiceApi } from "src/app/services/form-api.service";
import { FormService } from "../services/form.service";

@Component({
  selector: "app-edit-tracker-question-modal",
  templateUrl: "./edit-tracker-question-modal.component.html",
  styleUrls: ["./edit-tracker-question-modal.component.scss"]
})

export class EditTrackerQuestionModalComponent implements OnInit {
  @ViewChild('fieldItemsContainer') fieldItemsContainer: ElementRef;
  
  public FIELD_ITEMS = 'FIELD_ITEMS';

  @Input() formVersionId: number;
  @Input() title: string;
  @Input() actionButtonLabel: string;

  @Input() editQuestion: IEditTrackerQuestion;

  public fieldTypes$: Observable<IOption[]>;
  public fieldItems: IFormFieldItem[] = [];

  public questionForm : FormGroup;

  public spinnerType: string;

  public errors: any[] = [];

  private isFieldItemAdded = false;
  
  constructor(
    private formServiceApi: FormServiceApi,
    private formService: FormService,
    private formBuilder: FormBuilder,
    public activeModal: NgbActiveModal,
    private dragulaService: DragulaService,
    private spinner: NgxSpinnerService,) { 
      this.spinnerType = SPINNER_TYPE;

      this.dragulaService.createGroup(this.FIELD_ITEMS, {
        direction: 'vertical',
        removeOnSpill: false,
      });
  }

  ngOnInit() {
    this.fieldTypes$ = this.formServiceApi.getFieldTypeOptions();

    this.questionForm = this.formBuilder.group({
      question: new FormControl({value: this.editQuestion?.question, disabled: false}, Validators.required),
      fieldType: new FormControl({value: this.editQuestion?.fieldType, disabled: false}, Validators.required),
      isRequired: new FormControl({value: this.editQuestion?.isRequired ?? false, disabled: false}),
      compliance: new FormControl({value: this.editQuestion?.compliance ?? false, disabled: false}),
      quality: new FormControl({value: this.editQuestion?.quality ?? false, disabled: false}),
      priority: new FormControl({value: this.editQuestion?.priority ?? false, disabled: false}),
    });

    this.fieldTypes$
    .pipe(first())
    .subscribe((types) => {
      const defaultType: IOption = types.find((type) => type.id === 4);
      if(!this.questionForm.value['fieldType']) {
        this.questionForm.patchValue({fieldType: defaultType});
      }
    });

    if(this.editQuestion)
    {
      this.fieldItems = this.editQuestion.items?.map((item: IFormFieldItem) => {return {...item}}) ?? [];
    }
  }

  ngOnDestroy() {
    this.dragulaService.destroy(this.FIELD_ITEMS);
  }

  ngAfterViewChecked(): void {
    if (this.isFieldItemAdded) this.scrollToBottom();
    
    this.isFieldItemAdded = false;
  }

  public onFieldTypeChanged(fieldType: IOption): void {
    this.errors['Items'] = null;
  }

  public onAddFieldItemClick(): void {
    this.fieldItems.push({});
    this.errors['Items'] = null;

    this.isFieldItemAdded = true;
  }

  public onDeleteFieldItemClick(index: number): void {
    this.fieldItems.splice(index, 1);
    this.errors['Items'] = null;
  }

  public valueChanged(value: string): void {
    this.errors['Items'] = null;
  }

  public onSaveClick(): void {
    if(this.isMultivalue())
    {
      if(this.fieldItems.some((item: IFormFieldItem) => !item.value)) {
        this.errors['Items'] = "'List of values' must not contain empty values";
        return;
      }

      if(this.formService.isItemsDuplicated(this.fieldItems, 'value')) {
        this.errors['Items'] = "'List of Items' should contain only unique values";
        return;
      }

      this.fieldItems.map((item: IFormFieldItem, index: number) => item.sequence = index + 1);
    } else {
      this.fieldItems = null;
    }

    const question: IEditTrackerQuestion = {
      ...this.questionForm.value,
      id: this.editQuestion?.id,
      formVersionId: this.formVersionId,
      items: this.fieldItems
    };

    if(this.editQuestion?.id) {
      this.formServiceApi.editTrackerQuestion(question)
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
          this.spinner.hide("trackerQuestionEditSpinner");
          this.errors = response.error?.errors;
          console.error(response);
        }
      });
    } else {
      this.formServiceApi.addTrackerQuestion(question)
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
          this.spinner.hide("trackerQuestionEditSpinner");
          this.errors = response.error?.errors;
          console.error(response);
        }
      });
    }
  }

  public isMultivalue () {
    const selectedFieldType = this.questionForm.value.fieldType;

    return selectedFieldType?.id === FormFieldTypes.dropdownSingleSelect.id || 
    selectedFieldType?.id === FormFieldTypes.dropdownMultiselect.id || 
    selectedFieldType?.id === FormFieldTypes.toggleSingleSelect.id ||
    selectedFieldType?.id === FormFieldTypes.toggleMultiselect.id
  }

  private scrollToBottom() {
    try{
      this.fieldItemsContainer.nativeElement.scrollTop = this.fieldItemsContainer.nativeElement.scrollHeight;
    } catch(err) {}
  }
}
