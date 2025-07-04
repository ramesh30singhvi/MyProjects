import { NO_ERRORS_SCHEMA } from "@angular/core";
import { SetSectionModalComponent } from "./set-section-modal.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("SetSectionModalComponent", () => {

  let fixture: ComponentFixture<SetSectionModalComponent>;
  let component: SetSectionModalComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [SetSectionModalComponent]
    });

    fixture = TestBed.createComponent(SetSectionModalComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
