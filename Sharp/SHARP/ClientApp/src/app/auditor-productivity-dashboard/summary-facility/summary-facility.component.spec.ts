import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SummaryFacilityComponent } from './summary-facility.component';

describe('SummaryFacilityComponent', () => {
  let component: SummaryFacilityComponent;
  let fixture: ComponentFixture<SummaryFacilityComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SummaryFacilityComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SummaryFacilityComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
