import { NO_ERRORS_SCHEMA } from "@angular/core";
import { AuditsGridButtonsRendererComponent } from "./AuditsGridButtonsRenderer.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("AuditsGridButtonsRendererComponent", () => {

  let fixture: ComponentFixture<AuditsGridButtonsRendererComponent>;
  let component: AuditsGridButtonsRendererComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [AuditsGridButtonsRendererComponent]
    });

    fixture = TestBed.createComponent(AuditsGridButtonsRendererComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
