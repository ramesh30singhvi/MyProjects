import { NO_ERRORS_SCHEMA } from "@angular/core";
import { EditTrackerQuestionModalComponent } from "./edit-tracker-question-modal.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("EditTrackerQuestionModalComponent", () => {

  let fixture: ComponentFixture<EditTrackerQuestionModalComponent>;
  let component: EditTrackerQuestionModalComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [EditTrackerQuestionModalComponent]
    });

    fixture = TestBed.createComponent(EditTrackerQuestionModalComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
