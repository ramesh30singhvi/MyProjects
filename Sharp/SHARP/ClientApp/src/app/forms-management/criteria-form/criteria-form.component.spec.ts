import { NO_ERRORS_SCHEMA } from "@angular/core";
import { CriteriaFormComponent } from "./criteria-form.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("KeywordFormComponent", () => {

  let fixture: ComponentFixture<CriteriaFormComponent>;
  let component: CriteriaFormComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [CriteriaFormComponent]
    });

    fixture = TestBed.createComponent(CriteriaFormComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
