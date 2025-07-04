import { HttpErrorResponse } from "@angular/common/http";
import { Component, Input, OnDestroy, OnInit, ViewEncapsulation } from "@angular/core";
import { FormBuilder} from "@angular/forms";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { DragulaService } from "ng2-dragula";
import { first, Subscription } from "rxjs";
import { Question, QuestionGroup } from "src/app/models/audits/questions.model";
import { FormStatuses, IFormField, IFormVersion } from "src/app/models/forms/forms.model";
import { FormServiceApi } from "src/app/services/form-api.service";
import { ConfirmationDialogComponent } from "src/app/shared/confirmation-dialog/confirmation-dialog.component";
import { EditCrieriaFormModalComponent } from "../edit-criteria-form-modal/edit-crieria-form-modal.component";
import { EditSubheaderModalComponent } from "../edit-subheader-modal/edit-subheader-modal.component";
import { FormService } from "../services/form.service";
import { SetSectionModalComponent } from "../set-section-modal/set-section-modal.component";

@Component({
  selector: "app-criteria-form",
  templateUrl: "./criteria-form.component.html",
  styleUrls: ["./criteria-form.component.scss"],
  encapsulation: ViewEncapsulation.None,
})

export class CriteriaFormComponent implements OnInit, OnDestroy {
  public QUESTIONS = 'CRITERIA_QUESTIONS';
  public FORM_FIELDS = 'FORM_FIELDS';

  public searchTerm: string;

  public errors: any[];

  public formStatuses = FormStatuses;

  private _formVersion: IFormVersion;
  private _questionGroups: QuestionGroup[];
  private _initQuestionGroups: QuestionGroup[];
  private _formFields: IFormField[];

  private subscriptions = new Subscription();

  @Input() public set formVersion(value: IFormVersion) {
    this._formVersion = value;
  }

  @Input() public set questionGroups(value: QuestionGroup[]) {
    this._questionGroups = value;
    this._initQuestionGroups = value ? [...value] : null;
  }

  @Input() public set formFields(value: IFormField[]) {
    this._formFields = value;
  }

  constructor(
    private formServiceApi: FormServiceApi,
    private formService: FormService,
    private formBuilder: FormBuilder,
    private dragulaService: DragulaService,
    private modalService: NgbModal,
  ) { 
    this.dragulaService.createGroup(this.QUESTIONS, {
      direction: 'vertical',
      removeOnSpill: false,
      moves: function (el, container, handle) {
        return handle.classList.contains('question-handle');
    }});

    this.dragulaService.createGroup(this.FORM_FIELDS, {
      direction: 'vertical',
      removeOnSpill: false,
      moves: function (el, container, handle) {
        return handle.classList.contains('field-handle');
    }});

    this.handleRearrangeQuestions();

    this.handleRearrangeFormFields();
  }

  ngOnInit() {

  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
    this.dragulaService.destroy(this.QUESTIONS);
    this.dragulaService.destroy(this.FORM_FIELDS);
  }

  public get formVersion(): IFormVersion {
    return this._formVersion;
  }

  public get formFields(): IFormField[] {
    return this._formFields;
  }

  public get questionGroupList(): QuestionGroup[] {
    if(this._questionGroups && !this._questionGroups.find((group: QuestionGroup) => !group?.id && !group?.name)) {
        this._questionGroups.unshift({id: null, name: null, sequence: 0, questions: []});
        this.questionGroups = this._questionGroups;
    }

    return this._questionGroups;
  }

  public get isFormDisabled(): boolean {
    return this.formVersion?.status !== this.formStatuses.Draft.id;
  }

  public getQuestionList(groupId: number): Question[] {
    if(!this._questionGroups) {
      return;
    }

    const group = this._questionGroups.find((group: QuestionGroup) => group?.id === groupId);

    if(this.searchTerm) {
      return group?.questions.filter((question: Question) => question?.value.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
      question.subQuestions.some((subQuestion: Question) => subQuestion.value.toLowerCase().includes(this.searchTerm.toLowerCase())
      ));
    }

    return group?.questions;
  }

  public getSubQuestionList(groupId: number, questionId: number): Question[] {
    if(!this._questionGroups) {
      return;
    }

    const group = this._questionGroups.find((group: QuestionGroup) => group?.id === groupId);

    const question = group.questions.find((question: Question) => question?.id === questionId);

    if(this.searchTerm) {
      return question?.subQuestions.filter((subQuestion: Question) => subQuestion?.value.toLowerCase().includes(this.searchTerm.toLowerCase()));
    }

    return question?.subQuestions;
  }

  public onSearchClear(): void {
    this.searchTerm = null;
  }

  public onDeleteQuestionClick(question: Question): void {
    const modalRef = this.modalService.open(ConfirmationDialogComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.confirmationBoxTitle = 'Confirmation?';
    modalRef.componentInstance.confirmationMessage = 'Do you want to delete the question?';
    
    modalRef.result.then((userResponse) => {
      if(userResponse) {
        this.formServiceApi.deleteColumn(question.id)
        .pipe(first())
        .subscribe((result: boolean) => {
          if(result) {
            this.questionGroups = this._questionGroups.map((questionGroup: QuestionGroup) => {
              questionGroup.questions = questionGroup.questions.filter((q: Question) => q.id !== question.id)
              return questionGroup;
            });
          }
        });
      }
    })
    .catch((res) => {});
  }

  public onDeleteSubQuestionClick(subQuestion: Question): void {
    const modalRef = this.modalService.open(ConfirmationDialogComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.confirmationBoxTitle = 'Confirmation?';
    modalRef.componentInstance.confirmationMessage = 'Do you want to delete the sub question?';
    
    modalRef.result.then((userResponse) => {
      if(userResponse) {
        this.formServiceApi.deleteColumn(subQuestion.id)
        .pipe(first())
        .subscribe((formDetails: any) => {      
          this.questionGroups = formDetails.questionGroups;
          this.formFields = formDetails.formFields;
        });
      }
    })
    .catch((res) => {});
  }

  public onSetSectionClick() {
    const modalRef = this.modalService.open(SetSectionModalComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.formVersionId = this._formVersion.id;
    modalRef.componentInstance.sections = this._questionGroups?.filter((section: QuestionGroup) => section.name);

    modalRef.result
    .then((formDetails: any) => {      
      this.questionGroups = formDetails.questionGroups;
      this.formFields = formDetails.formFields;
    })
    .catch((res) => {});
  }

  public onAddCriteriaClick() {
    const modalRef = this.modalService.open(EditCrieriaFormModalComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.formVersionId = this._formVersion.id;
    modalRef.componentInstance.title = 'Add Criteria';
    modalRef.componentInstance.actionButtonLabel = 'Create';
    
    modalRef.result
    .then((formDetails: any) => {      
      this.questionGroups = formDetails.questionGroups;
      this.formFields = formDetails.formFields;
    })
    .catch((res) => {});
  }

  public onAddChildQuestionClick(question: Question) {
    const modalRef = this.modalService.open(EditCrieriaFormModalComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.formVersionId = this._formVersion.id;
    modalRef.componentInstance.title = 'Add Sub Question';
    modalRef.componentInstance.actionButtonLabel = 'Create';
    modalRef.componentInstance.parentId = question.id;
    
    modalRef.result
    .then((formDetails: any) => {      
      this.questionGroups = formDetails.questionGroups;
      this.formFields = formDetails.formFields;
    })
    .catch((res) => {});
  }

  public onEditQuestionClick(group: QuestionGroup, question: Question): void {
    const modalRef = this.modalService.open(EditCrieriaFormModalComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.formVersionId = this._formVersion.id;
    modalRef.componentInstance.title = 'Edit Criteria';
    modalRef.componentInstance.actionButtonLabel = 'Update';
    modalRef.componentInstance.editQuestion = {
      ...question, 
      ...question.criteriaOption,
      question: question.value,
      group: group?.id ? group : null};
    
    modalRef.result
    .then((formDetails: any) => {      
      this.questionGroups = formDetails.questionGroups;
      this.formFields = formDetails.formFields;
    })
    .catch((res) => {});
  }

  public onEditSubQuestionClick(question: Question): void {
    const modalRef = this.modalService.open(EditCrieriaFormModalComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.formVersionId = this._formVersion.id;
    modalRef.componentInstance.title = 'Edit Sub Question';
    modalRef.componentInstance.actionButtonLabel = 'Update';
    modalRef.componentInstance.parentId = question.parentId;
    modalRef.componentInstance.editQuestion = {
      ...question, 
      ...question.criteriaOption,
      question: question.value};
    
    modalRef.result
    .then((formDetails: any) => {      
      this.questionGroups = formDetails.questionGroups;
      this.formFields = formDetails.formFields;
    })
    .catch((res) => {});
  }

  public onAddSubheaderClick(): void {
    const modalRef = this.modalService.open(EditSubheaderModalComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.formVersionId = this._formVersion.id;
    modalRef.componentInstance.title = 'Add Subheader Field';
    modalRef.componentInstance.actionButtonLabel = 'Create';
    
    modalRef.result
    .then((formDetails: any) => {      
      this.questionGroups = formDetails.questionGroups;
      this.formFields = formDetails.formFields;
    })
    .catch((res) => {});
  }

  public onEditSubheaderClick(formField: IFormField): void {
    const modalRef = this.modalService.open(EditSubheaderModalComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.formVersionId = this._formVersion.id;
    modalRef.componentInstance.title = 'Edit Subheader Field';
    modalRef.componentInstance.actionButtonLabel = 'Update';
    modalRef.componentInstance.editField = {...formField};
    
    modalRef.result
    .then((formDetails: any) => {      
      this.questionGroups = formDetails.questionGroups;
      this.formFields = formDetails.formFields;
    })
    .catch((res) => {});
  }

  public onDeleteSubheaderClick(formField: IFormField): void{
    const modalRef = this.modalService.open(ConfirmationDialogComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.confirmationBoxTitle = 'Confirmation?';
    modalRef.componentInstance.confirmationMessage = 'Do you want to delete the form field?';
    
    modalRef.result.then((userResponse) => {
      if(userResponse) {
        this.formServiceApi.deleteFormField(this._formVersion.id, formField.id)
        .pipe(first())
        .subscribe((formDetails: any) => {      
          this.questionGroups = formDetails.questionGroups;
          this.formFields = formDetails.formFields;
        });
      }
    });
  }

  private handleResponseError(response: HttpErrorResponse){
    this.errors = response.error.errors;
    console.error(response);
  }

  private handleRearrangeQuestions() {
    this.subscriptions.add(this.dragulaService.dropModel()
      .subscribe(({ el, target, source, sourceModel, targetModel, item }) => {
        if(!el.classList.contains('question-row') && !el.classList.contains('questions-wrapper')) {
          return;
        }

        const targetGroupId = !target.id || target.id === "null" ? null : Number.parseInt(target.id);
        const sourceGroupId = !source.id || source.id === "null" ? null : Number.parseInt(source.id);

        let questions = targetModel.map((question: Question, index: number) => {
          if(question?.id === item?.id) {
            return { id: question?.id, groupId: targetGroupId, sequence: index + 1 };
          }

          return { id: question?.id, groupId: question?.groupId, sequence: index + 1 };
        });

        if(targetGroupId !== sourceGroupId) {
          const sourceQuestions = sourceModel.map((question: Question, index: number) => {
            return { id: question?.id, groupId: question?.groupId, sequence: index + 1 };
          });

          questions = [...questions, ...sourceQuestions];
        }

        this.rearrangeQuestions(questions);
    }));
  }

  private rearrangeQuestions(questions: any[]) {
    this.formServiceApi.rearrangeQuestions(questions)
    .pipe(first())
    .subscribe({
      next : (result: boolean) => {
        if(result) {
          this.questionGroups = this._questionGroups.map((questionGroup: QuestionGroup) => {
              questionGroup.questions = questionGroup.questions.map((question: Question) => {
                const targetQuestion = questions.find((targetQuestion) => targetQuestion?.id === question?.id);

                if(targetQuestion) {
                  return {...question, groupId: targetQuestion.groupId, sequence: targetQuestion.sequence};
                }

                return question;
              });

            return questionGroup;
          });
        }
      },
      error: (response: HttpErrorResponse) => this.questionGroups = this._initQuestionGroups
    });
  }

  private handleRearrangeFormFields() {
    this.subscriptions.add(this.dragulaService.dropModel(this.FORM_FIELDS)
      .subscribe(({ el, target, source, sourceModel, targetModel, item }) => {

        let formFields = targetModel.map((formField: IFormField, index: number) => {
          return { id: formField.id, sequence: index + 1 };
        });

        this.formServiceApi.rearrangeFormFields(this._formVersion.id, formFields)
        .pipe(first())
        .subscribe({
          next : (result: boolean) => {
            if(result) {
              this.formFields = this._formFields.map((formField: IFormField) => { 
                const targetField = formFields.find((field) => field.id === formField.id);

                if(targetField) {
                  return {...formField, sequence: targetField.sequence};
                }

                return formField;
              });
            }
          },
          error: (response: HttpErrorResponse) => this.formFields = this._formFields.sort((current, next) => { return current.sequence - next.sequence})
        });
      }));
  }
}