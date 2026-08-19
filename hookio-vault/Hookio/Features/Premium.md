---
date: 2026-08-20
tags:
  - hookio
  - feature
project: Hookio
---

# Premium

**Stub.** There is no Patreon integration, no webhook, and no way to set `User.PremiumExpires` from the API.

## Current behavior

- `User.PremiumExpires` column exists. `User.Premium` is true only if that timestamp is in the future. Nothing writes it.
- `CurrentUserResponse.Premium` is **always `0`** (`SubscriptionMapper.ToCurrentUser`).
- Creating a third subscription for a guild throws `RequiresPremiumException` (“This feature requires premium”) → HTTP 400. The check is **count ≥ 2**, not `user.Premium`.
- Client: create button is inert at `count >= 2` with tooltip “subscribe in Patreon”. User `premium` field is unused in UI logic.

## How it is wired

| Piece | Path |
| --- | --- |
| Entity TODO | `server/Hookio.Data/Entities/User.cs` |
| Gate | `server/Hookio.DataManagers/SubscriptionService.cs` create |
| Exception | `server/Hookio.Shared/Exceptions/RequiresPremiumException.cs` |
| UI | `client/src/modules/subscriptions-manager.tsx` |

## Env vars (names only)

None.

## Links

- [[Hookio/Features/Subscriptions]]
- [[Hookio/Decisions/Overhaul locked answers]]

## Official docs

None in-app. If this is implemented later, Patreon’s API docs would apply: [Patreon API](https://docs.patreon.com/).
