import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api';
import { Port, Vessel, VesselPrediction } from '../../models';
import { PortSelectorComponent } from '../port-selector/port-selector';
import { VesselListComponent } from '../vessel-list/vessel-list';
import { VesselDetailComponent } from '../vessel-list/vessel-detail';
import { MapComponent } from '../map/map';
import { Observable, of, BehaviorSubject } from 'rxjs';
import { catchError, tap, switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, PortSelectorComponent, VesselListComponent, VesselDetailComponent, MapComponent],
  template: `
    <div class="d-flex flex-column vh-100">
      <nav class="navbar navbar-dark bg-primary px-3 shadow">
        <span class="navbar-brand mb-0 h1">Vessel ETA Predictor</span>
      </nav>

      <div class="row g-0 flex-grow-1 overflow-hidden">
        <!-- Sidebar -->
        <div class="col-md-3 bg-light border-end d-flex flex-column p-3 sidebar">
          <app-port-selector 
            [ports]="(ports$ | async) || []" 
            [selectedPort]="selectedPort"
            (portSelected)="onPortSelected($event)">
          </app-port-selector>
          
          <hr>
          
          <h6 class="text-muted">Vessels in Port</h6>
          <app-vessel-list 
            class="flex-grow-1 overflow-auto"
            [vessels]="(vessels$ | async) || []"
            [selectedVessel]="selectedVessel"
            (vesselSelected)="onVesselSelected($event)">
          </app-vessel-list>

          <app-vessel-detail 
            [vessel]="selectedVessel" 
            [history]="(history$ | async) || []">
          </app-vessel-detail>
        </div>

        <!-- Main Map Area -->
        <div class="col-md-9 position-relative">
          <app-map 
            [vessels]="(vessels$ | async) || []"
            [selectedVessel]="selectedVessel"
            [selectedPort]="selectedPort">
          </app-map>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .sidebar {
      max-height: calc(100vh - 56px);
    }
  `]
})
export class LayoutComponent implements OnInit {
  ports$: Observable<Port[]> = of([]);
  vessels$: Observable<Vessel[]> = of([]);
  history$: Observable<VesselPrediction[]> = of([]);
  
  selectedPort: Port | null = null;
  selectedVessel: Vessel | null = null;

  constructor(private apiService: ApiService) {}

  ngOnInit() {
    this.ports$ = this.apiService.getPorts().pipe(
      tap(ports => {
        if (ports && ports.length > 0) {
          // Select first port by default as requested
          this.onPortSelected(ports[0]);
        }
      }),
      catchError(err => {
        console.error('Error fetching ports', err);
        return of([]);
      })
    );
  }

  onPortSelected(port: Port) {
    this.selectedPort = port;
    this.selectedVessel = null;
    this.vessels$ = this.apiService.getVessels(port.portCode).pipe(
        catchError(err => {
            console.error('Error fetching vessels', err);
            return of([]);
        })
    );
  }

  onVesselSelected(vessel: Vessel) {
    this.selectedVessel = vessel;
    this.history$ = this.apiService.getVesselHistory(vessel.mmsi).pipe(
        catchError(err => {
            console.error('Error fetching vessel history', err);
            return of([]);
        })
    );
  }
}
