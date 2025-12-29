import { Component, Input, OnChanges, SimpleChanges, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import * as L from 'leaflet';
import { Vessel, Port } from '../../models';

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="map-container rounded shadow-sm">
      <div id="map" style="height: 100%; width: 100%;"></div>
    </div>
  `,
  styles: [`
    .map-container {
      height: 600px;
      width: 100%;
    }
  `]
})
export class MapComponent implements AfterViewInit, OnChanges {
  @Input() vessels: Vessel[] = [];
  @Input() selectedVessel: Vessel | null = null;
  @Input() selectedPort: Port | null = null;

  private map: L.Map | undefined;
  private markers: L.Marker[] = [];
  private highlightedMarker: L.Marker | null = null;

  private defaultIcon = L.icon({
    iconUrl: 'assets/marker-icon.png',
    iconRetinaUrl: 'assets/marker-icon-2x.png',
    shadowUrl: 'assets/marker-shadow.png',
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    popupAnchor: [1, -34],
    shadowSize: [41, 41]
  });

  private selectedIcon = L.icon({
    iconUrl: 'assets/marker-icon.png',
    iconRetinaUrl: 'assets/marker-icon-2x.png',
    shadowUrl: 'assets/marker-shadow.png',
    iconSize: [35, 57], // Larger size
    iconAnchor: [17, 57],
    popupAnchor: [1, -45],
    shadowSize: [57, 57],
    className: 'selected-vessel-marker' // Can use CSS for more styling
  });

  constructor() { }

  ngAfterViewInit(): void {
    this.initMap();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.map) {
      if (changes['vessels']) {
        this.updateMarkers();
      }
      if (changes['selectedPort'] && this.selectedPort) {
        this.map.setView([this.selectedPort.latitude, this.selectedPort.longitude], 11);
      }
      if (changes['selectedVessel'] && this.selectedVessel) {
        this.focusVessel(this.selectedVessel);
      }
    }
  }

  private initMap(): void {
    this.map = L.map('map').setView([51.505, -0.09], 13);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '© OpenStreetMap'
    }).addTo(this.map);
    
    // Set default icon for all markers
    L.Marker.prototype.options.icon = this.defaultIcon;
  }

  private updateMarkers(): void {
    if (!this.map) return;
    
    // Clear existing markers
    this.markers.forEach(m => this.map?.removeLayer(m));
    this.markers = [];
    this.highlightedMarker = null;

    this.vessels.forEach(vessel => {
      if (vessel.latitude && vessel.longitude) {
         const marker = L.marker([vessel.latitude, vessel.longitude], {
           icon: this.selectedVessel?.mmsi === vessel.mmsi ? this.selectedIcon : this.defaultIcon
         })
          .bindPopup(`
            <div class="vessel-popup">
              <h6 class="border-bottom pb-1 mb-2">${vessel.name || 'Unknown Vessel'}</h6>
              <div class="mb-1"><strong>MMSI:</strong> ${vessel.mmsi}</div>
              <div class="mb-1"><strong>ETA:</strong> ${new Date(vessel.estimatedArrivalUtc).toLocaleString()}</div>
              <div class="mt-2">
                <span class="badge ${this.getDelayRiskClass(vessel.delayRisk)}">
                  ${this.getDelayRiskLabel(vessel.delayRisk)} Delay Risk
                </span>
              </div>
            </div>
          `)
          .addTo(this.map!);
         
         if (this.selectedVessel?.mmsi === vessel.mmsi) {
           this.highlightedMarker = marker;
         }

         // Store reference to match with selectedVessel
         (marker as any).vesselMmsi = vessel.mmsi;
         this.markers.push(marker);
      }
    });

    if (this.selectedVessel) {
      this.focusVessel(this.selectedVessel);
    }
  }

  private focusVessel(vessel: Vessel): void {
     if (!this.map || !vessel.latitude || !vessel.longitude) return;
     
     // Reset previous highlighted marker
     if (this.highlightedMarker) {
       this.highlightedMarker.setIcon(this.defaultIcon);
       this.highlightedMarker.setZIndexOffset(0);
     }

     const marker = this.markers.find(m => (m as any).vesselMmsi === vessel.mmsi);
     const maxZoom = this.map.getMaxZoom();
     
     if (marker) {
       this.highlightedMarker = marker;
       marker.setIcon(this.selectedIcon);
       marker.setZIndexOffset(1000); // Bring to front
       marker.openPopup();
       
       // Zoom max to the selected vessel's marker
       this.map.setView([vessel.latitude, vessel.longitude], maxZoom);
     } else {
       // If marker not found (e.g. data still loading), just center map
       this.map.setView([vessel.latitude, vessel.longitude], maxZoom);
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
}
