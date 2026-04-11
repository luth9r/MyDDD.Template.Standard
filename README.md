# MyDDD.Template.Standard

A modern, production-ready .NET 10 template based on Domain-Driven Design (DDD) and Command Query Responsibility Segregation (CQRS) principles, orchestrated with .NET Aspire.

## 🚀 Key Features

- **Clean Architecture**: Strict separation of concerns across Domain, Application, Infrastructure, and Api layers.
- **DDD Building Blocks**: Implementation of Entities, Aggregate Roots, Value Objects, and Domain Events.
- **CQRS with MediatR**: Fully decoupled commands and queries with native MediatR integration.
- **.NET Aspire Orchestration**: Seamless local development experience with Postgres, Redis, Keycloak, and Seq integration.
- **Advanced Observability**: Built-in OpenTelemetry tracing and Serilog logging (Seq).
- **Modern API Standards**: Minimal APIs, API Versioning, and Scalar (OpenAPI) documentation.
- **Robust Testing Strategy**: xUnit unit tests for Domain/Application and Aspire-based integration tests.

## 🏗️ Project Structure

- **src/MyDDD.Template.Domain**: Core business logic, entities, and domain events. No external dependencies.
- **src/MyDDD.Template.Application**: Use cases, MediatR handlers, and application abstractions.
- **src/MyDDD.Template.Infrastructure**: Persistence (EF Core), Authentication (Keycloak), and external service implementations.
- **src/MyDDD.Template.Api**: Minimal API endpoints, configuration, and entry point.
- **infra/MyDDD.Template.AppHost**: Aspire orchestration project.
- **infra/MyDDD.Template.ServiceDefaults**: Shared service configuration (OTEL, Health Checks).
- **tests/**: Unit and Integration test suites.

## 🛠️ Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or [Podman](https://podman.io/) (for Aspire)
- [dotnet aspire workload](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling)

## 🏁 Getting Started

1. **Clone the repository**:
   ```bash
   git clone https://github.com/your-repo/MyDDD.Template.Standard.git
   cd MyDDD.Template.Standard
   ```

2. **Run the application**:
   Start the AppHost project to spin up the API and all dependencies:
   ```bash
   dotnet run --project infra/MyDDD.Template.AppHost
   ```

3. **Access the Dashboard**:
   Aspire will provide a dashboard URL in the console. From there, you can access:
   - **API Reference**: Scalar UI at `/scalar/v1`
   - **Observability**: Seq interface via the dashboard
   - **Identity**: Keycloak administration via the dashboard

### 🔑 Default Development Credentials (Local Only)

| Service | Username | Password |
| :--- | :--- | :--- |
| **PostgreSQL** | `postgres` | `password` |
| **Keycloak Admin** | `admin` | `admin` |

## 🧪 Testing

Run all tests from the root directory:
```bash
dotnet test
```

- **Unit Tests**: Fast, in-memory tests for Domain and Application logic.
- **Integration Tests**: End-to-end tests that start the full AppHost orchestration.

## 📜 License

This project is licensed under the MIT License.
