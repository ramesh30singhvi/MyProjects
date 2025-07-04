export class AnswerGroup {
  name: string;
  answers: Answer[];
}

export class Answer {
  id?: number;

  tableColumnId: number;

  value: string;

  auditorComment: string = '';
}