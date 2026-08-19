---
date: 2026-08-20
tags:
  - hookio
  - feature
project: Hookio
---

# Twitch EventSub

Product path for Twitch: Helix registration when a subscription is saved, then anonymous HTTPS POSTs from Twitch that become Discord webhook messages. Service-level Helix details: [[Hookio/Services/Twitch]].

## Current behavior

### Outbound (create/update/delete)

See [[Hookio/Services/Twitch]]. Failed EventSub create rolls back the DB transaction and deletes any ids already created.

### Inbound `POST /api/twitch/eventsub`

1. Buffer body, verify HMAC (`Twitch-Eventsub-Message-Id` + `Timestamp` + body) with `TWITCH_EVENTSUB_SECRET`. Invalid → 403.
2. `webhook_callback_verification` → 200 `text/plain` challenge.
3. `notification` → `TwitchEventSubHandler` (always 204 after handling). Other types (e.g. revocation) currently return 204 without extra logic.

### Notification → Discord

| EventSub type | Hookio `EventType` | Discord action |
| --- | --- | --- |
| `stream.online` | TwitchStreamStarted | create webhook message; cache message id; set `twitch:live:{broadcasterId}` |
| `stream.offline` | TwitchStreamEnded | create webhook message; delete live key |
| `channel.update` | TwitchStreamUpdated | **edit** cached start message **only if** live key exists |

Templates: `{user}`, `{login}`, `{title}`, `{game}`, `{category}`, `{url}`. Missing event message on the subscription → skip that guild.

401/404 webhook → disable subscription.

## How it is wired

| Piece | Path |
| --- | --- |
| Controller | `server/Hookio/Controllers/TwitchEventSubController.cs` |
| Handler | `server/Hookio.DataManagers/Twitch/TwitchEventSubHandler.cs` |
| Tests | `EventSubCallbackTests.cs`, `TwitchEventSubSignatureTests.cs` |

nginx must forward `/api` (and TLS if not tunneled).

## Env vars (names only)

`TWITCH_CLIENT_ID`, `TWITCH_CLIENT_SECRET`, `TWITCH_EVENTSUB_SECRET`, `TWITCH_EVENTSUB_CALLBACK_URL`.

## Operator notes

Callback **must be HTTPS**. Compose default is port 80 — use TLS nginx example or a tunnel to `:80`. Cookie Secure is separate (`HOOKIO_COOKIE_SECURE`) but production HTTPS usually implies both.

## Links

- [[Hookio/Services/Twitch]] · [[Hookio/Services/Nginx]] · [[Hookio/Services/Dragonfly]] · [[Hookio/Services/Discord]]
- [[Hookio/Features/Subscriptions]] · [[Hookio/Features/Message templates]] · [[Hookio/Infrastructure]]

## Official docs

- [Handling webhook events](https://dev.twitch.tv/docs/eventsub/handling-webhook-events)
- [EventSub](https://dev.twitch.tv/docs/eventsub/)
- [stream.online](https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#streamonline) / [stream.offline](https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#streamoffline) / [channel.update](https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelupdate)
