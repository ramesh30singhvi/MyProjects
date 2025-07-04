import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ConfirmationDialogComponent } from "./confirmation-dialog.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("ConfirmationDialogComponent", () => {

  let fixture: ComponentFixture<ConfirmationDialogComponent>;
  let component: ConfirmationDialogComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [ConfirmationDialogComponent]
    });

    fixture = TestBed.createComponent(ConfirmationDialogComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
