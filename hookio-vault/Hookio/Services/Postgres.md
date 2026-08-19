---
date: 2026-08-20
tags:
  - hookio
  - service
project: Hookio
---

# Postgres

PostgreSQL 16.15 is the system of record: users, subscriptions, events, Discord message templates, YouTube feeds.

## Current behavior

- Compose hostname `postgres`, image `postgres:16.15`, volume `postgres_data`.
- Healthcheck: `pg_isready -U $POSTGRES_USER -d $POSTGRES_DB`.
- The API uses **Npgsql + EF Core 10** (`HookioContext`). `Database.Migrate()` runs on API startup.
- Unique index on `Feeds.Url`. Subscriptions may share a feed. Deleting the last enabled subscription for a feed sets `Feed.Disabled = true`.

### Tables (EF)

`Users`, `Subscriptions`, `Events`, `Messages`, `Embeds`, `EmbedFields`, `Feeds` — see `server/Hookio.Data/Entities/` and `server/Hookio.Data/HookioContext.cs`.

Cascade: subscription → events → message → embeds → fields. Feed delete sets `Subscription.FeedId` null.

`User.PremiumExpires` exists; `User.Premium` is computed and unused for gating. API always maps `Premium = 0`. See [[Hookio/Features/Premium]].

Latest named migration: `20260819120000_UniqueFeedsUrlAndTwitchEventSub` (unique feed URL + Twitch EventSub columns + `WebhookChannel`).

## How it is wired

| Piece | Path |
| --- | --- |
| Compose | `postgres` service, `env_file: .env.postgres` |
| Context | `server/Hookio.Data/HookioContext.cs` |
| Factory (design-time) | `server/Hookio.Data/HookioContextFactory.cs` |
| Packages | `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3 |
| Health | `PostgresHealthCheck` in `server/Hookio/Health/InfrastructureHealthChecks.cs` |

## Env vars (names only)

Container: `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB` (`.env.postgres.example`).

API: `PG_CONNECTION_STRING` (example `Host=postgres;Database=hookio;Username=hookio;Password=changeme`). User/password/db must match.

## Operator notes

Create `.env.postgres` from `.env.postgres.example` before `docker compose up`. Tests use EF InMemory, not this container.

## Links

- [[Hookio/Infrastructure]]
- [[Hookio/Services/Hookio API]]
- [[Hookio/Features/Subscriptions]] · [[Hookio/Features/Auth]] · [[Hookio/Features/Health]]

## Official docs

- [PostgreSQL 16](https://www.postgresql.org/docs/16/index.html)
- [Npgsql EF Core](https://www.npgsql.org/efcore/)
- [EF Core](https://learn.microsoft.com/en-us/ef/core/)
- [postgres Docker image](https://hub.docker.com/_/postgres)
