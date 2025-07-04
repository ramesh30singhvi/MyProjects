import { NO_ERRORS_SCHEMA } from "@angular/core";
import { KeywordFormComponent } from "./keyword-form.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("KeywordFormComponent", () => {

  let fixture: ComponentFixture<KeywordFormComponent>;
  let component: KeywordFormComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [KeywordFormComponent]
    });

    fixture = TestBed.createComponent(KeywordFormComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
