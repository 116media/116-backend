# Operations reference

## Environment variables

The full template is [`.env.template`](../../.env.template); configuration is read through
`AppEnvironment` classes and validated in one pass by `EnvSchema.ValidateAtBoot()` — never
through `IOptions`/`IConfiguration` binding. The core set:

```bash
# Database
DB_HOST=localhost
DB_PORT=5432
DB_USER=postgres
DB_PASSWORD=your-password
DB_NAME=116_db

# JWT — see reference/authentication.md
JWT_SECRET=your-secret-key-minimum-32-characters
JWT_ISSUER=116_frontend
JWT_AUDIENCE=116_client

# Cloudinary (file uploads)
CLOUDINARY_CLOUD_NAME=your-cloud-name
CLOUDINARY_API_KEY=your-api-key
CLOUDINARY_API_SECRET=your-api-secret

ASPNETCORE_ENVIRONMENT=Development
```

## Logging

- Serilog structured logging: console sink in development, Seq sink (port 5341) centralized.
- HTTP requests logged automatically (path, method, status, response time, trace id).
- Levels configured in `appsettings.json`.

## Docker

```bash
# Development (with hot reload)
docker-compose -f docker-compose.yml -f docker-compose.override.yml up

# Production
docker-compose up -d
```

| Service | Ports |
| --- | --- |
| `116_api` | 5025 |
| `116_db` (PostgreSQL) | 5432 |
| `116_seq` | 5341 (ingest) / 9091 (web UI) |

## Troubleshooting

- **Database connection**: `.env` exists with correct values; `docker-compose ps` shows the
  database up; connection string visible in logs.
- **Migrations**: startup project is `src/Api`; the context is specified;
  `dotnet ef migrations list` shows pending ones.
- **JWT**: `JWT_SECRET` is at least 32 characters; issuer/audience match between generation
  and validation.
