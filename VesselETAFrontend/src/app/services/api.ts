import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Port, Vessel, VesselPrediction } from '../models';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) { }

  getPorts(): Observable<Port[]> {
    return this.http.get<any>(`${this.apiUrl}/Ports`).pipe(
      map(response => response.value || response)
    );
  }

  getVessels(portCode: string): Observable<Vessel[]> {
    return this.http.get<any>(`${this.apiUrl}/Ports/${portCode}/vessels`).pipe(
      map(response => response.value || response)
    );
  }

  getVesselEta(mmsi: string): Observable<any> {
      return this.http.get<any>(`${this.apiUrl}/Vessels/${mmsi}/eta`);
  }

  getVesselHistory(mmsi: string, hours: number = 24): Observable<VesselPrediction[]> {
    return this.http.get<any>(`${this.apiUrl}/Vessels/${mmsi}/history?hours=${hours}`).pipe(
      map(response => response.value || response)
    );
  }
}
