---
date: 2026-08-20
tags:
  - hookio
  - feature
project: Hookio
---

# Announcements

Background RSS watcher: poll unique enabled feeds, detect new vs updated entries, execute or edit Discord webhook messages. This is the YouTube (and any feed-backed) announcement path. Twitch live announcements are [[Hookio/Features/Twitch EventSub]].

## Current behavior

`RssWatcherService` every **15 minutes**:

1. Load non-disabled feeds with non-disabled subscriptions and their event messages.
2. Per feed host, serialize requests (semaphore). Honor `x-ratelimit-remaining` / `x-ratelimit-reset-after` when present; otherwise ~1s spacing.
3. Parse latest item ([[Hookio/Services/YouTube]]). Skip if missing id or all timestamps.
4. **New id:** reset Dragonfly message map, `InsertNewFeed`, send `EventType.NewFeed` via `SendWebhookMessage`, store returned message id.
5. **Same id, different published/updated:** `UpdateWebhookMessage` using cached message id and `EventType.UpdatedFeed`.
6. Persist `LastId` / `LastPublishedAt`. If no enabled subscriptions, set `Feed.Disabled`.

401/404 from Discord → `DisableSubscription`. Other send failures are logged; the cycle continues.

`RssCleanupService` hourly drops sent-message hashes for feeds whose `SENT_FEEDS` score is older than one day.

Templates: [[Hookio/Features/Message templates]]. Discord HTTP: [[Hookio/Services/Discord]] queue.

## How it is wired

| Piece | Path |
| --- | --- |
| Watcher | `server/Hookio.DataManagers/Feeds/RssWatcherService.cs` |
| Cleanup | `server/Hookio.DataManagers/Feeds/RssCleanupService.cs` |
| Cache | `server/Hookio.DataManagers/Feeds/FeedsCacheService.cs` |
| Feeds repo | `server/Hookio.DataManagers/FeedRepository.cs` |
| Registration | `server/Hookio/Program.cs` (skipped in `Testing`) |

## Env vars (names only)

Indirect: `REDIS_CONNECTION_STRING`, `PG_CONNECTION_STRING`. No YouTube API key.

## Operator notes

First cycle runs immediately on API start, then every 15 minutes. Duplicate compose replicas would double-poll (no leader election).

## Links

- [[Hookio/Services/YouTube]] · [[Hookio/Services/Discord]] · [[Hookio/Services/Dragonfly]]
- [[Hookio/Features/Subscriptions]] · [[Hookio/Features/Twitch EventSub]]

## Official docs

- [Hosted services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
- [Execute webhook](https://discord.com/developers/docs/resources/webhook#execute-webhook)
- [Edit webhook message](https://discord.com/developers/docs/resources/webhook#edit-webhook-message)
- [Atom](https://www.rfc-editor.org/rfc/rfc4287)
