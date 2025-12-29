import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Port } from '../../models';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-port-selector',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="port-selector p-3 bg-white shadow-sm rounded">
      <label class="form-label fw-bold">Select Port</label>
      <select class="form-select" [ngModel]="selectedPort" (ngModelChange)="onPortChange($event)">
        <option *ngFor="let port of ports" [ngValue]="port">
          {{ port.name }} ({{ port.country }})
        </option>
      </select>
    </div>
  `,
  styles: []
})
export class PortSelectorComponent {
  @Input() ports: Port[] = [];
  @Input() selectedPort: Port | null = null;
  @Output() portSelected = new EventEmitter<Port>();

  onPortChange(port: Port) {
    this.portSelected.emit(port);
  }
}
