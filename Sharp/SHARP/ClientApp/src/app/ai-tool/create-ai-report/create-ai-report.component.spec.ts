import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateAiReportComponent } from './create-ai-report.component';

describe('CreateAiReportComponent', () => {
  let component: CreateAiReportComponent;
  let fixture: ComponentFixture<CreateAiReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CreateAiReportComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateAiReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
