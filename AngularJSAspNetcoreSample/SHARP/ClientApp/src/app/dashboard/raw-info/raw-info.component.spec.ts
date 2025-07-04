import { NO_ERRORS_SCHEMA } from "@angular/core";
import { RawInfoComponent } from "./raw-info.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("RawInfoComponent", () => {

  let fixture: ComponentFixture<RawInfoComponent>;
  let component: RawInfoComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [RawInfoComponent]
    });

    fixture = TestBed.createComponent(RawInfoComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
