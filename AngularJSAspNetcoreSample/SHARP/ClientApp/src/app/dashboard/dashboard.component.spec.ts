import { NO_ERRORS_SCHEMA } from "@angular/core";
import { DashboardComponent } from "./dashboard.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("DashboardComponent", () => {

  let fixture: ComponentFixture<DashboardComponent>;
  let component: DashboardComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [DashboardComponent]
    });

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
