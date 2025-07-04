import { NO_ERRORS_SCHEMA } from "@angular/core";
import { TrackerFormComponent } from "./tracker-form.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("TrackerFormComponent", () => {

  let fixture: ComponentFixture<TrackerFormComponent>;
  let component: TrackerFormComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [TrackerFormComponent]
    });

    fixture = TestBed.createComponent(TrackerFormComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
