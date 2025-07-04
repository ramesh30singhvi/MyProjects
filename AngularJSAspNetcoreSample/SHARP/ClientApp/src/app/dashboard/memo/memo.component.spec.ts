import { NO_ERRORS_SCHEMA } from "@angular/core";
import { MemoComponent } from "./memo.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("MemoComponent", () => {

  let fixture: ComponentFixture<MemoComponent>;
  let component: MemoComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [MemoComponent]
    });

    fixture = TestBed.createComponent(MemoComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
