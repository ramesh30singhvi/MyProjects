export interface StudyDetails {
  id: number;
  priority: string;
  modality: string;
  typeOfStudy: string;
  patientMRNumber: string;
  patientFullName: string;
  dOB: Date;
  serviceDate: Date;
  diagnoses: string;
  userID: string;
  studyID: string;
  studyToken: string;
  meddreamStudyURL: string;
  assignedUser: string;
}
