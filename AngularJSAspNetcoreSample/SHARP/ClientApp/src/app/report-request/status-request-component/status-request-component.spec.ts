import { NO_ERRORS_SCHEMA } from "@angular/core";
import { StatusRequestComponent } from "./status-request-component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("StatusRequestComponentComponent", () => {

  let fixture: ComponentFixture<StatusRequestComponent>;
  let component: StatusRequestComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [StatusRequestComponent]
    });

    fixture = TestBed.createComponent(StatusRequestComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
