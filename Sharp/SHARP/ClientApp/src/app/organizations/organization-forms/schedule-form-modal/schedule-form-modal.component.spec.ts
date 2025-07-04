import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ScheduleFormModalComponent } from "./schedule-form-modal.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("ScheduleFormModalComponent", () => {

  let fixture: ComponentFixture<ScheduleFormModalComponent>;
  let component: ScheduleFormModalComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [ScheduleFormModalComponent]
    });

    fixture = TestBed.createComponent(ScheduleFormModalComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
