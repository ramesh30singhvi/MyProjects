import { NO_ERRORS_SCHEMA } from "@angular/core";
import { OrganizationFormsBtnCellRendererComponent } from "./organization-forms-btn-cell-renderer.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("OrganizationFormsBtnCellRendererComponent", () => {

  let fixture: ComponentFixture<OrganizationFormsBtnCellRendererComponent>;
  let component: OrganizationFormsBtnCellRendererComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [OrganizationFormsBtnCellRendererComponent]
    });

    fixture = TestBed.createComponent(OrganizationFormsBtnCellRendererComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
