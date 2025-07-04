import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { BehaviorSubject, first, forkJoin, Observable, Subscription, switchMap, interval } from 'rxjs';
import { DomSanitizer } from '@angular/platform-browser';
import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbNavChangeEvent } from '@ng-bootstrap/ng-bootstrap';
import { ToastrService } from 'ngx-toastr';
import * as moment from 'moment';
import { MM_DD_YYYY_HH_MM_SLASH } from '../../../common/constants/date-constants';
import { AddKeywordReportComponent } from '../../add-keyword-report/add-keyword-report.component';
import { AIResult, ReportAIItem } from '../../../models/reports/file-upload.model';
import { IFilterOption } from '../../../models/audits/audit.filters.model';
import { KeywordAIReportService } from '../services/keyword-ai-report.service';
import { ReportAIService } from '../../../services/reportAI.service';
import { IReportAIContent, ReportAIStatuses, IReportAIStatus, ReportAIStatusEnum, ReportAIActions, IReportAIAction } from '../../../models/reports/reports.model';
import { number } from 'ngx-custom-validators/src/app/number/validator';
import { AuthService } from "src/app/services/auth.service";
import { RolesEnum } from "src/app/models/roles.model";
import { HttpErrorResponse } from "@angular/common/http";
import { NgxSpinnerService } from 'ngx-spinner';

export const HIDE_AI_SUMMARY = "HideAISummary";

@Component({
  selector: 'app-edit-ai-audit',
  templateUrl: './edit-ai-audit.component.html',
  styleUrls: ['./edit-ai-audit.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EditAiAuditComponent implements OnInit, OnDestroy {

  public paramId: number;
  public reportAIContent: IReportAIContent;

  public reportForm: FormGroup;
  public reportFormByID: FormGroup;

  public error$ = new BehaviorSubject<any>(null);
  private resultAISubject$ = new BehaviorSubject<AIResult>(null);
  private subscriptionResult: Subscription;
  public resultAI: AIResult = null;
  public active = 1;
  mapResultsByName: Map<string, any[]> = new Map();

  isSelectAllInKeywordChecked: boolean;
  isSelectAllInResidentChecked: boolean;

  public isAuditor: boolean;
  public isAdmin: boolean;
  public isReviewer: boolean;
  public isFacility: boolean;
  public isDisabled: boolean = false;
  public hideAISummary: boolean;

  public actions: Array<IReportAIAction> = [];
  public statuses = ReportAIStatuses;

  isShowExportPdf: boolean = false;
  public sortGridType: string = "Keyword";
  public status: IReportAIStatus;

  autoSaveSubscription: Subscription;

  activeKeywordIds: string[] = ['static-0'];
  activeResidentIds: string[] = ['static-0'];

  constructor(
    private formBuilder: FormBuilder,
    private activateRoute: ActivatedRoute,
    private reportAIService: ReportAIService,
    private keywordAIReportService: KeywordAIReportService,
    private modalService: NgbModal, private spinner: NgxSpinnerService,
    private sanitizer: DomSanitizer, private cdr: ChangeDetectorRef,
    private toastr: ToastrService,
    private authService: AuthService,
  ) {
    this.activateRoute.paramMap
      .pipe(switchMap(params => params.getAll('id')))
      .subscribe(data => this.paramId = +data);

    this.isAuditor = this.authService.isUserInRole(RolesEnum.Auditor);
    this.isAdmin = this.authService.isUserInRole(RolesEnum.Admin);
    this.isReviewer = this.authService.isUserInRole(RolesEnum.Reviewer);
    this.isFacility = this.authService.isUserInRole(RolesEnum.Facility);

    this.reportAIService.getAppSettingsValue(HIDE_AI_SUMMARY)
      .pipe(first())
      .subscribe((value: string) => {
        if (value && value == "1" && !this.isAdmin) {
          this.hideAISummary = true;
        } else {
          this.hideAISummary = false;
        }
      });

    this.reportForm = this.formBuilder.group({
      reportItems: new FormArray([])
    });

    this.reportFormByID = this.formBuilder.group({
      reportItems: new FormArray([])
    });
    this.subscriptionResult = this.resultAISubject$.asObservable().subscribe();

    this.autoSaveSubscription = interval(60000).subscribe((val) => {
      if (this.reportAIContent && this.reportAIContent.status != ReportAIStatusEnum.Submitted && (this.reportForm.dirty || this.reportFormByID.dirty)) {
        this.reportForm.markAsPristine();
        this.reportFormByID.markAsPristine();
        this.autoSaveData();
      }
    });
  }

  ngOnInit(): void {
    this.getAIAuditData();
  }

  ngOnDestroy(): void {
    if (this.subscriptionResult != undefined)
      this.subscriptionResult.unsubscribe();

    if (this.autoSaveSubscription != undefined)
      this.autoSaveSubscription.unsubscribe();
  }

  private getAIAuditData() {
    if (this.paramId) {
      this.spinner.show("loading");
      this.reportAIService.getAIAudit(this.paramId,true)
        .pipe(first())
        .subscribe((aiAudit: any) => {
          if (aiAudit) {
            
            this.reportAIContent = aiAudit;

            this.loadData(this.reportAIContent,true);

 
            this.spinner.hide("loading");
            this.setActions(this.reportAIContent.status);

            this.status = this.keywordAIReportService.getStatus(this.reportAIContent.status);

            if (this.reportAIContent.status == ReportAIStatusEnum.Submitted) {
              this.isShowExportPdf = true;
              this.isDisabled = true;

            }
            this.cdr.detectChanges();
          }
        });
    }
  }

  private loadData(data: IReportAIContent,useSummaryAI :boolean) {


    if (useSummaryAI == true) {
      this.resultAI = {
        jsonResult: data.summaryAI,
        date: moment(data.auditDate).format("MMM-DD, YYYY"),
        time: data.auditTime,
        user: data.auditorName,
        auditDate: data.auditDate,
        status: data.status,
        organization: data.organization,
        facility: data.facility,
        keywords: data.keywords,
        reportFileName: data.pdfFileName,
        containerName: data.containerName,
        error: ""
      };
    } else {
        this.resultAI.auditDate = data.auditDate;
        this.resultAI.date = moment(data.auditDate).format("MMM-DD, YYYY");
        this.resultAI.time = data.auditTime;
        this.resultAI.user = data.auditorName;
        this.resultAI.status = data.status;
        this.resultAI.facility = data.facility;
        this.resultAI.organization = data.organization;
        this.resultAI.reportFileName = data.pdfFileName;
        this.resultAI.keywords = data.keywords;
        this.resultAI.containerName = data.containerName;

  
    }
    this.resultItemsArray.clear();
    this.resultItemsByNameArray.clear();


    if (this.sortGridType == "Resident")
      this.createForms(this.resultAI.jsonResult, this.resultItemsByNameArray);
    else
      this.createForms(this.resultAI.jsonResult, this.resultItemsArray);
    this.resultAISubject$.next(this.resultAI);
    this.cdr.detectChanges();
  }

  get dataKeywords() {
    if (this.resultAISubject$.getValue() == null)
      return null;

    var j = JSON.parse(this.resultAISubject$.getValue().jsonResult);

    var keys = Object.keys(j);
    if (keys.length == 0)
      return null;

    return keys;
  }

  get dataByName() {
    var keys = [...this.mapResultsByName.keys()];
    if (keys.length == 0)
      return null;

    return keys;
  }
  trackKey(index: any, key: any) {
    return key;
  }
  trackById(index: any, item: any) {
    return item?.value.UUID;
  }
  getFormattedText(value, text) {
    if (value == undefined) return text;

    if (text == undefined)
      return text;

    try {
      var regEx = new RegExp(value, "ig");
      var replaceMask = "<span style='background-color: lightskyblue'>" + value + "</span>";
      text = text.replace(regEx, replaceMask);
      return this.sanitizer.bypassSecurityTrustHtml(text);
    } catch (x) {

    }
    return text;
  }

  get resultItemsArray() {
    return this.reportForm.controls["reportItems"] as FormArray;
  }

  get resultItemsByNameArray() {
    return this.reportFormByID.controls["reportItems"] as FormArray;
  }

  private addNewKeyword(keywordData: any) {

    if (this.resultAI == undefined)
      return;

    if (this.resultAI.jsonResult == undefined)
      return;

    if (this.resultAI.jsonResult == "{}")
      return;

    var objectJson = JSON.parse(this.resultAI.jsonResult);
    let strDate = keywordData.date + " " + keywordData.time;
    let selectDate = moment(strDate, MM_DD_YYYY_HH_MM_SLASH).format(MM_DD_YYYY_HH_MM_SLASH);

    var items = [];
    let item = {
      Acceptable: 1, SearchWord: keywordData.keyword, UUID: Math.floor(Math.random() * 1000000),
      ID: "", Name: keywordData.resident, Date: selectDate, Original: "", UserSummary: keywordData.report
    };
    items.push(item);

    var arrayKeyw = JSON.parse(this.resultAI.keywords).map(x => x);
    var existsKey = arrayKeyw.filter(x => x.toLowerCase() == keywordData.keyword.toLowerCase());

    if (existsKey.length == 0) {
      arrayKeyw.push(keywordData.keyword);
      this.resultAI.keywords = JSON.stringify(arrayKeyw);
    }

    let type: string = this.sortGridType;
    var existInJson = Object.keys(objectJson).filter(x => x.toLowerCase() == keywordData.keyword.toLowerCase());

    if (existInJson.length == 0)
        objectJson[keywordData.keyword] = items;
    else {
        var existItems = objectJson[existInJson[0]];

        if (existItems != undefined) {
          existItems.push(item);
          objectJson[existInJson[0]] = existItems;
        }
    }
    


    this.resultAI.jsonResult = JSON.stringify(objectJson);
    this.resultAISubject$.next(this.resultAI);
   

    var itemForm = this.formBuilder.group({
      Acceptable: item.Acceptable,
      Name: item.Name,
      Summary: "",
      Date: item.Date,
      ID: item.ID,
      UUID: item.UUID,
      UserSummary: item.UserSummary,
      SearchWord: existInJson.length == 0 ? keywordData.keyword : existInJson[0],
      PdfText: "",
      Original: ""
    })




    if (type == "Resident") {
      this.resultItemsByNameArray.push(itemForm);

      if (this.mapResultsByName.has(item.Name.toLowerCase())) {

        let arr = this.mapResultsByName.get(item.Name.toLowerCase());
        arr.push(items);
        this.mapResultsByName.set(item.Name.toLowerCase(), arr);
      }
      else {
        let arr = [];

        arr.push(items);
        this.mapResultsByName.set(item.Name.toLowerCase(), arr);
      }
    } else {
      this.resultItemsArray.push(itemForm);
    }
    this.updateReport();
  }
  public addKeywordToReport() {
    const modalRef = this.modalService.open(AddKeywordReportComponent, { modalDialogClass: 'add-keyword-modal' });
    modalRef.result.then((result) => { if (result) { this.addNewKeyword(result); } }, (reason) => {

    });
  }

  createByNameView() {

    var keys = this.dataKeywords;
    if (keys == null)
      return;

    this.resultItemsByNameArray.clear();
    this.mapResultsByName.clear();
    keys.forEach(key => {
      this.formsForKey(key, "Keyword").forEach(f => {
        let name = f.value["Name"];
        if (this.mapResultsByName.has(name.toLowerCase())) {
          let arr = this.mapResultsByName.get(name.toLowerCase());
          arr.push(f.value);
          this.mapResultsByName.set(name.toLowerCase(), arr);
        }
        else {
          let arr = [];
          arr.push(f.value);
          this.mapResultsByName.set(name.toLowerCase(), arr);
        }
      })
    });

    this.createForms(JSON.stringify(this.mapToObj(this.mapResultsByName)), this.resultItemsByNameArray);


  }
  createByKeywordView() {
    var map = new Map<string, any[]>();
    var keys = this.dataByName;
    if (keys == null)
      return;

    this.resultItemsArray.clear();
    keys.forEach(key => {
      this.formsForName(key).forEach(f => {
        let searchW = f.value["SearchWord"];
        if (map.has(searchW.toLowerCase())) {
          let arr = map.get(searchW.toLowerCase());
          arr.push(f.value);
          map.set(searchW.toLowerCase(), arr);
        }
        else {
          let arr = [];
          arr.push(f.value);
          map.set(searchW.toLowerCase(), arr);
        }
      })
    })


    this.resultAI.jsonResult = JSON.stringify(this.mapToObj(map));
    this.createForms(this.resultAI.jsonResult, this.resultItemsArray);

    this.resultAISubject$.next(this.resultAI);
  }

  formsForKey(key, type: string) {
    let forms = [];

    if (type == "Resident") {
      this.resultItemsByNameArray.controls.forEach(group => {
        if (group.value?.SearchWord == key) {
          if (group.value?.Acceptable === true || group.value?.Acceptable == 1) {
            group.value.Acceptable = 1;
          } else {
            group.value.Acceptable = 0;
          }
          forms.push(group);
        }

      });
    } else {
      this.resultItemsArray.controls.forEach(group => {
        if (group.value?.SearchWord == key) {
          if (group.value?.Acceptable === true || group.value?.Acceptable == 1) {
            group.value.Acceptable = 1;
          } else {
            group.value.Acceptable = 0;
          }
          forms.push(group);
        }

      });
    }
    return forms;
  }

  formsForName(name: string) {
    let forms = [];
    this.resultItemsByNameArray.controls.forEach(group => {
      if (group.value?.Name.toLowerCase() == name.toLowerCase()) {
        forms.push(group);
      }

    });
    return forms;
  }

  createForms(json: string, formArray: FormArray) {

    var j = JSON.parse(json);
    var keys = Object.keys(j);


    keys.forEach(key => {
      
      j[key].forEach(item => {
        var itemForm = this.formBuilder.group({
          Acceptable: item.Acceptable,
          Name: item.Name,
          Summary: item.Summary,
          Date: item.Date,
          ID: item.ID,
          UUID: item.UUID,
          UserSummary: item.UserSummary != undefined ? item.UserSummary : "",
          SearchWord: item.SearchWord,
          PdfText: item.PdfText,
          Original: this.getFormattedText(item.SearchWord, item.PdfText)
        })
        formArray.push(itemForm);

      })

    })

    
  }

  private mapToObj(map) {
    var obj = {}
    map.forEach(function (v, k) {
      obj[k] = v
    })
    return obj;
  }

  onNavChange(changeEvent: NgbNavChangeEvent) {
    if (changeEvent.nextId === 2) {
      this.sortGridType = "Resident";
      this.createByNameView();
    } else {
      this.sortGridType = "Keyword";
      this.createByKeywordView();
    }
  }

  selectAllChange(e) {
    let isChecked: boolean = e.currentTarget.checked;

    if (isChecked) {
      this.isSelectAllInKeywordChecked = true;
    } else {
      this.isSelectAllInKeywordChecked = false;
    }

    this.resultItemsArray.controls.forEach(group => {
      if (isChecked) {
        group.patchValue({ Acceptable: 1 });
      } else {
        group.patchValue({ Acceptable: 0 });
      }
    });
  }

  selectAllInKeywordChange(e, key: string) {
    let isChecked: boolean = e.currentTarget.checked;
    this.resultItemsArray.controls.forEach(group => {
      if (group.value?.SearchWord == key) {
        if (isChecked) {
          group.patchValue({ Acceptable: 1 });
        } else {
          group.patchValue({ Acceptable: 0 });
        }
      }
    });
  }

  selectAllResidentChange(e) {
    let isChecked: boolean = e.currentTarget.checked;

    if (isChecked) {
      this.isSelectAllInResidentChecked = true;
    } else {
      this.isSelectAllInResidentChecked = false;
    }

    this.resultItemsByNameArray.controls.forEach(group => {
      if (isChecked) {
        group.patchValue({ Acceptable: 1 });
      } else {
        group.patchValue({ Acceptable: 0 });
      }
    });
  }

  selectAllInResidentChange(e, key: string) {
    let isChecked: boolean = e.currentTarget.checked;
    this.resultItemsByNameArray.controls.forEach(group => {
      if (group.value?.Name == key) {
        if (isChecked) {
          group.patchValue({ Acceptable: 1 });
        } else {
          group.patchValue({ Acceptable: 0 });
        }
      }
    });
  }

  public createReport() {
    console.log(this.resultAI);
    this.reportAIService.downloadUpdatedManualReport(this.resultAI,this.reportAIContent?.id);
  }

  setActions(status: number | undefined) {
    if (!this.reportAIContent.id || status < 0) {
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
    if (!this.reportAIContent.id) {
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

    this.resultAI.status = statusId;

    let type: string = this.sortGridType;

    var j = JSON.parse(this.resultAI.jsonResult);
    var keys = Object.keys(j);
    var map = new Map<string, any[]>();

    keys.forEach(key => {
      let arr = [];
      this.formsForKey(key, type).forEach(f => {
        arr.push(f.value);
      })
      if (arr.length > 0)
        map[key] = arr;
    })
    this.resultAI.jsonResult = JSON.stringify(map).toString();

    if (this.reportForm.dirty || this.reportFormByID.dirty) {

      this.reportForm.markAsPristine();
      this.reportFormByID.markAsPristine();
      this.updateReport();
    } else {
      this.setStatusOnly();
    }

  }
  private updateReport() {
    this.reportAIService.updateAIAuditReport(this.resultAI, this.reportAIContent.id, true)
      .pipe(first())
      .subscribe(
        (response) => {
          if (response != null) {
            this.reportAIContent = response;
            this.loadData(this.reportAIContent,false);
            this.status = this.keywordAIReportService.getStatus(response.status);

            if (response.status == ReportAIStatusEnum.Submitted) {
              this.isShowExportPdf = true;
              this.isDisabled = true;
            }

            this.setActions(response.status);
            this.toastr.success("AI Audit data saved successully");
          } else {
            this.toastr.error("Error while saving AI Audit data");
          }
          this.cdr.detectChanges();
        },
        (error: HttpErrorResponse) => {
          console.log(error);
        }
      );
  }


  private setStatusOnly() {
    this.reportAIService.setAIAuditStatus(this.reportAIContent.id, this.resultAI.status)
      .pipe(first())
      .subscribe(
        (response) => {
          if (response) {
            this.status = this.keywordAIReportService.getStatus(response);
            this.reportAIContent.status = response;

            if (this.reportAIContent.status == ReportAIStatusEnum.Submitted) {
              this.isShowExportPdf = true;
              this.isDisabled = true;
            }

            this.setActions(response);
            this.cdr.detectChanges();
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
    let type: string = this.sortGridType;

    var j = JSON.parse(this.resultAI.jsonResult);
    var keys = Object.keys(j);
    var map = new Map<string, any[]>();

    keys.forEach(key => {
      let arr = [];
      this.formsForKey(key, type).forEach(f => {
        arr.push(f.value);
      })
      if (arr.length > 0)
        map[key] = arr;
    })
    this.resultAI.jsonResult = JSON.stringify(map).toString();

    this.reportAIService.updateAIAuditReport(this.resultAI, this.reportAIContent.id, true)
      .pipe(first())
      .subscribe(
        (response) => {
          if (response != null) {
            this.reportAIContent = response;
            this.loadData(response,false);
            if (showToast == true)
                this.toastr.success("AI Audit data saved successully");
          } else {
            if (showToast == true)
              this.toastr.error("Error while saving AI Audit data");
          }
        },
        (error: HttpErrorResponse) => {
          console.log(error);
        }
      );
  }

  autoSaveData() {
    console.log("Running Auto Save at " + new Date().toLocaleTimeString());
    if (this.resultAI.jsonResult == "{}")
      return;
    let type: string = this.sortGridType;

    var j = JSON.parse(this.resultAI.jsonResult);
    var keys = Object.keys(j);
    var map = new Map<string, any[]>();

    keys.forEach(key => {
      let arr = [];
      this.formsForKey(key, type).forEach(f => {
        arr.push(f.value);
      })
      if (arr.length > 0)
        map[key] = arr;
    })
    this.resultAI.jsonResult = JSON.stringify(map).toString();

    this.reportAIService.updateAIAuditReport(this.resultAI, this.reportAIContent.id, false)
      .pipe(first())
      .subscribe(
        (response) => {
          this.reportAIContent = response;
          console.log("Auto saved data successfully");
        },
        (error: HttpErrorResponse) => {
          console.log("Auto save error occurred");
          console.log(error);
        }
      );
  }

  onShownKeywordAccordion(e) {
    this.activeKeywordIds.push(e);
  }

  onHiddenKeywordAccordion(e) {
    this.activeKeywordIds = this.activeKeywordIds.filter((f) => f != e);
  }

  onShownResidentAccordion(e) {
    this.activeResidentIds.push(e);
  }

  onHiddenResidentAccordion(e) {
    this.activeResidentIds = this.activeResidentIds.filter((f) => f != e);
  }

}
