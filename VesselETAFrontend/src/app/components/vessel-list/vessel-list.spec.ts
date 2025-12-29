import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VesselList } from './vessel-list';

describe('VesselList', () => {
  let component: VesselList;
  let fixture: ComponentFixture<VesselList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VesselList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VesselList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
