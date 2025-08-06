CustomMapOSM - Architecture Design

1. Layered Architecture:
┌──────────────────────┐
│         API          │  - REST controllers
├──────────────────────┤
│    Application       │  - Business logic services
├──────────────────────┤
│      Domain          │  - Core business models
├──────────────────────┤
│   Infrastructure     │  - Data persistence
├──────────────────────┤
│       Shared         │  - Common utilities
└──────────────────────┘

2. Component Mapping:
- API Layer: 
  • MapController.cs
  • ExportController.cs
  • PaymentController.cs
  
- Application Layer:
  • MapService.cs (map operations)
  • ExportService.cs (file generation)
  • PaymentService.cs (transaction handling)
  
- Domain Layer:
  • Map.cs (map entity)
  • Layer.cs (layer entity)
  • Organization.cs
  
- Infrastructure Layer:
  • MapRepository.cs (DB access)
  • FileStorageService.cs (MinIO integration)
  • PaymentGatewayAdapter.cs
  
- Shared Layer:
  • SpatialUtils.cs (geo calculations)
  • Logger.cs (unified logging)
  • AuthHelper.cs (JWT handling)

3. Key Flows:
3.1 Map Export Flow:
Frontend → ExportController → ExportService → 
FileStorageService → TransactionService

3.2 Collaboration Flow:
Frontend → MapController → MapService → 
MapRepository → NotificationService

4. Technology Stack:
- API: ASP.NET Core 9 Web API
- Application: C# 12
- Domain: POCO Entities
- Infrastructure: Entity Framework Core 9, Pomelo MySQL
- Shared: .NET Common Libraries