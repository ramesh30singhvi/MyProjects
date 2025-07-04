import { Component, ElementRef, OnDestroy, OnInit, Renderer2, ViewEncapsulation } from "@angular/core";
import { Subscription } from "rxjs";
import { first } from "rxjs/operators";
import { IOption, IProgressNoteKeyword, IProgressNote, IProgressNoteDetails } from "src/app/models/audits/audits.model";
import { AuditServiceApi } from "src/app/services/audit-api.service";
import { AuditService } from "../services/audit.service";

@Component({
  selector: "app-progress-notes-section",
  templateUrl: "./progress-notes-section.component.html",
  styleUrls: ["./progress-notes-section.component.scss"],
  encapsulation: ViewEncapsulation.None,
})

export class ProgressNotesSectionComponent implements OnInit, OnDestroy {
  public isEditable: boolean;

  public keywordsTotalCount: number = 0;
  public progressNotes: IProgressNote[] = [];
  public timeZoneOffset: number = 0;
  public keyword: IOption = null;
  public dateTimeFormat: string = "MM/dd/YYYY HH:mm";
  public keywordIds: IProgressNoteKeyword[] = [];
  public selectedKeywordIndex: number = -1;
  public selectedKeywordName: string;

  public auditId: number;
  private facilityId: number;
  private dateFrom: Date;
  private dateTo: Date;

  private subscription: Subscription;

  constructor(
    private auditServiceApi: AuditServiceApi,
    private auditService: AuditService,
    private elementRef: ElementRef,
    private renderer: Renderer2) { 
    this.subscription = new Subscription();

    this.subscription.add(
      this.auditService
      .isEditable$
      .subscribe((isEditable) => this.isEditable = isEditable));

    this.subscription.add(
      this.auditService
      .audit$
      .subscribe((audit) => {
        this.auditId = audit?.id;
        this.facilityId = audit?.facility?.id;
        this.timeZoneOffset = audit?.facility?.timeZoneOffset;
        this.dateFrom = audit?.incidentDateFrom;
        this.dateTo = audit?.incidentDateTo;
      }));

      this.subscription.add(
        this.auditService
        .keyword$
        .subscribe((keyword) => {

          this.removeActiveClassFromKeyword();
          
          this.selectedKeywordIndex = -1;

          if(this.keyword?.id !== keyword?.id) {
            this.keyword = keyword;
            this.selectedKeywordName = null;
            this.keywordIds = [];
            this.getProgressNotes();
          }
        }));
  }

  ngOnInit() {

  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  ngAfterViewChecked (){
    const keywords = this.elementRef.nativeElement.querySelectorAll('.keyword');
    keywords.forEach((keyword: HTMLAnchorElement) => {

      if (keyword.getAttribute('listener') !== 'true') {
        keyword.addEventListener('click', this.onKeywordClick.bind(this));
        keyword.setAttribute('listener', 'true');
      }
    });
  } 

  public onPreviousKeywordClick(): void {
    if(this.selectedKeywordIndex <= -1) {      
      return;
    }

    if(this.selectedKeywordIndex > -1){
      this.removeActiveClassFromKeyword();
      this.selectedKeywordIndex--;
    }

    if(this.selectedKeywordIndex >= 0){
      this.addActiveClassToKeyword();
    }

    if(this.selectedKeywordIndex === -1) {
      this.setSelectedKeyword(null);
    }
  }

  public onNextKeywordClick(): void {
    if(this.selectedKeywordIndex + 1 >= this.keywordsTotalCount) {
      return;
    }

    if(this.selectedKeywordIndex >= 0){
      this.removeActiveClassFromKeyword();
    }

    if (this.selectedKeywordIndex + 1 < this.keywordsTotalCount) {
      this.selectedKeywordIndex++;
    } else {
      return;
    }

    this.addActiveClassToKeyword();
  }

  public onKeywordClick(event) {
    if(this.selectedKeywordIndex >= 0){
      this.removeActiveClassFromKeyword();
    }

    this.selectedKeywordIndex = this.keywordIds.findIndex((keyword) => keyword.keywordId === event.target.id);
    
    this.addActiveClassToKeyword();
  }

  private addActiveClassToKeyword(): void {
    const selectedKeyword: IProgressNoteKeyword = this.keywordIds[this.selectedKeywordIndex];
    const element = this.elementRef.nativeElement.querySelector(`#${selectedKeyword.keywordId}`);
    this.renderer.addClass(element, 'active');

    element.scrollIntoView({block: "center", behavior: "smooth"});

    this.setSelectedKeyword(selectedKeyword);
  }

  private removeActiveClassFromKeyword(): void {
    if(!this.keywordIds || this.keywordIds.length === 0) {
      return;
    }

    const selectedKeyword = this.keywordIds[this.selectedKeywordIndex];

    if(!selectedKeyword) {
      return;
    }

    const element = this.elementRef.nativeElement.querySelector(`#${selectedKeyword.keywordId}`);
    this.renderer.removeClass(element, 'active');
  }

  private setSelectedKeyword(selectedKeyword: IProgressNoteKeyword): void {
    if(selectedKeyword)
    {
    const progressNote = this.progressNotes.find((note) => note.id === selectedKeyword.noteId);
        selectedKeyword = { 
          ...selectedKeyword, 
          resident: progressNote.resident, 
          createdDate: progressNote.createdDate, 
          effectiveDate: progressNote.effectiveDate, 
          timeZoneOffset: this.timeZoneOffset
        };
    }

    this.auditService.setProgressNoteKeyword(selectedKeyword);
  }

  private getProgressNotes(): void {
    if(!this.facilityId || !this.keyword || !this.dateFrom){
      return;
    }
    
    this.auditServiceApi.getProgressNotes(this.auditId, this.facilityId, this.keyword, this.dateFrom, this.dateTo, this.timeZoneOffset)
    .pipe(first())
    .subscribe((details: IProgressNoteDetails) => {
      this.selectedKeywordName = this.keyword?.name;
      this.keywordsTotalCount = details.keywordsTotalCount;
      this.progressNotes = this.getProgressNotesWithMarkedKeywords(details.progressNotes);
      //this.timeZoneOffset = details.timeZoneOffset;

      if (this.progressNotes) {
          const element = this.elementRef.nativeElement.querySelector(`.progress-notes-wrapper`);
          if (element) {
              element.scrollTop = 0;
          }
      }
    }, 
    (error) => {console.log(error)});
  }

  private getProgressNotesWithMarkedKeywords(progressNotes: IProgressNote[]): IProgressNote[] {
    this.keywordsTotalCount = 0;
    
    const keywords = this.keyword?.name.split("/").map(keyword => keyword.trim());

    let noteIndex = 0;
    for (let note of progressNotes) {
      note.progressNoteHtml = note.progressNoteText;      

      keywords?.forEach(keyword => {        
        const entryWordRegExp = new RegExp(`\\b${keyword}\\b`, 'gi');
        note.progressNoteHtml = this.higlightKeyword(note.id, noteIndex, note.progressNoteHtml, entryWordRegExp, "keyword");
      });
      
      keywords?.forEach(keyword => {
        const partWordRegExp = new RegExp(`${keyword}`, 'gi');

        note.progressNoteHtml = note.progressNoteHtml.replace(new RegExp(`(\\b\\w*${keyword}\\w*\\b)(?!</mark>)`, 'gi'), (match, p1) => {
          const keywordWrap = `<mark class="keyword-wrapper">${match}</mark>`;
          return this.higlightKeyword(note.id, noteIndex, keywordWrap, partWordRegExp, "keyword");
        });
      });

      noteIndex++;
    }

    this.keywordIds.sort((current, next) => { return current.noteIndex - next.noteIndex || current.keywordIndex - next.keywordIndex});

    return progressNotes;
  }

  private higlightKeyword(noteId: number, noteIndex: number, text: string, regExp: RegExp, className: string): string {
    return text.replace(regExp, (match, p1) => {
      const keywordId = `K-${noteId}-${p1}`;
      this.keywordIds.push({keywordId, noteId: noteId, noteIndex: noteIndex, keywordIndex: p1});

      this.keywordsTotalCount++;

      return `<mark id="${keywordId}" class="${className}" click="onKeywordClick($event)">${match}</mark>`;
    });
  }
}

function complete(arg0: (details: IProgressNoteDetails) => void, complete: any, arg2: () => void) {
  throw new Error("Function not implemented.");
}

