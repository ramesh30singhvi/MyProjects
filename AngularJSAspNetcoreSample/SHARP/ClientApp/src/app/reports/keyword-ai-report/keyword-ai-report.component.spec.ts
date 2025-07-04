import { ComponentFixture, TestBed } from '@angular/core/testing';

import { KeywordAiReportComponent } from './keyword-ai-report.component';

describe('KeywordAiReportComponent', () => {
  let component: KeywordAiReportComponent;
  let fixture: ComponentFixture<KeywordAiReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ KeywordAiReportComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(KeywordAiReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
