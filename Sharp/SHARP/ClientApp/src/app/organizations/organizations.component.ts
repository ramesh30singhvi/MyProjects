import { Component, OnInit, ViewChild } from '@angular/core';
import { Title } from "@angular/platform-browser";
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AddOrganizationComponent } from './add-organization/add-organization.component';
import { OrganizationService } from '../services/organization.service';
import { OrganizationDetailed } from '../models/organizations/organization.detailed.model';
import { EditOrganizationComponent } from './edit-organization/edit-organization.component';
import { FacilitiesComponent } from './facilities/facilities.component';
import { IOption } from '../models/audits/audits.model';

@Component({
  selector: "app-organizations",
  templateUrl: "./organizations.component.html",
  styleUrls: ["./organizations.component.scss"],
})
export class OrganizationsComponent implements OnInit {

  public searchTerm: string;
  organizations: Array<OrganizationDetailed> = [];
  portalFeatures:IOption[]
  shownSectionName: string = 'facilities';
  public organizationId: number;
  @ViewChild(FacilitiesComponent) facilitiesComponent: FacilitiesComponent;

  public selectedOrganization: IOption;

  constructor(
    private titleService: Title,
    private modalService: NgbModal,
    private organizationService: OrganizationService
  ) {

  }

  selectOrganization(organization: OrganizationDetailed) {
    if (this.organizationId == organization.id)
      return;

    this.organizationId = organization.id;

    this.selectedOrganization = {id: organization.id, name: organization.name};
  }

  ngOnInit(): void {
    this.titleService.setTitle("SHARP organizations");
    this.getOrganizations();
    this.organizationService.getPortalFeatures().subscribe(portalfeatures => {
      this.portalFeatures = portalfeatures;
    })
  }

  showSection(sectionName: string) {
    this.shownSectionName = sectionName;
  }

  getOrganizations() {
    this.organizationService.getOrganizations().subscribe(organizations => {
      this.organizations = organizations;
    })
  }

  public get organizationsList(): Array<OrganizationDetailed> {

    if (this.searchTerm) {
      return this.organizations.filter((organization: OrganizationDetailed) => organization.name.toLowerCase().includes(this.searchTerm.toLowerCase()));
    }

    return this.organizations;
  }

  public onSearchClear(): void {
    this.searchTerm = null;
  }

  public onAddOrganization(): void {
    this.modalService.open(AddOrganizationComponent).result.then(
      (r) => {
        this.getOrganizations();
      },
       () => {}
     );
  }
  public onEditOrganization(organization: OrganizationDetailed): void {
    const modalRef = this.modalService.open(EditOrganizationComponent);

    console.log(organization);
    modalRef.componentInstance.portalFeatures = this.portalFeatures;
    modalRef.componentInstance.detailedOrganization = organization;
    modalRef.componentInstance.organizationName = organization.name;
   
    modalRef.result.then(
      (r) => {
        this.getOrganizations();
      },
      () => { }
    );
  }
}
