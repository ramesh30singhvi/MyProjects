import { NO_ERRORS_SCHEMA } from "@angular/core";
import { CriteriaPdfFilterPopupComponent } from "./criteria-pdf-filter-popup.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("CriteriaPdfFilterPopupComponent", () => {

  let fixture: ComponentFixture<CriteriaPdfFilterPopupComponent>;
  let component: CriteriaPdfFilterPopupComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [CriteriaPdfFilterPopupComponent]
    });

    fixture = TestBed.createComponent(CriteriaPdfFilterPopupComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
