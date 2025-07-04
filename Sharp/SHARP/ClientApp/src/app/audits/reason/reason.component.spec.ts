import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ReasonComponent } from "./reason.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("ReasonComponent", () => {

  let fixture: ComponentFixture<ReasonComponent>;
  let component: ReasonComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [ReasonComponent]
    });

    fixture = TestBed.createComponent(ReasonComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
