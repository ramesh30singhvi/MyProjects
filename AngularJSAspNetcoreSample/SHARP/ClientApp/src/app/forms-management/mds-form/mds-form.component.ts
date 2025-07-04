import { Component, Input, OnInit } from "@angular/core";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { IEditTrackerQuestion, IGroup, ISection, ITrackerQuestion } from "src/app/models/audits/questions.model";
import { FormStatuses, IFormVersion } from "src/app/models/forms/forms.model";
import { EditMdsSectionModalComponent } from "../edit-mds-section-modal/edit-mds-section-modal.component";
import { EditMdsGroupModalComponent } from "../edit-mds-group-modal/edit-mds-group-modal.component";
import { EditMdsQuestionModalComponent } from "../edit-mds-question-modal/edit-mds-question-modal.component";

@Component({
  selector: "app-mds-form",
  templateUrl: "./mds-form.component.html",
  styleUrls: ["./mds-form.component.scss"]
})

export class MdsFormComponent implements OnInit {

  public formStatuses = FormStatuses;
  public SECTIONS = 'SECTIONS';

  private _formVersion: IFormVersion;
  private _sections: ISection[];
  private _initSections: ISection[];


  @Input() public set formVersion(value: IFormVersion) {
    this._formVersion = value;
  }

  @Input() public set sections(value: ISection[]) {
    this._sections = value;
    this._initSections = value ? [...value] : null;
  }

  constructor(private modalService: NgbModal) {}

  ngOnInit(): void {

  }

  public get formVersion(): IFormVersion {
    return this._formVersion;
  }

  public get sections(): ISection[] {
    return this._sections;
  }

  public get isFormDisabled(): boolean {
    return this.formVersion?.status !== this.formStatuses.Draft.id;
  }

  public onAddSectionClick(): void {
    const modalRef = this.modalService.open(EditMdsSectionModalComponent, {modalDialogClass: 'custom-modal'});
    modalRef.componentInstance.formVersionId = this._formVersion.id;
    modalRef.componentInstance.title = 'Add Section';
    modalRef.componentInstance.actionButtonLabel = 'Create';

    modalRef.result.then((formDetails: any) => {
      this.sections = formDetails.sections;
    }).catch((res) => {});
  }

  public onAddGroupClick(formSectionId: number): void {
    const modalRef = this.modalService.open(EditMdsGroupModalComponent, {modalDialogClass: 'custom-modal'});
    modalRef.componentInstance.formVersionId = this._formVersion.id;
    modalRef.componentInstance.formSectionId = formSectionId;
    modalRef.componentInstance.title = 'Add Group';
    modalRef.componentInstance.actionButtonLabel = 'Create';

    modalRef.result.then((formDetails: any) => {
      this.sections = formDetails.sections;
    }).catch((res) => {});
  }

  public onAddQuestionClick(formGroupId: number): void {
    const modalRef = this.modalService.open(EditMdsQuestionModalComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.formVersionId = this._formVersion.id;
    modalRef.componentInstance.title = 'Add Question';
    modalRef.componentInstance.formGroupId = formGroupId;
    modalRef.componentInstance.actionButtonLabel = 'Create';

    modalRef.result
    .then((formDetails: any) => {
      this.sections = formDetails.sections;
    })
    .catch((res) => {});
  }

  public onEditQuestionClick(question: IEditTrackerQuestion): void {
    const modalRef = this.modalService.open(EditMdsQuestionModalComponent, { modalDialogClass: 'custom-modal' });
    modalRef.componentInstance.formVersionId = this._formVersion.id;
    modalRef.componentInstance.title = 'Edit Question';
    modalRef.componentInstance.formGroupId = question.formGroupId;
    modalRef.componentInstance.actionButtonLabel = 'Update';
    modalRef.componentInstance.editQuestion = question;

    modalRef.result
    .then((formDetails: any) => {
      this.sections = formDetails.sections;
    })
    .catch((res) => {});
  }
  public onDeleteQuestionClick(question: IEditTrackerQuestion): void {
  }

  public onEditGroupClick(group: IGroup): void {
    const modalRef = this.modalService.open(EditMdsGroupModalComponent, {modalDialogClass: 'custom-modal'});
    modalRef.componentInstance.formVersionId = this._formVersion.id;
    modalRef.componentInstance.formSectionId = group.formSectionId;
    modalRef.componentInstance.title = 'Edit Group';
    modalRef.componentInstance.actionButtonLabel = 'Update';
    modalRef.componentInstance.editGroup = group;

    modalRef.result.then((formDetails: any) => {
      this.sections = formDetails.sections;
    }).catch((res) => {});
  }
  public onDeleteGroupClick(group: IGroup): void {
  }

  public onEditSectionClick(section: ISection): void {
    const modalRef = this.modalService.open(EditMdsSectionModalComponent, {modalDialogClass: 'custom-modal'});
    modalRef.componentInstance.formVersionId = this._formVersion.id;
    modalRef.componentInstance.title = 'Edit Section';
    modalRef.componentInstance.actionButtonLabel = 'Update';
    modalRef.componentInstance.editSection = section;

    modalRef.result.then((formDetails: any) => {
      this.sections = formDetails.sections;
    }).catch((res) => {});
  }
  public onDeleteSectionClick(section: ISection): void {
  }
}
