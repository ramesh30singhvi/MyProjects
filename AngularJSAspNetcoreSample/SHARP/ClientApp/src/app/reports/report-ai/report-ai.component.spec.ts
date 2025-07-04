import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReportAiComponent } from './report-ai.component';

describe('ReportAiComponent', () => {
  let component: ReportAiComponent;
  let fixture: ComponentFixture<ReportAiComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ReportAiComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ReportAiComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
