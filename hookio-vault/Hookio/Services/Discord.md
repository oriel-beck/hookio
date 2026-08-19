---
date: 2026-08-20
tags:
  - hookio
  - service
project: Hookio
---

# Discord

Hookio is a Discord OAuth app plus a webhook announcer. All Discord HTTP goes through a priority `TaskQueue` that honors Discord rate-limit headers. There is **no Discord.Net** dependency.

## Current behavior

### OAuth2

- Scopes required: **`identify` `guilds` `email`**. Missing any of those aborts login.
- `POST /api/v10/oauth2/token` — authorization code exchange and refresh.
- `GET /api/v10/users/@me` and `GET /api/v10/users/@me/guilds`.
- Guilds shown to the user are filtered to **Manage Server** (`permissions & 0x20`). JWT stores those guild ids. See [[Hookio/Features/Auth]] and [[Hookio/Features/Guilds]].

### Webhooks

- Validate URL with `GET {webhookUrl}` into `WebhookInfo` (must belong to the same guild).
- Create: `POST {webhookUrl}?wait=true` (JSON `DiscordMessageCreatePayload`).
- Edit: `PATCH {webhookUrl}/messages/{id}`.
- HTTP **401 or 404** disables the subscription (`Disabled` + `DisabledReason`). See [[Hookio/Features/Subscriptions]].

### Rate limits (as implemented)

`TaskQueue` (`server/Hookio.DataManagers/Utils/TaskQueue.cs`):

- Priorities: **0** OAuth token, **1** user/guilds, **2** webhooks.
- Global: at most **50 requests/second** (Discord IP/bot global).
- On **429**: wait `X-RateLimit-Reset-After`, `Retry-After`, or JSON `retry_after`. `X-RateLimit-Scope: global` (or body `global: true`) blocks the whole queue; otherwise per `X-RateLimit-Bucket`.
- When remaining is 0, pause that bucket until `X-RateLimit-Reset-After`.

RSS polling also reads `x-ratelimit-remaining` / `x-ratelimit-reset-after` **from YouTube-style feed responses**, not Discord — that lives in [[Hookio/Features/Announcements]].

## How it is wired

| Piece | Path |
| --- | --- |
| Manager | `server/Hookio.DataManagers/Discord/DiscordRequestManager.cs` |
| Queue | `server/Hookio.DataManagers/Utils/TaskQueue.cs` |
| JSON options | `server/Hookio.DataManagers/Discord/DiscordJson.cs` |
| Contracts | `server/Hookio.DataManagers/Discord/Contracts/` |
| Cookie / JWT | [[Hookio/Features/Auth]] |

Client login button uses `VITE_DISCORD_LOGIN_URL` (Discord authorize URL). Redirect URI must match `DISCORD_REDIRECT_URI`.

## Env vars (names only)

`DISCORD_CLIENT_ID`, `DISCORD_CLIENT_SECRET`, `DISCORD_REDIRECT_URI`. Client: `VITE_DISCORD_LOGIN_URL`.

## Operator notes

Register the OAuth2 redirect on the Discord application. Compose examples use `http://localhost/oauth` vs client `http://localhost` — they must match each other and the portal. For HTTPS production, set cookie Secure via TLS or `HOOKIO_COOKIE_SECURE=true`.

## Links

- [[Hookio/Features/Auth]] · [[Hookio/Features/Guilds]] · [[Hookio/Features/Announcements]] · [[Hookio/Features/Twitch EventSub]]
- [[Hookio/Services/Hookio API]]

## Official docs

- [OAuth2](https://discord.com/developers/docs/topics/oauth2)
- [Get current user](https://discord.com/developers/docs/resources/user#get-current-user)
- [Get current user guilds](https://discord.com/developers/docs/resources/user#get-current-user-guilds)
- [Webhooks](https://discord.com/developers/docs/resources/webhook)
- [Execute webhook](https://discord.com/developers/docs/resources/webhook#execute-webhook)
- [Rate limits](https://discord.com/developers/docs/topics/rate-limits)
- [Permissions](https://discord.com/developers/docs/topics/permissions) (`MANAGE_GUILD` = `0x20`)
