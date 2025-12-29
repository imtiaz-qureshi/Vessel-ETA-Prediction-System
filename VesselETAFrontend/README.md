# Vessel ETA Frontend

This is the Angular frontend dashboard for the Vessel ETA Prediction System. It provides real-time visualization of vessel movements, ETA predictions, and port operations for UK maritime traffic.

## Features

- **Real-time Vessel Tracking**: Live updates of vessel positions and movements
- **ETA Predictions**: Display of calculated arrival times with risk assessments
- **Port Operations Dashboard**: Overview of vessels approaching UK ports
- **Interactive Maps**: Geographic visualization of vessel routes and positions
- **WebSocket Integration**: Real-time updates via SignalR connection to the API Gateway

## Technology Stack

- **Angular 21**: Modern web framework
- **TypeScript**: Type-safe development
- **Angular Material**: UI component library
- **SignalR Client**: Real-time communication
- **Leaflet/OpenLayers**: Interactive mapping (planned)
- **Chart.js**: Data visualization (planned)

## Prerequisites

- Node.js 18+ and npm
- Angular CLI (`npm install -g @angular/cli`)
- Running Vessel ETA backend services (API Gateway, Kafka, etc.)

## Development Setup

1. **Install dependencies**
   ```bash
   npm install
   ```

2. **Configure API endpoint**
   Update `src/environments/environment.ts` with your API Gateway URL:
   ```typescript
   export const environment = {
     production: false,
     apiUrl: 'http://localhost:5000/api',
     signalRUrl: 'http://localhost:5000/hubs/eta'
   };
   ```

3. **Start development server**
   ```bash
   ng serve
   ```
   Navigate to `http://localhost:4200/`

## Backend Integration

The frontend connects to the Vessel ETA backend services:

- **API Gateway**: REST endpoints for vessel and port data
- **SignalR Hub**: Real-time ETA updates and vessel position streams
- **Data Sources**: Supports both simulated and real AIS data from the backend

## Project Structure

```
src/
├── app/
│   ├── components/          # UI components
│   ├── services/           # API and SignalR services
│   ├── models/             # TypeScript interfaces
│   └── pages/              # Route components
├── assets/                 # Static assets
└── environments/           # Environment configurations
```

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
