import { NO_ERRORS_SCHEMA } from "@angular/core";
import { EditFacilityModalComponent } from "./edit-facility-modal.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("EditFacilityModalComponent", () => {

  let fixture: ComponentFixture<EditFacilityModalComponent>;
  let component: EditFacilityModalComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [EditFacilityModalComponent]
    });

    fixture = TestBed.createComponent(EditFacilityModalComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
