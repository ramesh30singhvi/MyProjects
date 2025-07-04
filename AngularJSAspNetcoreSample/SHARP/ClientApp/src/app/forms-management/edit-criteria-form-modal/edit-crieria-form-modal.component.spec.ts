import { NO_ERRORS_SCHEMA } from "@angular/core";
import { EditCrieriaFormModalComponent } from "./edit-crieria-form-modal.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("EditCrieriaFormModalComponent", () => {

  let fixture: ComponentFixture<EditCrieriaFormModalComponent>;
  let component: EditCrieriaFormModalComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [EditCrieriaFormModalComponent]
    });

    fixture = TestBed.createComponent(EditCrieriaFormModalComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
