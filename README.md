# Product CRUD REST API

This project is a high-performance **RESTful API** developed with **ASP.NET Core (.NET 8)** following **Clean Architecture** principles. It provides a secure, scalable backend solution for managing product inventory with nested item resources, utilizing industry-standard patterns for authentication, data validation, and structured logging.

---

## Project Purpose

The primary objective of this API is to provide a standardized interface for **Product & Item Management**. It allows authorized users to perform full **CRUD** (Create, Read, Update, Delete) operations on product and item data, ensuring that sensitive information remains protected via **JWT (JSON Web Token)** authentication with **Refresh Token Rotation**.

---

## Technical Architecture

To ensure the project remains scalable, testable, and easy to maintain, it follows a strict **Clean Architecture** pattern with four isolated layers:

| Layer | Project | Responsibility |
| :--- | :--- | :--- |
| **Domain** | `src/Domain/` | Defines core entities (`Product`, `Item`, `User`, `RefreshToken`), enums, domain events, and custom exceptions. Zero external dependencies. |
| **Application** | `src/Application/` | Contains business logic contracts (service interfaces), DTOs, FluentValidation rules, and mapping extensions. Depends only on Domain. |
| **Infrastructure** | `src/Infrastructure/` | Implements data access (EF Core DbContext, Generic Repository, Unit of Work), JWT token generation, Serilog logging config, and external service adapters. |
| **API** | `src/API/` | Exposes versioned RESTful endpoints via Controllers, handles HTTP requests/responses, registers middleware (exception handling, security headers, response compression), and manages Swagger documentation. |

### Dependency Flow

```
┌─────────────────────────────────────────────────────┐
│                      API Layer                      │
│         (Controllers, Middleware, Filters)           │
└──────────────┬──────────────────────┬───────────────┘
               │                      │
               ▼                      ▼
┌──────────────────────┐  ┌───────────────────────────┐
│   Application Layer  │  │   Infrastructure Layer    │
│  (Services, DTOs,    │  │  (DbContext, Repos,       │
│   Interfaces,        │◄─│   JWT, Serilog,           │
│   Validators)        │  │   Unit of Work)           │
└──────────┬───────────┘  └───────────┬───────────────┘
           │                          │
           ▼                          ▼
┌─────────────────────────────────────────────────────┐
│                    Domain Layer                     │
│       (Entities, Enums, Events, Exceptions)         │
│              No external dependencies               │
└─────────────────────────────────────────────────────┘
```

---

## Repository Structure

```text
/
├── src/
│   ├── Domain/                    # Enterprise domain layer
│   │   ├── Entities/              # Product, Item, User, RefreshToken
│   │   ├── Enums/                 # UserRole enum
│   │   ├── Events/                # ProductCreatedEvent
│   │   └── Exceptions/            # NotFoundException
│   │
│   ├── Application/               # Business logic layer
│   │   ├── DTOs/                  # ProductDto, ItemDto, AuthDtos
│   │   ├── Interfaces/            # IGenericRepository, IUnitOfWork, IProductService, IItemService, IIdentityService
│   │   ├── Services/              # ProductService, ItemService
│   │   ├── Validators/            # FluentValidation rules for Product and Item
│   │   └── Mapping/               # Entity-to-DTO mapping extensions
│   │
│   ├── Infrastructure/            # Data access and external services
│   │   ├── Data/                  # ApplicationDbContext, EntityConfigurations
│   │   │   └── Repositories/      # GenericRepository, UnitOfWork
│   │   ├── Identity/              # IdentityService, TokenService, JwtSettings
│   │   ├── Logging/               # SerilogConfiguration
│   │   └── Services/              # DateTimeService
│   │
│   └── API/                       # Presentation layer
│       ├── Controllers/           # AuthController, ProductsController
│       ├── Extensions/            # ServiceExtensions (Swagger, ResponseCompression)
│       ├── Filters/               # ValidationFilter (FluentValidation action filter)
│       ├── Middleware/            # ExceptionHandlingMiddleware
│       ├── Program.cs             # Entry point, DI config, database seeding
│       └── Dockerfile             # Multi-stage Docker build
│
├── tests/
│   ├── Application.Tests/         # Unit tests (xUnit + Moq)
│   ├── Infrastructure.Tests/      # Repository integration tests (EF Core InMemory)
│   └── API.Tests/                 # Controller integration tests (WebApplicationFactory)
│
├── docker-compose.yml             # SQL Server + API container orchestration
├── ProductAPI.sln                 # Visual Studio Solution file
├── .gitignore                     # Standard .NET gitignore
└── README.md                      # This file
```

---

## Key Features

| Feature | Description |
| :--- | :--- |
| **Secure Auth** | JWT Bearer token authentication with refresh token rotation strategy. |
| **Role-Based Access** | Admin-only endpoints (e.g., DELETE) enforced via `[Authorize(Roles = "Admin")]`. |
| **Input Validation** | FluentValidation integrated as a global action filter for automatic request validation. |
| **Pagination & Search** | `GET /products` supports `pageNumber`, `pageSize`, and `searchTerm` query parameters. |
| **Structured Logging** | Serilog configured for console and file-based structured logging. |
| **API Versioning** | All routes are versioned (`/api/v1/...`) using `Asp.Versioning`. |
| **API Documentation** | Interactive Swagger UI available at `/swagger`. |
| **Security Headers** | `X-Frame-Options`, `X-Content-Type-Options`, `X-XSS-Protection`, `Referrer-Policy` injected on all responses. |
| **Response Compression** | ASP.NET Core response compression middleware enabled for faster payloads. |
| **Container Ready** | Docker Compose setup with SQL Server 2022 and the API running out of the box. |
| **Database Seeding** | Automatic migration, user account creation, and sample product data on first startup. |

---

## Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Quick Start (Docker)

1. **Clone the Repo:**
   ```bash
   git clone https://github.com/priyashixsarkar/dotnet-product-api
   cd dotnet-product-api
   ```

2. **Run with Docker Compose:**
   ```bash
   docker-compose up -d --build
   ```

3. **Interact:**
   Navigate to [http://localhost:5272/swagger](http://localhost:5272/swagger) to open the Swagger UI.

### Running Locally (without Docker)

1. Update the database connection string in `src/API/appsettings.json`.
2. Run the API:
   ```bash
   dotnet run --project src/API
   ```

### Seeded Test Accounts

| Username | Password | Role |
| :--- | :--- | :--- |
| `admin` | `AdminPassword123!` | Admin |
| `user` | `UserPassword123!` | User |

---

## API Endpoints

> **Base URL:** `http://localhost:5272/api/v1`

### Authentication

| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/auth/register` | Registers a new user account. Returns JWT token and refresh token. | No |
| `POST` | `/api/v1/auth/login` | Authenticates user credentials. Returns JWT token and refresh token. | No |
| `POST` | `/api/v1/auth/refresh-token` | Exchanges an expired JWT for a new one using a valid refresh token. Old refresh token is revoked. | No |
| `POST` | `/api/v1/auth/revoke-token` | Revokes a refresh token to invalidate a user session (Logout). | Yes |

**Example — Login Request:**
```json
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "AdminPassword123!"
}
```

**Example — Login Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "username": "admin",
  "role": "Admin",
  "expiresAt": "2026-06-11T06:15:00Z"
}
```

> **Note:** For all protected endpoints, include the header:
> ```
> Authorization: Bearer <your_token>
> ```

---

### Products

| Method | Endpoint | Description | Auth Required | Role |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/products` | Retrieves all products. Supports pagination (`pageNumber`, `pageSize`) and search (`searchTerm`). | No | — |
| `GET` | `/api/v1/products/{id}` | Retrieves a specific product by its ID, including its nested items. | No | — |
| `POST` | `/api/v1/products` | Creates a new product. The `createdBy` field is set from the JWT claim. | Yes | User, Admin |
| `PUT` | `/api/v1/products/{id}` | Updates an existing product's name. The `modifiedBy` field is set from the JWT claim. | Yes | User, Admin |
| `DELETE` | `/api/v1/products/{id}` | Permanently deletes a product and its associated items. | Yes | **Admin only** |

**Example — Create Product:**
```json
POST /api/v1/products
Authorization: Bearer <your_token>
Content-Type: application/json

{
  "productName": "Bluetooth Speaker"
}
```

**Example — Paginated Response:**
```json
GET /api/v1/products?pageNumber=1&pageSize=10

{
  "items": [
    {
      "id": 1,
      "productName": "Wireless Headphones",
      "createdBy": "System",
      "createdOn": "2026-06-11T04:00:00Z",
      "items": [{ "id": 1, "productId": 1, "quantity": 120 }]
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 3,
  "totalPages": 1
}
```

---

### Items (Nested under Products)

| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/products/{productId}/items` | Retrieves all items belonging to a specific product. | No |
| `GET` | `/api/v1/products/{productId}/items/{id}` | Retrieves a specific item by its ID. Validates it belongs to the given product. | No |
| `POST` | `/api/v1/products/{productId}/items` | Adds a new item (with quantity) to a product. | Yes |
| `PUT` | `/api/v1/products/{productId}/items/{id}` | Updates an item's quantity. Validates item belongs to the product. | Yes |
| `DELETE` | `/api/v1/products/{productId}/items/{id}` | Removes an item from a product. Validates item belongs to the product. | Yes |

**Example — Add Item to Product:**
```json
POST /api/v1/products/1/items
Authorization: Bearer <your_token>
Content-Type: application/json

{
  "quantity": 50
}
```

---

## Running the Tests

The project includes three test suites covering all layers:

```bash
dotnet test
```

| Test Project | Type | Description |
| :--- | :--- | :--- |
| `Application.Tests` | Unit Tests | Tests business logic using xUnit and Moq to mock repository dependencies. |
| `Infrastructure.Tests` | Integration Tests | Tests GenericRepository CRUD operations against an EF Core InMemory database. |
| `API.Tests` | Integration Tests | Tests HTTP endpoints using WebApplicationFactory to simulate real API requests with JWT auth. |

---

## Docker Configuration

The `docker-compose.yml` orchestrates two services:

| Service | Image | Port |
| :--- | :--- | :--- |
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-latest` | `1433` |
| `api` | `crn-product-api:latest` (built from `src/API/Dockerfile`) | `5272 → 8080` |

The API container waits for SQL Server to become healthy before starting, then automatically runs database migrations and seeds sample data.
