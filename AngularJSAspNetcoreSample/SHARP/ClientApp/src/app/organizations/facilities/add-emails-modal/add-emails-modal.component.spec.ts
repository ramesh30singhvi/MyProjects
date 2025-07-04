import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEmailsModalComponent } from './add-emails-modal.component';

describe('AddEmailsModalComponent', () => {
  let component: AddEmailsModalComponent;
  let fixture: ComponentFixture<AddEmailsModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AddEmailsModalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AddEmailsModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
