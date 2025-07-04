import { HttpErrorResponse } from "@angular/common/http";
import { Component, Input, OnInit } from "@angular/core";
import { FormBuilder } from "@angular/forms";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { DragulaService } from "ng2-dragula";
import { first, Subscription } from "rxjs";
import { ITrackerQuestion } from "src/app/models/audits/questions.model";
import { FormStatuses, IFormVersion } from "src/app/models/forms/forms.model";
import { FormServiceApi } from "src/app/services/form-api.service";
import { ConfirmationDialogComponent } from "src/app/shared/confirmation-dialog/confirmation-dialog.component";
import { EditTrackerQuestionModalComponent } from "../edit-tracker-question-modal/edit-tracker-question-modal.component";
import { FormService } from "../services/form.service";

@Component({
  selector: "app-tracker-form",
  templateUrl: "./tracker-form.component.html",
  styleUrls: ["./tracker-form.component.scss"]
})

export class TrackerFormComponent implements OnInit {
  public QUESTIONS = 'TRACKER_QUESTIONS';

  public errors: any[];

  public formStatuses = FormStatuses;

  private _formVersion: IFormVersion;

  private _questions: ITrackerQuestion[];
  private _initQuestions: ITrackerQuestion[];

  private subscriptions = new Subscription();

  @Input() public set formVersion(value: IFormVersion) {
    this._formVersion = value;
  }

  @Input() public set questions(value: ITrackerQuestion[]) {
    this._questions = value;
    this._initQuestions = value ? [...value] : null;
  }
  
  constructor(
    private formServiceApi: FormServiceApi,
    private formService: FormService,
    private formBuilder: FormBuilder,
    private dragulaService: DragulaService,
    private modalService: NgbModal) { 
      this.dragulaService.createGroup(this.QUESTIONS, {
        direction: 'vertical',
        removeOnSpill: false,
        moves: function (el, container, handle) {
          return handle.classList.contains('question-handle');
      }});
      
      this.handleRearrangeQuestions();
  }

  ngOnInit() {

  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
    this.dragulaService.destroy(this.QUESTIONS);
  }

  public get formVersion(): IFormVersion {
    return this._formVersion;
  }

  public get questions(): ITrackerQuestion[] {
    return this._questions;
  }

  public get isFormDisabled(): boolean {
    return this.formVersion?.status !== this.formStatuses.Draft.id;
  }

  public onAddQuestionClick(): void {
    const modalRef = this.modalService.open(EditTrackerQuestionModalComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.formVersionId = this._formVersion.id;
    modalRef.componentInstance.title = 'Add Question';
    modalRef.componentInstance.actionButtonLabel = 'Create';
    
    modalRef.result
    .then((formDetails: any) => {      
      this.questions = formDetails.questions;
    })
    .catch((res) => {});
  }

  public onEditQuestionClick(question: ITrackerQuestion): void {
    const modalRef = this.modalService.open(EditTrackerQuestionModalComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.formVersionId = this._formVersion.id;
    modalRef.componentInstance.title = 'Edit Question';
    modalRef.componentInstance.actionButtonLabel = 'Update';
    modalRef.componentInstance.editQuestion = {
      ...question, 
      ...question.trackerOption,
    };
    
    modalRef.result
    .then((formDetails: any) => {      
      this.questions = formDetails.questions;
    })
    .catch((res) => {});
  }

  public onDeleteQuestionClick(question: ITrackerQuestion): void {
    const modalRef = this.modalService.open(ConfirmationDialogComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.confirmationBoxTitle = 'Confirmation?';
    modalRef.componentInstance.confirmationMessage = 'Do you want to delete the question?';
    
    modalRef.result.then((userResponse) => {
      if(userResponse) {
        this.formServiceApi.deleteColumn(question.id)
        .pipe(first())
        .subscribe((result: boolean) => {
          if(result) {
            this.questions = this._questions.filter((q: ITrackerQuestion) =>  q.id !== question.id);
          }
        });
      }
    });
  }

  private handleRearrangeQuestions() {
    this.subscriptions.add(this.dragulaService.dropModel(this.QUESTIONS)
      .subscribe(({ el, target, source, sourceModel, targetModel, item }) => {
        let questions = targetModel.map((question: ITrackerQuestion, index: number) => {
          return { id: question?.id, sequence: index + 1 };
        });

        this.rearrangeQuestions(questions);
    }));
  }

  private rearrangeQuestions(questions: any[]) {
    this.formServiceApi.rearrangeQuestions(questions)
    .pipe(first())
    .subscribe({
      next : (result: boolean) => {
        if(result) {
          this.questions = this._questions.map((question: ITrackerQuestion) => {
              const targetQuestion = questions.find((targetQuestion) => targetQuestion?.id === question?.id);

              if(targetQuestion) {
                return {...question, sequence: targetQuestion.sequence};
              }

              return question;
          });
        }
      },
      error: (response: HttpErrorResponse) => this.questions = this._initQuestions
    });
  }
}
