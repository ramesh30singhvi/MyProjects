import { NO_ERRORS_SCHEMA } from "@angular/core";
import { KeywordInputSectionComponent } from "./keyword-input-section.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("KeywordInputSectionComponent", () => {

  let fixture: ComponentFixture<KeywordInputSectionComponent>;
  let component: KeywordInputSectionComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [KeywordInputSectionComponent]
    });

    fixture = TestBed.createComponent(KeywordInputSectionComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
