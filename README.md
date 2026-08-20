# net9-cap-rabbitmq

A small demo/learning solution exploring **[DotNetCore.CAP](https://cap.dotnetcore.xyz/)** - an
event bus and outbox library for distributed transactions and publish/subscribe messaging in .NET.

The project shows the CAP publish -> subscribe flow running out of the box with an **in-memory**
message queue and storage. RabbitMQ and PostgreSQL packages are already referenced and wired in
(commented out), so you can switch to a real broker and durable outbox with a couple of edits.

## Tech stack

- **.NET 10** / ASP.NET Core Web API
- **DotNetCore.CAP 10.0.1** (event bus + outbox)
  - In-memory storage + in-memory message queue (default)
  - Optional: RabbitMQ transport, PostgreSQL storage
- CAP Dashboard
- OpenAPI (Development only)

## Project structure

```
src/
├── OrderSystem.slnx
├── Order.Api/           # ASP.NET Core Web API — hosts CAP, publisher & subscriber
└── Shipping.Service/    # Console worker (stub — scaffolded, not yet implemented)
```

| Project            | Role                                                                                  |
| ------------------ | ------------------------------------------------------------------------------------- |
| `Order.Api`         | Registers CAP, exposes the publish endpoint and a CAP subscriber, serves the Dashboard |
| `Shipping.Service`  | Console app referencing CAP/RabbitMQ/PostgreSQL; currently a `Hello, World!` stub      |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- (Optional) RabbitMQ and PostgreSQL — only needed if you switch off the in-memory setup.
  The quickest way is Docker:

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
docker run -d --name postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:16
```

## Getting started

Run the API:

```bash
dotnet run --project src/Order.Api
```

It listens on:

- `http://localhost:5152` (http profile)
- `https://localhost:7257` (https profile)

Useful endpoints:

- **CAP Dashboard** — `http://localhost:5152/cap` (inspect published/received messages)
- **OpenAPI document** — `http://localhost:5152/openapi/v1.json` (Development only)

## Try the messaging flow

1. Trigger a publish:

   ```bash
   curl http://localhost:5152/publish/send
   ```

   `PublishController` publishes a message on the topic `test.show.time` carrying the current
   `DateTime` via `ICapPublisher`.

2. The subscriber `ConsumerController.ReceiveMessage`, decorated with
   `[CapSubscribe("test.show.time")]`, receives it and writes to the console:

   ```
   message time is: 08/20/2026 11:09:00
   ```

3. Open the CAP Dashboard at `http://localhost:5152/cap` to see the published and received messages.

## Switching to RabbitMQ + PostgreSQL

The default configuration in [`src/Order.Api/Program.cs`](src/Order.Api/Program.cs) uses in-memory
storage and queue:

```csharp
builder.Services.AddCap(options =>
{
    options.UseInMemoryStorage();
    // options.UseRabbitMQ("localhost");
    options.UseInMemoryMessageQueue();
    options.UseDashboard();
});
```

To use a real broker and durable outbox:

1. Add a `Database` connection string and enable the `DbContext`:

   ```csharp
   builder.Services.AddDbContext<AppDbContext>(opt =>
       opt.UseNpgsql(builder.Configuration.GetConnectionString("Database")));
   ```

2. Update the CAP registration to use PostgreSQL storage and RabbitMQ transport, e.g.:

   ```csharp
   builder.Services.AddCap(options =>
   {
       options.UseEntityFramework<AppDbContext>();   // or options.UsePostgreSql("...")
       options.UseRabbitMQ("localhost");
       options.UseDashboard();
   });
   ```

The required packages (`DotNetCore.CAP.RabbitMQ`, `DotNetCore.CAP.PostgreSql`) are already
referenced in both projects.

## Notes

- `Shipping.Service` is a scaffolded stub intended to consume events in a separate process; it
  currently only prints `Hello, World!`.
- `AppDbContext` is empty — it exists so CAP's EF Core storage can be enabled later.
