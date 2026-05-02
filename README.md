# Warehouse API

REST API for managing warehouse inventory. Built with ASP.NET Core and Redis.

## Stack

- **Runtime**: .NET 8 / ASP.NET Core
- **Storage**: Redis (StackExchange.Redis)
- **Docs**: Swagger / OpenAPI
- **Tests**: xUnit + Moq

## Requirements

- Docker + Docker Compose (recommended)
- or .NET 8 SDK + Redis (manual setup)

## Run

### Via Docker (recommended)

```bash
docker compose up --build
```

App: `http://localhost:5000`  
Swagger: `http://localhost:5000/swagger`  
Redis: `localhost:6379`

### Without Docker

```bash
# Start Redis
docker run -d -p 6379:6379 redis

# Run the app
dotnet run --project Warehouse
```

## Environment variables

| Variable | Default | Description |
|----------|---------|-------------|
| `ConnectionStrings__Redis` | `localhost:6379` | Redis connection string |

## API endpoints

| Method | Route | Description |
|--------|-------|-------------|
| POST | /add | Add a new product |
| GET | /products | List all products |
| GET | /product/{id} | Get product by ID |
| GET | /product/del/{id} | Delete product by ID |
| GET | /searchProduct?word= | Search by any field |

## Product model

```json
{
  "name": "Apple",
  "sku": "A1",
  "category": "Food",
  "quantity": 10
}
```

## Data storage

Products are stored as Redis hashes under `product:{id}`.  
Active IDs are tracked in the `product:list` list.  
Auto-incremented IDs via `product:id` counter.

## Tests

```bash
dotnet test
```

Covers: model creation, delete, get all, search.
