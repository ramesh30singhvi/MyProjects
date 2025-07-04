import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AuditSubmitPopupComponent } from './audit-submit-popup.component';

describe('AuditSubmitPopupComponent', () => {
  let component: AuditSubmitPopupComponent;
  let fixture: ComponentFixture<AuditSubmitPopupComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AuditSubmitPopupComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AuditSubmitPopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
