import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PortSelector } from './port-selector';

describe('PortSelector', () => {
  let component: PortSelector;
  let fixture: ComponentFixture<PortSelector>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PortSelector]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PortSelector);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
