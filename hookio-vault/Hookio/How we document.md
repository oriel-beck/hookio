---
date: 2026-08-17
updated: 2026-08-20
tags:
  - hookio
  - documentation
project: Hookio
---

# How we document

Every meaningful Hookio change is recorded in this vault. Cursor enforces this via the always-apply project rule `.cursor/rules/obsidian-documentation.mdc`.

Vault root: `hookio-vault/` in the Hookio repo. Do not invent another vault. Document **current** infrastructure and features from the code, not overhaul plans.

## Where

| Kind | Path |
| --- | --- |
| Index / MOC | `Hookio.md` |
| Changelog | `Hookio/Changelog.md` (newest first) |
| Infrastructure | `Hookio/Infrastructure.md` |
| Services | `Hookio/Services/<name>.md` |
| Features | `Hookio/Features/<name>.md` |
| Decisions | `Hookio/Decisions/<short-name>.md` |
| This convention | `Hookio/How we document.md` |

Update the existing service or feature note when that topic already exists. Do not duplicate. Do not create `Hookio/Plans/` or competing “do this next” sequences.

A new reader should see:

1. [[Hookio.md]] → [[Hookio/Infrastructure]]
2. A service note for every container / external API
3. A feature note for every user-facing or operator-facing capability

## Required fields (changelog + decisions)

- **Date** (`YYYY-MM-DD`)
- **What changed**
- **Why**
- **Files / packages touched**
- **Follow-ups**
- **PR** if any

Skip typos and formatting-only edits unless they encode a decision.
