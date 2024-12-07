import { z } from "zod";

export enum SubscriptionType {
    YouTube = 1,
    Twitch
}

export enum MessageAction {
    CreateMessage = 1,
    UpdateMessage,
    DeleteMessage
}

export enum MessageType {
    YouTubeVideoCreated = 1,
    YouTubeVideoUpdated,
    YouTubeVideoDeleted,
    TwitchStreamStarted,
    TwitchStreamUpdated,
    TwitchStreamEnded,
}

export type Subscription = z.TypeOf<typeof subscription>;
export type Message = z.TypeOf<typeof message>;
export type Embed = z.TypeOf<typeof embed>;
export type EmbedField = z.TypeOf<typeof field>;

export const footer = z.object({
    text: z.string().max(256),
    iconUrl: z.optional(z.string().url())
});

export const image = z.object({
    url: z.string().url()
});

export const thumbnail = z.object({
    url: z.optional(z.string())
});

export const author = z.object({
    name: z.string().max(256),
    url: z.optional(z.string().url()),
    iconUrl: z.optional(z.string().url())
});

export const field = z.object({
    name: z.string().max(256),
    value: z.string().max(1024),
    inline: z.boolean()
})

export const embed = z.object({
    title: z.optional(z.string().max(250)),
    description: z.optional(z.string().max(4096)),
    url: z.optional(z.string().url()),
    timestamp: z.optional(z.date()),
    color: z.optional(z.number().positive()),
    footer: z.optional(footer),
    image: z.optional(image),
    thumbnail: z.optional(thumbnail),
    author: z.optional(author),
    fields: z.array(field)
})

export const message = z.object({
    id: z.number(),
    content: z.string().max(2000).or(z.null()),
    embeds: z.array(embed),
    action: z.nativeEnum(MessageAction).or(z.null()),
    type: z.nativeEnum(MessageType)
});

export const subscription = z.object({
    id: z.number(),
    guildId: z.string(),
    subscriptionType: z.nativeEnum(SubscriptionType),
    source: z.string(),
    messages: z.array(message),
    webhookAvatar: z.optional(z.string()),
    webhookUsername: z.string()
});