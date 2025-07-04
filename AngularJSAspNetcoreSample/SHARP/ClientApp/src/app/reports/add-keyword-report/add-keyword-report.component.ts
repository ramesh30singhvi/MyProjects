import { AfterViewInit, Component, ElementRef, EventEmitter, Input, OnDestroy, OnInit, Output, Renderer2 } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal, NgbDate, NgbDateAdapter, NgbDateParserFormatter, NgbTimeAdapter, NgbTimepickerConfig } from '@ng-bootstrap/ng-bootstrap';
import * as moment from 'moment';
import { NgbTimeStringAdapter } from '../../audits/keyword-input-section/keyword-input-section.component';
import { MM_DD_YYYY_DOT } from '../../common/constants/date-constants';
import { CustomDateParserAdapter, CustomDateParserFormatter } from '../../shared/datepicker-adapters';

@Component({
  selector: 'app-add-keyword-report',
  templateUrl: './add-keyword-report.component.html',
  styleUrls: ['./add-keyword-report.component.scss'],
  providers: [
    { provide: NgbDateAdapter, useClass: CustomDateParserAdapter },
    { provide: NgbDateParserFormatter, useClass: CustomDateParserFormatter },
    NgbTimepickerConfig,
    { provide: NgbTimeAdapter, useClass: NgbTimeStringAdapter }
  ],
})
export class AddKeywordReportComponent implements  OnInit, AfterViewInit, OnDestroy {

  @Output() newKeyword: EventEmitter<any> = new EventEmitter();


  private defaultDate: string = '';
  public errors: any[];
  public minDate: NgbDate | null;
  public maxDate: NgbDate | null;
  private hourElement: any;
  private prevHour: number;
  private minuteElement: any;
  public keywordForm: FormGroup;
  constructor(public activeModal: NgbActiveModal, private formBuilder: FormBuilder,
    private renderer: Renderer2,
    private timePickerConfig: NgbTimepickerConfig, private elem: ElementRef) {
    this.timePickerConfig.seconds = false;
    this.timePickerConfig.spinners = false;

    var d = new Date();
    this.defaultDate = moment(d).format(MM_DD_YYYY_DOT);
   
    const minDateMoment = moment(d.getDate() - 5);
    const maxDateMoment = moment(d);

    this.minDate = new NgbDate(minDateMoment.year(), minDateMoment.month() + 1, minDateMoment.date());
    this.maxDate = new NgbDate(maxDateMoment.year(), maxDateMoment.month() + 1, maxDateMoment.date());

    this.keywordForm = this.formBuilder.group({
      resident: new FormControl({ value: '' }, Validators.required),
      customKeyword: new FormControl({ value: '' }, Validators.required ),
      date: new FormControl({ value: '' }, Validators.required ),
      time: new FormControl({ value: '' }, Validators.required ),
      report: new FormControl({ value: '' }, Validators.required)
    });
    this.keywordForm?.setValue({
      resident: '',
      date: this.defaultDate,
      time:  '',
      report:'',
      customKeyword: ''
    });

  }

  ngAfterViewInit() {
    this.hourElement = this.elem.nativeElement.querySelector('[placeholder="HH"]');
    this.minuteElement = this.elem.nativeElement.querySelector('[placeholder="MM"]');

    this.unlistener = this.renderer.listen(this.hourElement, "input", event => {
      this.timeChanged(event.target.value);
    });
  }

  private unlistener: () => void;

  ngOnDestroy(): void {

    this.unlistener();
  }
  ngOnInit(): void {
  }

  public onAddKeyword() {
    if (this.keywordForm.invalid)
      return;

    var keyword= this.keywordForm.controls['customKeyword'].value;
    var resident = this.keywordForm.controls['resident'].value;
    var date = this.keywordForm.controls['date'].value;
    var time = this.keywordForm.controls['time'].value;

    var report = this.keywordForm.controls['report'].value;
    keyword = keyword.trim();
   
    this.activeModal.close({ keyword, resident, date, time, report });
  

  }
  public timeChanged(value: string): void {
    if (this.hourElement !== document.activeElement || !value) {
      return;
    }

    const hour = Number.parseInt(value.substring(0, 2));

    if (hour > 2) {
      this.minuteElement.focus();
      this.minuteElement.select();
    }
  }

}
