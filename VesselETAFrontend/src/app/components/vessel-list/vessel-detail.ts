import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Vessel, VesselPrediction } from '../../models';

@Component({
  selector: 'app-vessel-detail',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngIf="vessel" class="vessel-detail card mt-3 shadow-sm">
      <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
        <h6 class="mb-0">{{ vessel.name || 'Vessel Detail' }}</h6>
        <span class="badge" [ngClass]="getDelayRiskClass(vessel.delayRisk)">
          {{ getDelayRiskLabel(vessel.delayRisk) }} Risk
        </span>
      </div>
      <div class="card-body">
        <div class="row mb-3">
          <div class="col-6">
            <small class="text-muted d-block">MMSI</small>
            <span>{{ vessel.mmsi }}</span>
          </div>
          <div class="col-6">
            <small class="text-muted d-block">Port</small>
            <span>{{ vessel.portCode }}</span>
          </div>
        </div>

        <div *ngIf="history.length > 0">
          <h6 class="mb-2 border-bottom pb-1">ETA Trend (Last 24h)</h6>
          <div class="chart-container" style="height: 100px; width: 100%;">
            <svg viewBox="0 0 100 40" preserveAspectRatio="none" style="width: 100%; height: 100%;">
              <!-- Grid lines -->
              <line x1="0" y1="10" x2="100" y2="10" stroke="#eee" stroke-width="0.5" />
              <line x1="0" y1="20" x2="100" y2="20" stroke="#eee" stroke-width="0.5" />
              <line x1="0" y1="30" x2="100" y2="30" stroke="#eee" stroke-width="0.5" />
              
              <!-- Trend Line -->
              <polyline
                fill="none"
                stroke="#007bff"
                stroke-width="1.5"
                [attr.points]="chartPoints"
              />
            </svg>
            <div class="d-flex justify-content-between mt-1">
              <small class="text-muted">24h ago</small>
              <small class="text-muted">Now</small>
            </div>
          </div>
          <p class="small text-muted mt-2">
            The chart shows the change in estimated distance to port over time.
          </p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .vessel-detail {
        border: none;
    }
    .chart-container {
        background: #fcfcfc;
        border-radius: 4px;
        padding: 5px;
    }
  `]
})
export class VesselDetailComponent implements OnChanges {
  @Input() vessel: Vessel | null = null;
  @Input() history: VesselPrediction[] = [];

  chartPoints: string = '';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['history'] && this.history.length > 0) {
      this.generateChartPoints();
    }
  }

  getDelayRiskClass(risk?: number): string {
    if (risk === undefined) return 'bg-secondary';
    if (risk < 1) return 'bg-success';
    if (risk < 2) return 'bg-warning text-dark';
    return 'bg-danger';
  }

  getDelayRiskLabel(risk?: number): string {
    if (risk === undefined) return 'N/A';
    if (risk < 1) return 'Low';
    if (risk < 2) return 'Medium';
    return 'High';
  }

  private generateChartPoints(): void {
    if (this.history.length < 2) {
      this.chartPoints = '';
      return;
    }

    const minDistance = Math.min(...this.history.map(h => h.distanceNauticalMiles));
    const maxDistance = Math.max(...this.history.map(h => h.distanceNauticalMiles));
    const range = maxDistance - minDistance || 1;

    // Map time to X (0-100) and Distance to Y (35 to 5, inverted)
    this.chartPoints = this.history
      .map((h, i) => {
        const x = (i / (this.history.length - 1)) * 100;
        const y = 35 - ((h.distanceNauticalMiles - minDistance) / range) * 30;
        return `${x},${y}`;
      })
      .join(' ');
  }
}
