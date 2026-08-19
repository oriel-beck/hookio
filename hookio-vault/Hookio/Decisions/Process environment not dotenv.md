---
date: 2026-08-20
status: accepted
tags:
  - hookio
  - decision
project: Hookio
---

# Decision: process environment, not a dotenv parser

- **Date:** 2026-08-20
- **What:** Deleted `DotEnv`. The API reads `Environment.GetEnvironmentVariable` only. Compose `env_file` (`server/Hookio/.env.production`) injects those names into the container process. Images still must not bake secrets (`.dockerignore` already excludes `.env*`).
- **Why:** A hand-rolled `.env` text parser is redundant in Compose (no-op: cwd is `/app`, no `.env` file) and wrong for local `dotnet run` (naive `KEY=value` split, overwrites existing env). The host already loads env files.
- **Files / packages:** deleted `server/Hookio.Shared/DotEnv.cs`; `server/Hookio/Program.cs`; `server/Hookio/.env.example`; README; [[Hookio/Infrastructure]]; [[Hookio/Services/Hookio API]].
- **Follow-ups:** Local `dotnet run` outside Compose needs the `EnvNames` keys in the process environment (shell or IDE). Do not add a replacement file parser. User secrets would only work if callers read `IConfiguration` instead of `GetEnvironmentVariable`.
- **PR:** none
