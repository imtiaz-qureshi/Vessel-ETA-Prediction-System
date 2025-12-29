export interface Port {
  portCode: string;
  name: string;
  latitude: number;
  longitude: number;
  country?: string;
}

export interface VesselPrediction {
  mmsi: number;
  portCode: string;
  estimatedArrivalUtc: string;
  delayRisk: number;
  distanceNauticalMiles: number;
}

export interface Vessel {
  mmsi: string;
  portCode: string;
  estimatedArrivalUtc: string; // ISO Date string
  name?: string; // If available
  latitude?: number;
  longitude?: number;
  delayRisk?: number;
}
