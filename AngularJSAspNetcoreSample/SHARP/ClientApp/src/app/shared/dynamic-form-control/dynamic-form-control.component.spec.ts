import { NO_ERRORS_SCHEMA } from "@angular/core";
import { DynamicFormControlComponent } from "./dynamic-form-control.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("DynamicFormControlComponent", () => {

  let fixture: ComponentFixture<DynamicFormControlComponent>;
  let component: DynamicFormControlComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [DynamicFormControlComponent]
    });

    fixture = TestBed.createComponent(DynamicFormControlComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
