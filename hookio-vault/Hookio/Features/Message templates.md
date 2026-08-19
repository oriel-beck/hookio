---
date: 2026-08-20
tags:
  - hookio
  - feature
project: Hookio
---

# Message templates

`{placeholder}` substitution in webhook content, username, avatar, and embed strings before Discord execute/edit.

## Current behavior

`TemplateHandler` replaces `{key}` when `key` exists in the dictionary; unknown placeholders are left as-is. Regex: `\{([^{}]*)\}`.

### YouTube / RSS

Keys come from walking the Atom/RSS XML: element paths (`feed.entry.title`, namespaced `prefix:local`) and attributes as `path#attr`. Values that look like `http…` URLs are typed `Url` in the contract (for the editor to treat as links). See `FeedUtils.Parse`.

### Twitch

Fixed keys from the EventSub payload: `user`, `login`, `title`, `game`, `category`, `url` (`https://www.twitch.tv/{login}`).

Users type these tokens in [[Hookio/Features/Subscription editor]]. There is no in-app catalog of RSS keys; they follow the feed XML.

## How it is wired

| Piece | Path |
| --- | --- |
| Parser | `server/Hookio.DataManagers/Utils/TemplateHandler.cs` |
| RSS keys | `server/Hookio.DataManagers/Feeds/FeedUtils.cs` |
| Twitch keys | `server/Hookio.DataManagers/Twitch/TwitchEventSubHandler.cs` |
| Embed apply | `server/Hookio.DataManagers/Utils/DiscordUtils.cs` |
| Enum | `server/Hookio.Shared/Enums/TemplateStringType.cs` |

## Env vars (names only)

None.

## Links

- [[Hookio/Features/Announcements]] · [[Hookio/Features/Twitch EventSub]] · [[Hookio/Features/Subscription editor]]
- [[Hookio/Services/YouTube]]

## Official docs

- [RFC 4287 Atom](https://www.rfc-editor.org/rfc/rfc4287)
- [Discord message content](https://discord.com/developers/docs/resources/message#create-message)
