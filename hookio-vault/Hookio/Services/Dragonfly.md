---
date: 2026-08-20
tags:
  - hookio
  - service
project: Hookio
---

# Dragonfly

DragonflyDB speaks the **Redis protocol**. Hookio uses it as cache and as a short-lived map of Discord message ids for edit-in-place announcements. Client: StackExchange.Redis 2.13.17.

## Current behavior

- Compose hostname `dragonfly`, image `docker.dragonflydb.io/dragonflydb/dragonfly:v1.40.4`, volume `dragonfly_data`, port 6379.
- Healthcheck: TCP connect to `127.0.0.1:6379`.
- API health: `PING` via `RedisHealthCheck`.

### Keys used today

| Key | Type | Purpose |
| --- | --- | --- |
| `discord:guilds:{userId}` | string JSON, TTL **10 minutes** | cached Discord guild list; deleted on login |
| `SENT_FEED_{feedId}` | hash `subscriptionId → messageId` | last Discord message per subscription for a YouTube feed (or Twitch cache id) |
| `SENT_FEEDS` | sorted set, score = Unix **seconds** | feed ids eligible for cleanup after **1 day** |
| `twitch:live:{broadcasterId}` | string `"1"` | stream is live; `channel.update` ignored unless this key exists |

Twitch reuses the sent-message map with a synthetic feed id `-subscriptionId` (`TwitchEventSubHandler.TwitchCacheFeedId`).

`RssCleanupService` hourly deletes `SENT_FEEDS` members older than one day and their hashes.

## How it is wired

| Piece | Path |
| --- | --- |
| Connect | `Program.cs` → `ConnectionMultiplexer.Connect(REDIS_CONNECTION_STRING)` |
| Guild cache | `server/Hookio.DataManagers/UserAuthService.cs` |
| Feed message cache | `server/Hookio.DataManagers/Feeds/FeedsCacheService.cs` |
| Cleanup | `server/Hookio.DataManagers/Feeds/RssCleanupService.cs` |
| Twitch live flag | `server/Hookio.DataManagers/Twitch/TwitchEventSubHandler.cs` |

## Env vars (names only)

`REDIS_CONNECTION_STRING` — compose example `dragonfly:6379`.

## Links

- [[Hookio/Infrastructure]]
- [[Hookio/Features/Auth]] · [[Hookio/Features/Announcements]] · [[Hookio/Features/Twitch EventSub]] · [[Hookio/Features/Health]]

## Official docs

- [Dragonfly docs](https://www.dragonflydb.io/docs)
- [Dragonfly Redis clients](https://www.dragonflydb.io/docs/cloud/connect/redis-clients)
- [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)
- [Redis protocol](https://redis.io/docs/latest/develop/reference/protocol-spec/)
