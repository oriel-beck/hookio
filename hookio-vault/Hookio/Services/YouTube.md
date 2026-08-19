---
date: 2026-08-20
tags:
  - hookio
  - service
project: Hookio
---

# YouTube

Hookio does **not** call the YouTube Data API. It polls each unique channel Atom feed every 15 minutes.

## Current behavior

- User pastes a channel URL matching `https://www.youtube.com/channel/{id}`.
- Stored feed URL: `https://www.youtube.com/feeds/videos.xml?channel_id={id}`.
- `FeedUtils` parses Atom (`entry`) or RSS (`item`): requires `id`/`guid` and `published`/`pubDate` or `updated`. Only the **latest** entry/item is used.
- New `id` → [[Hookio/Features/Announcements]] **Video Uploaded** (`EventType.NewFeed`). Same id, different timestamp → **Video Edited** (`UpdatedFeed`).
- Template keys are XML paths (e.g. namespaced element paths and `#attribute` keys) plus URLs detected as `TemplateStringType.Url`. See [[Hookio/Features/Message templates]].
- Watcher User-Agent `Hookio 1.0.0` when fetching via `FeedUtils.Parse(url, httpClient)`; the hosted watcher uses a plain `HttpClient.GetAsync`.
- Per-host semaphore plus optional `x-ratelimit-*` headers on the feed response (YouTube sometimes sends these).

`SubscriptionType.Youtube = 1`. Enum `Custom = 3` exists in the API but has **no UI** and is not a YouTube path.

## How it is wired

| Piece | Path |
| --- | --- |
| URL + feed create | `server/Hookio.DataManagers/SubscriptionService.cs` |
| Poll loop | `server/Hookio.DataManagers/Feeds/RssWatcherService.cs` |
| XML parse | `server/Hookio.DataManagers/Feeds/FeedUtils.cs` |
| Unique feeds | `server/Hookio.Data/Entities/Feed.cs` |
| Client provider | `client/src/modules/provider-selection.tsx` path `youtube` |

No YouTube env vars. Deleted leftovers: there is no `YOUTUBE_API_KEY` in `EnvNames`.

## Operator notes

Polling is best-effort (15-minute cadence, public feed). Handles (`/@name`) are **not** accepted — channel id URLs only.

## Links

- [[Hookio/Features/Announcements]] · [[Hookio/Features/Subscriptions]] · [[Hookio/Features/Message templates]]
- [[Hookio/Services/Postgres]] · [[Hookio/Services/Dragonfly]]
- [[Hookio/Decisions/Overhaul locked answers]]

## Official docs

YouTube’s channel Atom feed is a **legacy Google Data / Atom** endpoint; it is still what this app uses. The documented API (not used) is [YouTube Data API v3](https://developers.google.com/youtube/v3/getting-started). Feed format: [RFC 4287 Atom](https://www.rfc-editor.org/rfc/rfc4287). Example URL shape: `https://www.youtube.com/feeds/videos.xml?channel_id=CHANNEL_ID`.
