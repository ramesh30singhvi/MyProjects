export interface Study {
  id: string;
  priority: string;
  modality: string;
  typeOfStudy: string;
  patientMRNumber: string;
  patientFullName: string;
  dob: Date;
  serviceDate: Date;
  diagnoses: string;
  facility: string;
  userID: string;
  studyID: string;
  meddreamStudyURL: string;
  findings: string;
  conclusions: string;
  isUrgent: boolean;
  status: string;
  statusId: number;
}
