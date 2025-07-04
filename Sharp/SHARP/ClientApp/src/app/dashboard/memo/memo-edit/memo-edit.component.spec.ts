import { NO_ERRORS_SCHEMA } from "@angular/core";
import { MemoEditComponent } from "./memo-edit.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("MemoEditComponent", () => {

  let fixture: ComponentFixture<MemoEditComponent>;
  let component: MemoEditComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [MemoEditComponent]
    });

    fixture = TestBed.createComponent(MemoEditComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
