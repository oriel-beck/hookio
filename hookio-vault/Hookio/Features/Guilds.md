---
date: 2026-08-20
tags:
  - hookio
  - feature
project: Hookio
---

# Guilds

Server picker and guild-scoped authorization. Hookio is per-Discord-guild: subscriptions always carry `guildId`.

## Current behavior

- After login, `/servers` lists guilds from `GET /api/users/current`.
- Only guilds where the user has **Manage Server** (`permissions & 0x20`) are returned (`SubscriptionMapper.ToGuilds`).
- Clicking a guild goes to `/servers/:serverId` (YouTube vs Twitch picker).
- `LoginGuard` redirects home if there is no user, and back to `/servers` if `serverId` is not in `user.guilds`.
- API: `Util.CanAccessGuild` checks the JWT `guilds` claim. Mismatch → 401. This is **not** re-checked against Discord on every subscription call; it is whatever guilds were snapshotted at login (until cookie expiry).

Webhook create/update also requires the webhook’s `guild_id` to match the route `guildId`.

## How it is wired

| Piece | Path |
| --- | --- |
| List UI | `client/src/modules/guilds.tsx`, `client/src/components/guild.tsx` |
| Guard | `client/src/components/guard.tsx` |
| JWT check | `server/Hookio/Util.cs` `CanAccessGuild` |
| Filter | `server/Hookio.DataManagers/SubscriptionMapper.cs` |
| Tests | `server/Hookio.Tests/JwtGuildClaimTests.cs`, `GuildAccessApiTests.cs` |

Routes: `/servers`, `/servers/:serverId`, then provider and editor routes in `client/src/main.tsx`.

## Env vars (names only)

None beyond [[Hookio/Features/Auth]].

## Links

- [[Hookio/Features/Auth]] · [[Hookio/Features/Subscriptions]]
- [[Hookio/Services/Discord]]

## Official docs

- [Discord permissions](https://discord.com/developers/docs/topics/permissions) (`MANAGE_GUILD`)
- [Get current user guilds](https://discord.com/developers/docs/resources/user#get-current-user-guilds)
