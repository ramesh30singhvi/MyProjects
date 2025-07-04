import { HttpErrorResponse } from "@angular/common/http";
import { AfterContentInit, AfterViewInit, Component, ElementRef, Input, OnInit, QueryList, ViewChild, ViewChildren } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { DragulaService } from "ng2-dragula";
import { first } from "rxjs";
import { SPINNER_TYPE } from "src/app/common/constants/audit-constants";
import { QuestionGroup } from "src/app/models/audits/questions.model";
import { FormServiceApi } from "src/app/services/form-api.service";

@Component({
  selector: "app-set-section-modal",
  templateUrl: "./set-section-modal.component.html",
  styleUrls: ["./set-section-modal.component.scss"]
})

export class SetSectionModalComponent implements OnInit {
  @ViewChild('sectionContainer') sectionContainerRef: ElementRef;

  public SECTIONS = 'SECTIONS';

  public spinnerType: string;

  public errors: any[] = [];

  public formSections: QuestionGroup[] = [];

  private isSectionAdded = false;

  @Input() formVersionId: number;

  @Input() sections: QuestionGroup[];

  constructor(private formServiceApi: FormServiceApi,
    public activeModal: NgbActiveModal,
    private dragulaService: DragulaService,) { 
      this.spinnerType = SPINNER_TYPE;
  }

  ngOnInit() {
    this.formSections = this.sections?.map((section: QuestionGroup) => {return {...section}}) ?? [];    
  }

  ngAfterViewChecked(): void {
    if (this.isSectionAdded) this.scrollToBottom();
    
    this.isSectionAdded = false;
  }

  public onAddSectionClick(): void {
    this.formSections.push({});
    this.errors['sections'] = null;

    this.isSectionAdded = true;
  }

  public onDeleteSectionClick(index: number): void {
    this.formSections.splice(index, 1);
    this.errors['sections'] = null;
  }

  public valueChanged(value: string): void {
    this.errors['sections'] = null;
  }

  public onSaveClick(): void {
    if(this.formSections.some((section: QuestionGroup) => !section.name)) {
      this.errors['sections'] = "'List of sections' should not contain empty values";
      return;
    }

    if(this.isSectionNameDuplicated(this.formSections, 'name')) {
      this.errors['sections'] = "'List of sections' should contain only unique values";
      return;
    }

    const sections = this.formSections.map((section: QuestionGroup, index: number) => {return {id: section.id, name: section.name, sequence: index + 1}});

    this.formServiceApi.editFormSections(this.formVersionId, sections)
      .pipe(first())
      .subscribe({
        next: (formDetails: any) => {
          if(formDetails) {
            this.activeModal.close(formDetails);
          }
        },
        error: (response: HttpErrorResponse) =>
        {
          this.errors = response.error?.errors;
          console.error(response);
        }
      });
  }

  private scrollToBottom() {
    try{
      this.sectionContainerRef.nativeElement.scrollTop = this.sectionContainerRef.nativeElement.scrollHeight;
    } catch(err) {}
  }

  private isSectionNameDuplicated(sections, keyName): boolean {
    return new Set(sections.map((section: QuestionGroup) => section.name?.trim().toLowerCase())).size !== sections.length
  }
}
