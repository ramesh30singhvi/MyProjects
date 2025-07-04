import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgbAccordion, NgbModal, NgbNavChangeEvent } from '@ng-bootstrap/ng-bootstrap';
import * as moment from 'moment';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject, concatMap, delay, first, forkJoin, from, mergeMap, Observable, of, switchMap, take } from 'rxjs';
import { MM_DD_YYYY_HH_MM_SLASH } from '../../common/constants/date-constants';
import { AIAudit, AIKeywordSummary, AIProgressNotes, AIServiceRespond, PCCNotes } from '../../models/reports/reportAI.model';
import { IReportAIAction, IReportAIStatus, ReportAIActions, ReportAIStatusEnum, ReportAIStatuses } from '../../models/reports/reports.model';
import { RolesEnum } from '../../models/roles.model';
import { AddKeywordReportComponent } from '../../reports/add-keyword-report/add-keyword-report.component';
import { KeywordAIReportService } from '../../reports/keyword-ai-report/services/keyword-ai-report.service';
import { AuthService } from '../../services/auth.service';
import { ReportAIService } from '../../services/reportAI.service';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-edit-ai-report',
  templateUrl: './edit-ai-report.component.html',
  styleUrls: ['./edit-ai-report.component.scss']
})
export class EditAiReportComponent implements OnInit {
  @ViewChild('acc') accordion: NgbAccordion;

  isEditable: boolean;
  public startProcessing: boolean;
  public error$ = new BehaviorSubject<any>(null);
  resultFromAI: Map<string, AIProgressNotes[]> = new Map();
  public active = 1;
  isSelectAllChecked: boolean = false;
  paramId: number;
  public isAuditor: boolean;
  public isAdmin: boolean;
  public isReviewer: boolean;
  public isFacility: boolean;
  public isDisabled: boolean = false;
  public actions: Array<IReportAIAction> = [];
  public statuses = ReportAIStatuses;
  public status: IReportAIStatus;
  isShowExportPdf: boolean = false;

  aiAudit: AIAudit;

  constructor(private reportAIServiceApi: ReportAIService, private spinner: NgxSpinnerService, private keywordAIReportService: KeywordAIReportService,
    private activateRoute: ActivatedRoute, private toastr: ToastrService, private authService: AuthService,
    private userServiceApi: UserService, private modalService: NgbModal) {
    this.activateRoute.paramMap
      .pipe(switchMap(params => params.getAll('id')))
      .subscribe(data => this.paramId = +data);

    this.isAuditor = this.authService.isUserInRole(RolesEnum.Auditor);
    this.isAdmin = this.authService.isUserInRole(RolesEnum.Admin);
    this.isReviewer = this.authService.isUserInRole(RolesEnum.Reviewer);
    this.isFacility = this.authService.isUserInRole(RolesEnum.Facility);

  }

  ngOnInit(): void {
    this.getAIAuditV2();
  }

  private getAIAuditV2() {
    if (this.paramId) {
      //this.spinner.show("loading");
      this.reportAIServiceApi.getAIAuditV2(this.paramId)
        .pipe(first())
        .subscribe((aiAudit: any) => {
          if (aiAudit) {
           
            this.aiAudit = aiAudit;
            console.log(this.aiAudit);
         //   this.isSelectAllChecked =
            //let allAccepted = [];
            //this.aiAudit.values.forEach(value => {
            //    var accepted = value.summaries.filter(x => x.accept == true);
            //    if (accepted.length == value.summaries.length) {

            //    }
            //});

            this.aiAudit.values.forEach(pdfNotes => {
              if (pdfNotes.summaries != undefined && pdfNotes.summaries.length > 0) {
                let d = 's-' + pdfNotes.patientId;
                this.activeIds.push(d);
              }

            })

          //  this.spinner.hide("loading");
            this.setActions(this.aiAudit.status);

            this.status = this.keywordAIReportService.getStatus(this.aiAudit.status);

            if (this.aiAudit.status == ReportAIStatusEnum.Submitted) {
              this.isShowExportPdf = true;
              this.isDisabled = true;

            }
      
          }
        });
    }
  }

  public createReport() {

    this.reportAIServiceApi.downloadAIAuditV2Report(this.aiAudit);
  }

  get error() {
    if (this.error$.getValue() == null)
      return;

    return this.error$.getValue();
  }

  startTime: string = ""

  processInBatches(dataArray: any[], batchSize: number): void {
    let currentIndex = 1;

    const processNextBatch = () => {
      if (currentIndex >= dataArray.length) {
        console.log('✅ All data processed!');
        return;
      }

      const batch = dataArray.slice(currentIndex, currentIndex + batchSize);
      console.log(`Processing batch from index ${currentIndex} to ${currentIndex + batch.length - 1}`);

      const requests = batch.map(data => this.reportAIServiceApi.getProgressNotes(data.id));

      forkJoin(requests).subscribe(
        (responses) => {
          responses.forEach(res => {
            if (res.error !== "") {
              console.error('API error:', res.error);
            } else if (res.items?.length > 0) {
              const note = res.items[0];
              const pdfNotes = this.aiAudit.values.filter(x => x.id === note.auditAIPatientPdfNotesID);

              if (pdfNotes.length > 0) {
                pdfNotes[0].summaries = res.items;
                const accordionId = 's-' + pdfNotes[0].patientId;
                this.accordion?.expand(accordionId);
              }
            }
          });

          // Move to next batch after a short delay
          of(null).pipe(delay(200)).subscribe(() => {
            currentIndex += batchSize;
            processNextBatch(); // continue the loop
          });
        },
        (error) => {
          console.error('❌ Batch failed:', error);
        }
      );
    };

    // Start processing
    processNextBatch();
  }
  startIndex = 1;
  batchSize = 40;
  sendFirstRequest(dataArray) {

    this.reportAIServiceApi.getProgressNotes(dataArray[0].id).subscribe(
      res => {
        console.log(res);
        if (res.error != "") {
          
          // this.toastr.error(res.error);
          console.log(res.error);  // Log the response from each request
        } else if (res.items.length > 0) {

          console.log(res.items[0])
          var pdfNotes = this.aiAudit.values.filter(x => x.id == res.items[0].auditAIPatientPdfNotesID);

          if (pdfNotes.length > 0) {
            pdfNotes[0].summaries = res.items;
            console.log(pdfNotes[0].patientId);
            let d = 's-' + pdfNotes[0].patientId;
            this.accordion?.expand(d);
          } else {
            //this.toastr.error(result.error);
          }
          of(null).pipe(delay(2000)).subscribe(() => {
            this.startIndex = this.startIndex + this.batchSize;
            if (this.startIndex > dataArray.length) {
              this.batchSize = dataArray.length - this.startIndex;
              this.startIndex = dataArray.length - this.batchSize;
            }
            console.log("start index after 1 " + this.startIndex + " batch size after 1" + this.batchSize);
            this.processInBatches(dataArray, this.batchSize); // Continue with next batch
          });
        }

      },
      error => {
        console.error('Error occurred:', error);  // Handle errors
      }
    );
  }
  getSummaryForAll() {


    // Create an Observable from resultParse
    console.log(this.accordion);
    console.log("Start Time : " + moment().toLocaleString())
    this.startTime = "Start Time: " + moment().toLocaleString();
   // this.spinner("loading").s
    var dataFromPdf = this.aiAudit.values.filter(x => x.patientId != "1");

    dataFromPdf = dataFromPdf.filter(x => x.summaries == undefined || x.summaries.length == 0);
    console.log("total " + dataFromPdf.length);
    this.sendFirstRequest(dataFromPdf);
    //delay(2000);

    //from(dataFromPdf).pipe(

    //  // Sequentially execute each HTTP request using concatMap
    // // concatMap(data => this.reportAIServiceApi.getProgressNotes(data.id))
    //  take(20), // limit to 20 items
    //  mergeMap(data => this.reportAIServiceApi.getProgressNotes(data.id), 20) // concurrency of 5 (can increase if needed)

    //).subscribe(
    //  res => {
    //    console.log(res);
    //    if (res.error != "") {
    //      // this.toastr.error(res.error);
    //      //  console.log(res.keyword);  // Log the response from each request
    //    } else if (res.items.length > 0) {

    //      console.log(res.items[0])
    //      var pdfNotes = this.aiAudit.values.filter(x => x.id == res.items[0].auditAIPatientPdfNotesID);

    //      if (pdfNotes.length > 0) {
    //        pdfNotes[0].summaries = res.items;
    //        console.log(pdfNotes[0].patientId);
    //        let d = 's-' + pdfNotes[0].patientId;
    //        this.accordion?.expand(d);
    //      } else {
    //        //this.toastr.error(result.error);
    //      }
    //      //let name = res.items[0].patientName;
    //      //if (!this.resultFromAI.has(name.toLowerCase())) {
    //      //  this.resultFromAI.set(name.toLowerCase(), res.items);
    //      //  let d = 's-' + res.items[0].patientId;
    //      //  this.accordion?.expand(d);
    //      //  console.log(res.items[0].patientName + " end Time : " + moment().toLocaleString())
    //      //}
    //    }

    //  },
    //  error => {
    //    console.error('Error occurred:', error);  // Handle errors
    //  }
    //);


  }




  activeIds: string[] = [];
  onShownKeywordAccordion(e) {
    if (this.activeIds.indexOf(e) == -1)
      this.activeIds.push(e);
  }

  onHiddenKeywordAccordion(e) {
    this.activeIds = this.activeIds.filter((f) => f != e);
  }
  trackById(index: any, item: any) {
    return item?.patientId;
  }

  onNavChange(changeEvent: NgbNavChangeEvent) {

  }

  getNotes(id: number) {

    if (this.aiAudit == undefined)
      return [];

    if (id == undefined) {
      return [];
    }

    var pdfNotes = this.aiAudit.values.filter(x => x.id == id);

    if (pdfNotes.length == 0)
      return [];

    let summary = pdfNotes[0].summaries;

    if (summary == undefined)
      return [];

    return pdfNotes[0].summaries;
  }
  waitForAIRespond = false;
  getProgressNotes(data: AIProgressNotes) {

    //const button = event.target as HTMLButtonElement;
    //const data = (button as any).data;
    console.log('Data:', data);
    // event?.stopPropagation();
    let pdfNotesID = data.id;
    console.log(data.summaries);

    if (data.summaries != undefined && data.summaries.length > 0 ) {
      return;
    }
    if (data.patientId == "1")
      return;

    
    const obs$: Observable<any>[] = [];

    this.waitForAIRespond = true;
    obs$.push(this.reportAIServiceApi.getProgressNotes( data.id));
    forkJoin(obs$).subscribe(respond => {
      console.log(respond);
      if (respond != undefined && respond.length > 0) {
        var result = respond[0] as AIServiceRespond;

        if (result.items.length > 0) { 
          var pdfNotes = this.aiAudit.values.filter(x => x.id == result.items[0].auditAIPatientPdfNotesID)

          if (pdfNotes.length > 0 && result.error == "")
            pdfNotes[0].summaries = result.items;
        } else {
          this.toastr.error(result.error);
        }

        this.waitForAIRespond = false;
      }

    });
  }

  public addNewToReport() {
    const modalRef = this.modalService.open(AddKeywordReportComponent, { modalDialogClass: 'add-keyword-modal' });
    modalRef.result.then((result) => { if (result) { this.addNewItem(result); } }, (reason) => {

    });
  }

  private addNewItem(keywordData: any) {

    if (this.resultFromAI == undefined)
      return;

    let strDate = keywordData.date + " " + keywordData.time;
    let selectDate = moment(strDate, MM_DD_YYYY_HH_MM_SLASH).format(MM_DD_YYYY_HH_MM_SLASH);


    var existValues = this.aiAudit.values.filter(x => keywordData.resident.toLowerCase() == x.patientName.toLowerCase());
    if (existValues?.length > 0) {

      console.log(existValues[0]);
      let summary: AIKeywordSummary = {
        auditAIPatientPdfNotesID: existValues[0].id, summary: keywordData.report, id: 0, keyword: keywordData.keyword, accept: true
      }
  
      this.reportAIServiceApi.addKeyWordSummary(summary).subscribe(result => {
        console.log(result);
        if(result.id > 0)
           existValues[0].summaries.push(result);
      })
    } else {
      console.log("add patient Info");
      let summary: AIKeywordSummary = {
        auditAIPatientPdfNotesID:0, summary: keywordData.report, id: 0, keyword: keywordData.keyword, accept: true
      }
      let arr = [];
      arr.push(summary);
      let item: AIProgressNotes = {
        reportId: this.aiAudit.id, facilityId: this.aiAudit.facility.id, facilityName: this.aiAudit.facility.name, id: 0, date: "", time: "",
        patientId: "", patientName: keywordData.resident, dateTime: selectDate, summaries: arr
      };

      this.reportAIServiceApi.addPatientNoteWithSummary(item).subscribe(result => {
  
        if (result.id > 0)
          this.aiAudit.values.push(result);
      })

    }
  }
  selectAllChange(e) {
    let isChecked: boolean = e.currentTarget.checked;

    if (isChecked) {
      this.isSelectAllChecked = true;
    } else {
      this.isSelectAllChecked = false;
    }
    this.aiAudit.values?.forEach(value => {
      value.summaries.forEach(keyword => keyword.accept = this.isSelectAllChecked)
    });
    this.reportAIServiceApi.updateAuditAIV2(this.aiAudit).subscribe(res => {
      this.aiAudit = res;

    });
  
  }
  getCheckValue(item: AIProgressNotes) {

    let acceptedCount = item.summaries.filter(x => x.accept == true);
    if (acceptedCount.length == item.summaries.length)
      return true;

    return false;
  }
  selectSummary(event, res: AIKeywordSummary) {
    console.log(res);
    let isChecked: boolean = event.currentTarget.checked;
    res.accept = isChecked;
    this.reportAIServiceApi.updateKeywordSummary(res).subscribe(res => {
      console.log(res);

    });
  }

  selectAll(e, item: AIProgressNotes) {
    let isChecked: boolean = e.currentTarget.checked;

    var values = this.aiAudit.values.filter(x => x.id == item.id);

    if (values.length > 0) {
      values[0].summaries.forEach(keyword => keyword.accept = isChecked);

      this.reportAIServiceApi.updatePatientNoteWithSummary(values[0]).subscribe(res => {
        console.log(res);

      });
    }


  }

  changeSummary(event) {
    console.log(event);
  }
  //// old


  setActions(status: number | undefined) {
    if (!this.aiAudit.id || status < 0) {
      return;
    }

    let actions: Array<IReportAIAction> = [];

    switch (status) {
      case ReportAIStatuses.InProgress.id:
        if (this.isAuditor) {
          actions.push({
            id: ReportAIActions.SendForApproval.id,
            label: ReportAIActions.SendForApproval.label,
            classes: "blue",
          });
        }
        break;
      case ReportAIStatuses.WaitingForApproval.id:
        if (this.isAuditor) {
          actions.push({
            id: ReportAIActions.ReopenToInProgress.id,
            label: ReportAIActions.ReopenToInProgress.label,
            classes: "white",
          });
        }

        if (this.isReviewer) {
          actions.push({
            id: ReportAIActions.Approve.id,
            label: ReportAIActions.Approve.label,
            classes: "green",
          });
          actions.push({
            id: ReportAIActions.Disapprove.id,
            label: ReportAIActions.Disapprove.label,
            classes: "red",
          });
        }
        break;
      case ReportAIStatuses.Approved.id:
        if (this.isReviewer) {
          actions.push({
            id: ReportAIActions.ReopenToReopened.id,
            label: ReportAIActions.ReopenToReopened.label,
            classes: "white",
          });
          actions.push({
            id: ReportAIActions.Submit.id,
            label: ReportAIActions.Submit.label,
            classes: "blue",
          });
        }
        break;
      case ReportAIStatuses.Reopened.id:
        if (this.isReviewer) {
          actions.push({
            id: ReportAIActions.Approve.id,
            label: ReportAIActions.Approve.label,
            classes: "blue",
          });
        }
        break;
      case ReportAIStatuses.Disapproved.id:
        if (this.isAuditor) {
          actions.push({
            id: ReportAIActions.SendForApproval.id,
            label: ReportAIActions.SendForApproval.label,
            classes: "blue",
          });
        }
        break;
      default:
        break;
    }

    this.actions = actions;
  }

  onActionClick(actionId) {
    if (!this.aiAudit.id) {
      return;
    }

    let statusId: number;

    switch (actionId) {
      case ReportAIActions.SendForApproval.id:
        statusId = ReportAIStatuses.WaitingForApproval.id;
        break;

      case ReportAIActions.ReopenToInProgress.id:
        statusId = ReportAIStatuses.InProgress.id;
        break;

      case ReportAIActions.ReopenToReopened.id:
        statusId = ReportAIStatuses.Reopened.id;
        break;

      case ReportAIActions.Approve.id:
        statusId = ReportAIStatuses.Approved.id;
        break;

      case ReportAIActions.Disapprove.id:
        statusId = ReportAIStatuses.Disapproved.id;
        break;

      case ReportAIActions.Submit.id:
        statusId = ReportAIStatuses.Submitted.id;
        break;

      default:
        break;
    }

    if (!statusId || statusId < 1) {
      return;
    }

    this.aiAudit.status = statusId;



     this.setStatusOnly();
    

  }
  private updateReport() {

    this.reportAIServiceApi.updateAuditAIV2(this.aiAudit).subscribe(res => {
      this.aiAudit = res;
      this.toastr.success("AI Audit data saved successully");
    });

  }
  //  this.reportAIService.updateAIAuditReport(this.resultAI, this.reportAIContent.id, true)
  //    .pipe(first())
  //    .subscribe(
  //      (response) => {
  //        if (response != null) {
  //          this.reportAIContent = response;
  //          this.loadData(this.reportAIContent, false);
  //          this.status = this.keywordAIReportService.getStatus(response.status);

  //          if (response.status == ReportAIStatusEnum.Submitted) {
  //            this.isShowExportPdf = true;
  //            this.isDisabled = true;
  //          }

  //          this.setActions(response.status);
  //          this.toastr.success("AI Audit data saved successully");
  //        } else {
  //          this.toastr.error("Error while saving AI Audit data");
  //        }
  //        this.cdr.detectChanges();
  //      },
  //      (error: HttpErrorResponse) => {
  //        console.log(error);
  //      }
  //    );
  //}


  private setStatusOnly() {
    this.reportAIServiceApi.setAIAuditV2Status(this.aiAudit.id, this.aiAudit.status)
      .pipe(first())
      .subscribe(
        (response) => {
          if (response) {
            this.status = this.keywordAIReportService.getStatus(response);
            this.aiAudit.status = response;

            if (this.aiAudit.status == ReportAIStatusEnum.Submitted) {
              this.isShowExportPdf = true;
              this.isDisabled = true;
            }

            this.setActions(response);
         
          } else {
            this.toastr.error("Error while updating AI Audit Status");
          }
        },
        (error: HttpErrorResponse) => {
          console.log(error);
        }
      );
  }
  onSaveClick(showToast: boolean) {
  

    //this.reportAIService.updateAIAuditReport(this.resultAI, this.reportAIContent.id, true)
    //  .pipe(first())
    //  .subscribe(
    //    (response) => {
    //      if (response != null) {
    //        this.reportAIContent = response;
    //        this.loadData(response, false);
    //        if (showToast == true)
    //          this.toastr.success("AI Audit data saved successully");
    //      } else {
    //        if (showToast == true)
    //          this.toastr.error("Error while saving AI Audit data");
    //      }
    //    },
    //    (error: HttpErrorResponse) => {
    //      console.log(error);
    //    }
    //  );
  }

}
