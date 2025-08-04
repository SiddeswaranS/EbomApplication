# EBOM (Engineering Bill of Materials) Project

## Project Overview
EBOM.Shared is a .NET class library designed for automated part number selection in elevator manufacturing based on configuration rules and dependencies.

## Solution Structure
```
EBOM.Solution/
├── src/
│   ├── EBOM.Core/                 # Core entities, interfaces, enums
│   ├── EBOM.Data/                 # Data access layer, EF Core, repositories
│   ├── EBOM.Services/             # Business logic services
│   ├── EBOM.Common/               # Shared utilities, helpers, extensions
│   └── EBOM.API/                  # Optional REST API project
├── tests/
│   ├── EBOM.Core.Tests/           # Unit tests for core
│   ├── EBOM.Services.Tests/       # Unit tests for services
│   └── EBOM.Integration.Tests/    # Integration tests
├── docs/                          # Documentation
└── scripts/                       # Database scripts, deployment scripts
```

## Key Technologies
- **.NET 8**: Target framework
- **Entity Framework Core 8**: ORM for database access
- **SQL Server**: Database
- **EPPlus**: Excel file processing
- **Serilog**: Structured logging
- **xUnit**: Testing framework
- **FluentValidation**: Input validation

## Development Guidelines

### 1. Architecture Principles
- **Clean Architecture**: Separate concerns into layers
- **SOLID Principles**: Single responsibility, dependency injection
- **Domain-Driven Design**: Rich domain models
- **Repository Pattern**: Abstract data access
- **Unit of Work**: Transaction management

### 2. Coding Standards
- Use C# 12 features where appropriate
- Follow .NET naming conventions
- Async/await for all I/O operations
- Comprehensive XML documentation
- No hardcoded strings (use constants/resources)

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