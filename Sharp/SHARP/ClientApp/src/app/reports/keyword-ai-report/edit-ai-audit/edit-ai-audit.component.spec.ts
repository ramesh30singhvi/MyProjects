import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditAiAuditComponent } from './edit-ai-audit.component';

describe('EditAiAuditComponent', () => {
  let component: EditAiAuditComponent;
  let fixture: ComponentFixture<EditAiAuditComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EditAiAuditComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(EditAiAuditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
