
import { AfterViewInit,  ChangeDetectionStrategy,  ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { DomSanitizer } from '@angular/platform-browser';
import {  NgbModal, NgbNavChangeEvent } from '@ng-bootstrap/ng-bootstrap';
import * as moment from 'moment';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject, first, forkJoin, Observable, Subscription, interval } from 'rxjs';
import { MM_DD_YYYY_HH_MM_SLASH } from '../../common/constants/date-constants';
import { IFacilityOption, IOption, IUserOption } from '../../models/audits/audits.model';
import { AIResult, ReportAIItem } from '../../models/reports/file-upload.model';
import { FacilityService } from '../../services/facility.service';
import { OrganizationService } from '../../services/organization.service';
import { ReportAIService } from '../../services/reportAI.service';
import { UserService } from '../../services/user.service';
import { AddKeywordReportComponent } from '../add-keyword-report/add-keyword-report.component';
import { HttpErrorResponse } from "@angular/common/http";
import { Router } from '@angular/router';
import { IReportAIContent, ReportAIStatuses, IReportAIStatus, ReportAIStatusEnum, ReportAIActions, IReportAIAction } from '../../models/reports/reports.model';
import { AuthService } from "src/app/services/auth.service";
import { RolesEnum } from "src/app/models/roles.model";
import { KeywordAIReportService } from '../keyword-ai-report/services/keyword-ai-report.service';


export const HIDE_AI_SUMMARY = "HideAISummary";

@Component({
  selector: 'app-report-ai',
  templateUrl: './report-ai.component.html',
  styleUrls: ['./report-ai.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,

})
export class ReportAiComponent implements OnInit, AfterViewInit, OnDestroy {
  public submitReportForm: FormGroup;
  public reportForm: FormGroup;
  public reportFormByID: FormGroup;

  public error$ = new BehaviorSubject<any>(null);
  private resultAISubject$ = new BehaviorSubject<AIResult>(null);
  private subscriptionResult: Subscription;
  private subscriptionError: Subscription;
  public startProcessing: boolean;
  public resultAI: AIResult = null;
  public active = 1;
  isEditable: boolean;

  organizations$: Observable<IOption[]>;
  facilities$: Observable<IFacilityOption[]>;
  users$: Observable<IUserOption[]>;

  isSelectAllInKeywordChecked: boolean = false;
  isSelectAllInResidentChecked: boolean = false;

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
  checkOnlineSubscription: Subscription;

  activeKeywordIds: string[] = ['static-0'];
  activeResidentIds: string[] = ['static-0'];
  mapResultsByName: Map<string, any[]> = new Map();
  public reportAIContent: IReportAIContent = undefined;

  public isAIAuditSaved: boolean = false;

  isCallSaveFunction: boolean = false;
  isAIReportSaveProcessing = false;
  isProcessingReport = false;

  constructor(private formBuilder: FormBuilder,
    private reportAIServiceApi: ReportAIService,
    private organizationServiceApi: OrganizationService,
    private cdr: ChangeDetectorRef, private modalService: NgbModal, private spinner: NgxSpinnerService,
    private facilityServiceApi: FacilityService, private userServiceApi: UserService, private reportAIService: ReportAIService,
    private sanitizer: DomSanitizer, private toastr: ToastrService, private router: Router,
    private authService: AuthService, private keywordAIReportService: KeywordAIReportService) {

    this.submitReportForm = this.formBuilder.group({
      pdfFile: new FormControl([], Validators.required),
      keywordFile: [],
      organization: new FormControl(null, Validators.required),
      facility: new FormControl(null, Validators.required),
      user: new FormControl(null, Validators.required)
    });

    this.isAuditor = this.authService.isUserInRole(RolesEnum.Auditor);
    this.isAdmin = this.authService.isUserInRole(RolesEnum.Admin);
    this.isReviewer = this.authService.isUserInRole(RolesEnum.Reviewer);
    this.isFacility = this.authService.isUserInRole(RolesEnum.Facility);

    this.reportForm = this.formBuilder.group({
      reportItems: new FormArray([])
    });

    this.reportFormByID = this.formBuilder.group({
      reportItems: new FormArray([])
    });
    this.subscriptionResult = this.resultAISubject$.asObservable().subscribe();
    this.subscriptionError = this.error$.asObservable().subscribe();
    this.startProcessing = false;
    this.reportAIService.getAppSettingsValue(HIDE_AI_SUMMARY)
      .pipe(first())
      .subscribe((value: string) => {
        if (value && value == "1" && !this.isAdmin) {
          this.hideAISummary = true;
        } else {
          this.hideAISummary = false;
        }
      });
    this.autoSaveSubscription = interval(30000).subscribe((val) => {
      if (this.resultAI != null && this.resultAI.jsonResult != "{}") {
        if (this.reportForm.dirty || this.reportFormByID.dirty) {
          this.reportForm.markAsPristine();
          this.reportFormByID.markAsPristine();
          this.autoSaveData();
        }
      }
    });
  }

  startAutoSave() {
    this.autoSaveSubscription = interval(30000).subscribe((val) => {
      if (this.resultAI != null && this.resultAI.jsonResult != "{}") {
        if (this.reportForm.dirty || this.reportFormByID.dirty) {

          this.reportForm.markAsPristine();
          this.reportFormByID.markAsPristine();
          this.autoSaveData();
        }
      }
    });
  }
  ngOnDestroy(): void {
    if (this.subscriptionResult != undefined)
      this.subscriptionResult.unsubscribe();

    if (this.subscriptionError != undefined)
      this.subscriptionError.unsubscribe();

    if (this.autoSaveSubscription != undefined)
      this.autoSaveSubscription.unsubscribe();

    if (this.checkOnlineSubscription != undefined)
      this.checkOnlineSubscription.unsubscribe();
  }
  ngAfterViewInit(): void {

  }

  ngOnInit(): void {
    this.organizations$ = this.organizationServiceApi.getOrganizationOptions();
    this.submitReportForm.get("user").setValue(null);
    this.submitReportForm.get("organization").setValue(null);
  }
  clearFacility() {
    this.facilities$ = null;
    this.submitReportForm.get("facility").patchValue('');
  }
  onOrganizationChanged(organization: IOption): void {
    this.clearFacility();


    if (!organization) {
      this.resultAI = null;
      this.resultAISubject$.next(null);
      this.users$ = null;
      this.submitReportForm.get("user").patchValue(null);
      return;
    }
    this.submitReportForm.get("organization").patchValue(organization);
    this.facilities$ = this.facilityServiceApi.getFacilityOptions(
      organization.id
    );
    this.users$ = this.userServiceApi.getUserOptions(this.organization.id);

  }
  public errors: any[];

  onUserChanged(user: IUserOption) {
    if (!user) {
      return;
    }

    this.submitReportForm.get("user").patchValue(user);
  }
  public onFacilityChanged(facility: IFacilityOption): void {

    if (!facility) {
      return;
    }
    this.submitReportForm.get("facility").patchValue(facility);
    if (this.errors && this.errors["FacilityId"]) {
      this.errors["FacilityId"] = null;
    }

  }
  public onKeywordPicked(event) {
    if (event.target.files && event.target.files[0]) {
      const reader = new FileReader();
      this.buidlWordIndex = "";
      this.currentStep = "";

      reader.onload = () => {
        this.submitReportForm.get('keywordFile').patchValue(event.target.files[0]);
      };
      reader.readAsDataURL(event.target.files[0]);

    }
  }
  public onPdfPicked(event) {
    if (event.target.files && event.target.files[0]) {
      const reader = new FileReader();
      this.foundKeywordText = "";
      this.buidlWordIndex = "";
      this.currentStep = "";

      reader.onload = () => {
        this.submitReportForm.get('pdfFile').patchValue(event.target.files[0]);
      };
      reader.readAsDataURL(event.target.files[0]);

    }
  }

  get error() {
    if (this.error$.getValue() == null)
      return;

    return this.error$.getValue();
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

    var regEx = new RegExp(value, "ig");
    var replaceMask = "<span style='background-color: lightskyblue'>" + value + "</span>";
    text = text.replace(regEx, replaceMask);
    return this.sanitizer.bypassSecurityTrustHtml(text);
  }

  get resultItemsArray() {
    return this.reportForm.controls["reportItems"] as FormArray;
  }

  get resultItemsByNameArray() {
    return this.reportFormByID.controls["reportItems"] as FormArray;
  }
  getResultForKeyword(key) {
    if (this.resultAISubject$.getValue() == null)
      return "";

    var j = JSON.parse(this.resultAISubject$.getValue().jsonResult);
    return j[key];
  }

  public createReport() {
    this.reportAIServiceApi.downloadUpdatedManualReport(this.resultAI,this.reportAIContent.id);
  }
  get organization(): any {
    return this.submitReportForm.get('organization').value
  }

  formsForKey(key, type: string) {
    let forms = [];

    if (type == "Resident") {
      this.resultItemsByNameArray.controls.forEach(group => {
        if (group.value?.SearchWord.toLowerCase() == key.toLowerCase()) {
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
      //console.log("get resident for key " + key + "  size " + forms.length);
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
          UserSummary: item.UserSummary != undefined ? item.UserSummary : "",
          SearchWord: item.SearchWord,
          PdfText: item.PdfText,
          Original: this.getFormattedText(item.SearchWord, item.PdfText)
        })
        formArray.push(itemForm);

      })

    })
  }

  get isStartDisable() {
    return this.buidlWordIndex == "" || (this.startProcessing);
  }

  separateIt(arr, size) {
  var newArr = [];
  for (var i = 0; i < arr.length; i += size) {
    var sliceIt = arr.slice(i, i + size);
    newArr.push(sliceIt);
  }
  return newArr;
}

  startSpinner() {
    this.spinner.show("processing", {
      type: "ball-elastic-dots",
      size: "medium",
      bdColor: "rgba(20,81,150,1)",
      color: "#fff",
    });
  }
  foundKeywordText: string;
  buidlWordIndex: string = "";
  time: string;
  date: string;
  currentStep: string;

  private buildFormFromParsingResult(keywords: any) {
 
    try {
      var j = JSON.parse(this.buidlWordIndex);
    } catch (e) {
      this.currentStep = "the pdf parsing has the error " + e;
      this.spinner.hide("processing");
      return;
    }

    this.startProcessing = false;
    this.currentStep = "Got result and preparing the table";

    this.spinner.hide("processing");

    this.resultAI = {
      date: this.date,
      time: this.time,
      auditDate:null,
      error: "",
      status: ReportAIStatusEnum.InProgress,
      user: this.submitReportForm.get("user").value?.fullName,
      keywords: keywords,
      containerName: "",
      facility: { id: this.submitReportForm.get("facility").value?.id, name: this.submitReportForm.get("facility").value?.name },
      organization: {id: this.submitReportForm.get("organization").value?.id, name: this.submitReportForm.get("organization").value?.name},
      reportFileName: this.submitReportForm.get('pdfFile').value?.name,
      jsonResult: this.buidlWordIndex
    }


  
    this.createFormsFromBuildIndex(this.resultAI.jsonResult, this.resultItemsArray);

    this.resultAISubject$.next(this.resultAI);

    this.cdr.detectChanges();
    this.SaveReportData();

  }
  createFormsFromBuildIndex(json: string, formArray: FormArray) {

    var j = JSON.parse(json);
    var keys = Object.keys(j);

    console.log(keys.length);

    keys.forEach(key => {
      j[key].forEach(item => {
        var itemForm = this.formBuilder.group({
          Acceptable: false,
          Name: item.Name,
          Summary: "",
          Date: item.Date,
          ID: item.ID,
          UUID:item.UUID,
          UserSummary: item.UserSummary != undefined ? item.UserSummary : "",
          SearchWord: item.keyword,
          PdfText: item.PdfText,
          Original: this.getFormattedText(item.keyword, item.PdfText)
          }
        )
        formArray.push(itemForm);

      })

    })
  }


  public submit() {
    this.foundKeywordText = "";
    this.buidlWordIndex = "";

    this.resultAI = null;

    if (!this.submitReportForm.valid) {
      this.toastr.error("Please select required fields");
      return;
    }

    this.startProcessing = true;

    this.isProcessingReport = true;

    this.isCallSaveFunction = false;

    this.resultItemsArray.clear();
    this.resultAISubject$.next(null);
    this.mapResultsByName.clear();
    this.resultItemsByNameArray.clear();
    this.error$.next("");


    const uploadData = new FormData();
    if (!this.submitReportForm.valid) {
      this.toastr.error("Please select required fields");
      return;
    }
    if (this.submitReportForm.get('pdfFile') != null) {
      let pdf = this.submitReportForm.get('pdfFile').value;
      uploadData.append('PdfFile', pdf, pdf?.name);
    } 
    if (this.submitReportForm.get('keywordFile') != null && this.submitReportForm.get('keywordFile').value != null) {
      let keyword = this.submitReportForm.get('keywordFile').value;
      uploadData.append('KeywordFileJson', keyword, keyword?.name);;
    }
    uploadData.append("OrganizationId", this.submitReportForm.get("organization").value?.id);
    uploadData.append("FacilityId", this.submitReportForm.get("facility").value?.id);
    this.error$.next("");
    this.buidlWordIndex = "";
    this.active = 1;
    const obs$: Observable<any>[] = [];
    this.startSpinner();

    this.currentStep = "Parsing and Analysing the " + this.submitReportForm.get('pdfFile')?.value?.name;
    obs$.push(this.reportAIServiceApi.uploadReportForAnalyse(uploadData));
    forkJoin(obs$).subscribe(result => {
      if (result != undefined && result.length > 0 && result[0].buildIndexJson != null) {
        
        this.foundKeywordText = "The  " + result[0].numberKeywordsFound + " keywords was found in the report.";
        this.buidlWordIndex = result[0].buildIndexJson;
        this.time = result[0].time;
        this.date = result[0].date;
        this.spinner.hide("processing");
       
        this.buildFormFromParsingResult(result[0].keywords);
        // this.sendToAI();
      } else if (result != undefined && result.length > 0){
        this.toastr.error(result[0].error);
        this.spinner.hide("processing");
      }
    },
    error => {
      console.log(error);
      this.handleError(error);
    });


  }
  private mapToObj(map) {
    var obj = {}
    map.forEach(function (v, k) {
      obj[k] = v
    })
    return obj;
  }

  public async sendToAI() {
    
    const obs$: Observable<any>[] = [];

    try {
      var j = JSON.parse(this.buidlWordIndex);
    } catch (e) {
      this.currentStep = "the pdf parsing has the error " + e;
      this.spinner.hide("processing");
      return;
    }


    //let keys = Object.keys(j);
    //var keysBy5 = this.separateIt(keys, 5);
   

    //for (let i = 0; i < keysBy5.length; i++) {
    //  this.spinner.show();
    //  let keyGroup = keysBy5[i];

    //  let map = new Map();
    //  keyGroup.forEach(key => {
    //    map.set(key, j[key]);
    //  });
    //  const uploadData = new FormData();
    //  if (this.submitReportForm.get('pdfFile') != null) {
    //    let pdf = this.submitReportForm.get('pdfFile').value;
    //    uploadData.append('PdfFile', pdf, pdf?.name);;
    //  }
    //  if (this.submitReportForm.get('keywordFile') != null && this.submitReportForm.get('keywordFile').value != null) {
    //    let keyword = this.submitReportForm.get('keywordFile').value;
    //    uploadData.append('KeywordFileJson', keyword, keyword?.name);;
    //  }
    //  uploadData.append("OrganizationId", this.submitReportForm.get("organization").value?.id);
    //  uploadData.append("FacilityId", this.submitReportForm.get("facility").value?.id);
    //  uploadData.append("BuidlWordIndex", JSON.stringify(this.mapToObj(map)));
    //  uploadData.append("keyword", keyGroup.toString());
    //  this.startSpinner()
    //  obs$.push(await this.reportServiceApi.uploadReportForProcess(uploadData));
    
    //}


    for (const [key, val] of Object.entries(j)) {
      try {
        const uploadData = new FormData();
        if (this.submitReportForm.get('pdfFile') != null) {
          let pdf = this.submitReportForm.get('pdfFile').value;
          uploadData.append('PdfFile', pdf, pdf?.name);;
        }
        if (this.submitReportForm.get('keywordFile') != null && this.submitReportForm.get('keywordFile').value != null) {
          let keyword = this.submitReportForm.get('keywordFile').value;
          uploadData.append('KeywordFileJson', keyword, keyword?.name);;
        }
        uploadData.append("OrganizationId", this.submitReportForm.get("organization").value?.id);
        uploadData.append("FacilityId", this.submitReportForm.get("facility").value?.id);

        let map = new Map();
        map.set(key, val);

        uploadData.append("BuidlWordIndex", JSON.stringify(this.mapToObj(map)));
        uploadData.append("keyword", key);
        this.currentStep = "Sending to AI the report with keywords  " ;
        obs$.push((await this.reportAIServiceApi.uploadReportForProcess(uploadData)));
      } catch (error) {
        console.log(error);
      }

    }

    forkJoin(obs$).subscribe(results => {

      this.startProcessing = false;
      this.currentStep = ""


      let mapResults: Map<string, any> = new Map();
      
      if (results.length > 0) {

        this.resultAI = results[0];
        results.forEach(result => {
          if (result.jsonResult != "{}" && result.jsonResult != null) {

            var j = JSON.parse(result.jsonResult);

            for (const [key, val] of Object.entries(j)) {
              if (mapResults.has(key)) {
                let arr = mapResults.get(key);
                arr.concat(val);
                mapResults.set(key, arr);
              }
              else {
                mapResults.set(key, val);
              }

            }
          }
        })

       
        this.resultAI.jsonResult = JSON.stringify(this.mapToObj(mapResults));
        this.createForms(this.resultAI.jsonResult, this.resultItemsArray);
        
        this.resultAISubject$.next(this.resultAI);

        this.sortGridType = "Keyword";
        this.SaveReportData();
      } else {
        this.toastr.info("AI has empty result for the report.");
      }

      console.log('All subscriptions done');

      this.isProcessingReport = false;
    },
    error => {
      this.handleError(error);

      this.startProcessing = false;
    });
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


    //  console.log(objectJson[keywordData.keyword]);
    this.resultAI.jsonResult = JSON.stringify(objectJson);


    this.resultAISubject$.next(this.resultAI);


    var itemForm = this.formBuilder.group({
      Acceptable: item.Acceptable,
      Name: item.Name,
      Summary: "",
      Date: item.Date,
      ID: item.ID,
      UUID:item.UUID,
      UserSummary: item.UserSummary,
      SearchWord: existInJson.length == 0 ? keywordData.keyword : existInJson[0],
      PdfText: "",
      Original: ""
    })



    if (this.sortGridType == "Resident") {
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
          let arr = map.get(searchW);
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
  onNavChange(changeEvent: NgbNavChangeEvent) {
    this.spinner.show("switch");
    if (changeEvent.nextId === 2) {
      this.sortGridType = "Resident";
      this.createByNameView();
    } else {
      this.sortGridType = "Keyword";
      this.createByKeywordView();
    }
    this.spinner.hide("switch");
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
    this.reportAIServiceApi.updateAIAuditReport(this.resultAI, this.reportAIContent.id, true)
      .pipe(first())
      .subscribe(
        (response) => {
          if (response != null) {
            this.reportAIContent = response;
           
            this.loadData(this.reportAIContent);
            this.status = this.keywordAIReportService.getStatus(response.status);

            this.isAIAuditSaved = true;
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
    this.reportAIServiceApi.setAIAuditStatus(this.reportAIContent.id, this.resultAI.status)
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
    if (this.resultAI.jsonResult == "{}") {
      this.toastr.error("Error while saving AI Audit data");
      return;
    }

    this.reportAIServiceApi.updateAIAuditReport(this.resultAI, this.reportAIContent.id, true)
      .pipe(first())
      .subscribe(
        (response) => {
          if (response != null) {
            this.reportAIContent = response;
            this.isAIAuditSaved = true;
            this.loadData(this.reportAIContent);
            if (showToast == true)
              this.toastr.success("AI Audit data saved successully");
          } else {
            if (showToast == true)
              this.toastr.error("Error while saving AI Audit data");
          }
          this.cdr.detectChanges();
        },
        (error: HttpErrorResponse) => {
          console.log(error);
        }
      );
  }

  autoSaveData() {
    if (this.reportAIContent?.id == undefined)
      return;

    if (this.reportAIContent.status == ReportAIStatusEnum.Submitted)
      return;

    console.log("Running Auto Save at " + new Date().toLocaleTimeString());
    if (this.resultAI.jsonResult == "{}") {
      return;
    }

    var j = JSON.parse(this.resultAI.jsonResult);
    var keys = Object.keys(j);
    var map = new Map<string, any[]>();

    keys.forEach(key => {
      let arr = [];
      this.formsForKey(key, this.sortGridType).forEach(f => {
        arr.push(f.value);
      })
      if (arr.length > 0)
        map[key] = arr;
    })
    this.resultAI.jsonResult = JSON.stringify(map).toString();

    this.reportAIServiceApi.updateAIAuditReport(this.resultAI, this.reportAIContent.id, false)
      .pipe(first())
      .subscribe(
        (response) => {
          this.reportAIContent = response;
          this.isAIAuditSaved = true;
          this.loadData(this.reportAIContent);

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


  resetReports() {
    if (this.resultItemsArray) {
      this.resultItemsArray.controls.forEach(group => {
        group.patchValue({ Acceptable: 1 });
      });
    }
    if (this.resultItemsByNameArray) {
      this.resultItemsByNameArray.controls.forEach(group => {
        group.patchValue({ Acceptable: 1 });
      });
    }
  }

  public async SaveReportData() {
    this.resultAI.organization = this.submitReportForm.get("organization").value;
    this.resultAI.facility = this.submitReportForm.get("facility").value;

    this.currentStep = "Saving Report Data";



    var j = JSON.parse(this.resultAI.jsonResult);
    var keys = Object.keys(j);

    var map = new Map<string, any[]>();

     keys.forEach(key => {
        let arr = [];
        this.formsForKey(key, "Keyword").forEach(f => {
          arr.push(f.value);
        })
        if (arr.length > 0)
          map[key] = arr;
     })
    
    this.resultAI.user = this.submitReportForm.get("user").value?.fullName;
    this.resultAI.jsonResult = JSON.stringify(map).toString();
    this.resultAI.date = this.date;
    this.resultAI.time = this.time;

    if (this.resultAI.jsonResult == "{}") {
      this.toastr.error("Error while saving report data");
      return;
    }


    this.reportAIServiceApi.saveReportAIData(this.resultAI, false)
      .subscribe(
        (aiAudit) => {
          if (aiAudit != null) {
            console.log(aiAudit);
              this.reportAIContent = aiAudit;
              this.isAIAuditSaved = true;

              this.loadData(this.reportAIContent);

              this.setActions(this.reportAIContent.status);

              this.status = this.keywordAIReportService.getStatus(this.reportAIContent.status);

              if (this.reportAIContent.status == ReportAIStatusEnum.Submitted) {
                this.isShowExportPdf = true;
                this.isDisabled = true;           
              }
              this.cdr.detectChanges();

          } else {
            this.toastr.error("Error while saving report data");
          }
        },
        (error: HttpErrorResponse) => {
          console.log(error);
          this.toastr.error("Error while saving report data");
        }
      );

  }

  private loadData(data: IReportAIContent) {
    if (data == undefined)
      return;

    console.log(data);
    this.resultAI.auditDate = data.auditDate;
    this.resultAI.date = moment(data.auditDate).format("MMM-DD, YYYY");
    this.resultAI.time = data.auditTime;
    this.resultAI.user = data.auditorName;
    this.resultAI.status = data.status;
    this.resultAI.facility = { id: this.submitReportForm.get("facility").value?.id, name: this.submitReportForm.get("facility").value?.name };
    this.resultAI.organization = { id: this.submitReportForm.get("organization").value?.id, name: this.submitReportForm.get("organization").value?.name };
    this.resultAI.reportFileName = this.submitReportForm.get('pdfFile').value?.name;
    this.resultAI.keywords = data.keywords;
    this.resultAI.containerName = data.containerName;


    //this.resultItemsArray.clear();
    //this.resultItemsByNameArray.clear();


    //if (this.sortGridType == "Resident")
    //  this.createForms(this.resultAI.jsonResult, this.resultItemsByNameArray);
    //else
    //  this.createForms(this.resultAI.jsonResult, this.resultItemsArray);
    this.resultAISubject$.next(this.resultAI);
    this.cdr.detectChanges();


  }

  private handleError(error: HttpErrorResponse) {
    if (error.status === 0 && error.error instanceof ProgressEvent) {
      // A client-side or network error occurred. Handle it accordingly.
      console.log('Client side error:', error?.error)
    } else {
      this.toastr.error(error?.error);
    }
  };
}
  
