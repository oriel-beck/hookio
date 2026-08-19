---
date: 2026-08-20
tags:
  - hookio
  - service
project: Hookio
---

# Twitch

Twitch is **EventSub over HTTPS webhooks**, not RSS and not PubSub. Helix app-access tokens come from the client-credentials grant.

## Current behavior

On create (and on channel URL change):

1. Parse login from `https://www.twitch.tv/{login}` (4–25 chars).
2. `POST https://id.twitch.tv/oauth2/token` (`grant_type=client_credentials`), cached in memory until ~60s before expiry.
3. `GET helix/users?login=`
4. `POST helix/eventsub/subscriptions` three times:
   - `stream.online` version **1**
   - `stream.offline` version **1**
   - `channel.update` version **2**
5. Transport: webhook, `callback` = `TWITCH_EVENTSUB_CALLBACK_URL`, `secret` = `TWITCH_EVENTSUB_SECRET` (must be **10–100** characters).
6. Store `TwitchLogin`, `TwitchBroadcasterId`, comma-separated EventSub ids.

Unregister: `DELETE helix/eventsub/subscriptions?id=` for each id (on delete, failed create rollback, or channel change).

Inbound: `POST /api/twitch/eventsub` — see [[Hookio/Features/Twitch EventSub]]. HMAC-SHA256 over `message-id + timestamp + body`; reject if timestamp older than **10 minutes**. Challenge: return raw `challenge` as `text/plain`.

## How it is wired

| Piece | Path |
| --- | --- |
| Helix subscribe | `server/Hookio.DataManagers/Twitch/TwitchEventSubService.cs` |
| HMAC | `server/Hookio.DataManagers/Twitch/TwitchEventSubSignature.cs` |
| Controller | `server/Hookio/Controllers/TwitchEventSubController.cs` |
| Notifications | `server/Hookio.DataManagers/Twitch/TwitchEventSubHandler.cs` |
| DTOs | `server/Hookio.DataManagers/Twitch/Contracts/TwitchHelixDtos.cs` |
| Client | provider path `twitch` in `client/src/modules/provider-selection.tsx` |

## Env vars (names only)

`TWITCH_CLIENT_ID`, `TWITCH_CLIENT_SECRET`, `TWITCH_EVENTSUB_SECRET`, `TWITCH_EVENTSUB_CALLBACK_URL`.

Callback must be public HTTPS, e.g. `https://your.public.hostname/api/twitch/eventsub` (nginx `/api` prefix).

## Operator notes

Default compose is HTTP `:80`. Use `client/nginx.tls.conf.example` or a tunnel. EventSub will not verify a `http://` callback. See [[Hookio/Infrastructure]].

## Links

- [[Hookio/Features/Twitch EventSub]] · [[Hookio/Features/Subscriptions]]
- [[Hookio/Services/Nginx]] · [[Hookio/Services/Dragonfly]] · [[Hookio/Services/Discord]]

## Official docs

- [EventSub](https://dev.twitch.tv/docs/eventsub/)
- [Handling webhook events](https://dev.twitch.tv/docs/eventsub/handling-webhook-events)
- [Subscription types](https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/) (`stream.online`, `stream.offline`, `channel.update`)
- [Create EventSub subscription](https://dev.twitch.tv/docs/api/reference/#create-eventsub-subscription)
- [Delete EventSub subscription](https://dev.twitch.tv/docs/api/reference/#delete-eventsub-subscription)
- [Get users](https://dev.twitch.tv/docs/api/reference/#get-users)
- [Client credentials grant](https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#client-credentials-grant-flow)
