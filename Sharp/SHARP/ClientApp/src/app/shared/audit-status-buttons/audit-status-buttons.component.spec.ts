import { NO_ERRORS_SCHEMA } from "@angular/core";
import { AuditStatusButtonsComponent } from "./audit-status-buttons.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("AuditStatusButtonsComponent", () => {

  let fixture: ComponentFixture<AuditStatusButtonsComponent>;
  let component: AuditStatusButtonsComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [AuditStatusButtonsComponent]
    });

    fixture = TestBed.createComponent(AuditStatusButtonsComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
