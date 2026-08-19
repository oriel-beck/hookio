---
date: 2026-08-17
updated: 2026-08-20
tags:
  - hookio
  - changelog
project: Hookio
---

# Changelog

Newest first. One entry per meaningful change. See [[Hookio/How we document]].

## 2026-08-20 — Production overhaul landed in git

- **What:** Committed the remaining overhaul: .NET 10, Twitch EventSub HTTP, health checks, DataManagers split, unique feeds + EventSub migration, client Vite 6 / Node 24 / ESLint 9, compose health + env files, CI, tests, env examples, vault notes. Root `.gitignore` also skips Obsidian workspace and downloaded plugins.
- **Why:** That work was already in the tree; only the gitignore commit had landed.
- **Files / packages:** `server/`, `client/`, `docker-compose.yml`, `.github/workflows/ci.yml`, `hookio-vault/`, `.cursor/rules/`, `*.env.example`. Not `Hookio/Plans/` (superseded notes, left untracked).
- **Follow-ups:** Operator fills env. Cookie `Secure` is automatic on HTTPS or `HOOKIO_COOKIE_SECURE=true`.
- **PR:** none

## 2026-08-20 — One root gitignore

- **What:** Deleted `client/.gitignore` and `server/Hookio/.gitignore`. All ignore rules live in the repo-root `.gitignore` (env, Compose `stacks`, .NET `bin`/`obj`/DLL/PDB, Node `node_modules`/`dist`, editors). Did not copy client’s `*.sln` pattern so `server/server.sln` stays tracked.
- **Why:** Nested gitignores were easy to miss; only `server/Hookio/` had `bin`/`obj`, which is why other projects leaked build files.
- **Files / packages:** `.gitignore`; removed `client/.gitignore`, `server/Hookio/.gitignore`; [[Hookio/Infrastructure]].
- **Follow-ups:** none — committed with the build-output untrack below.
- **PR:** none (`9873a43`)

## 2026-08-20 — Ignore .NET build outputs in git

- **What:** Root `.gitignore` now ignores `bin/`, `obj/`, `Debug/`, `Release/`, `*.dll`, `*.pdb`, `*.exe`, NuGet packages, test results, and coverage. Previously only `server/Hookio/` ignored `bin`/`obj`, so Contracts/Data/DataManagers/Shared/Tests build artifacts (including already-tracked net8 outputs) showed up as thousands of files to stage.
- **Why:** Generated assemblies and MSBuild intermediates are not source; they bloated `git status` (~1240 files).
- **Files / packages:** `.gitignore`; `git rm --cached` of tracked `bin/` and `obj/` under the library projects. No app code.
- **Follow-ups:** none — those files are untracked in the same commit as the root gitignore.
- **PR:** none (`9873a43`)

## 2026-08-20 — Vault rebuilt around current infrastructure

- **What:** Replaced the overhaul-plan vault with an index, [[Hookio/Infrastructure]], one note per service, and one note per feature, written from compose, env examples, README, and code as of today. Deleted the `Hookio/Plans/` pile. Updated `.cursor/rules/obsidian-documentation.mdc` and [[Hookio/How we document]] so future work updates service/feature notes, not plans.
- **Why:** The overhaul is in the code. Docs should describe what exists, not competing “do this next” sequences.
- **Files / packages:** vault under `hookio-vault/`; Cursor rule `.cursor/rules/obsidian-documentation.mdc`. No app code. No commit.
- **Follow-ups:** Keep service/feature notes in sync when the running system changes.
- **PR:** none

## 2026-08-20 — Vault: overhaul plans marked complete / archived

- **What:** (Superseded the same day by the vault rebuild above.) Had aligned plan notes with the completed overhaul and archived investigation notes.
- **Why:** After F1–F22 landed in code, notes still read as in-progress.
- **Files / packages:** vault only at the time. Plan files later deleted.
- **Follow-ups:** none — structure replaced 2026-08-20.
- **PR:** none

## 2026-08-19 — Overhaul F14–K (EventSub + .NET 10)

- **What:** Redis seconds scores, JWT guild claims, `WebApplicationFactory` tests. Split data managers, `DELETE` subscriptions, disable on Discord 401/404. Secrets not baked into images, JSON logs + request id, `/health` `/healthz`, compose `service_healthy`, TLS documented. Node 24, Vite 6.4.3, React 18.3.1, React Router 7.18.2, ESLint 9, TS 5.9.3. Twitch EventSub HTTP. `net10.0`, JwtBearer/EF 10.0.10, Npgsql 10.0.3, Swashbuckle 10.2.3, CI `10.0.x`.
- **Why:** Production hardening, EventSub on HTTPS, .NET 8 EOS.
- **Files / packages:** server DataManagers split, Twitch EventSub controller, health checks, EF migration `UniqueFeedsUrlAndTwitchEventSub`; client loaders/editor/Dockerfile; `docker-compose.yml`, README, `.env.example` placeholders only.
- **Follow-ups:** Operator fills env. Cookie `Secure` is automatic on HTTPS or `HOOKIO_COOKIE_SECURE=true`.
- **PR:** none

## 2026-08-19 — Overhaul phases A–E + F14 start

- **What:** Boot/ports, WebhookInfo, auth routes, OAuth seconds, JWT signing, enums, subscription URL, 201/400 + partitioned limiter, Discord webhook JSON without Discord.Net, Redis message map, RSS new vs update, TaskQueue BackgroundService. Started `Hookio.Tests`, Vitest enum tests, `.github/workflows/ci.yml`.
- **Why:** Correctness first so YouTube → Discord works, then cleanup and CI.
- **Files / packages:** compose, Dockerfiles, README; server auth/subscriptions/Discord/RSS/queue; client enums/loaders/home; removed Discord.Net, NRedisStack.
- **Follow-ups:** Completed in the 2026-08-19 F14–K entry.
- **PR:** none

## 2026-08-17 — Final overhaul plans (locked answers)

- **What:** Wrote sequenced overhaul plans (later executed in code; plan notes deleted 2026-08-20). Recorded locked product answers, now trimmed in [[Hookio/Decisions/Overhaul locked answers]].
- **Why:** User locked correctness-first, EventSub in-scope, RSS polling, aggressive cleanup, tests+CI.
- **Files / packages:** vault only at the time.
- **Follow-ups:** Execution completed in code 2026-08-19.
- **PR:** none

## 2026-08-17 — Draft upgrade overhaul plan

- **What:** Package/runtime upgrade inventory as of 2026-08-17 (later superseded by what shipped: net10, Vite 6, Node 24).
- **Why:** Repo was on .NET 8 / Node 18 / Vite 5 while .NET 8 EOS is 2026-11-10.
- **Files / packages:** vault notes since deleted.
- **Follow-ups:** none.
- **PR:** none

## 2026-08-17 — Document repo changes in Obsidian

- **What:** Always-apply Cursor project rule so agents document every meaningful Hookio change in this vault. Created the Hookio MOC, changelog, documentation convention, and the first decision note.
- **Why:** Changes were not being captured in a consistent place.
- **Files / packages:** `.cursor/rules/obsidian-documentation.mdc`; vault notes `Hookio.md`, `Hookio/Changelog.md`, `Hookio/How we document.md`, `Hookio/Decisions/Document changes in Obsidian.md`.
- **Follow-ups:** Keep changelog and service/feature notes updated as work lands.
- **PR:** none
