---
date: 2026-08-20
tags:
  - hookio
  - service
project: Hookio
---

# Hookio API

ASP.NET Core 10 web app (`Hookio.dll`) running Kestrel inside the `server` Compose service. This is the only backend process: HTTP API, JWT auth, EF migrations, RSS watcher, EventSub handler, Discord HTTP queue.

## Current behavior

- Listens on **8080** (`ASPNETCORE_HTTP_PORTS=8080`). nginx proxies `/api`, `/health`, `/healthz` here.
- JSON console logs with UTC timestamps and an `X-Request-Id` header (`TraceIdentifier`).
- On startup (non-`Testing`): connect Npgsql + Redis from process env, **`Database.Migrate()`**, start hosted services. Compose `env_file` injects `server/Hookio/.env.production`; the API does not parse `.env` files.
- Swagger UI only when `ASPNETCORE_ENVIRONMENT=Development` (`/swagger`). Local `dotnet run` profile `http` binds `http://localhost:5093`.
- `UseHttpsRedirection` is on except in `Testing`. Compose serves HTTP on 8080 behind nginx.

## How it is wired

| Piece | Path |
| --- | --- |
| Entry | `server/Hookio/Program.cs` |
| Dockerfile | `server/Dockerfile` (`sdk:10.0` build, `aspnet:10.0` runtime + `curl` for healthcheck) |
| Solution | `server/server.sln` |
| Web project | `server/Hookio/` (`net10.0`) |
| Contracts | `server/Hookio.Contracts/` |
| EF + entities | `server/Hookio.Data/` |
| Services / HTTP | `server/Hookio.DataManagers/` |
| Shared env/enums | `server/Hookio.Shared/` |
| Tests | `server/Hookio.Tests/` (xunit, `WebApplicationFactory`, NSubstitute, EF InMemory) |

### HTTP route families

| Route | Auth | Feature |
| --- | --- | --- |
| `POST /api/users/authenticate?code=` | anonymous | [[Hookio/Features/Auth]] |
| `GET /api/users/current` | JWT | [[Hookio/Features/Auth]] |
| `POST /api/users/logout` | anonymous (clears cookie) | [[Hookio/Features/Auth]] |
| `GET /api/subscriptions/{guildId}` | JWT + guild claim | [[Hookio/Features/Subscriptions]] |
| `POST /api/subscriptions/{guildId}` | JWT + rate limit | [[Hookio/Features/Subscriptions]] |
| `GET /api/subscriptions/{guildId}/{id}` | JWT + guild claim | [[Hookio/Features/Subscriptions]] |
| `PATCH /api/subscriptions/{guildId}/{id}` | JWT + rate limit | [[Hookio/Features/Subscriptions]] |
| `DELETE /api/subscriptions/{guildId}/{id}` | JWT + rate limit | [[Hookio/Features/Subscriptions]] |
| `POST /api/twitch/eventsub` | anonymous + HMAC | [[Hookio/Features/Twitch EventSub]] |
| `GET /health`, `GET /healthz` | anonymous | [[Hookio/Features/Health]] |

Guild ids on subscription routes use `[DiscordGuildId]` model binding (`server/Hookio/ModelBinding/DiscordGuildId.cs`).

### Hosted services

| Service | Interval | Role |
| --- | --- | --- |
| `RssWatcherService` | 15 minutes | [[Hookio/Features/Announcements]] |
| `RssCleanupService` | 1 hour | expire Dragonfly sent-message maps older than 1 day |
| `TaskQueue` | continuous | Discord HTTP priority queue ([[Hookio/Services/Discord]]) |

Not registered in `Testing` (except `TaskQueue`): RSS watcher/cleanup and real Postgres/Redis.

### API rate limit

Policy `subscriptions` on POST/PATCH/DELETE: **5 requests / 10 seconds**, partitioned by JWT `id` claim or client IP. 429 JSON `{ message: "Too many requests, please try again later..." }`.

## Env vars (names only)

See [[Hookio/Infrastructure]]. Required at runtime for a full stack: `PG_CONNECTION_STRING`, `REDIS_CONNECTION_STRING`, `JWT_SECRET`, Discord trio, Twitch quartet, optional `HOOKIO_COOKIE_SECURE`.

## Links

- [[Hookio/Infrastructure]]
- [[Hookio/Services/Nginx]] · [[Hookio/Services/Postgres]] · [[Hookio/Services/Dragonfly]]
- [[Hookio/Services/Discord]] · [[Hookio/Services/YouTube]] · [[Hookio/Services/Twitch]]
- [[Hookio/Services/GitHub Actions]]

## Official docs

- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core)
- [Kestrel / HTTP ports](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel)
- [Hosted services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
- [Rate limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [JWT bearer](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [EF Core](https://learn.microsoft.com/en-us/ef/core/)
- [Health checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
