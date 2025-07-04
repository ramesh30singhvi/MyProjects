import { NO_ERRORS_SCHEMA } from "@angular/core";
import { FormsComponent } from "./forms.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("FormsComponent", () => {

  let fixture: ComponentFixture<FormsComponent>;
  let component: FormsComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [FormsComponent]
    });

    fixture = TestBed.createComponent(FormsComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
