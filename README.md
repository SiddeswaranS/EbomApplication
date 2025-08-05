# EBOM System - Engineering Bill of Materials

A comprehensive system for managing Engineering Bill of Materials in the elevator manufacturing industry. The system automates part number selection based on elevator configurations and provides template-based data management.

## Architecture

### Backend (.NET 8)
- **EBOM.Core**: Domain entities, interfaces, and models
- **EBOM.Data**: Entity Framework Core data access layer
- **EBOM.Services**: Business logic and processing services
- **EBOM.Common**: Shared utilities and configuration models
- **EBOM.API**: REST API with JWT authentication

### Frontend (Angular 17)
- **DevExtreme**: UI components and data grids
- **Tailwind CSS**: Utility-first CSS framework
- **NgRx**: State management
- **JWT Authentication**: Secure API access

## Features

- **Entity Management**: Create and manage entity types (ISR, PSR, CSR, CMN)
- **Template Processing**: Upload and process Excel templates with validation
- **Data Upload**: Bulk data upload with normalization
- **Dynamic Tables**: Automatic table creation based on templates
- **Version Control**: Template and data revision management
- **Dashboard**: Real-time analytics and KPIs
- **File Upload**: Drag-and-drop file upload with progress tracking

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

## Getting Started

### Backend Setup

1. Clone the repository
2. Navigate to the root directory
3. Restore packages:
   ```bash
   dotnet restore
   ```
4. Update the connection string in `src/EBOM.API/appsettings.json`
5. Build the solution:
   ```bash
   dotnet build
   ```
6. Run the API:
   ```bash
   cd src/EBOM.API
   dotnet run
   ```

The API will be available at `https://localhost:7001` (or `http://localhost:5000`)

### Frontend Setup

1. Navigate to the Angular project:
   ```bash
   cd ebom-client
   ```
2. Install dependencies:
   ```bash
   npm install
   ```
3. Run the development server:
   ```bash
   npm start
   ```

The application will be available at `http://localhost:4200`

## Default Credentials

- **Email**: admin@ebom.com
- **Password**: admin123

## Project Structure

```
EbomApplication/
├── src/
│   ├── EBOM.Core/          # Domain layer
│   ├── EBOM.Data/          # Data access layer
│   ├── EBOM.Services/      # Business logic
│   ├── EBOM.Common/        # Shared utilities
│   └── EBOM.API/           # Web API
├── ebom-client/            # Angular frontend
│   └── src/
│       └── app/
│           ├── core/       # Core services
│           ├── features/   # Feature modules
│           ├── layouts/    # Layout components
│           └── shared/     # Shared components
└── CLAUDE.md              # Detailed documentation
```

## Key Technologies

### Backend
- ASP.NET Core 8
- Entity Framework Core 8
- SQL Server
- EPPlus (Excel processing)
- Serilog (Logging)
- JWT Authentication

### Frontend
- Angular 17
- DevExtreme 25.1
- Tailwind CSS 3
- NgRx 17
- RxJS 7.8

## Development Guidelines

See [CLAUDE.md](CLAUDE.md) for detailed development guidelines and conventions.

## License

This project is proprietary software for elevator manufacturing industry use.