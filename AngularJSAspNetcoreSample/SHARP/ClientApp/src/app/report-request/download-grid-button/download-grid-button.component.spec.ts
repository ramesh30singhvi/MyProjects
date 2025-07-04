import { NO_ERRORS_SCHEMA } from "@angular/core";
import { DownloadGridButtonComponent } from "./download-grid-button.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("DownloadGridButtonComponent", () => {

  let fixture: ComponentFixture<DownloadGridButtonComponent>;
  let component: DownloadGridButtonComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [DownloadGridButtonComponent]
    });

    fixture = TestBed.createComponent(DownloadGridButtonComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
