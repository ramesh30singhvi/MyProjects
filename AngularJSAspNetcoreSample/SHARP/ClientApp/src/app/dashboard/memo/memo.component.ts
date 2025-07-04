import { Component, Input, OnInit } from "@angular/core";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import * as moment from "moment";
import { first, Observable, Subscription } from "rxjs";
import { M_D_YYYY_SLASH } from "src/app/common/constants/date-constants";
import { IOption, IUserOption } from "src/app/models/audits/audits.model";
import { IMemo } from "src/app/models/memos/memos.model";
import { RolesEnum } from "src/app/models/roles.model";
import { AuthService } from "src/app/services/auth.service";
import { MemoServiceApi } from "src/app/services/memo-api.service";
import { MemoEditComponent } from "./memo-edit/memo-edit.component";

@Component({
  selector: "app-memo",
  templateUrl: "./memo.component.html",
  styleUrls: ["./memo.component.scss"]
})

export class MemoComponent implements OnInit {
  public memos: IMemo[];
  
  @Input() organizations: IOption[];

  private _selectedOrganizations: IOption[];
  @Input() public set selectedOrganizations(value: IOption[]){
    this._selectedOrganizations = value;

    this.getMemos(this._selectedOrganizations);
  };

  public canAddMemo: boolean;

  public isAdmin: boolean;
  public isReviewer: boolean;

  private subscription: Subscription;
  
  constructor(
    private memoServiceApi: MemoServiceApi,
    private modalService: NgbModal,
    private authService: AuthService,
  ) { 
    this.subscription = new Subscription();

    this.isAdmin = this.authService.isUserInRole(RolesEnum.Admin);
    this.isReviewer = this.authService.isUserInRole(RolesEnum.Reviewer);

    this.canAddMemo = this.isAdmin || this.isReviewer;
  }

  ngOnInit() {
    this.getMemos();
  }

  public canEditMemo(memo: IMemo): boolean {
    if(this.isAdmin) {
      return true;
    }

    if(this.isReviewer) {
      return memo.user?.id === this.authService.getCurrentUserSharpId();
    }

    return false;
  }

  public onAddMemoClick() {
    const modalRef = this.modalService.open(MemoEditComponent, { modalDialogClass: 'custom-modal edit-memo-modal-dialog', centered: true });
    modalRef.componentInstance.title = "Add Memo";
    //modalRef.componentInstance.organizations = [...this.organizations];

    modalRef.result.then(
      (memo: IMemo) => {
        this.memos.unshift(memo);
      },
      () => { }
    );
  }

  public onEditMemoClick(editMemo: IMemo) {
    const modalRef = this.modalService.open(MemoEditComponent, { modalDialogClass: 'custom-modal edit-memo-modal-dialog' });
    modalRef.componentInstance.title = "Edit Memo";
    //modalRef.componentInstance.organizations = [...this.organizations];
    modalRef.componentInstance.editMemo = editMemo;

    modalRef.result.then(
      (memo: any) => {
        if(memo?.isDeleted) {
          this.memos = this.memos.filter((m: IMemo) => m.id !== memo.id);
          return;
        }

        const memoIndex = this.memos.findIndex((m: IMemo) => m.id === memo.id);

        if(memoIndex >= 0) {
          this.memos[memoIndex] = memo;
        }
      },
      () => { }
    );
  }

  public getUserInitials(user: IUserOption): string {
    return `${user?.firstName?.charAt(0)}${user?.lastName?.charAt(0)}`
  }

  public getOrganizationsString(organizations: IOption[]): string {
    return organizations?.map((option: IOption) => option.name).join(" | ")
  }

  public formatCreatedDate(date: Date): string {
    return moment(date).format(M_D_YYYY_SLASH)
  }

  public onRefreshMemoClick(): void {
    this.getMemos();
  }

  private getMemos(selectedOrganizations: IOption[] = null): void {
    this.memoServiceApi
    .getMemos(selectedOrganizations?.map((option: IOption) => option.id))
    .pipe(first())
    .subscribe((data: IMemo[]) => {
      this.memos = data;
    });
  }
}
