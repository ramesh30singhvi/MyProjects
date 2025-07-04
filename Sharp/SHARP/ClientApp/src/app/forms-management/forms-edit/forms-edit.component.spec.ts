import { NO_ERRORS_SCHEMA } from "@angular/core";
import { FormsEditComponent } from "./forms-edit.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("FormsEditComponent", () => {

  let fixture: ComponentFixture<FormsEditComponent>;
  let component: FormsEditComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [FormsEditComponent]
    });

    fixture = TestBed.createComponent(FormsEditComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
