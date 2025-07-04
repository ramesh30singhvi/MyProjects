import { Component, Input, ViewEncapsulation } from "@angular/core";
import {
  NgbActiveModal,
  NgbDate,
  NgbDateParserFormatter,
} from "@ng-bootstrap/ng-bootstrap";
import { UserFiltersModel } from "src/app/models/users/users.model";

import { UserService } from "src/app/services/user.service";

export enum UserProductivityReportType {
  Single = "single",
  Multiple = "multiple",
}

@Component({
  selector: "app-productivity-user",
  templateUrl: "./user-productivity.component.html",
  styleUrls: ["./user-productivity.component.scss"],
  encapsulation: ViewEncapsulation.None,
})
export class UserProductivityComponent {
  @Input() userId: number | undefined;
  @Input() type: UserProductivityReportType;
  @Input() userFilterValues: UserFiltersModel | undefined = undefined;
  @Input() filterModel: any | undefined = undefined;

  public fromDate: NgbDate | null;
  public toDate: NgbDate | null;
  public maxDate: NgbDate = new NgbDate(
    new Date().getFullYear(),
    new Date().getMonth() + 1,
    new Date().getDate()
  );

  public hoveredDate: NgbDate | null = null;

  constructor(
    public formatter: NgbDateParserFormatter,
    public activeModal: NgbActiveModal,
    public userService: UserService
  ) {}

  public onClose(): void {
    this.activeModal.dismiss();
  }

  public onDateSelection(date: NgbDate) {
    if (!this.fromDate && !this.toDate) {
      this.fromDate = date;
    } else if (
      this.fromDate &&
      !this.toDate &&
      date &&
      (date.after(this.fromDate) || date == this.fromDate)
    ) {
      this.toDate = date;
    } else {
      this.toDate = null;
      this.fromDate = date;
    }
  }

  public isHovered(date: NgbDate) {
    return (
      this.fromDate &&
      !this.toDate &&
      this.hoveredDate &&
      date.after(this.fromDate) &&
      date.before(this.hoveredDate)
    );
  }

  public isInside(date: NgbDate) {
    return this.toDate && date.after(this.fromDate) && date.before(this.toDate);
  }

  public isRange(date: NgbDate) {
    return (
      date.equals(this.fromDate) ||
      (this.toDate && date.equals(this.toDate)) ||
      this.isInside(date) ||
      this.isHovered(date)
    );
  }

  public rangeFormat(dateFrom: NgbDate | null, dateTo: NgbDate | null): string {
    let dateRange: string = "";

    if (dateFrom && dateTo) {
      dateRange = `${this.formatter.format(dateFrom)} - ${this.formatter.format(
        dateTo
      )}`;
    } else {
      dateRange = this.formatter.format(dateFrom);
    }

    return dateRange;
  }

  public onDownloadClick() {
    this.activeModal.dismiss();
    this.userService.downloadReport({
      fromDate: this.formatter.format(this.fromDate),
      toDate: this.formatter.format(this.toDate),
      userId: this.userId,
      type: this.type,
      filterModel: this.filterModel,
      userFilterValues: this.userFilterValues,
    });
  }
}
