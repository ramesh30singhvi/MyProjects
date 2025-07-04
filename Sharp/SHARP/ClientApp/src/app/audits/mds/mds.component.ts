import { Component, Input, OnInit, ViewEncapsulation } from "@angular/core";
import { FormGroup } from "@angular/forms";
import { FieldBase } from "src/app/models/audits/questions.model";
import { Audit } from "src/app/models/audits/audits.model";

export interface SectionForm {
  name: string;
  groups: GroupForm[];
}

export interface GroupForm {
  name: string;
  form: FormGroup;
  questions: FieldBase<any>[];
}

@Component({
  selector: 'app-mds',
  templateUrl: "./mds.component.html",
  styleUrls: ["./mds.component.scss"],
  encapsulation: ViewEncapsulation.None
})
export class MDSComponent implements OnInit {

  public _sectionForms: SectionForm[] = [];

  private _audit: Audit;

  public isEditable: boolean;
  
  @Input() public set sectionForms(value: SectionForm[]) {
    this._sectionForms = value;
  }

  @Input() public set audit(value: Audit) {
    this._audit = value;
  }

  public get audit(): Audit {
    return this._audit;
  }

  public get sectionForms(): SectionForm[] {
    return this._sectionForms;
  }

  constructor() {}
  ngOnInit(): void {
  }


}
