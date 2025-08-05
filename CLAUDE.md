# EBOM (Engineering Bill of Materials) System

## Project Overview
The EBOM System is a comprehensive full-stack application for managing Engineering Bill of Materials in elevator manufacturing. It provides template-based data management, dynamic table creation, file processing, and advanced reporting capabilities.

## Architecture Overview
This is a **full-stack application** with:
- **Backend**: .NET 8 Web API with Clean Architecture
- **Frontend**: Angular 17 with standalone components and NgRx state management
- **Database**: SQL Server with Entity Framework Core 8
- **File Processing**: Excel template and data processing with EPPlus

## Solution Structure
```
EBOM.Solution/
├── src/
│   ├── EBOM.Core/                 # Domain entities, interfaces, models
│   ├── EBOM.Data/                 # Data access layer, EF Core, configurations
│   ├── EBOM.Services/             # Business logic services and implementations
│   ├── EBOM.Common/               # Shared utilities, DTOs, configuration models
│   └── EBOM.API/                  # REST API controllers and middleware
├── ebom-client/                   # Angular 17 frontend application
│   ├── src/app/
│   │   ├── core/                  # Core services (auth, notifications)
│   │   ├── features/              # Feature modules (dashboard, entities, etc.)
│   │   ├── layouts/               # Layout components
│   │   ├── shared/                # Shared components, pipes, directives
│   │   └── store/                 # NgRx state management
├── tests/
│   ├── EBOM.Core.Tests/           # Unit tests for core domain
│   ├── EBOM.Services.Tests/       # Unit tests for business logic
│   └── EBOM.Integration.Tests/    # Integration tests
└── docs/                          # Documentation
```

## Key Technologies

### Backend Stack
- **.NET 8**: Target framework with C# 12 features
- **Entity Framework Core 8**: ORM for database access with migrations
- **SQL Server**: Primary database with dynamic table creation
- **EPPlus**: Excel template and data file processing
- **FluentValidation**: Input validation and business rules
- **Serilog**: Structured logging
- **JWT Bearer**: Authentication and authorization
- **AutoMapper**: Object-to-object mapping

### Frontend Stack
- **Angular 17**: Modern web framework with standalone components
- **TypeScript**: Type-safe JavaScript development
- **NgRx**: State management with actions, reducers, effects
- **DevExtreme**: Enterprise UI component library
- **Tailwind CSS**: Utility-first CSS framework
- **RxJS**: Reactive programming for async operations

### Development Tools
- **xUnit**: Testing framework for .NET
- **Jasmine/Karma**: Testing for Angular
- **Git**: Version control
- **npm**: Package management for frontend

## Core Features

### 1. Template Management
- **Excel Template Upload**: Upload and process Excel templates with column classification
- **Template Validation**: Automatic validation of template structure and data types
- **Revision Control**: Track template versions with automatic revision numbering
- **Column Classification**: Distinguish between ValueType and DependencyType columns
- **Mirror Entity Support**: Handle relationships between entities through templates

### 2. Data Management
- **Data File Processing**: Upload Excel data files with validation against templates
- **Dynamic Table Creation**: Automatically create SQL tables based on template structures
- **Data Validation**: Real-time validation of data against template definitions
- **Bulk Data Import**: Efficient processing of large data sets with progress tracking
- **Data Revision Control**: Track all data uploads with versioning

### 3. Entity Management
- **Entity CRUD Operations**: Complete create, read, update, delete operations
- **Master-Detail Views**: Comprehensive entity detail pages with related data
- **Entity Types**: Support for ISR, PSR, CSR, and CMN entity classifications
- **Entity Dependencies**: Manage relationships and dependencies between entities
- **Search and Filtering**: Advanced filtering by entity type and search terms

### 4. Dashboard and Analytics
- **Real-time Dashboard**: KPIs, metrics, and system health monitoring
- **Interactive Charts**: Data visualization with DevExtreme chart components
- **System Metrics**: Track entities, templates, data uploads, and system usage
- **Upload Activity Tracking**: Monitor file upload trends and patterns

### 5. Reports and Analytics
- **System Overview**: Comprehensive system metrics and health indicators
- **Entity Reports**: Detailed reporting on entity data and template usage
- **Data Quality Monitoring**: Track data validation issues and quality metrics
- **Export Capabilities**: Export reports to various formats (Excel, PDF)

### 6. Authentication and Security
- **JWT Authentication**: Secure token-based authentication system
- **Role-based Access**: Authorization guards and route protection
- **Session Management**: Automatic token refresh and session handling
- **API Security**: Protected endpoints with authentication middleware

## Development Guidelines

### 1. Architecture Principles
- **Clean Architecture**: Separate concerns into distinct layers (Core, Data, Services, API)
- **SOLID Principles**: Single responsibility, dependency injection, interface segregation
- **Domain-Driven Design**: Rich domain models with business logic encapsulation
- **Repository Pattern**: Abstract data access with Entity Framework Core
- **CQRS Pattern**: Separate read and write operations where appropriate

### 2. Backend Coding Standards
- Use C# 12 features where appropriate
- Follow .NET naming conventions (PascalCase for public members)
- Async/await for all I/O operations
- Comprehensive XML documentation
- No hardcoded strings (use constants/resources)
- Dependency injection for all services
- Use nullable reference types

### 3. Frontend Coding Standards
- Follow Angular style guide conventions
- Use TypeScript strict mode
- Standalone components with signal-based change detection
- RxJS for reactive programming patterns
- Consistent naming: kebab-case for files, camelCase for properties
- Use OnPush change detection strategy where possible

### 3. Project Dependencies
```
EBOM.Core (no dependencies on other projects)
    ↑
EBOM.Data (depends on Core)
    ↑
EBOM.Services (depends on Core and Data)
    ↑
EBOM.API (depends on all)
```

### 4. Key Components

#### Core Components
- **Entities**: Entity, EntityValue, EntityTemplateRevision, etc.
- **Enums**: EntityType (ISR, PSR, CSR, CMN), DataType
- **Interfaces**: ITemplateProcessor, IDataProcessor, IValidationService

#### Data Processing Flow
1. Excel file upload → Validation
2. Template processing → Entity management
3. Revision control → Column classification
4. Data normalization → Dynamic table creation
5. Bulk data insertion → Completion

#### Column Classification System
- Separator column divides ValueType and DependencyType columns
- Excluded columns are filtered out
- Dynamic processing based on configuration

### 5. Configuration
Location: `appsettings.json`
```json
{
  "EbomSettings": {
    "SeparatorColumn": "Status",
    "ExcludedColumns": ["Concat", "SerialNumber"],
    "MaxFileSize": 52428800,
    "BatchSize": 1000
  }
}
```

### 6. Database Schema
- **Static Tables**: Entity, EntityValue, EntityTemplateRevision, etc.
- **Dynamic Tables**: `data_{EntityType}_{EntityName}_{Revision:0000}`
- **Naming Convention**: Use PascalCase for table and column names

### 7. Testing Strategy
- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test complete workflows
- **Performance Tests**: Ensure scalability with large datasets
- **Test Coverage**: Aim for >80% code coverage

### 8. Error Handling
- Use custom exceptions (ValidationException, TemplateException, etc.)
- Structured error responses with error codes
- Comprehensive logging of all errors
- User-friendly error messages

### 9. Security Considerations
- Input validation and sanitization
- File upload restrictions (size, type)
- SQL injection prevention
- No sensitive data in logs

### 10. Performance Optimizations
- Bulk insert for large datasets
- Caching for frequently accessed data
- Async processing throughout
- Efficient value normalization with hashing

## Build and Run Instructions

### Prerequisites
- .NET 8 SDK
- SQL Server 2019+
- Visual Studio 2022 or VS Code

### Setup Steps
1. Clone the repository
2. Restore NuGet packages: `dotnet restore`
3. Update connection string in appsettings.json
4. Run migrations: `dotnet ef database update`
5. Build solution: `dotnet build`
6. Run tests: `dotnet test`

### Development Workflow
1. Create feature branch from main
2. Implement feature with tests
3. Ensure all tests pass
4. Update documentation
5. Submit pull request

## Common Tasks

### Adding a New Entity
1. Create entity class in EBOM.Core/Entities
2. Add DbSet to EbomDbContext
3. Create EntityConfiguration in EBOM.Data/Configurations
4. Add migration: `dotnet ef migrations add AddEntityName`
5. Create repository interface and implementation
6. Add unit tests

### Processing a Template
1. Validate file format and naming
2. Extract column information
3. Create/update entities
4. Manage template revision
5. Process mirror entities
6. Return processing result

### Processing Data
1. Validate against active template
2. Create data revision
3. Normalize values to EntityValue table
4. Create/update dynamic table
5. Bulk insert data
6. Return processing summary

## Troubleshooting

### Common Issues
1. **File Upload Fails**: Check file naming convention, size limits
2. **Template Not Found**: Ensure template is uploaded before data
3. **Performance Issues**: Check batch size, enable bulk operations
4. **Migration Errors**: Verify connection string, database permissions

### Debugging Tips
- Enable detailed logging in appsettings.json
- Check SQL Profiler for generated queries
- Use integration tests to reproduce issues
- Monitor memory usage for large files

## Deployment

### Production Checklist
- [ ] Update connection strings
- [ ] Run all migrations
- [ ] Configure logging levels
- [ ] Set up health checks
- [ ] Configure backup strategy
- [ ] Enable performance monitoring
- [ ] Review security settings

### Monitoring
- Application Insights or similar APM
- Database performance metrics
- Error rate monitoring
- File processing throughput

## Contact and Support
- Project Lead: [Name]
- Technical Lead: [Name]
- Documentation: /docs
- Issue Tracking: [URL]