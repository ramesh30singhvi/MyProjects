import { WeekDay } from "@angular/common";
import { IFilterOption } from "../audits/audit.filters.model";
import { IFormOption, IKeywordTrigger, IOption, IUserOption } from "../audits/audits.model";
import { ISection, ITrackerQuestion, QuestionGroup } from "../audits/questions.model";

export interface FormGridItem {
  id: number;
  name: string;
  organizationName: string;
  auditType: string;
}
export class IHighAlert {
  highAlertCategory: IOption;
  highAlertDescription: string;
  highAlertNotes: string;
  id: number;
}
export interface OrganizationFormGridItem {
  formOrganizationId: number;
  formId: number;
  name: string;
  auditType: string;
  settingType: SettingType;
  isFormActive: boolean;

  scheduleSetting: IScheduleSetting;
}

export enum SettingType {
  Triggered = 1,
  Scheduled
}


export interface IFormSetting {
  id: number;
  settingType: SettingType;

  scheduleSetting: IScheduleSetting;
}


export interface IScheduleSetting {
  id: number;
  scheduleType: ScheduleType;
  days: number[];
}

export enum ScheduleType {
  Weekly = 1,
  Monthly
}

export interface IFormVersion {
  id: number;
  version: number;
  form: IFormOption;
  organization: IOption;
  status: number;
  createdByUserId: IUserOption;
  createdDate: Date;
}

export class AddEditForm {
  id: number;
  name: string;
  auditType: IOption;
  organization: IOption;
  allowEmptyComment: number = 0;
  disableCompliance: number = 0;
  useHighAlert: boolean = false;
  AHTime: number = 0;
}

export interface IFormStatus {
  id: number,
  label: string,
  bgColor: string,
  txtColor: string
}

export const FormStatuses = {
  Draft: {id: 1, label: 'Draft', bgColor: '#DDE0E4', txtColor: '#54667A'},
  Published: {id: 2, label: 'Published', bgColor: '#145196c2', txtColor: '#fff'},
  Archived: { id: 3, label: 'Archived', bgColor: '#f62d5124', txtColor: '#f62d51' },
  Deleted: { id: 4, label: 'Deleted', bgColor: '#f62d5124', txtColor: '#f62d51' },
}

export interface IFormDetails {
  form: IFormVersion,
}
export interface IKeywordFormDetails extends IFormDetails {
  keywords: IKeywordTrigger,
}

export interface ICriteriaFormDetails extends IFormDetails {
  questionGroups: QuestionGroup[],
  formFields: IFormField[],
}

export interface ITrackerFormDetails {
  questions: ITrackerQuestion[],
}

export interface IMdsFormDetails extends IFormDetails {
  sections: ISection[],
  formFields: IFormField[],
}

export interface IFormField {
  id: number,
  formGroupId?: number;
  formVersionId?: number;
  fieldType: IOption,
  sequence: number,
  fieldName: string,
  labelName: string,
  isRequired: boolean,
  items: IFormFieldItem[],

  value: IControlOption;
}

export interface IFormFieldItem {
  id?: number,
  value?: string,
  code?: string,
  sequence?: number,
}

export interface IFormFieldValue {
  id: number;
  formFieldId: number;
  value: string;
}

/*export enum FieldTypes  {
  checkbox = 1,
  datePicker,
  dropdownSingleSelect,
  text,
  toggle,
  dropdownMultiselect,
}*/

export const FormFieldTypes = {
  checkbox: { id: 1, fieldType: "checkbox" },
  datePicker: { id: 2, fieldType: "datePicker" },
  dropdownSingleSelect: { id: 3, fieldType: "singleDropdown"},
  text: { id: 4, fieldType: "textbox" },
  toggleSingleSelect: { id: 5, fieldType: "singleToggle" },
  dropdownMultiselect: { id: 6, fieldType: "multipleDropdown" },
  toggleMultiselect : { id: 7, fieldType: "multipleToggle"},
  textArea: { id: 8, fieldType: "textarea" },
};

export interface IControlOption {
  id: number;
  value: string;
}

export const weekDays = [
  {id: WeekDay.Sunday, name: 'Sunday', smallName: 'Sun'},
  {id: WeekDay.Monday, name: 'Monday', smallName: 'Mon'},
  {id: WeekDay.Tuesday, name: 'Tuesday', smallName: 'Tues'},
  {id: WeekDay.Wednesday, name: 'Wednesday', smallName: 'Wed'},
  {id: WeekDay.Thursday, name: 'Thursday', smallName: 'Thurs'},
  {id: WeekDay.Friday, name: 'Friday', smallName: 'Fri'},
  {id: WeekDay.Saturday, name: 'Saturday', smallName: 'Sat'},
];

export const FormStates = {
  active: {id: 1, label: 'Active', color: '#0b860b'},
  inactive: {id: 0, label: 'Inactive', color: '#ff3737'},
};

export const SettingTypes = [
  {id: SettingType.Triggered, label: 'On trigger'},
  {id: SettingType.Scheduled, label: 'Always required'},
];

export const ScheduleTypes = [
  {id: 0, label: 'Empty'},
  {id: ScheduleType.Weekly, label: 'Weekly'},
  {id: ScheduleType.Monthly, label: 'Monthly'},
];

export interface FormManagementFiltersModel {
  organizationName?: Array<IFilterOption>;
  formName?: Array<IFilterOption>;
  auditType?: Array<IFilterOption>;
};

export interface FormFiltersModel {
  name?: Array<IFilterOption>;
  auditType?: Array<IFilterOption>;
  settingType?: Array<IFilterOption>;
  scheduleSetting?: Array<IFilterOption>;
  isFormActive?: Array<IFilterOption>;
}
