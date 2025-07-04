import { NO_ERRORS_SCHEMA } from "@angular/core";
import { DisapproveAuditModalComponent } from "./DisapproveAuditModal.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("DisapproveAuditModalComponent", () => {

  let fixture: ComponentFixture<DisapproveAuditModalComponent>;
  let component: DisapproveAuditModalComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [DisapproveAuditModalComponent]
    });

    fixture = TestBed.createComponent(DisapproveAuditModalComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });  
});
