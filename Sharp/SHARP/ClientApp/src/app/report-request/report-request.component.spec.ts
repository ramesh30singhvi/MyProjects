import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ReportRequestComponent } from "./report-request.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("ReportRequestComponent", () => {

  let fixture: ComponentFixture<ReportRequestComponent>;
  let component: ReportRequestComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [ReportRequestComponent]
    });

    fixture = TestBed.createComponent(ReportRequestComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
