---
date: 2026-08-17
updated: 2026-08-20
status: accepted
tags:
  - hookio
  - decision
project: Hookio
---

# Decision: product constraints that still hold

- **Date:** 2026-08-17 (answers); 2026-08-20 (trimmed to what is still true in the running product)
- **What:** Hard product constraints from the 2026 overhaul. Implementation lives in the code and in [[Hookio/Infrastructure]] — not in a plan sequence.
- **Why:** These choices still describe how Hookio behaves. Historical F/R/U plan notes were deleted on 2026-08-20.
- **Files / packages:** Vault only.
- **Follow-ups:** Change these only with a new decision note plus updates to the matching service/feature notes.
- **PR:** none

## Still true

1. **YouTube** is RSS/Atom polling of `feeds/videos.xml?channel_id=`. No YouTube Data API key, no PubSubHubbub. See [[Hookio/Services/YouTube]] and [[Hookio/Features/Announcements]].
2. **Twitch** is EventSub HTTP webhooks (`stream.online` / `stream.offline` v1, `channel.update` v2). The Twitch UI stays. See [[Hookio/Services/Twitch]] and [[Hookio/Features/Twitch EventSub]].
3. **Runtime policy:** conservative LTS. The app is on **.NET 10** and **Node 24** (see [[Hookio/Infrastructure]]).
4. **Guild cap:** two subscriptions per guild unless [[Hookio/Features/Premium]] is implemented. Premium/Patreon is a stub.
5. **Tests and CI exist:** `Hookio.Tests`, Vitest, [[Hookio/Services/GitHub Actions]].
