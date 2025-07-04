import { Study } from "./studies.model";

export interface StudyGridResponse {
  rows: Array<Study>;
  totalCount: number;
}
