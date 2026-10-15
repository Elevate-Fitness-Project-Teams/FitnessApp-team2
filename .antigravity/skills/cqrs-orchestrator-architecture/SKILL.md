---
name: cqrs-orchestrator-architecture
description: Enforce CQRS, Saga Orchestrator, Transactional Outbox, and repository/UoW patterns for backend development. Use when designing, creating, or reviewing commands, queries, orchestrators, sagas, outbox messaging, repositories, or unit of work implementations in C# or .NET backends.
---

# CQRS Orchestrator & Outbox Architecture

A comprehensive backend architectural standard enforcing strict Command Query Responsibility Segregation (CQRS), single-aggregate mutations, process orchestrators, Saga state machines with compensation, the Transactional Outbox pattern, projection-only queries, and repository/Unit of Work data access boundaries.

## Core Architectural Rules

### 1. Command & Query Boundaries
- **Queries can call queries:** Side-effect free; can compose read DTOs.
- **Commands NEVER call other commands:** Decompose multi-step mutations into dedicated **Orchestrators**.
- **Orchestrator Role:** Queries read data, then issues isolated commands sequentially or conditionally.

### 2. Command Mutations & Return Types
- **Single Aggregate Only:** Mutates strictly one domain entity/aggregate root.
- **Create Command:** Returns entity ID only (`Guid`, `int`, `string`). Never return the domain entity.
- **Update Command:** Returns `bool` or `Result`.
- **Delete Command:** Returns `bool` or `Result`.

### 3. Unit of Work & Bulk Operations
- **UoW Execution Delegate:** Wrap all mutations and outbox records in `_unitOfWork.ExecuteAsync(async () => { ... })`.
- **Bulk Updates:** Use `ExecuteUpdateAsync` directly on `_repo.GetQueryable()`; do not load tracked entities into memory.
- **Upsert Pattern:** Attempt `ExecuteUpdateAsync`; if `rowsAffected == 0`, call `_repo.AddAsync(newEntity, ct)`.
- **Bulk Deletes:** Use `ExecuteDeleteAsync` on `_repo.GetQueryable()`.

### 4. Transactional Outbox
- Never call external brokers (RabbitMQ/Kafka) inside command transactions.
- Stage events to `OutboxMessage` table inside `_unitOfWork.ExecuteAsync(...)`.
- Background workers poll and dispatch outbox messages with at-least-once delivery.

### 5. Saga Orchestrator & Compensations
- Track distributed progress using persistent saga state keyed by `CorrelationId`.
- Forward operations must have corresponding compensating commands for failure rollbacks.

### 6. Query Projections & Repositories
- Never inject `DbContext` into handlers or orchestrators; use `IRepository` and `IUnitOfWork`.
- Repositories expose `IQueryable<T>`.
- Queries must project directly to DTOs via `.Select(...)`.
