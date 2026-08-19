---
date: 2026-08-20
tags:
  - hookio
  - feature
project: Hookio
---

# Home

Public landing page at `/`. Explains that Hookio announces YouTube uploads/edits and Twitch start/update/end via Discord webhooks.

## Current behavior

- If the user is logged in: button to `/servers`.
- Else: “Log in with Discord” using `VITE_DISCORD_LOGIN_URL`, or a message to set that env if missing.
- Strips `?code=` after OAuth so the code is not left in the address bar (exchange already happened in the root loader). See [[Hookio/Features/Auth]].
- Header + background are in `layout.tsx` for all routes. `react-helmet` meta tags on the app shell.

Not a marketing CMS — copy is hardcoded in `client/src/modules/home.tsx`.

## How it is wired

| Piece | Path |
| --- | --- |
| Page | `client/src/modules/home.tsx` |
| Router | `client/src/main.tsx` path `""` |
| Shell | `client/src/modules/layout.tsx`, `client/src/components/header.tsx` |

## Env vars (names only)

`VITE_DISCORD_LOGIN_URL`.

## Links

- [[Hookio/Features/Auth]] · [[Hookio/Features/Guilds]]
- [[Hookio/Services/Nginx]]

## Official docs

- [React Router](https://reactrouter.com/)
