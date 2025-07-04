import { FormGroup } from "@angular/forms";
import { Editor } from "ngx-editor";
import { FormFieldTypes, IControlOption, IFormField, IFormFieldItem, IHighAlert } from "../forms/forms.model";
import { Answer } from "./answers.model";
import { IOption } from "./audits.model";

export class Question {
  id: number;
  value: string;
  sequence: number;

  groupId?: number;
  parentId?: number;

  criteriaOption: ICriteriaOption;
  answer: Answer;

  subQuestions: Question[];
}

export class Section {
  id: number;
  name: string;
  formVersionId: number;
}

export interface ITrackerQuestion {
  id: number;
  question: string;
  sequence: number;

  sortOrder?: string;

  trackerOption: ITrackerOption;
  HighAlertAuditValue: IHighAlert;
}

export class QuestionGroup {
  id?: number;
  name?: string;
  sequence?: number;
  questions?: Question[];
}

export interface ICriteriaOption {
  showNA: boolean;
  compliance: boolean;
  quality: boolean;
  priority: boolean;
}

export interface ISection {
  id: number;
  name: string;
  formVersionId: number;
  groups: IGroup[];
}

export interface IGroup {
  id: number;
  name: string;
  formVersionId: number;
  formSectionId?: number;
  formFields: IFormField[];
}


export interface ITrackerOption {
  fieldType: IOption;

  isRequired: boolean;
  compliance: boolean;
  quality: boolean;
  priority: boolean;

  items: IFormFieldItem[];
}

export class IEditQuestion implements ICriteriaOption {
  id: number;
  question: string;

  formVersionId: number;

  group?: IOption;
  parentId?: number;

  sequence: number;

  showNA: boolean;
  compliance: boolean;
  quality: boolean;
  priority: boolean;
}

export class IEditTrackerQuestion implements ITrackerOption {
  id: number;
  question: string;

  formVersionId: number;

  formGroupId?: number;

  fieldType: IOption;

  sequence: number;

  isRequired: boolean;

  compliance: boolean;
  quality: boolean;
  priority: boolean;

  items: IFormFieldItem[];
}

export interface ISortModel {
  orderBy: string;
  sortOrder: string;
}

export interface IQuestionEditor {
  key: number | string;
  editor: Editor;
}

export interface IFieldBase {
  id: number;
  value: any|undefined;
  key: string;
  label: string;
  required: boolean;
  order: number;
  options: IControlOption[];
}
export class FieldBase<T> implements IFieldBase {
  id: number;
  value: T|undefined;
  key: string;
  label: string;
  required: boolean;
  showError: boolean;
  order: number;
  controlType: string;
  type: string;
  options: IControlOption[];
  editor: Editor;

  constructor(options: {
    id?:number;
    value?: T;
    key?: string;
    label?: string;
    required?: boolean;
    showError?: boolean;
    order?: number;
    controlType?: string;
    type?: string;
    options?: IControlOption[];
    editor?: Editor;
  } = {}) {
    this.id = options.id;
    this.value = options.value;
    this.key = options.key || '';
    this.label = options.label || '';
    this.required = !!options.required;
    this.showError = options.showError || true;
    this.order = options.order === undefined ? 1 : options.order;
    this.controlType = options.controlType || '';
    this.type = options.type || '';
    this.options = options.options || [];

    this.editor = options.editor;
  }
}

export class Textbox extends FieldBase<string> {
  controlType = FormFieldTypes.text.fieldType;
}

export class TextArea extends FieldBase<string> {
  controlType = FormFieldTypes.textArea.fieldType;
}

export class Checkbox extends FieldBase<boolean> {
  controlType = FormFieldTypes.checkbox.fieldType;
}

export class DatePicker extends FieldBase<any> {
  controlType = FormFieldTypes.datePicker.fieldType;
}

export class SingleDropdown extends FieldBase<IOption> {
  controlType = FormFieldTypes.dropdownSingleSelect.fieldType;
}

export class MultipleDropdown extends FieldBase<IOption[]> {
  controlType = FormFieldTypes.dropdownMultiselect.fieldType;
}

export class SingleToggle extends FieldBase<number> {
  controlType = FormFieldTypes.toggleSingleSelect.fieldType;
}

export class MultipleToggle extends FieldBase<FormGroup> {
  controlType = FormFieldTypes.toggleMultiselect.fieldType;
}
