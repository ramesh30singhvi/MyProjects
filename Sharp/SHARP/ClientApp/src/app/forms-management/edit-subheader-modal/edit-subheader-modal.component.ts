import { HttpErrorResponse } from "@angular/common/http";
import { Component, ElementRef, Input, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormControl, FormGroup, Validators } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { DragulaService } from "ng2-dragula";
import { NgxSpinnerService } from "ngx-spinner";
import { first, map, Observable } from "rxjs";
import { SPINNER_TYPE } from "src/app/common/constants/audit-constants";
import { IOption } from "src/app/models/audits/audits.model";
import { FormFieldTypes, IFormField, IFormFieldItem } from "src/app/models/forms/forms.model";
import { FormServiceApi } from "src/app/services/form-api.service";
import { FormService } from "../services/form.service";

@Component({
  selector: "app-edit-subheader-modal",
  templateUrl: "./edit-subheader-modal.component.html",
  styleUrls: ["./edit-subheader-modal.component.scss"]
})

export class EditSubheaderModalComponent implements OnInit {
  @ViewChild('fieldItemsContainer') fieldItemsContainer: ElementRef;
  
  public FIELD_ITEMS = 'FIELD_ITEMS';

  @Input() formVersionId: number;
  @Input() title: string;
  @Input() actionButtonLabel: string;

  @Input() editField: IFormField;
  
  public fieldTypes$: Observable<IOption[]>;

  public subheaderForm : FormGroup;

  public fieldItems: IFormFieldItem[] = [];

  public spinnerType: string;

  public fieldType = FormFieldTypes;

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
    this.fieldTypes$ = this.formServiceApi
    .getFieldTypeOptions()
    .pipe(
      map((data: IOption[]) => {
      return data.filter(fieldType => fieldType?.id !== FormFieldTypes.textArea.id);
    }));

    this.subheaderForm = this.formBuilder.group({    
      label: new FormControl({value: this.editField?.labelName, disabled: false}, Validators.required),
      isRequired: new FormControl({value: this.editField?.isRequired ?? false, disabled: false}),
      fieldType: new FormControl({value: this.editField?.fieldType, disabled: false}, Validators.required),
    });

    if(this.editField)
    {
      this.fieldItems = this.editField.items?.map((item: IFormFieldItem) => {return {...item}}) ?? [];
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

    const formField: IFormField = {
      ...this.subheaderForm.value,
      id: this.editField?.id,
      labelName: this.subheaderForm.value.label,
      fieldName: this.subheaderForm.value.label.replace(" ", ""),
      items: this.fieldItems
    };

    if(this.editField?.id) {
      this.formServiceApi.editFormField(this.formVersionId, formField)
      .pipe(first())
      .subscribe({
        next: (formDetails: any) => {
          if(formDetails) {
            this.subheaderForm.reset();
            this.activeModal.close(formDetails);
          }
        },
        error: (response: HttpErrorResponse) =>
        {
          this.spinner.hide("criteriaSubheaderEditSpinner");
          this.errors = response.error?.errors;
          console.error(response);
        }
      });
    } else {
      this.formServiceApi.addFormField(this.formVersionId, formField)
      .pipe(first())
      .subscribe({
        next: (formDetails: any) => {
          if(formDetails) {
            this.subheaderForm.reset();
            this.activeModal.close(formDetails);
          }
        },
        error: (response: HttpErrorResponse) =>
        {
          this.spinner.hide("criteriaSubheaderEditSpinner");
          this.errors = response.error?.errors;
          console.error(response);
        }
      });
    }
  }

  public isMultivalue () {
    const selectedFieldType = this.subheaderForm.value.fieldType;

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
