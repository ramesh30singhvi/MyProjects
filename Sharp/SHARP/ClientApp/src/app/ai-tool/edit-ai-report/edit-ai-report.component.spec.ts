import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditAiReportComponent } from './edit-ai-report.component';

describe('EditAiReportComponent', () => {
  let component: EditAiReportComponent;
  let fixture: ComponentFixture<EditAiReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EditAiReportComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(EditAiReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
