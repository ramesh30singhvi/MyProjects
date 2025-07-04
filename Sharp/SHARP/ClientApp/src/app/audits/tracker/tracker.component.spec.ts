import { NO_ERRORS_SCHEMA } from "@angular/core";
import { TrackerComponent } from "./tracker.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("TrackerComponent", () => {

  let fixture: ComponentFixture<TrackerComponent>;
  let component: TrackerComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [TrackerComponent]
    });

    fixture = TestBed.createComponent(TrackerComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
