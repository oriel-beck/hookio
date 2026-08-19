---
date: 2026-08-20
tags:
  - hookio
  - feature
project: Hookio
---

# Subscriptions

CRUD for per-guild YouTube or Twitch subscriptions. Each subscription owns a Discord webhook URL, a source URL, and a map of [[Hookio/Features/Message templates]] events.

## Current behavior

- Types: `Youtube = 1`, `Twitch = 2`, `Custom = 3`. **Custom has no client UI** and is unused.
- **Hard cap: 2 subscriptions per guild** on create (`RequiresPremiumException`). The count is all types combined. UI shows a Patreon tooltip at `count >= 2`. Premium is not implemented — see [[Hookio/Features/Premium]].
- Combined content + embed length cannot exceed **6000** characters (`EmbedTooLongException`).
- YouTube: persist shared `Feed` (unique URL). Twitch: no feed row; EventSub ids on the subscription.
- Webhook GET must succeed and `guild_id` must match.
- `Disabled` / `DisabledReason`: set when Discord webhook returns 401/404, or when a feed has no enabled subscribers (feed itself disabled). Disabled subscriptions are skipped by watchers.
- Delete: 204, unregisters Twitch EventSub, removes row (cascades events/messages), may disable the feed.
- List supports `?subscriptionType=` and `?withCounts=true` (total guild count vs filtered list). Client list uses `withCounts=true` for the create-button cap.

### API

All under `api/subscriptions`, `[Authorize]`, guild claim required. POST/PATCH/DELETE use rate policy `subscriptions` (5 / 10s).

Client errors (400): embed too long, premium required, invalid channel URL, EventSub register failure, validation.

## How it is wired

| Piece | Path |
| --- | --- |
| Controller | `server/Hookio/Controllers/SubscriptionsController.cs` |
| Service | `server/Hookio.DataManagers/SubscriptionService.cs` |
| Entity | `server/Hookio.Data/Entities/Subscription.cs` |
| List UI | `client/src/modules/subscriptions-manager.tsx` |
| Loaders | `client/src/loaders/get-all-subscriptions.ts`, `get-subscription.ts` |
| Fetch helpers | `client/src/util/util.ts` `submitSubscription` / `deleteSubscription` |

Delete button lives on [[Hookio/Features/Subscription editor]].

## Env vars (names only)

None extra. Twitch create needs Twitch env ([[Hookio/Services/Twitch]]). Discord webhook validation hits Discord without extra env.

## Links

- [[Hookio/Features/Subscription editor]] · [[Hookio/Features/Announcements]] · [[Hookio/Features/Twitch EventSub]] · [[Hookio/Features/Guilds]]
- [[Hookio/Services/Postgres]] · [[Hookio/Services/Hookio API]]

## Official docs

- [ASP.NET controllers](https://learn.microsoft.com/en-us/aspnet/core/web-api/)
- [Rate limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
