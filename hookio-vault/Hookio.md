---
date: 2026-08-17
updated: 2026-08-20
tags:
  - hookio
  - moc
project: Hookio
---

# Hookio

Discord-webhook announcer: users log in with Discord, pick a guild they can manage, configure YouTube (RSS) and Twitch (EventSub) subscriptions, then Hookio posts or edits webhook messages when those sources change.

This vault documents **what exists in the repo today**. Start at [[Hookio/Infrastructure]], then the service and feature notes below.

## How to use this vault

- [[Hookio/How we document]]
- [[Hookio/Changelog]]
- [[Hookio/Decisions/Document changes in Obsidian]]
- [[Hookio/Decisions/Process environment not dotenv]] — Compose `env_file`, not an in-app `.env` parser
- [[Hookio/Decisions/Overhaul locked answers]] — product constraints that still hold

## Infrastructure

- [[Hookio/Infrastructure]] — compose topology, ports, images, how pieces talk

## Services

Internal containers and the third-party APIs they call:

- [[Hookio/Services/Hookio API]] — ASP.NET on Kestrel (`server`)
- [[Hookio/Services/Nginx]] — SPA + `/api` reverse proxy (`client`)
- [[Hookio/Services/Postgres]] — EF Core database
- [[Hookio/Services/Dragonfly]] — Redis-protocol cache
- [[Hookio/Services/Discord]] — OAuth, guilds, webhooks, rate-limit queue
- [[Hookio/Services/YouTube]] — channel Atom/RSS polling
- [[Hookio/Services/Twitch]] — Helix + EventSub
- [[Hookio/Services/GitHub Actions]] — CI

## Features

- [[Hookio/Features/Home]]
- [[Hookio/Features/Auth]]
- [[Hookio/Features/Guilds]]
- [[Hookio/Features/Subscriptions]]
- [[Hookio/Features/Subscription editor]]
- [[Hookio/Features/Announcements]]
- [[Hookio/Features/Twitch EventSub]]
- [[Hookio/Features/Message templates]]
- [[Hookio/Features/Health]]
- [[Hookio/Features/Premium]]

## Folders

- `Hookio/Services/` — one note per service
- `Hookio/Features/` — one note per product feature
- `Hookio/Decisions/` — durable choices still true
- `Hookio/Changelog.md` — dated log of meaningful repo changes
