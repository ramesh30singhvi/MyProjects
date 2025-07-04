import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddKeywordReportComponent } from './add-keyword-report.component';

describe('AddKeywordReportComponent', () => {
  let component: AddKeywordReportComponent;
  let fixture: ComponentFixture<AddKeywordReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AddKeywordReportComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AddKeywordReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
