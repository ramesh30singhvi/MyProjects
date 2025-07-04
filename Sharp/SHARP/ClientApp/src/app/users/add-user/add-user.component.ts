import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { OrganizationService } from 'src/app/services/organization.service';
import { UserService } from '../../services/user.service';
import { AddEditUserBase } from '../add-edit-user-base';
import { FacilityService } from 'src/app/services/facility.service';
import { ActionTypeEnum } from "src/app/models/users/users.model";

@Component({
  selector: "app-add-user",
  templateUrl: "./add-user.component.html",
  styleUrls: ["./add-user.component.scss"],
})
export class AddUserComponent extends AddEditUserBase {
  constructor(
    public activeModal: NgbActiveModal,
    public userService: UserService,
    public organizationService: OrganizationService,
    public facilityService: FacilityService,
  ) {
    super(userService, organizationService, facilityService, activeModal);
  }

  public onSave(): void {
    this.userService.createUser(this.mapUserData()).subscribe(
      (data) => {
        try {
          if (data && data > 0) {
            this.userService.addUserActivityLog(ActionTypeEnum.NewAccount, null, data).subscribe();
          }
        } catch (error) {
          console.log(error);
        }
        this.activeModal.close();
      },
      (response: HttpErrorResponse) => {
        this.errors = response.error.errors;
        this.errorMessage = response.error.errorMessage;
      }
    );
  }
}
