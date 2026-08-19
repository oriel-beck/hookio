---
date: 2026-08-20
tags:
  - hookio
  - service
project: Hookio
---

# GitHub Actions

CI workflow `.github/workflows/ci.yml`. Two jobs on `ubuntu-latest`, triggered on push to `main`/`master` and on pull requests.

## Current behavior

### `server`

- `actions/checkout@v5`
- `actions/setup-dotnet@v4` with `dotnet-version: "10.0.x"`
- `dotnet restore` / `build` / `test` on `server/server.sln` (Release)

Covers `Hookio.Tests`: enum contracts, EventSub callback/HMAC, JWT guild claims, guild-access API, Redis score units, FeedUtils, Discord JSON.

### `client`

- `actions/setup-node@v4` with **Node 24**, npm cache on `client/package-lock.json`
- `npm ci`, `npm run lint` (ESLint max-warnings 0), `npm test` (Vitest), `npm run build` (`tsc && vite build`)

No Docker compose job. No deploy.

## How it is wired

Path: `.github/workflows/ci.yml`. Local equivalents in README: `dotnet test server/server.sln` and `cd client && npm test && npm run build`.

## Env vars (names only)

None required in the workflow file. Jobs do not start Postgres/Dragonfly; server tests use InMemory + `Testing` environment.

## Links

- [[Hookio/Infrastructure]]
- [[Hookio/Services/Hookio API]] · [[Hookio/Services/Nginx]]

## Official docs

- [GitHub Actions](https://docs.github.com/en/actions)
- [setup-dotnet](https://github.com/actions/setup-dotnet)
- [setup-node](https://github.com/actions/setup-node)
- [xUnit](https://xunit.net/)
- [Vitest](https://vitest.dev/)
