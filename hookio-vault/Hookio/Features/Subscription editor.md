---
date: 2026-08-20
tags:
  - hookio
  - feature
project: Hookio
---

# Subscription editor

Formik + Yup embed builder for one subscription. Users configure webhook URL, channel URL, webhook username/avatar, content, and Discord embeds (fields, color, timestamp). Live preview and copy-event-between-types.

## Current behavior

Routes:

- `/servers/:serverId/:provider/new`
- `/servers/:serverId/:provider/:subscriptionId`

`provider` is `youtube` or `twitch` (invalid → redirect). Event sets:

- YouTube: Video Uploaded, Video Edited
- Twitch: Stream Started, Stream Updated, Stream Ended

Validation: Discord webhook URL regex; YouTube `/channel/{22-char}`; Twitch login regex; image URLs must end in png/jpeg/gif/bmp/webp for some image fields.

Copy modal copies a message template from one event type to another. Preview renders markdown-ish content via `client/src/simple-markdown/`.

Delete (existing only) calls `DELETE /api/subscriptions/{guildId}/{id}` then navigates back.

Default webhook username `"Hookio"` and a stock avatar URL.

## How it is wired

| Piece | Path |
| --- | --- |
| Editor | `client/src/modules/subscription-editor/editor.tsx` |
| Embed form / fields / preview / copy | `embed-form.tsx`, `embed-field-form.tsx`, `preview.tsx`, `copy-modal.tsx` |
| Enums | `client/src/util/enums.ts` (kept in sync with server enums; Vitest `enums.test.ts`) |
| Provider picker | `client/src/modules/provider-selection.tsx` |

Server mapping: `SubscriptionMapper` converts stored feed/EventSub back to public YouTube/Twitch URLs for the form.

## Env vars (names only)

None. Uses cookie auth from [[Hookio/Features/Auth]].

## Links

- [[Hookio/Features/Subscriptions]] · [[Hookio/Features/Message templates]]
- [[Hookio/Services/Discord]] (embed payload shape)

## Official docs

- [Discord embeds (message structure)](https://discord.com/developers/docs/resources/message#embed-object)
- [Formik](https://formik.org/docs/overview)
- [Yup](https://github.com/jquense/yup)
