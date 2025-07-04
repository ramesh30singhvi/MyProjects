import { Component, EventEmitter, Input, OnInit, Output, ViewEncapsulation } from "@angular/core";
import { FormGroup } from "@angular/forms";
import { Toolbar } from "ngx-editor";
import { COLOR_PRESETS } from "src/app/common/constants/audit-constants";
import { FieldBase } from "src/app/models/audits/questions.model";

@Component({
  selector: "app-dynamic-form-control",
  templateUrl: "./dynamic-form-control.component.html",
  styleUrls: ["./dynamic-form-control.component.scss"],
  encapsulation: ViewEncapsulation.None
})

export class DynamicFormControlComponent implements OnInit {
  @Input() public fields!: FieldBase<any>;
  @Input() public form!: FormGroup;
  @Input() public columns: number = 1;
  @Input() public showHighAlert: boolean = false;
  @Output() highAlertChanged = new EventEmitter<boolean>();
  @Output() residentNameChanged = new EventEmitter<string>();
  @Input()  public useHighAlert: boolean = false;

  public toolbar: Toolbar = [
    ['bold', 'italic', 'underline'],
    ['ordered_list', 'bullet_list'],
    ['text_color', 'background_color']
  ];

  public colorPresets = COLOR_PRESETS;
  
  constructor() { 

  }

  ngOnInit() {
    
  }
  
  public getColumnsClass(): string {
    if (this.columns==2) return "row-cols-2";
    return "";
  }

  public onHighAlertChanged(event) {
    this.useHighAlert = event.target.checked;
    this.highAlertChanged?.emit(this.useHighAlert);
  }
  onChangeField(label: string, value: any) {
    if (label == "")
      return;

    if (label == null)
      return;

    if (label == undefined)
      return;

    if (label.toLowerCase() == "resident name" || label.toLowerCase() == "resident" || label.toLowerCase().indexOf("resident") != -1) {
      this.residentNameChanged?.emit(value);
    }
  }
}
