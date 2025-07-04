import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, ViewEncapsulation } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Editor, nodes as basicNodes, marks, toHTML, Toolbar } from 'ngx-editor';
import { Schema } from 'prosemirror-model';
import { Subscription } from 'rxjs';
import { COLOR_PRESETS, NA, NO, YES } from 'src/app/common/constants/audit-constants';
import { Answer } from 'src/app/models/audits/answers.model';
import { FieldBase, IQuestionEditor, Question, QuestionGroup } from 'src/app/models/audits/questions.model';
import { ICriteriaFormDetails, IFormField } from 'src/app/models/forms/forms.model';
import { ControlService } from 'src/app/services/control.service';
import { Audit } from '../../models/audits/audits.model';
import { AuditServiceApi } from '../../services/audit-api.service';
import { AuditService } from '../services/audit.service';

@Component({
  selector: "app-criteria",
  templateUrl: "./criteria.component.html",
  styleUrls: ["./criteria.component.scss"],
  encapsulation: ViewEncapsulation.None
})
export class CriteriaComponent implements OnDestroy {
  public _subHeaders: FieldBase<string>[];
  public isEditable: boolean;

  private _subHeaderForm: FormGroup;

  private _audit: Audit;

  private _criteriaFormDetails: ICriteriaFormDetails;

  @Input() public set subHeaders(value: FieldBase<string>[]) {
    this._subHeaders = value;
  }

  @Input() public set subHeaderForm(value: FormGroup) {
    this._subHeaderForm = value;

    this.subscription.add(this._subHeaderForm.valueChanges.subscribe(value  => {
      this.audit.isReadyForNextStatus = false;
    }));

    Object.values(this._subHeaderForm.controls).forEach((control: FormControl) => {
      if (control.value instanceof FormGroup) {
        this.subscription.add(control.value.valueChanges.subscribe(value  => {
          this.audit.isReadyForNextStatus = false;
        }));
      } 
    });
  }

  @Input() public set audit(value: Audit) {
    this._audit = value;
  }

  @Input() public set criteriaFormDetails(value: ICriteriaFormDetails) {
    this._criteriaFormDetails = value;
  }

  @Output() onCriteriaAuditChanged = new EventEmitter<boolean>();

  @Output() onCriteriaAnswerCommentFocusout = new EventEmitter<Question>();

  public get subHeaders(): FieldBase<string>[] {
    return this._subHeaders;
  }

  public get subHeaderForm(): FormGroup {
    return this._subHeaderForm;
  }

  public get audit(): Audit {
    return this._audit;
  }

  public get questionGroups(): QuestionGroup[] {
    return this._criteriaFormDetails?.questionGroups;
  }

  public get totalQuestionCount(): number {
    if (!this.audit) {
      return 0;
    }

    if (!this.questionGroups) {
      return 0;
    }

    return this.questionGroups.reduce(
      (length, group: QuestionGroup) =>
        length +
        group.questions?.length +
        group.questions?.reduce(
          (length, question: Question) =>
            length +
            question.subQuestions?.filter(
              (subQuestion: Question) => question.answer?.value === YES
            )?.length,
          0
        ),
      0
    );
  }

  public get complianceQuestionCount(): number {
    if (!this.audit) {
      return 0;
    }

    if (!this.questionGroups) {
      return 0;
    }

    return this.questionGroups.reduce(
      (length, group: QuestionGroup) =>
        length +
        group.questions?.filter(
          (question: Question) => question.criteriaOption?.compliance
        )?.length +
        group.questions?.reduce(
          (length, question: Question) =>
            length +
            question.subQuestions?.filter(
              (subQuestion: Question) =>
                subQuestion.criteriaOption?.compliance &&
                question.answer?.value === YES
            )?.length,
          0
        ),
      0
    );
  }

  private subscription: Subscription;

  public questionEditors: IQuestionEditor[] = [];

  public toolbar: Toolbar = [
    ['bold', 'italic', 'underline'],
    ['ordered_list', 'bullet_list'],
    ['text_color', 'background_color'],
    //['link', 'image'],
  ];

  public colorPresets = COLOR_PRESETS;

  constructor(
    private auditServiceApi: AuditServiceApi,
    private auditService: AuditService,
    private controlService: ControlService,
  ) {
    this.subscription = new Subscription();

    this.subscription.add(
      this.auditService.isEditable$.subscribe((isEditable) => {
        this.isEditable = isEditable;

        this.questionEditors.forEach((questionEditor: IQuestionEditor) => questionEditor?.editor?.destroy());
        this.questionEditors = [];
      })
    );
  }

  ngOnInit() {
    
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    this.questionEditors.forEach((questionEditor: IQuestionEditor) => questionEditor.editor.destroy());
  }

  public getQuestionEditor(questionId: number): Editor {
    const questionEditor = this.questionEditors.find((questionEditor: IQuestionEditor) => questionEditor.key === questionId);
    if(questionEditor) {
      return questionEditor.editor;
    }

    const editor = new Editor({
      schema: new Schema({
        marks,
        nodes: {...basicNodes, paragraph: {...basicNodes.paragraph, whitespace: 'pre'/*, parseDOM: [{...basicNodes.paragraph.parseDOM[0], preserveWhitespace: 'full'}]*/}}
      })
    });

    this.questionEditors.push({key: questionId, editor: editor});

    return editor;
  }

  public onClear(question: Question): void {
    const editor = this.questionEditors.find((questionEditor: IQuestionEditor) => questionEditor.key === question.id);

    if(editor) {
      editor.editor.setContent('');
    }

    question.answer.auditorComment = null;
    question.answer.value = null;

    question.subQuestions = this.clearSubQuestionAnswers(question.subQuestions);

    this.calculateTotals();
    this.onCriteriaAuditChanged.emit(true);
  }

  public onAnswerValueChanged(question: Question): void {
    if (question.answer.value !== YES) {
      question.subQuestions = this.clearSubQuestionAnswers(
        question.subQuestions
      );
    }

    this.calculateTotals();
    this.onCriteriaAuditChanged.emit(true);
  }

  public onAnswerCommentChanged(comment: string, answer: Answer): void {
    if (answer.auditorComment !== comment) {
      this.onCriteriaAuditChanged.emit(false);
    }

    answer.auditorComment = comment;
  }

  public getAnswerClass(
    question: Question,
    value: string,
    isDefault: boolean
  ): string {
    if (!question.answer.value && isDefault) {
      question.answer.value = value;
    }

    if (question.answer.value !== value) {
      return "";
    }

    switch (question.answer.value) {
      case NO:
        return question.criteriaOption?.compliance ? "no-label" : "na-label";
      case YES:
        return question.criteriaOption?.compliance ? "yes-label" : "na-label";
      case NA:
        return question.criteriaOption?.compliance ? "na-label" : "na-label";
      default:
        return "";
    }
  }

  private clearSubQuestionAnswers(subQuestions: Question[]): Question[] {
    if (subQuestions) {
      subQuestions = subQuestions.map((subQuestion: Question) => {
        const editor = this.questionEditors.find((questionEditor: IQuestionEditor) => questionEditor.key === subQuestion.id);

        if(editor) {
          editor.editor.setContent('');
        }

        return {
          ...subQuestion,
          answer: {
            ...subQuestion.answer,
            id: null,
            value: "",
            auditorComment: "",
          },
        };
      });
    }

    return subQuestions;
  }

  private calculateTotals(): void {
    this.audit.totalYES = this.getAnswerCount(YES);
    this.audit.totalNO = this.getAnswerCount(NO);
    this.audit.totalNA = this.getAnswerCount(NA);

    const totalCompliance = this.complianceQuestionCount > 0 ?
      ((this.audit.totalYES + this.audit.totalNA) / this.complianceQuestionCount) * 100 : 0;

    this.audit.totalCompliance = Math.round(totalCompliance);
  }

  private getAnswerCount(answerValue: string): number {
    return this.questionGroups.reduce(
      (total, group) =>
        (total +=
          group.questions.filter(
            (question) =>
              question.answer.value === answerValue &&
              question.criteriaOption?.compliance
          ).length +
          group.questions.reduce(
            (total, question) =>
              (total += question.subQuestions.filter(
                (subQuestion) =>
                  subQuestion.answer.value === answerValue &&
                  subQuestion.criteriaOption?.compliance
              ).length),
            0
          )),
      0
    );
  }

  public onAnswerCommentFocusout(event: any, question: Question): void {
    if (question) {
      this.onCriteriaAnswerCommentFocusout.emit(question);
    }
  }
}
