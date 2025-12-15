# CustomMapOSM - Backend (IMOS)

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=.net)
![Docker](https://img.shields.io/badge/Docker-Supported-2496ED?style=flat&logo=docker)
![License](https://img.shields.io/badge/License-MIT-green.svg)

**Interactive Map for Open Source (IMOS)** - Backend API for an educational interactive mapping platform that enables educators to create, customize, and share interactive maps with curriculum-aligned content.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Development](#development)
- [API Documentation](#api-documentation)
- [Database](#database)
- [Testing](#testing)
- [Deployment](#deployment)
- [Documentation](#documentation)
- [Contributing](#contributing)

## 🎯 Overview

CustomMapOSM Backend is a robust RESTful API service built with .NET 8.0 that powers the IMOS platform. It provides comprehensive functionality for:

- **User & Organization Management**: Multi-tenant architecture with role-based access control
- **Interactive Map Creation**: Create and manage custom maps with OpenStreetMap integration
- **Geospatial Data Processing**: Handle GeoJSON, KML, Shapefile formats
- **Story Mapping**: Create narrative-driven educational content with timeline features
- **Collaboration Tools**: Share maps and manage permissions across teams
- **Payment Integration**: Subscription management with VNPay, PayPal, and Stripe
- **Export Services**: Generate maps in multiple formats (PDF, PNG, SVG, GeoJSON)

## ✨ Features

### Core Functionality

#### 👥 User Management
- User registration with email verification
- JWT-based authentication & authorization
- Password reset functionality
- Role-Based Access Control (Guest, Registered User, Admin)
- Profile management

#### 🏢 Organization Management
- Create and manage organizations
- Multi-role member system (Owner, Admin, Member, Viewer)
- Organization invitations and permissions
- Team collaboration features

#### 🗺️ Map & Layer Management
- Create and customize interactive maps
- Multiple layer types support (Vector, Raster, WMS)
- Layer styling and ordering
- Map feature annotations (markers, lines, polygons)
- Reusable map templates

#### 📍 Location & Zone Management
- Point of Interest (POI) management
- Location types and tags
- Zone definitions with polygon support
- Zone tagging and categorization

#### 📖 Story Mapping
- Create narrative-driven story maps
- Timeline-based events
- Multi-segment stories
- Media integration (images, videos)
- Animation support between segments
- Embeddable story map widgets

#### 💳 Payment & Subscription
- Multiple payment gateways (VNPay, PayPal, Stripe, PayOS)
- Subscription plan management
- Usage quota tracking
- Payment transaction history

#### 📤 Export System
- Multi-format export (PDF, PNG, SVG, GeoJSON, KML, MBTiles)
- Resolution options (72-300 DPI)
- Quota-based export limits
- Export history tracking

## 🛠️ Technology Stack

### Core Framework
- **.NET 8.0** - Modern cross-platform framework
- **ASP.NET Core Minimal API** - High-performance REST API
- **Entity Framework Core** - ORM for data access
- **AutoMapper** - Object-object mapping

### Databases
- **MySQL 8.0** - Primary relational database (Production: Remote server)
- **MongoDB Atlas** - Cloud-hosted document database for geospatial data
- **Redis** - Caching and session management (with authentication)

### Authentication & Security
- **JWT Bearer** - Token-based authentication
- **BCrypt** - Password hashing
- **Firebase Admin** - Firebase authentication and storage integration

### Payment Gateways
- **Stripe** - International payment processing
- **PayOS** - Vietnamese payment gateway
- **VNPay** - Vietnamese payment gateway (configurable)

### Cloud & Storage
- **Firebase Storage** - File upload and storage
- **Azure Container Registry** - Docker image hosting (planned)

### Additional Services
- **Gmail SMTP** - Email notifications
- **Swagger/OpenAPI** - API documentation
- **Serilog** - Structured logging

## 🏗️ Architecture

The project follows **Clean Architecture** principles with clear separation of concerns:

```
FA25_CusomMapOSM_BE/
├── CusomMapOSM_API/              # Presentation Layer
│   ├── Endpoints/                # Minimal API endpoints
│   ├── Middlewares/              # HTTP middleware
│   ├── Extensions/               # Service extensions
│   └── Program.cs                # Application entry point
│
├── CusomMapOSM_Application/      # Application Layer
│   ├── Services/                 # Business logic services
│   ├── DTOs/                     # Data Transfer Objects
│   ├── Validators/               # Input validation
│   └── Mappings/                 # AutoMapper profiles
│
├── CusomMapOSM_Domain/           # Domain Layer
│   ├── Entities/                 # Domain entities
│   ├── Interfaces/               # Repository interfaces
│   └── Enums/                    # Domain enumerations
│
├── CusomMapOSM_Infrastructure/   # Infrastructure Layer
│   ├── Repositories/             # Data access implementation
│   ├── Data/                     # EF Core DbContext
│   ├── Migrations/               # Database migrations
│   └── Services/                 # External service implementations
│
├── CusomMapOSM_Commons/          # Shared Layer
│   ├── Utilities/                # Helper utilities
│   └── Constants/                # Application constants
│
└── CusomMapOSM_*.Tests/          # Test Projects
    ├── Unit/                     # Unit tests
    └── Integration/              # Integration tests
```

### Design Patterns
- **Repository Pattern** - Data access abstraction
- **Dependency Injection** - Loose coupling
- **CQRS** - Command Query Responsibility Segregation
- **Unit of Work** - Transaction management

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for local development)
- Git

### Installation

1. **Clone the repository**
```powershell
git clone <repository-url>
cd FA25_CapstoneProject_BE/FA25_CusomMapOSM_BE
```

2. **Configure environment variables**
```powershell
cp env.example .env
# Edit .env file with your configuration (see .env.example for reference)
```

3. **Setup Firebase credentials**
```powershell
# Place your firebase-service-account.json in the root directory
# Update FIREBASE_CREDENTIALS_PATH in .env if needed
```

4. **Choose your deployment method:**

#### Option A: Production Environment (Recommended)
The application is configured to run with production databases:

- **MySQL**: Remote server (103.157.205.191)
- **MongoDB**: MongoDB Atlas cluster
- **Redis**: Local Redis instance with authentication

```powershell
# For production deployment
dotnet build --configuration Release
dotnet run --project CusomMapOSM_API --configuration Release
```

#### Option B: Docker Development Environment
```powershell
# Start local infrastructure (MySQL, MongoDB, Redis)
docker-compose up -d mysql mongodb redis

# Update connection strings in .env to use localhost services
# DATABASE_CONNECTION_STRING=Server=localhost;Port=3306;Database=imos;User=imosuser;Password=imospassword;
# MONGO_CONNECTION_STRING=mongodb://localhost:27017
# REDIS_CONNECTION_STRING=localhost:6379

# Run the API
make start
```

#### Option C: Local Development (Without Docker)
```powershell
# Ensure local services are running:
# - MySQL on port 3306
# - MongoDB on port 27017
# - Redis on port 6379

# Update .env with local connection strings
# Run migrations
make migration

# Start development server
make start
```

5. **Verify installation**
```powershell
# API should be running at http://localhost:5000
# Swagger documentation: http://localhost:5000/swagger
# Frontend: https://custommaposm.vercel.app
```

## 💻 Development

### Using Makefile Commands

```powershell
# Start development server with hot reload
make start

# Build the project
make build

# Run tests
make test

# Create new migration
make new-migration name=YourMigrationName

# Apply migrations
make migration

# Build Docker image
make docker

# Deploy to production
make deploy
```

### Database Migrations

```powershell
# Create a new migration
dotnet ef migrations add MigrationName --project CusomMapOSM_Infrastructure --startup-project CusomMapOSM_API

# Update database
dotnet ef database update --project CusomMapOSM_Infrastructure --startup-project CusomMapOSM_API

# Rollback to specific migration
dotnet ef database update MigrationName --project CusomMapOSM_Infrastructure --startup-project CusomMapOSM_API
```

### Project Structure

```powershell
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run specific project
dotnet run --project CusomMapOSM_API

# Watch for changes
dotnet watch --project CusomMapOSM_API
```

## 📚 API Documentation

### Swagger/OpenAPI
Once the application is running, access interactive API documentation at:
- **Swagger UI**: `http://localhost:5000/swagger`
- **OpenAPI JSON**: `http://localhost:5000/swagger/v1/swagger.json`

### Main API Endpoints

#### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/reset-password` - Reset password

#### Users
- `GET /api/users/profile` - Get current user profile
- `PUT /api/users/profile` - Update user profile
- `GET /api/users` - List all users (Admin)

#### Organizations
- `POST /api/organizations` - Create organization
- `GET /api/organizations` - List organizations
- `GET /api/organizations/{id}` - Get organization details
- `POST /api/organizations/{id}/invite` - Invite member
- `PUT /api/organizations/{id}/members/{userId}` - Update member role

#### Maps
- `POST /api/maps` - Create new map
- `GET /api/maps` - List user maps
- `GET /api/maps/{id}` - Get map details
- `PUT /api/maps/{id}` - Update map
- `DELETE /api/maps/{id}` - Delete map

#### Layers
- `POST /api/layers` - Create layer
- `GET /api/layers/{id}` - Get layer data
- `PUT /api/layers/{id}` - Update layer
- `DELETE /api/layers/{id}` - Delete layer

#### Story Maps
- `POST /api/storymaps` - Create story map
- `GET /api/storymaps/{id}` - Get story map
- `POST /api/storymaps/{id}/segments` - Add segment
- `POST /api/storymaps/{id}/publish` - Publish story map

#### Payments
- `POST /api/payments/create-payment` - Create payment
- `GET /api/payments/history` - Get payment history
- `POST /api/payments/webhook/{provider}` - Payment webhooks

#### Exports
- `POST /api/exports` - Create export job
- `GET /api/exports/{id}` - Get export status
- `GET /api/exports/{id}/download` - Download export

For complete API documentation, refer to [docs/02_Software-Requirement-Specification_SRS.md](docs/02_Software-Requirement-Specification_SRS.md)

## 🗄️ Database

### Database Configuration

The project uses **MySQL** for relational data and **MongoDB Atlas** for geospatial data:

**Production Setup:**
- **MySQL**: Remote server (103.157.205.191:3306) - `imos` database
- **MongoDB**: Atlas cluster (imoscluster.akqslsi.mongodb.net) - `imos_mongo` database
- **Redis**: Local instance with password authentication

**Development Setup:**
- Local MySQL/MongoDB/Redis via Docker Compose
- Connection strings configured in `.env` file

### Database Schema

**MySQL Tables:**
- Users, Roles, Permissions
- Organizations, OrganizationMembers
- Maps, Layers, MapFeatures
- Locations, Zones, Tags
- StoryMaps, Segments, Events
- Subscriptions, Payments, Transactions
- Templates, Exports

**MongoDB Collections:**
- `layer_data` - Geospatial layer data
- `map_features` - Map feature geometries
- `map_history` - Version history
- `segment_locations` - Story map locations

### Entity Relationship Diagram
See [docs/logical-erd.mmd](docs/logical-erd.mmd) for complete database schema.

### Database Migrations

```powershell
# Create a new migration
dotnet ef migrations add MigrationName --project CusomMapOSM_Infrastructure --startup-project CusomMapOSM_API

# Update database (uses connection string from .env)
dotnet ef database update --project CusomMapOSM_Infrastructure --startup-project CusomMapOSM_API
```

### Sample Data
```powershell
# Load sample data (Development only)
dotnet run --project CusomMapOSM_API -- seed-data
```

## 🧪 Testing

### Run Tests
```powershell
# Run all tests
dotnet test

# Run specific test project
dotnet test CusomMapOSM_API.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure
- **Unit Tests**: Testing business logic in isolation
- **Integration Tests**: Testing API endpoints with test database
- **Repository Tests**: Testing data access layer

## 🚢 Deployment

### Current Production Configuration

The application is currently configured for production deployment with:

- **Database**: MySQL 8.0 on remote server (103.157.205.191:3306)
- **MongoDB**: MongoDB Atlas cluster (imoscluster.akqslsi.mongodb.net)
- **Redis**: Local Redis with authentication
- **Frontend**: Deployed on Vercel (https://custommaposm.vercel.app)
- **Payment Gateways**: Stripe (test), PayOS configured
- **Email**: Gmail SMTP configured

### Docker Deployment

```powershell
# Build production image
make docker

# For production deployment
docker run -d \
  --name imos_api \
  -p 5000:5000 \
  --env-file .env \
  -v $(pwd)/firebase-service-account.json:/app/firebase-service-account.json \
  imos_server:latest
```

### Azure Deployment

```powershell
# Deploy to Azure using PowerShell script
make deploy

# Or manually using deploy-config.json
az webapp deploy --resource-group <rg-name> --name <app-name> --src-path ./publish
```

### Environment Configuration

Based on the current `.env` file, ensure the following are properly configured:

```bash
# Database (Production)
DATABASE_CONNECTION_STRING=

# MongoDB Atlas
MONGO_CONNECTION_STRING=

# Redis (Local with auth)
REDIS_CONNECTION_STRING=localhost:6379,password=superSecretRedisPassword,ssl=false,abortConnect=false

# Firebase
FIREBASE_STORAGE_BUCKET=
FIREBASE_CREDENTIALS_PATH=/app/firebase-service-account.json

# Payment Gateways (Test Environment)
STRIPE_SECRET=sk_test_...
STRIPE_PUBLISHABLE_KEY=pk_test_...
PAYOS_CLIENT_ID=
PAYOS_API_KEY=
PAYOS_CHECKSUM_KEY=

# Email
SMTP_SERVER=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=
SMTP_PASS=[APP_PASSWORD]

# Frontend
FRONTEND_ORIGINS=http://localhost:3000,http://localhost:5173,http://localhost:5233,file://,null,https://imos.vercel.app
FRONTEND_BASE_URL=https://custommaposm.vercel.app
```

### Production Checklist

- [ ] Update database passwords to production values
- [ ] Configure production Redis instance
- [ ] Set up production SMTP credentials
- [ ] Configure production payment gateway keys
- [ ] Update CORS origins for production domain
- [ ] Set up SSL/TLS certificates
- [ ] Configure logging and monitoring
- [ ] Set up backup strategies for databases

## 📖 Documentation

Comprehensive documentation is available in the `/docs` folder:

- [User Requirements (URD)](docs/01_User-Requirement-Document_URD.md)
- [Software Requirements (SRS)](docs/02_Software-Requirement-Specification_SRS.md)
- [Architecture Design (ADD)](docs/03_Architecture-Design-Document_ADD.md)
- [Detailed Design (DDD)](docs/04_Detailed-Design-Document_DDD.md)
- [Implementation Guide (SID)](docs/05_System-Implementation-Document_SID.md)
- [Testing Document (TD)](docs/06_Testing-Document_TD.md)
- [Installation Guide (IG)](docs/07_Installation-Guide_IG.md)
- [Database Documentation (DB)](docs/08_Database_DB.md)
- [Business Rules (BR)](docs/09_Business-Rules_BR.md)
- [Features List](docs/11_Features.md)
- [Screen Flow](docs/12_Screen_Flow.md)

## 🤝 Contributing

We welcome contributions! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards
- Follow C# coding conventions
- Write unit tests for new features
- Update documentation as needed
- Use meaningful commit messages

## 📝 License

This project is developed as part of the FA25 Capstone Project.

## 👥 Team

**FA25 Capstone Project Team**

---

## 📧 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Refer to [documentation](docs/)

---

**Last Updated**: December 2025