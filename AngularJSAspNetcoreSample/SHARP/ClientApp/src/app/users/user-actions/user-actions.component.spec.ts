import { NO_ERRORS_SCHEMA } from "@angular/core";
import { UserActionsComponent } from "./user-actions.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";

describe("UserActionsComponent", () => {

  let fixture: ComponentFixture<UserActionsComponent>;
  let component: UserActionsComponent;
  beforeEach(() => {
    TestBed.configureTestingModule({
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
      ],
      declarations: [UserActionsComponent]
    });

    fixture = TestBed.createComponent(UserActionsComponent);
    component = fixture.componentInstance;

  });

  it("should be able to create component instance", () => {
    expect(component).toBeDefined();
  });
  
});
