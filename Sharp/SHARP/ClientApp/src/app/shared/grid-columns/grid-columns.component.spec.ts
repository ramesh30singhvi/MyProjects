import { NO_ERRORS_SCHEMA } from "@angular/core";
import { GridColumnsComponent } from "./grid-columns.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("GridColumnsComponent", () => {

  let fixture: ComponentFixture<GridColumnsComponent>;
  let component: GridColumnsComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [GridColumnsComponent]
    });

    fixture = TestBed.createComponent(GridColumnsComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
