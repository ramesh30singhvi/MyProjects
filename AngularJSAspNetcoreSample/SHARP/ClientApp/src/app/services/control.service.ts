import { Injectable } from "@angular/core";
import {
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
} from "@angular/forms";
import { NgbDateParserFormatter } from "@ng-bootstrap/ng-bootstrap";
import { Editor } from "ngx-editor";
import { NO, YES } from "../common/constants/audit-constants";
import {
  Checkbox,
  DatePicker,
  FieldBase,
  MultipleDropdown,
  SingleDropdown,
  Textbox,
  SingleToggle,
  MultipleToggle,
  IFieldBase,
  TextArea,
} from "../models/audits/questions.model";
import { FormFieldTypes, IControlOption, IFormFieldItem } from "../models/forms/forms.model";

@Injectable()
export class ControlService {
  constructor(
    public formatter: NgbDateParserFormatter,
    public formBuilder: FormBuilder
  ) {}

  public toFormGroup(fields: FieldBase<any>[]) {
    const group: any = {};

    fields.forEach((field) => {
      group[field.key] = field.required
        ? new FormControl(field.value, 
          field.controlType === FormFieldTypes.text.fieldType || field.controlType === FormFieldTypes.textArea.fieldType 
          ? [Validators.required, this.noWhitespaceValidator] 
          : Validators.required)
        : new FormControl(field.value);
    });
    return new FormGroup(group);
  }

  public noWhitespaceValidator(control: FormControl) {
    const isWhitespace = (control.value || '').replace(/<p>\s*<\/p>/g,"").trim().length === 0;
    const isValid = !isWhitespace;
    return isValid ? null : { 'whitespace': true };
  }

  public getControlFieldTypeById(id: number): string {
    return (
      Object.values(FormFieldTypes).find((type) => type.id === id)?.fieldType ??
      FormFieldTypes.text.fieldType
    );
  }

  public getControl(fieldTypeId: number, option: IFieldBase): FieldBase<any> {
    switch (fieldTypeId) {
      case FormFieldTypes.checkbox.id:
        return new Checkbox({
          ...option,
          value: this.parseToControlValue(
            FormFieldTypes.checkbox.fieldType,
            option
          ),
        });

      case FormFieldTypes.datePicker.id:
        return new DatePicker({
          ...option,
          value: this.parseToControlValue(
            FormFieldTypes.datePicker.fieldType,
            option
          ),
        });

      case FormFieldTypes.dropdownSingleSelect.id:
        return new SingleDropdown({
          ...option,
          value: this.parseToControlValue(
            FormFieldTypes.dropdownSingleSelect.fieldType,
            option
          ),
        });

      case FormFieldTypes.toggleSingleSelect.id:
        return new SingleToggle({
          ...option,
          value: this.parseToControlValue(
            FormFieldTypes.toggleSingleSelect.fieldType,
            option
          ),
        });

      case FormFieldTypes.dropdownMultiselect.id:
        return new MultipleDropdown({
          ...option,
          value: this.parseToControlValue(
            FormFieldTypes.dropdownMultiselect.fieldType,
            option
          ),
        });

      case FormFieldTypes.toggleMultiselect.id:
        return new MultipleToggle({
          ...option, 
          value: this.parseToControlValue(
            FormFieldTypes.toggleMultiselect.fieldType,
            option
          )
        });

      case FormFieldTypes.textArea.id:
        return new TextArea({
          ...option,
          value:this.parseToControlValue(
            FormFieldTypes.textArea.fieldType,
            option
          ),
          editor: new Editor()
        });

      default:
        return new Textbox({
          ...option,
          value: this.parseToControlValue(
            FormFieldTypes.text.fieldType,
            option
          ),
        });
    }
  }

  public parseToControlValue(
    controlType: string,
    option: IFieldBase
  ): any {
    switch (controlType) {
      case FormFieldTypes.checkbox.fieldType:
          return option.value ? JSON.parse(option.value) : false;

      case FormFieldTypes.datePicker.fieldType:
        return option.value ? this.formatter.parse(JSON.parse(option.value)) : null;

      case FormFieldTypes.toggleSingleSelect.fieldType:
        const toggleValue: IControlOption = option.value ? JSON.parse(option.value) : null;
        return toggleValue?.id;

      case FormFieldTypes.toggleMultiselect.fieldType:
        let toggleModel = {};
        const toggleValues: IControlOption[] = option.value ? JSON.parse(option.value) : [];

        option?.options?.forEach((item: IFormFieldItem) => {
          toggleModel[item.id] = toggleValues.map((value) => value.id).includes(item.id);
        });

        return this.formBuilder.group(toggleModel);

      case FormFieldTypes.text.fieldType:
        case FormFieldTypes.textArea.fieldType:
        return option.value;

      default:
        return option.value && this.isJson(option.value) ? JSON.parse(option.value) : option.value;
    }
  }

  public parseToStorageValue(
    question: FieldBase<any>,
    value: any): any {
      switch (question.controlType) {
        case FormFieldTypes.checkbox.fieldType:
          return value !== null ? JSON.stringify(value) : false;

        case FormFieldTypes.datePicker.fieldType:
          return value !== null ? JSON.stringify(this.formatter.format(value)) : null;

        case FormFieldTypes.toggleSingleSelect.fieldType:
          return value !== null
          ? JSON.stringify({
            id: Number.parseInt(value),
            value: question.options?.find((option) => option.id === Number.parseInt(value))?.value
          })
          : null;

        case FormFieldTypes.toggleMultiselect.fieldType:
          const toggleValue =
          value?.value !== null
            ? Object.entries(value.value)
                .filter((item) => item[1])
                .map((item) => {
                  const id = Number.parseInt(item[0]);
                  return {id, value: question.options?.find((option) => option.id === id)?.value}
                })
            : null;

          return toggleValue && toggleValue.length > 0 ? JSON.stringify(toggleValue) : null;

        case FormFieldTypes.dropdownMultiselect.fieldType:
          return value && value.length > 0 ? JSON.stringify(value) : null;

        case FormFieldTypes.dropdownSingleSelect.fieldType:
            return value ? JSON.stringify(value) : null;

        default:
          return value;
      }
  }

  /*public formatToString(
    controlType: string,
    value: string
  ): any {
    switch (controlType) {
      case FormFieldTypes.checkbox.fieldType:
        const checkboxValue = value ? this.isJson(value) ? JSON.parse(value) : value : false;
        return checkboxValue ? YES.toUpperCase() : NO.toUpperCase();

      case FormFieldTypes.dropdownSingleSelect.fieldType:
      case FormFieldTypes.toggleSingleSelect.fieldType:
        const item: IControlOption = value ? this.isJson(value) ? JSON.parse(value) : value : null
        return item?.value;

      case FormFieldTypes.dropdownMultiselect.fieldType:
      case FormFieldTypes.toggleMultiselect.fieldType:
        const items: IControlOption[] = value ? this.isJson(value) ? JSON.parse(value) : value : []
        return items.map((item: IControlOption) => item.value).join(", ");

      default:
        return value && this.isJson(value) ? JSON.parse(value) : value;
    }
  }*/

  public enableForm(form: FormGroup): void{
    if(!form) {
      return;
    }

    form?.enable();

    if(!form.controls) {
      return;
    }

    Object.values(form.controls).forEach((control: FormControl) => {
      if (control?.value instanceof FormGroup) {
        control?.value.enable();
      } 
    });
  }

  public disableForm(form: FormGroup): void{
    if(!form) {
      return;
    }

    form.disable();

    if(!form.controls) {
      return;
    }

    Object.values(form.controls).forEach((control: FormControl) => {
      if (control.value instanceof FormGroup) {
        control.value.disable();
      } 
    });
  }

  public clearForm(form: FormGroup): void {

    Object.values(form.controls).forEach((control: FormControl) => {
      if (control.value instanceof FormGroup) {
        control.value.reset();
      } else if (typeof control.value === "boolean") {
        control.setValue(false);
      } else {
        control.reset();
      }
    });
  }

  public isJson(str): boolean {
    try {
        JSON.parse(str);
    } catch (e) {
        return false;
    }
    return true;
}
}
