import { NO_ERRORS_SCHEMA } from "@angular/core";
import { AuditOptionsRendererComponent } from "./AuditOptionsRenderer.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("AuditOptionsRendererComponent", () => {

  let fixture: ComponentFixture<AuditOptionsRendererComponent>;
  let component: AuditOptionsRendererComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [AuditOptionsRendererComponent]
    });

    fixture = TestBed.createComponent(AuditOptionsRendererComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
