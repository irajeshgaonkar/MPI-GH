import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModifiedRecordsComponent } from './modified-records.component';

describe('ModifiedRecordsComponent', () => {
  let component: ModifiedRecordsComponent;
  let fixture: ComponentFixture<ModifiedRecordsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ModifiedRecordsComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ModifiedRecordsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
