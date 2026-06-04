# OrderFlow: High-Performance Event-Driven Order Management System

OrderFlow is an event-driven, microservices-style backend system built with **.NET 8/9** to manage the lifecycle of real-time orders, restaurant interactions, and courier logistics. The project is designed inside a clean monolithic boundary with decoupled background workers, ensuring eventual consistency and low-latency spatial tracking.

##  Architecture & Project Structure

The solution strictly adheres to **Clean Architecture** and **Domain-Driven Design (DDD)** principles, keeping business logic completely isolated from infrastructure dependencies.

~~~text
OrderFlow/
├── Src/
│   ├── OrderFlow.Domain/          # Aggregate roots, entities, value objects, and domain exceptions
│   ├── OrderFlow.Application/     # CQRS handlers (MediatR), FluentValidation behaviors, interfaces
│   ├── OrderFlow.Infrastructure/  # EF Core, ApplicationDbContext, Identity, MassTransit consumers, Redis tracking
│   ├── OrderFlow.Contracts/       # Integration events and SignalR Hub definitions shared across services
│   ├── OrderFlow.Api/             # REST API controllers, JWT/Refresh token endpoints, global middleware
│   ├── OrderFlow.Worker.Payment/ # Dedicated background worker for processing order payments
│   └── OrderFlow.Worker.Courier/  # Dedicated background worker for courier matching and Firebase push notifications
└── Tests/
    ├── OrderFlow.UnitTests/       # Isolated domain and command handler tests (Moq, XUnit)
    ├── OrderFlow.IntegrationTests/# Full E2E WebApplicationFactory testing with Testcontainers
    ├── OrderFlow.Worker.Payment.Tests/
    └── OrderFlow.Worker.Courier.Tests/
~~~

### Core Design Patterns Implemented:
* **CQRS via MediatR:** Strict segregation of read queries and write commands. Pipeline behaviors are utilized to handle cross-cutting concerns like `ValidationBehavior` and `PerformanceBehavior` globally.
* **Rich Domain Models:** Aggregates encapsulate state transitions. For instance, `CustomerOrder` enforces state invariants via domain exceptions rather than allowing primitive property manipulation.
* **Transactional Outbox Pattern:** Ensures reliable messaging. Integration events generated inside domain boundaries are saved within the same database transaction as business data before being dispatched to the broker.

---

## Infrastructure & Tech Stack

* **Database Engine:** SQL Server managed via Entity Framework Core with explicit model configurations. Concurrency check mechanisms (`RowVersion`) protect critical database operations.
* **Message Broker:** **MassTransit** over **RabbitMQ** coordinates async communication between the API, payment processor, and logistics system.
* **Spatial Tracking & Caching:** **Redis** handles real-time data. It powers low-latency courier location caching via **Redis GeoSearch** parameters for localized assignment.
* **Real-Time Streaming:** **SignalR** hubs (backed by a customized `OrderHubFilter`) broadcast immediate order status updates directly to clients.
* **Push Notifications:** The Courier Worker implements a native `FirebasePushNotificationService` to push delivery job alerts to mobile devices.

---

##  Core Subsystems & Event Flows

### 1. Order Lifecycle & Payment
* The client triggers `PlaceOrderCommand`. An integration event `OrderPlacedIntegrationEvent` is published via the outbox layer.
* **`OrderFlow.Workers.Payment`** consumes the event, processes the charge, and publishes either a `PaymentSucceededIntegrationEvent` or `PaymentFailedIntegrationEvent`.
* **`OrderFlow.Infrastructure`** consumers pick up the results to update the order state, triggering SignalR broadcasts to update the UI instantly.

### 2. Restaurant Delivery Prep
* Restaurant owners interact with endpoints like `StartPreparingOrder` and `MarkOrderReadyForPickup`.
* Transitioning an order to a ready state automatically publishes an `OrderReadyForPickupIntegrationEvent`.

### 3. Courier Logistics & Spatial Tracking
* **`OrderFlow.Worker.Courier`** catches ready orders. It looks up nearby delivery agents using spatial queries (`ICourierTrackerService`) and pushes dispatch invites via Firebase.
* Couriers constantly push telemetry via the `UpdateLocationCommand`, updating the system's live coordinate matrix in Redis.

---

##  Identity & Security Pipeline

* **Role-Based Access Control (RBAC):** Built on ASP.NET Core Identity (`AppIdentityUser`), establishing rigid operational lanes for `Admin`, `RestaurantOwner`, `Courier`, and `Customer`.
* **JWT Authentication:** Features a resilient auth system executing strict **Refresh Token rotation** logic to prevent token hijacking.
* **Resource-Level Authorization:** Custom authorization handlers (e.g., `RestaurantOwnerAuthorizationHandler`) guarantee users can only modify data assets they legitimately own.

---

##  Automated Testing Strategy

The solution features a rigorous testing matrix designed to keep the main development and branch pipelines highly stable.

* **Unit Testing:** Focuses on business rules, validation structures, and handler flows using `XUnit`, `Moq`, and `FluentAssertions`.
* **Integration Testing (Testcontainers):** Spins up completely isolated, real Docker containers for **SQL Server**, **Redis**, and **RabbitMQ** on the fly. This ensures integration logic is verified against actual running software rather than relying on brittle in-memory substitutes.
* **Deterministic CI Execution:** To prevent race conditions or threading crashes over static loggers (such as Serilog bootstrap components), the project's GitHub Actions configuration (`ci.yml`) is customized to run testing sub-suites sequentially:
  ~~~bash
  # Executed automatically via .github/workflows/ci.yml
  dotnet test OrderFlow.sln --configuration Release
  ~~~

---

##  Local Development Setup

### Prerequisites
* .NET  9 SDK
* Docker / Docker Desktop

### 1. Environment Configuration (.env)
OrderFlow relies on environment variables for sensitive configurations. Before starting the application, create a `.env` file in the root directory (based on the provided `.env.example`) and populate the following values:

~~~env
# Database & Infrastructure Default Password
PASSWORD=

# JWT Authentication Settings
JwtAudience=
JwtIssuer=
JwtSecretKey=
JwtExpirationMinutes=
JwtRefreshTokenExpirationDays=

# Message Broker (MassTransit/RabbitMQ)
RabbitMQ_Username=
RabbitMQ_Password=

#Redis
Redis_ConnectionString=

# CI/CD Container Registry
DOCKER_REGISTRY=
~~~

### 2. Initialize Containers
Bring up the required local ecosystem (SQL Server, Redis, RabbitMQ) via Docker Compose:
~~~bash
docker-compose up -d
~~~

### 3. Apply Migrations & Seed Data
Database schemas and system roles are populated automatically on startup by the `IdentityDataSeeder`. If manual migrations are needed:
bash
dotnet ef database update --project Src/OrderFlow.Infrastructure --startup-project Src/OrderFlow.Api
~~~

### 4. Run the System
You can start individual elements manually or run the solution through your IDE (Visual Studio / Rider) using the provided `docker-compose.dcproj` configuration:
~~~bash
dotnet run --project Src/OrderFlow.Api/OrderFlow.Api.csproj
~~~
