import { HttpErrorResponse } from "@angular/common/http";
import { AfterViewInit, Component, ElementRef, Input, OnInit, ViewChild, ViewEncapsulation } from "@angular/core";
import { FormBuilder, FormControl, FormGroup, Validators } from "@angular/forms";
import { first } from "rxjs";
import { splitString } from "src/app/common/helpers/string-helper";
import { IKeyword, IKeywordTrigger, IOption } from "src/app/models/audits/audits.model";
import { FormStatuses, IFormVersion } from "src/app/models/forms/forms.model";
import { FormServiceApi } from "src/app/services/form-api.service";
import { IDefaultFilters } from "../../common/types/types";
import { FormService } from "../services/form.service";

@Component({
  selector: "app-keyword-form",
  templateUrl: "./keyword-form.component.html",
  styleUrls: ["./keyword-form.component.scss"],
  encapsulation: ViewEncapsulation.None,
})

export class KeywordFormComponent implements OnInit {
  @ViewChild("keywordInput") keywordInputRef: any;
  @ViewChild("useTrigger") userTriggerRef: any;
  private _formVersion: IFormVersion;
  public _keywords: IKeywordTrigger[] = [];

  public keywordForm : FormGroup;
  public inputKeyword: string;

  public selectedKeyword: IKeywordTrigger = null;
  public forms: IOption[];

  public selectedTriggeredForms: IOption[];
  public searchTerm: string;

  public errors: any[];

  public errorMessage: string;

  @Input() public set formVersion(value: IFormVersion) {
    this._formVersion = value;
    if (this._formVersion?.status === this.formStatuses.Draft.id) {
      this.keywordForm.enable();
    } else {
      this.keywordForm.disable();
    }
  }

  @Input() public set keywords(value: IKeywordTrigger[]) {
    this._keywords = value ?? [];
    this.keywordForm.enable();
  }

  public formStatuses = FormStatuses;

  public validators = [this.validateDuplicates.bind(this)];

  public errorMessages = {
    'duplicate': 'Duplicated keyword',
  };

  constructor(
    private formServiceApi: FormServiceApi,
    private formService: FormService,
    private formBuilder: FormBuilder
  ) {
 
    this.keywordForm = formBuilder.group({
      keyword: new FormControl({ value: [], disabled: false }, Validators.required),
      trigger: new FormControl({ value: false, disabled: false }),
      formAudits: new FormControl({ value: [], disabled: false })
    });
  }

  ngOnInit() {
    if(this._formVersion?.status === this.formStatuses.Draft.id) {
      this.keywordForm.enable();
    } else {
      this.keywordForm.disable();
    }
  }

  public get formVersion(): IFormVersion {
    return this._formVersion;
  }

  public get submitButtonTitle(): string {
    return this.selectedKeyword?.id ? 'Update' : 'Add';
  }

  public get keywordsList(): IKeywordTrigger[] {
    if(!this._keywords) {
      return;
    }

    if(this.searchTerm) {
      return this._keywords.filter((keyword: IKeywordTrigger) => keyword.name.toLowerCase().includes(this.searchTerm.toLowerCase()));
    }

    return this._keywords;
  }
  private getKeyword():string {
    const keywords: any[] = this.keywordForm.value.keyword;

    let keywordStr: string = '';

    if (keywords) {
      keywordStr = keywords/*.map((keyword)=> keyword.display)*/.join("-");
    }

    if (this.inputKeyword) {
      keywordStr = keywordStr ? `${keywordStr}-${this.inputKeyword}` : this.inputKeyword;
    }

    if (!keywordStr) {
      return "";
    }
    return keywordStr;
  }
  public saveKeyword() {
    const keywords: any[] = this.keywordForm.value.keyword;

    let keywordStr: string = '';

    if(keywords) {
      keywordStr = keywords/*.map((keyword)=> keyword.display)*/.join(" / ");
    }

    if(this.inputKeyword) {
      keywordStr = keywordStr ? `${keywordStr} / ${this.inputKeyword}` : this.inputKeyword;
    }

    if(!keywordStr) {
      return;
    }

    const errorMesage = this.validateAllDuplicates(keywordStr, this._keywords, this.selectedKeyword?.id);

    if(errorMesage) {
      this.errorMessage = errorMesage;
      return;
    }
   
    let trigger = this.keywordForm.value.trigger;
    let formIds = this.selectedTriggeredForms != undefined ? this.selectedTriggeredForms : [];
    if (!this.selectedKeyword) {
      this.formServiceApi.addFormKeyword(this._formVersion.id, keywordStr, trigger, formIds )
      .pipe(first())
      .subscribe({
        next: (keyword: IKeywordTrigger) => this.handleAddResponseSuccess(keyword),
        error: (response: HttpErrorResponse) => this.handleResponseError(response)
      });
    } else {
      this.formServiceApi.editFormKeyword(this.selectedKeyword.id, this._formVersion.id, keywordStr, trigger, formIds)
      .pipe(first())
      .subscribe({
        next : (result: boolean) => {
          if(result) {
            this.handleEditResponseSuccess(keywordStr, trigger, formIds);
          }},
        error: (response: HttpErrorResponse) => this.handleResponseError(response)
      });
    }
  }

  public onClearClick(): void {
    this.clearForm();
  }

  public onKeywordDeleteClick(id: number): void {
    if(this.formVersion?.status !== FormStatuses.Draft.id) {
      return;
    }

    this.formServiceApi.deleteColumnKeyword(id)
    .pipe(first())
    .subscribe((result: boolean) => {
      if(result){
        this.handleDeleteResponseSuccess(id);
      }
    });
  }

  public onSearchClear(): void {
    this.searchTerm = null;
  }

  public onKeywordClick(keyword: IKeywordTrigger) {
    if(this.formVersion?.status !== FormStatuses.Draft.id) {
      return;
    }
    console.log(keyword);
    this.selectedKeyword = keyword;

    const splittedKeywords = splitString(keyword.name, "/");
    let forms = this.selectedKeyword?.id ? this.selectedKeyword.formsTriggeredByKeyword :  this.selectedTriggeredForms ;
    this.keywordForm.setValue({
      keyword: splittedKeywords/*.map((keyword: string) => {
        return {display: keyword, value: keyword}
      })*/,
      trigger: keyword.trigger,
      formAudits: forms
    });
   
  }
  public onSearchChange(search: any): void {
   
    if (search == undefined)
      return;

    this.forms = this.forms.filter(x => x.name.toLowerCase().indexOf(search.toLowerCase()) >= 0);
   
  }
  public onTextChange(inputKeyword: string) {
    this.inputKeyword = inputKeyword;

    this.errorMessage = null;
  }

  public onTagEdited(keyword) {
    this.errorMessage = null;
}


  public onSeparatorButtonClick() {
    const event = new KeyboardEvent("keydown",{
      "key": "Enter"
    });

    this.keywordInputRef.inputForm.input.nativeElement.dispatchEvent(event);
  }

  private clearForm(): void {
    if(this.keywordInputRef) {
      this.keywordInputRef.inputForm.form.reset();
    }

    this.selectedKeyword = null;
    this.keywordForm.reset();
    this.errorMessage = null;
  }


  private handleAddResponseSuccess(keyword: IKeywordTrigger) {
 
    this._keywords.push(keyword);
    this._keywords.sort(function (a: IKeywordTrigger, b: IKeywordTrigger) {
      return a.name.toLowerCase().localeCompare(b.name.toLowerCase());
    });

    this.clearForm();
    this.keywordInputRef.inputForm.input.nativeElement.focus();
  }

  private handleEditResponseSuccess(keywordStr, trigger, formIds) {
    console.log(formIds);

    let itemIndex = this._keywords.findIndex(item => item.id == this.selectedKeyword.id);
    this._keywords[itemIndex].name = keywordStr;
    this._keywords[itemIndex].trigger = trigger;
    this._keywords[itemIndex].formsTriggeredByKeyword = trigger === false ? [] : formIds;

    this.clearForm();
  }
  private getTriggeredFormsName(keyword: IKeywordTrigger) {
    return keyword.trigger != null && keyword.trigger === false ? "" : keyword.formsTriggeredByKeyword?.map(x => x.name).join("\r\n"); 
  }

  private keywordTrigger(keyword: IKeywordTrigger) {
    return keyword.trigger != null && keyword.trigger === true ? "Trigger" : "";
  }
  private handleDeleteResponseSuccess(id: number){
    this.keywords = this._keywords.filter((keyword: IKeywordTrigger) => keyword.id !== id);

    if(this.selectedKeyword?.id === id) {
      this.clearForm();
    }
  }

  private handleResponseError(response: HttpErrorResponse){
    this.errors = response.error?.errors;
    this.errorMessage = response.error?.errorMessage;
    console.log(response);
  }

  private validateDuplicates(control: FormControl) {
    let formKeywords: string[] = [];

    this._keywords
    ?.filter((ks:IKeyword) => ks.id !== this.selectedKeyword?.id)
    .map((ks:IKeyword) => ks.name.split('/').map((k:string) => formKeywords.push(k.trim().toLowerCase())));

    if (formKeywords.includes(control.value)) {
        return {
            'duplicate': true
        };
    }

    return null;
  }

  private validateAllDuplicates(currentKeyword: string, allKeywords: IKeywordTrigger[], currentKeywordId?: number): string {
    let formKeywords: string[] = [];

    allKeywords?.filter((ks: IKeywordTrigger) => ks.id !== currentKeywordId)
    .map((ks:IKeyword) => ks.name.split('/').map((k:string) => formKeywords.push(k.trim().toLowerCase())));

    const currentKeywords: string[] = currentKeyword?.split('/').map((k:string) => k.trim().toLowerCase());

    let duplicatedKeywords: string[] = [];

    currentKeywords.forEach((keyword: string) => {
      if(formKeywords.includes(keyword)) {
        duplicatedKeywords.push(keyword);
      } else {
        formKeywords.push(keyword);
      }
    });

    if (duplicatedKeywords.length > 0) {
      return `Duplicated keywords: ${duplicatedKeywords.join(", ")}`;
    }

    return null;
  }

  public onFormAuditChanged(formAudit: IOption[]): void {
    if (formAudit?.length > 0) {
      if (this.selectedKeyword != null)
        this.selectedKeyword.formsTriggeredByKeyword = formAudit;
      else
        this.selectedTriggeredForms = formAudit;
    }
  }

  public onTriggeredFromDropdownOpened(): void {
    let key = this.getKeyword(); 
    this.formServiceApi
      .getFormsForKeywordTrigger(this.formVersion.organization.id, key != "" ? key : "none")
      .subscribe((forms) => {
        this.forms = forms;
      });
  }

}
