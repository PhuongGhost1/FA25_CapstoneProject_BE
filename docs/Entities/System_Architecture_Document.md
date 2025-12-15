# System Architecture Document

# Custom Map OSM Backend System

## Table of Contents

1. [Introduction](#introduction)
2. [System Overview](#system-overview)
3. [Backend Architecture](#backend-architecture)
4. [Frontend Architecture](#frontend-architecture)
5. [Database Design](#database-design)
6. [Third-Party Integrations](#third-party-integrations)
7. [Deployment Architecture](#deployment-architecture)
8. [Security Architecture](#security-architecture)
9. [Text-Based Diagrams](#text-based-diagrams)

---

## Introduction

This document outlines the high-level system architecture for the Custom Map OSM Backend System. The system follows a layered architecture pattern with clear separation of concerns, supporting scalability, maintainability, and security requirements.

---

## System Overview

The Custom Map OSM Backend System is a comprehensive mapping platform built with:

- **Backend**: .NET 8 Web API with Clean Architecture
- **Frontend**: Next.js 14 with TypeScript
- **Database**: MySQL 8.0 with Entity Framework Core
- **Authentication**: JWT with ASP.NET Core Identity
- **Payment**: PayOS, Stripe, VNPay integration
- **Maps**: OpenStreetMap integration
- **Deployment**: Docker containers with Docker Compose

---

## Backend Architecture

### Layer Architecture (Clean Architecture)

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                       │
├─────────────────────────────────────────────────────────────┤
│  • Controllers/Endpoints                                    │
│  • Middleware (Authentication, Logging, Exception)         │
│  • DTOs and Request/Response Models                         │
│  • Validation Attributes                                    │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                    APPLICATION LAYER                        │
├─────────────────────────────────────────────────────────────┤
│  • Use Cases / Features                                     │
│  • Application Services                                     │
│  • Command/Query Handlers (CQRS)                           │
│  • DTOs and ViewModels                                      │
│  • Validation Logic                                         │
│  • Background Jobs (Hangfire)                              │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                      DOMAIN LAYER                           │
├─────────────────────────────────────────────────────────────┤
│  • Entities                                                 │
│  • Value Objects                                            │
│  • Domain Services                                          │
│  • Business Rules                                           │
│  • Domain Events                                            │
│  • Interfaces (Repository, Services)                       │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                   INFRASTRUCTURE LAYER                      │
├─────────────────────────────────────────────────────────────┤
│  • Data Access (Entity Framework Core)                     │
│  • External Services (Payment, Email, Maps)                │
│  • File Storage (Local/AWS S3)                             │
│  • Caching (Redis)                                          │
│  • Logging (Serilog)                                        │
│  • Background Jobs Implementation                           │
└─────────────────────────────────────────────────────────────┘
```

### Backend Technology Stack

| Component           | Technology                   | Purpose                               |
| ------------------- | ---------------------------- | ------------------------------------- |
| **Framework**       | .NET 8 Web API               | Main application framework            |
| **ORM**             | Entity Framework Core 8      | Database access and migrations        |
| **Authentication**  | ASP.NET Core Identity + JWT  | User authentication and authorization |
| **Validation**      | FluentValidation             | Input validation                      |
| **Logging**         | Serilog                      | Application logging                   |
| **Caching**         | Redis                        | Distributed caching                   |
| **Background Jobs** | Hangfire                     | Asynchronous job processing           |
| **File Processing** | NetTopologySuite             | Geospatial data processing            |
| **PDF Generation**  | iTextSharp                   | Export functionality                  |
| **Email**           | SendGrid/SMTP                | Email notifications                   |
| **Testing**         | xUnit, Moq, FluentAssertions | Unit and integration testing          |

---

## Frontend Architecture

### Next.js 14 Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                       │
├─────────────────────────────────────────────────────────────┤
│  • Pages (App Router)                                       │
│  • Components (React)                                       │
│  • UI Library (Tailwind CSS + Shadcn/ui)                   │
│  • State Management (Zustand/Redux Toolkit)                │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                    APPLICATION LAYER                        │
├─────────────────────────────────────────────────────────────┤
│  • API Services (Axios/Fetch)                              │
│  • Custom Hooks                                            │
│  • Context Providers                                       │
│  • Form Handling (React Hook Form)                         │
│  • Validation (Zod)                                        │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                      DOMAIN LAYER                           │
├─────────────────────────────────────────────────────────────┤
│  • Type Definitions (TypeScript)                           │
│  • Business Logic                                          │
│  • Constants and Enums                                     │
│  • Utility Functions                                       │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                   INFRASTRUCTURE LAYER                      │
├─────────────────────────────────────────────────────────────┤
│  • HTTP Client Configuration                               │
│  • Local Storage Management                                │
│  • File Upload Handling                                    │
│  • Map Integration (Leaflet/MapBox)                        │
│  • External API Integrations                               │
└─────────────────────────────────────────────────────────────┘
```

### Frontend Technology Stack

| Component            | Technology                   | Purpose                         |
| -------------------- | ---------------------------- | ------------------------------- |
| **Framework**        | Next.js 14                   | React framework with App Router |
| **Language**         | TypeScript                   | Type-safe JavaScript            |
| **Styling**          | Tailwind CSS                 | Utility-first CSS framework     |
| **UI Components**    | Shadcn/ui                    | Pre-built component library     |
| **State Management** | Zustand                      | Lightweight state management    |
| **Forms**            | React Hook Form + Zod        | Form handling and validation    |
| **HTTP Client**      | Axios                        | API communication               |
| **Maps**             | Leaflet + React-Leaflet      | Interactive maps                |
| **Charts**           | Recharts                     | Data visualization              |
| **Testing**          | Jest + React Testing Library | Unit and integration testing    |

---

## Database Design

### Database Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    DATABASE LAYER                           │
├─────────────────────────────────────────────────────────────┤
│  • MySQL 8.0 (Primary Database)                            │
│  • Redis (Caching & Sessions)                              │
│  • File Storage (Local/AWS S3)                             │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                    DATA ACCESS LAYER                        │
├─────────────────────────────────────────────────────────────┤
│  • Entity Framework Core 8                                 │
│  • Repository Pattern                                      │
│  • Unit of Work Pattern                                    │
│  • Database Migrations                                     │
│  • Connection Pooling                                      │
└─────────────────────────────────────────────────────────────┘
```

### Database Schema Overview

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│      Users      │    │ Organizations   │    │      Maps       │
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ • UserId (PK)   │    │ • OrgId (PK)    │    │ • MapId (PK)    │
│ • Email         │    │ • OrgName       │    │ • MapName       │
│ • PasswordHash  │    │ • OwnerUserId   │    │ • UserId (FK)   │
│ • FullName      │    │ • Description   │    │ • OrgId (FK)    │
│ • RoleId (FK)   │    │ • CreatedAt     │    │ • IsPublic      │
│ • CreatedAt     │    │ • IsActive      │    │ • CreatedAt     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Memberships   │    │   Layers        │    │  Annotations    │
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ • MembershipId  │    │ • LayerId (PK)  │    │ • AnnotationId  │
│ • UserId (FK)   │    │ • LayerName     │    │ • MapId (FK)    │
│ • OrgId (FK)    │    │ • LayerType     │    │ • Geometry      │
│ • PlanId (FK)   │    │ • SourceType    │    │ • Properties    │
│ • StartDate     │    │ • FilePath      │    │ • CreatedAt     │
│ • EndDate       │    │ • IsPublic      │    │                 │
│ • StatusId (FK) │    │ • CreatedAt     │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

---

## Third-Party Integrations

### External Services Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                 EXTERNAL SERVICES LAYER                     │
├─────────────────────────────────────────────────────────────┤
│  • Payment Gateways                                         │
│  • Email Services                                           │
│  • Mapping Services                                         │
│  • File Storage Services                                    │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                    INTEGRATION LAYER                        │
├─────────────────────────────────────────────────────────────┤
│  • Service Interfaces                                       │
│  • Adapter Pattern Implementation                           │
│  • Configuration Management                                 │
│  • Error Handling & Retry Logic                            │
└─────────────────────────────────────────────────────────────┘
```

### Third-Party Services

| Service                | Provider             | Purpose                | Integration Method  |
| ---------------------- | -------------------- | ---------------------- | ------------------- |
| **Payment Processing** | PayOS, Stripe, VNPay | Membership payments    | REST API + Webhooks |
| **Email Service**      | SendGrid/SMTP        | Notifications          | REST API            |
| **Maps & Geospatial**  | OpenStreetMap        | Base map data          | REST API + Tiles    |
| **File Storage**       | AWS S3 (Optional)    | Layer files, exports   | REST API            |
| **Caching**            | Redis                | Session & data caching | Redis Protocol      |
| **Monitoring**         | Application Insights | Application monitoring | SDK Integration     |

---

## Deployment Architecture

### Container Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    DOCKER ENVIRONMENT                       │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
│  │   Frontend  │  │   Backend   │  │   Database  │         │
│  │  (Next.js)  │  │ (.NET API)  │  │   (MySQL)   │         │
│  │   Port:3000 │  │  Port:5000  │  │  Port:3306  │         │
│  └─────────────┘  └─────────────┘  └─────────────┘         │
│                                                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
│  │    Redis    │  │  Hangfire   │  │   Nginx     │         │
│  │  (Cache)    │  │ (Jobs)      │  │ (Reverse    │         │
│  │  Port:6379  │  │ Port:5001   │  │  Proxy)     │         │
│  └─────────────┘  └─────────────┘  └─────────────┘         │
└─────────────────────────────────────────────────────────────┘
```

### Docker Compose Services

```yaml
services:
  frontend:
    build: ./frontend
    ports: ["3000:3000"]
    environment:
      - NEXT_PUBLIC_API_URL=http://backend:5000

  backend:
    build: ./backend
    ports: ["5000:5000"]
    environment:
      - ConnectionStrings__DefaultConnection=Server=mysql;Database=CustomMapOSM;Uid=root;Pwd=password;
      - Redis__ConnectionString=redis:6379
    depends_on: [mysql, redis]

  mysql:
    image: mysql:8.0
    ports: ["3306:3306"]
    environment:
      - MYSQL_ROOT_PASSWORD=password
      - MYSQL_DATABASE=CustomMapOSM
    volumes: ["./mysql-data:/var/lib/mysql"]

  redis:
    image: redis:7-alpine
    ports: ["6379:6379"]

  hangfire:
    build: ./backend
    command: ["dotnet", "run", "--project", "HangfireServer"]
    environment:
      - ConnectionStrings__DefaultConnection=Server=mysql;Database=CustomMapOSM;Uid=root;Pwd=password;
    depends_on: [mysql, redis]
```

---

## Security Architecture

### Security Layers

```
┌─────────────────────────────────────────────────────────────┐
│                    SECURITY LAYERS                          │
├─────────────────────────────────────────────────────────────┤
│  • HTTPS/TLS Encryption                                    │
│  • JWT Authentication                                       │
│  • Role-Based Authorization                                │
│  • Input Validation & Sanitization                         │
│  • SQL Injection Prevention                                │
│  • XSS Protection                                          │
│  • CSRF Protection                                         │
│  • Rate Limiting                                           │
│  • Data Encryption at Rest                                 │
└─────────────────────────────────────────────────────────────┘
```

### Security Implementation

| Security Aspect      | Implementation         | Technology                 |
| -------------------- | ---------------------- | -------------------------- |
| **Authentication**   | JWT Tokens             | ASP.NET Core Identity      |
| **Authorization**    | Role-based             | Policy-based authorization |
| **Data Encryption**  | AES-256                | .NET Cryptography          |
| **Password Hashing** | BCrypt                 | ASP.NET Core Identity      |
| **HTTPS**            | TLS 1.3                | Nginx/Reverse Proxy        |
| **Rate Limiting**    | Request throttling     | ASP.NET Core Middleware    |
| **Input Validation** | Server-side validation | FluentValidation           |
| **SQL Injection**    | Parameterized queries  | Entity Framework Core      |

---

## Text-Based Diagrams

### 1. Overall System Architecture

```
                    ┌─────────────────────────────────────┐
                    │           CLIENT LAYER              │
                    │  ┌─────────────┐  ┌─────────────┐   │
                    │  │   Web App   │  │  Mobile App │   │
                    │  │  (Next.js)  │  │  (Future)   │   │
                    │  └─────────────┘  └─────────────┘   │
                    └─────────────────────────────────────┘
                                    │
                                    │ HTTPS/REST API
                                    ▼
                    ┌─────────────────────────────────────┐
                    │        PRESENTATION LAYER           │
                    │  ┌─────────────┐  ┌─────────────┐   │
                    │  │   API       │  │  WebSocket  │   │
                    │  │  Gateway    │  │  (Real-time)│   │
                    │  └─────────────┘  └─────────────┘   │
                    └─────────────────────────────────────┘
                                    │
                                    ▼
                    ┌─────────────────────────────────────┐
                    │        APPLICATION LAYER            │
                    │  ┌─────────────┐  ┌─────────────┐   │
                    │  │   Business  │  │ Background  │   │
                    │  │   Logic     │  │    Jobs     │   │
                    │  └─────────────┘  └─────────────┘   │
                    └─────────────────────────────────────┘
                                    │
                                    ▼
                    ┌─────────────────────────────────────┐
                    │       INFRASTRUCTURE LAYER          │
                    │  ┌─────────────┐  ┌─────────────┐   │
                    │  │   Database  │  │   External  │   │
                    │  │   (MySQL)   │  │  Services   │   │
                    │  └─────────────┘  └─────────────┘   │
                    └─────────────────────────────────────┘
```

### 2. Data Flow Architecture

```
    User Request
         │
         ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │───▶│   API Gateway   │───▶│   Backend API   │
│   (Next.js)     │    │   (Nginx)       │    │   (.NET 8)      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                                       │
                                                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   External      │◀───│   Service       │◀───│   Application   │
│   Services      │    │   Layer         │    │   Layer         │
│   (PayOS, OSM)  │    │   (Adapters)    │    │   (Use Cases)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                                       │
                                                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   File Storage  │◀───│   Data Access   │◀───│   Domain        │
│   (Local/S3)    │    │   Layer (EF)    │    │   Layer         │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                                ▼
                       ┌─────────────────┐
                       │   Database      │
                       │   (MySQL)       │
                       └─────────────────┘
```

### 3. Authentication Flow

```
    User Login
         │
         ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │───▶│   Backend API   │───▶│   Identity      │
│   (Login Form)  │    │   (Auth Endpoint)│    │   Service       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       ▼
         │                       │              ┌─────────────────┐
         │                       │              │   Database      │
         │                       │              │   (User Store)  │
         │                       │              └─────────────────┘
         │                       │                       │
         │                       ▼                       │
         │              ┌─────────────────┐              │
         │              │   JWT Token     │              │
         │              │   Generation    │              │
         │              └─────────────────┘              │
         │                       │                       │
         │                       ▼                       │
         │              ┌─────────────────┐              │
         │              │   Response      │              │
         │              │   (Token + User)│              │
         │              └─────────────────┘              │
         │                       │                       │
         ▼                       │                       │
┌─────────────────┐              │                       │
│   Token Storage │◀─────────────┘                       │
│   (Local Storage)│                                     │
└─────────────────┘                                     │
         │                                               │
         ▼                                               │
┌─────────────────┐                                     │
│   Authenticated │                                     │
│   Requests      │─────────────────────────────────────┘
│   (Bearer Token)│
└─────────────────┘
```

### 4. Payment Integration Flow

```
    Payment Request
         │
         ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │───▶│   Backend API   │───▶│   Payment       │
│   (Checkout)    │    │   (Payment      │    │   Service       │
│                 │    │    Endpoint)    │    │   (Adapter)     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                                       │
                                                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Payment       │◀───│   Payment       │◀───│   Gateway       │
│   Gateway       │    │   Gateway       │    │   Selection     │
│   (PayOS/Stripe)│    │   (PayOS/Stripe)│    │   (Strategy)    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       ▼
         │                       │              ┌─────────────────┐
         │                       │              │   Transaction   │
         │                       │              │   Recording     │
         │                       │              └─────────────────┘
         │                       │                       │
         │                       ▼                       │
         │              ┌─────────────────┐              │
         │              │   Webhook       │              │
         │              │   Processing    │              │
         │              └─────────────────┘              │
         │                       │                       │
         │                       ▼                       │
         │              ┌─────────────────┐              │
         │              │   Membership    │              │
         │              │   Activation    │              │
         │              └─────────────────┘              │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Payment       │    │   User          │    │   Database      │
│   Confirmation  │    │   Notification  │    │   Update        │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### 5. Map Processing Flow

```
    Map Creation Request
         │
         ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │───▶│   Backend API   │───▶│   Map Service   │
│   (Map Editor)  │    │   (Map          │    │   (Use Case)    │
│                 │    │    Endpoint)    │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                                       │
                                                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Layer         │◀───│   Layer         │◀───│   Map           │
│   Processing    │    │   Service       │    │   Validation    │
│   (File Upload) │    │   (Use Case)    │    │   (Business     │
└─────────────────┘    └─────────────────┘    │    Rules)       │
         │                       │              └─────────────────┘
         │                       │                       │
         │                       ▼                       │
         │              ┌─────────────────┐              │
         │              │   Geospatial    │              │
         │              │   Processing    │              │
         │              │   (NetTopology) │              │
         │              └─────────────────┘              │
         │                       │                       │
         │                       ▼                       │
         │              ┌─────────────────┐              │
         │              │   File Storage  │              │
         │              │   (Local/S3)    │              │
         │              └─────────────────┘              │
         │                       │                       │
         │                       ▼                       │
         │              ┌─────────────────┐              │
         │              │   Database      │              │
         │              │   (Map Data)    │              │
         │              └─────────────────┘              │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Layer         │    │   Map           │    │   Usage         │
│   Metadata      │    │   Configuration │    │   Tracking      │
│   Storage       │    │   Storage       │    │   Update        │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

---

## Document Information

**Document Version**: 1.0  
**Last Updated**: [Current Date]  
**Author**: System Architect  
**Reviewers**: Technical Lead, DevOps Engineer, Security Engineer  
**Approval Status**: Draft

**Change Log**:

- v1.0: Initial system architecture document creation
- Defined layered architecture for backend (.NET 8)
- Outlined frontend architecture (Next.js 14)
- Specified database design and third-party integrations
- Created text-based diagrams for draw.io visualization
- Included security and deployment considerations

**Next Steps**:

1. Technical team review and validation
2. Infrastructure setup and configuration
3. Development environment setup
4. CI/CD pipeline configuration
5. Security audit and compliance review

**Implementation Priority**:

1. **Phase 1**: Core backend API with authentication
2. **Phase 2**: Frontend application with basic features
3. **Phase 3**: Payment integration and membership system
4. **Phase 4**: Advanced mapping features and collaboration
5. **Phase 5**: Performance optimization and scaling
