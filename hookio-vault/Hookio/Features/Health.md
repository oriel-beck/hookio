---
date: 2026-08-20
tags:
  - hookio
  - feature
project: Hookio
---

# Health

Liveness/readiness HTTP probes used by Compose and optionally by operators through nginx.

## Current behavior

`GET /healthz` and `GET /health` — same checks, same status mapping:

- Healthy or Degraded → **200**
- Unhealthy → **503**

Checks:

1. **postgres** — `CanConnectAsync` on `HookioContext`
2. **dragonfly** — Redis `PING`

Compose `server` healthcheck: `curl -f http://127.0.0.1:8080/healthz` (image installs `curl`). nginx proxies both paths so `http://localhost/healthz` works from the host.

Not registered as extra NuGet health packages; custom `IHealthCheck` classes.

## How it is wired

| Piece | Path |
| --- | --- |
| Registration | `server/Hookio/Program.cs` `MapHealthChecks` |
| Checks | `server/Hookio/Health/InfrastructureHealthChecks.cs` |
| Compose | `docker-compose.yml` `server.healthcheck` |
| nginx | `client/nginx.conf` |

## Env vars (names only)

Uses `PG_CONNECTION_STRING` and `REDIS_CONNECTION_STRING` indirectly via DI.

## Operator notes

`client` waits until `server` is healthy. If Postgres or Dragonfly is down, the API is 503 and nginx will not become “ready” in the compose sense (client depends on server healthy).

## Links

- [[Hookio/Infrastructure]]
- [[Hookio/Services/Postgres]] · [[Hookio/Services/Dragonfly]] · [[Hookio/Services/Nginx]] · [[Hookio/Services/Hookio API]]

## Official docs

- [ASP.NET Core health checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Compose healthcheck](https://docs.docker.com/compose/how-tos/startup-order/)
