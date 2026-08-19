---
date: 2026-08-17
updated: 2026-08-20
status: accepted
tags:
  - hookio
  - decision
project: Hookio
---

# Decision: document every Hookio change in this vault

- **Date:** 2026-08-17
- **What:** All meaningful repo work (features, refactors, upgrades, bugfixes, config, infra, decisions) is documented here, not only in git commit messages. As of 2026-08-20 the vault is structured around **current** services and features (`Hookio/Services/`, `Hookio/Features/`), not plan piles.
- **Why:** The vault is the project memory. Agents and humans need one structure: changelog for what landed, service/feature notes for how the system works, decisions for durable choices.
- **Files / packages:** Cursor rule `.cursor/rules/obsidian-documentation.mdc`. Convention note [[Hookio/How we document]].
- **Follow-ups:** Update [[Hookio/Changelog]] on each meaningful change. Edit the matching service or feature note. Add a note under `Hookio/Decisions/` when a choice should outlive the changelog entry.
- **PR:** none

**Convention:** update existing notes; do not duplicate. Do not create `Hookio/Plans/`.
