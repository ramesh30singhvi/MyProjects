import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StatusAiReportComponent } from './status-ai-report.component';

describe('StatusAiReportComponent', () => {
  let component: StatusAiReportComponent;
  let fixture: ComponentFixture<StatusAiReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ StatusAiReportComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(StatusAiReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
