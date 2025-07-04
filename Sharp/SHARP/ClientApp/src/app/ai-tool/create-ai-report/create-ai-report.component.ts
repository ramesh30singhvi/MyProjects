import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NgbAccordion, NgbModal, NgbNavChangeEvent } from '@ng-bootstrap/ng-bootstrap';
import * as moment from 'moment';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject, concatMap, forkJoin, from, Observable } from 'rxjs';
import { MM_DD_YYYY_HH_MM_SLASH } from '../../common/constants/date-constants';
import { IFacilityOption, IOption, IUserOption } from '../../models/audits/audits.model';
import { AIProgressNotes, AIServiceRespond, PCCNotes } from '../../models/reports/reportAI.model';
import { AddKeywordReportComponent } from '../../reports/add-keyword-report/add-keyword-report.component';
import { FacilityService } from '../../services/facility.service';
import { OrganizationService } from '../../services/organization.service';
import { ReportAIService } from '../../services/reportAI.service';
import { UserService } from '../../services/user.service';


@Component({
  selector: 'app-create-ai-report',
  templateUrl: './create-ai-report.component.html',
  styleUrls: ['./create-ai-report.component.scss']
})
export class CreateAiReportComponent implements OnInit {
  public submitReportForm: FormGroup;
  @ViewChild('acc') accordion: NgbAccordion;
  organizations$: Observable<IOption[]>;
  facilities$: Observable<IFacilityOption[]>;
  users$: Observable<IUserOption[]>;
  isEditable: boolean ;
  public startProcessing: boolean;
  public error$ = new BehaviorSubject<any>(null);



  constructor(private formBuilder: FormBuilder, private cdr: ChangeDetectorRef, private modalService: NgbModal,
    private reportAIServiceApi: ReportAIService, private toastr: ToastrService, private router: Router,
    private facilityServiceApi: FacilityService, private userServiceApi: UserService,
    private organizationServiceApi: OrganizationService) {

    this.submitReportForm = this.formBuilder.group({
      pdfFile: new FormControl([], Validators.required),
      organization: new FormControl(null, Validators.required),
      facility: new FormControl(null, Validators.required),
      user: new FormControl(null, Validators.required)
    });


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
  get organization(): any {
    return this.submitReportForm.get('organization').value
  }
  onOrganizationChanged(organization: IOption): void {
    this.clearFacility();


    if (!organization) {
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

  public onPdfPicked(event) {
    if (event.target.files && event.target.files[0]) {
      const reader = new FileReader();

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


  public submit() {

    if (!this.submitReportForm.valid) {
      this.toastr.error("Please select required fields");
      return;
    }

    this.startProcessing = true;
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

    uploadData.append("OrganizationId", this.submitReportForm.get("organization").value?.id);
    uploadData.append("FacilityId", this.submitReportForm.get("facility").value?.id);
    uploadData.append("User", this.submitReportForm.get("user").value?.fullName);

    this.error$.next("");
    const obs$: Observable<any>[] = [];
    obs$.push(this.reportAIServiceApi.uploadReportForAnalyseV2(uploadData));
    forkJoin(obs$).subscribe(result => {
      if (result != undefined ) {
        let id = result[0];
        if (id > 0) {


          this.router.navigate(['aitool/editAIAudit/' + id]);
        }
        else {
          this.toastr.error("Something went wrong. Send report to Inessa");
        }
      }
    },
      error => {
        console.log(error);
        // this.handleError(error);
      });


  }

}
