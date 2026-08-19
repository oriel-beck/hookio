---
date: 2026-08-20
tags:
  - hookio
  - feature
project: Hookio
---

# Auth

Discord OAuth2 login, HttpOnly JWT cookie, and current-user fetch. StrictMode is disabled on the client because Discord authorization codes are single-use.

## Current behavior

1. User hits `VITE_DISCORD_LOGIN_URL` (Discord authorize).
2. Discord redirects to the SPA with `?code=`. Root loader (`client/src/loaders/get-user.ts`) `POST /api/users/authenticate?code=`. Home then strips `code` from the URL.
3. Server exchanges the code ([[Hookio/Services/Discord]]), upserts `User` (tokens + email), caches guilds, issues JWT.
4. Cookie name **`Authorization`**, HttpOnly, SameSite **Strict**, 3-hour expiry. `Secure` if the request is HTTPS **or** `HOOKIO_COOKIE_SECURE=true`.
5. JWT: issuer/audience `hookio`, HMAC-SHA256 with `JWT_SECRET`, claims `id` (Discord snowflake) and `guilds` (JSON array of guild id strings).
6. JwtBearer also accepts `Authorization: Bearer …`. Cookie is preferred via `ConfigureJwtBearerOptions`.
7. `GET /api/users/current` refreshes Discord access token if it expires within **1 day**, then returns Discord profile + filtered guilds.
8. `POST /api/users/logout` overwrites the cookie with an expired empty value.

Guild list is cached in Dragonfly 10 minutes (`discord:guilds:{userId}`), invalidated on authenticate.

## How it is wired

| Piece | Path |
| --- | --- |
| Users API | `server/Hookio/Controllers/UsersController.cs` |
| Auth service | `server/Hookio.DataManagers/UserAuthService.cs` |
| Cookie options | `server/Hookio.DataManagers/UtilCookie.cs`, `server/Hookio/Util.cs` |
| JWT events | `server/Hookio/ConfigureJWTBearerOptions.cs` |
| Constants | `server/Hookio.Shared/AuthConstants.cs` |
| Client loader | `client/src/loaders/get-user.ts` |
| Login guard | `client/src/components/guard.tsx` |
| Header login/logout | `client/src/components/header.tsx` |
| Home CTA | `client/src/modules/home.tsx` |

## Env vars (names only)

`JWT_SECRET` (256-bit+), `DISCORD_CLIENT_ID`, `DISCORD_CLIENT_SECRET`, `DISCORD_REDIRECT_URI`, `HOOKIO_COOKIE_SECURE`. Client: `VITE_DISCORD_LOGIN_URL`.

## Operator notes

`JWT_SECRET` rotation invalidates all sessions (users re-login). Align redirect URIs. Behind TLS-terminating proxies that do not forward HTTPS to Kestrel, set `HOOKIO_COOKIE_SECURE=true` or cookies may stay non-Secure.

## Links

- [[Hookio/Features/Guilds]]
- [[Hookio/Services/Discord]] · [[Hookio/Services/Dragonfly]] · [[Hookio/Services/Hookio API]]

## Official docs

- [Discord OAuth2](https://discord.com/developers/docs/topics/oauth2)
- [ASP.NET JWT bearer](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
