import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Vessel } from '../../models';

@Component({
  selector: 'app-vessel-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="vessel-list list-group mt-3">
      <div *ngFor="let vessel of vessels" 
           class="list-group-item list-group-item-action"
           [class.active]="selectedVessel?.mmsi === vessel.mmsi"
           (click)="onSelect(vessel)">
        <div class="d-flex w-100 justify-content-between">
          <h5 class="mb-1">{{ vessel.name || vessel.mmsi }}</h5>
          <small>{{ vessel.estimatedArrivalUtc | date:'short' }}</small>
        </div>
        <p class="mb-1">ETA: {{ vessel.estimatedArrivalUtc | date:'medium' }}</p>
      </div>
      <div *ngIf="vessels.length === 0" class="alert alert-info mt-2">
        No vessels found for this port.
      </div>
    </div>
  `,
  styles: [`
    .vessel-list {
      max-height: 70vh;
      overflow-y: auto;
    }
  `]
})
export class VesselListComponent {
  @Input() vessels: Vessel[] = [];
  @Input() selectedVessel: Vessel | null = null;
  @Output() vesselSelected = new EventEmitter<Vessel>();

  onSelect(vessel: Vessel) {
    this.vesselSelected.emit(vessel);
  }
}
