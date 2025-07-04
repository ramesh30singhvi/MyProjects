import { NO_ERRORS_SCHEMA } from "@angular/core";
import { EditSubheaderModalComponent } from "./edit-subheader-modal.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("EditSubheaderModalComponent", () => {

  let fixture: ComponentFixture<EditSubheaderModalComponent>;
  let component: EditSubheaderModalComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [EditSubheaderModalComponent]
    });

    fixture = TestBed.createComponent(EditSubheaderModalComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
