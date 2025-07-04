import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ProgressNotesSectionComponent } from "./progress-notes-section.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("ProgressNotesSectionComponent", () => {

  let fixture: ComponentFixture<ProgressNotesSectionComponent>;
  let component: ProgressNotesSectionComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [ProgressNotesSectionComponent]
    });

    fixture = TestBed.createComponent(ProgressNotesSectionComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
