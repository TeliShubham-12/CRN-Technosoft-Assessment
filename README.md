# RESTful Backend API Solution - Technical Assessment (.NET 8 Clean Architecture)

This repository contains a solution for managing **Products** and **Items** built with **.NET 8** following Clean Architecture principles, Entity Framework Core with SQL Server, JWT Authentication with Refresh Token Rotation, FluentValidation, Serilog structured logging, Swashbuckle Swagger documentation, xUnit test suites, and Docker containerization.

---

## Technical Stack & Frameworks

- **Framework**: .NET 8 (C#)
- **API Framework**: ASP.NET Core Web API
- **Architecture**: Clean Architecture (Onion / Layered Architecture)
- **Database & ORM**: Entity Framework Core 8 (SQL Server with fallback support for In-Memory Database for local dev/testing)
- **Authentication**: JWT Access Token (short-lived) + Refresh Token Rotation strategy
- **Validation**: FluentValidation with automatic DI registration
- **Object Mapping**: AutoMapper
- **API Versioning**: `Asp.Versioning.Http` & `Asp.Versioning.Mvc.ApiExplorer` (v1.0 configured via URL segment and headers)
- **Logging**: Serilog structured logging
- **Documentation**: Swagger / OpenAPI with Swashbuckle (Includes JWT Bearer Authorization definition)
- **Testing**: xUnit, Moq, FluentAssertions, and `WebApplicationFactory` for integration tests
- **Containerization**: Docker (Multi-stage build) & Docker Compose

---

## Project Structure

```text
Solution/
├── src/
│   ├── API/                  # ASP.NET Core Web API (Controllers, Middleware, Filters, Program.cs)
│   │   ├── Controllers/v1/   # ProductsController, ItemsController, AuthController
│   │   ├── Extensions/       # DI, Swagger, JWT, and CORS setup
│   │   ├── Middleware/       # Global ExceptionHandlingMiddleware (RFC 7807 ProblemDetails)
│   │   ├── Program.cs        # Entry point & pipeline configuration
│   │   └── appsettings.json  # Configuration
│   ├── Application/          # Application Layer (Use cases, Interfaces, DTOs, Mapping, Validators)
│   │   ├── Common/           # PagedResult<T>, PaginationParams
│   │   ├── DTOs/             # Data Transfer Objects
│   │   ├── Interfaces/       # Repositories, UnitOfWork, Services contracts
│   │   ├── Mapping/          # AutoMapper Profiles
│   │   ├── Services/         # ProductService, ItemService, AuthService
│   │   └── Validators/       # FluentValidation rules
│   ├── Domain/               # Core Domain Layer (Entities, Exceptions)
│   │   ├── Entities/         # Product, Item, User, RefreshToken, AuditableEntity, BaseEntity
│   │   └── Exceptions/       # NotFoundException, ValidationException, UnauthorizedDomainException
│   └── Infrastructure/       # Data Access, EF Core, Repositories, Identity & JWT
│       ├── Data/             # ApplicationDbContext, UnitOfWork, Repository implementations
│       │   ├── Configurations/  # EF Core Entity Configurations (Product, Item, User, RefreshToken)
│       │   └── Repositories/    # ProductRepository, ItemRepository, UserRepository
│       └── Identity/          # JwtTokenGenerator, PasswordHasher (BCrypt)
├── tests/
│   ├── API.Tests/            # WebApplicationFactory integration tests for Controllers
│   ├── Application.Tests/    # Unit tests for Application Services & Validators
│   └── Infrastructure.Tests/ # EF Core InMemory integration tests for Repositories
├── Dockerfile                # Multi-stage Docker build file
├── docker-compose.yml        # Docker Compose config for API & SQL Server 2022
└── README.md                 # Technical assessment documentation
```

---

## Database Structure & Schemas

The Entity Framework Core configurations map directly to the requested database schema:

```sql
CREATE TABLE [dbo].[Product]
(
    [Id] INT NOT NULL PRIMARY KEY IDENTITY (1,1),
    [ProductName] NVARCHAR(255) NOT NULL,
    [CreatedBy]  NVARCHAR(100) NOT NULL,
    [CreatedOn]  DATETIME NOT NULL,
    [ModifiedBy]  NVARCHAR(100) NULL,
    [ModifiedOn]  DATETIME NULL
)

CREATE TABLE [dbo].[Item]
(
    [Id] INT NOT NULL PRIMARY KEY IDENTITY (1,1),
    [ProductId] INT NOT NULL FOREIGN KEY REFERENCES Product(Id),
    [Quantity] INT NOT NULL
)
```

Additionally, user identity and refresh tokens are stored in `[dbo].[Users]` and `[dbo].[RefreshTokens]` tables.

---

## Key Features & Architecture Highlights

### 1. Resource-Oriented API & Endpoints

| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| **POST** | `/api/v1/auth/register` | Register a new user | No |
| **POST** | `/api/v1/auth/login` | Authenticate and obtain Access + Refresh Token | No |
| **POST** | `/api/v1/auth/refresh-token` | Refresh Access Token using active Refresh Token | No |
| **POST** | `/api/v1/auth/revoke-token` | Revoke a active Refresh Token | **Yes** |
| **GET** | `/api/v1/products` | Get paginated list of products (`pageNumber`, `pageSize`, `searchTerm`) | No |
| **GET** | `/api/v1/products/{id}` | Get specific product with its nested items | No |
| **GET** | `/api/v1/products/{id}/items` | Get all items linked to a specific product | No |
| **POST** | `/api/v1/products` | Create a new product (Auto populates `CreatedBy` & `CreatedOn`) | **Yes** |
| **PUT** | `/api/v1/products/{id}` | Update product details (Auto populates `ModifiedBy` & `ModifiedOn`) | **Yes** |
| **DELETE** | `/api/v1/products/{id}` | Delete a product and its associated items | **Yes** |
| **GET** | `/api/v1/items/{id}` | Get specific item details | No |
| **POST** | `/api/v1/items` | Create a new item under a product | **Yes** |
| **PUT** | `/api/v1/items/{id}` | Update quantity of an item | **Yes** |
| **DELETE** | `/api/v1/items/{id}` | Delete an item | **Yes** |

### 2. Authentication & Refresh Token Strategy
- **Short-Lived Access Tokens**: Signed JWT containing User Claims (`NameIdentifier`, `Name`, `Email`, `Role`).
- **Refresh Token Rotation**: When `/api/v1/auth/refresh-token` is invoked, the used refresh token is marked as `RevokedOn = DateTime.UtcNow`, and a new cryptographically random refresh token is issued alongside a fresh JWT.
- **Password Hashing**: Secure password hashing using `BCrypt.Net-Next`.

### 3. Global Exception Handling Middleware
All exceptions are intercepted by `ExceptionHandlingMiddleware` and formatted into standard JSON `ProblemDetails` (RFC 7807):
- `NotFoundException` -> HTTP 404 Not Found
- `ValidationException` -> HTTP 400 Bad Request (includes property error dictionary)
- `UnauthorizedDomainException` -> HTTP 401 Unauthorized
- General Unhandled Exception -> HTTP 500 Internal Server Error

---

## Local Setup & Execution Guide

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Optional for containerized run)

### Running Locally (In-Memory DB Mode)
By default, `UseInMemoryDatabase: true` is configured in `appsettings.Development.json` for effortless instant execution without requiring a local SQL Server instance.

1. **Clone the repository**:
   ```bash
   git clone <repository_url>
   cd CRN-Technosoft-Assessment
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Run the Web API project**:
   ```bash
   dotnet run --project src/API
   ```

4. **Access Swagger UI**:
   Open browser at: `https://localhost:7082/swagger` or `http://localhost:5082/swagger` (or the URL output in terminal).

---

## Running with Docker Compose (SQL Server 2022)

To test full end-to-end containerization with real SQL Server 2022:

```bash
docker-compose up --build -d
```

This starts:
1. **`sqlserver`**: Container running SQL Server 2022 on port `1433`.
2. **`api`**: Container running ASP.NET Core Web API connected to SQL Server on port `5000` (HTTP) / `5001` (HTTPS).

Access Swagger UI when running in Docker: `http://localhost:5000/swagger`.

To stop services:
```bash
docker-compose down
```

---

## Testing Strategy & Execution

The solution contains comprehensive automated tests across all 3 layers:

- **`Application.Tests`**: Unit tests verifying business logic, service rules, validation, and AutoMapper mappings using `xUnit` and `Moq`.
- **`Infrastructure.Tests`**: Persistence integration tests using EF Core `InMemory` database provider for `ProductRepository`, `ItemRepository`, and `UserRepository`.
- **`API.Tests`**: Integration tests using `WebApplicationFactory<Program>` to test HTTP endpoints, status codes, and authorization policies.

### Running all tests:

```bash
dotnet test
```

Sample output:
```text
Passed! - Failed: 0, Passed: 8, Skipped: 0, Total: 8 - Application.Tests.dll
Passed! - Failed: 0, Passed: 3, Skipped: 0, Total: 3 - Infrastructure.Tests.dll
Passed! - Failed: 0, Passed: 3, Skipped: 0, Total: 3 - API.Tests.dll
```