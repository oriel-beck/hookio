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

export const footer = z.object({
    text: z.string().max(256),
    iconUrl: z.string().url().optional()
});

export const image = z.object({
    url: z.string().url()
});

export const thumbnail = z.object({
    url: z.string().optional()
});

export const author = z.object({
    name: z.string().max(256),
    url: z.string().url().optional(),
    iconUrl: z.string().url().optional()
});

export const field = z.object({
    name: z.string().max(256),
    value: z.string().max(1024),
    inline: z.boolean()
})

export const embed = z.object({
    title: z.string().max(250).optional(),
    description: z.string().max(4096).optional(),
    url: z.string().url().optional(),
    timestamp: z.date().optional(),
    color: z.number().positive().optional(),
    footer: footer.optional(),
    image: image.optional(),
    thumbnail: thumbnail.optional(),
    author: author.optional(),
    fields: z.array(field)
})

export const message = z.object({
    id: z.number(),
    content: z.string().max(2048).optional(),
    embeds: z.array(embed),
    action: z.nativeEnum(MessageAction).optional(),
    type: z.nativeEnum(MessageType)
});

export const subscription = z.object({
    id: z.number(),
    guildId: z.bigint(),
    subscriptionType: z.nativeEnum(SubscriptionType),
    source: z.string().optional(),
    messages: z.array(message)
});