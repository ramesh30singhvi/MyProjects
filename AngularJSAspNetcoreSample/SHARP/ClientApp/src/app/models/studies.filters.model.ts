export interface StudyFiltersModel {
  priority: FilterValues;
  modality: FilterValues;
  patientFullName: FilterValues;
  patientMRNumber: FilterValues;
  typeOfStudy: FilterValues;
  diagnoses: FilterValues;
  status: FilterValues;
  facility: FilterValues;
  assignedUser: FilterIdValues;
}




export interface FilterValues {
  values: Array<string>;
}

export interface FilterIdValues {
  values: Array<{id:number, text:string}>;
}
