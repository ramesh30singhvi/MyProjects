
export enum QuestionType {
  singleSelect,
  text,
  checkbox
}

export class Section {
  name: string;
  description?: string;
  notes?: string;
  groups: Group[];
}

export class Group {
  code?: string;
  name: string;
  questions: Question[];
}

export class Question {
  code?: string;
  name: string;
  type: string;
  required: boolean;
  variants?: Variant[];
}

export class Variant {
  code: string;
  name: string;
}
