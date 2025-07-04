import { NO_ERRORS_SCHEMA } from "@angular/core";
import { TwentyHourKeywordsComponent } from "./twenty-hour-keywords.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("TwentyHourKeywordsComponent", () => {

  let fixture: ComponentFixture<TwentyHourKeywordsComponent>;
  let component: TwentyHourKeywordsComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [TwentyHourKeywordsComponent]
    });

    fixture = TestBed.createComponent(TwentyHourKeywordsComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
