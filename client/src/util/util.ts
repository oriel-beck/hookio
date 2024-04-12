import { Embed, EmbedField, EmbedFormikInitialValue, EventFormikInitialValue, EventResponse, FormikInitialValue, Message, MessageFormikInitialValue } from "../types/types";
import { EventType, Provider } from "./enums";

export function getEventTypes(provider: Provider) {
    switch (provider) {
        case Provider.youtube:
            return [EventType["Video Uploaded"], EventType["Video Edited"]]
        case Provider.twitch:
            return [EventType["Stream Started"], EventType["Stream Updated"], EventType["Stream Ended"]]
    }
}

export function generateNewEmbed(): EmbedFormikInitialValue {
    return {
        id: makeid(10),
        addTimestamp: false,
        description: "",
        title: {
            text: "",
            url: ""
        },
        author: {
            text: "",
            url: "",
            icon: ""
        },
        color: "",
        image: "",
        footer: {
            text: "",
            icon: ""
        },
        thumbnail: "",
        fields: [] as EmbedField[]
    }
}

export function generateNewField(): EmbedField {
    return {
        id: makeid(10),
        name: "",
        value: "",
        inline: false
    }
}

export function generateDefaultEvents(provider: Provider): { [eventType: string]: EventFormikInitialValue } {
    switch (provider) {
        case Provider.youtube:
            return {
                [EventType["Video Edited"].toString()]: generateDefaultEvent(),
                [EventType["Video Uploaded"].toString()]: generateDefaultEvent()
            }
        case Provider.twitch:
            return {
                [EventType["Stream Started"].toString()]: generateDefaultEvent(),
                [EventType["Stream Updated"].toString()]: generateDefaultEvent(),
                [EventType["Stream Ended"].toString()]: generateDefaultEvent()
            }
    }
}

export function generateDefaultEvent() {
    return {
        id: makeid(10),
        message: generateDefaultMessage()
    }
}

export function generateDefaultMessage(): MessageFormikInitialValue {
    return {
        id: makeid(10),
        content: "",
        username: "Hookio",
        avatar: "https://c8.alamy.com/comp/R1PP58/hook-vector-icon-isolated-on-transparent-background-hook-transparency-logo-concept-R1PP58.jpg",
        embeds: []
    }
}

export async function submitSubscription(values: FormikInitialValue, type: Provider, guildId: string, id?: number) {
    const data = {
        subscriptionType: type,
        webhookUrl: values.webhookUrl,
        url: values.url,
        events: convertEventsToSendableData(values.events)
    }

    const headers = {
        'Content-Type': 'application/json'
    }
    console.log(data.events)

    const body = JSON.stringify(data);

    if (id) return await fetch(`/api/subscriptions/${guildId}/${id}`, { method: 'PATCH', body, headers });
    return await fetch(`/api/subscriptions/${guildId}`, { method: 'POST', body, headers });
}

const convertEventsToSendableData = (events: FormikInitialValue['events']) => Object.entries(events).reduce((acc, [eventType, { message, id }]) => ({ ...acc, [eventType]: { id: typeof id === 'string' ? null : id, message: convertMessageToSendableData(message), eventType: +eventType } }), {} as Record<string, EventResponse & { eventType: number }>)

function convertMessageToSendableData(message: MessageFormikInitialValue) {
    const embeds: Embed[] = [];
    for (let i = 0; i < message.embeds.length; i++) {
        embeds.push(convertEmbedsToSendableData(message.embeds[i], i));
    }

    const newMessage: Message = {
        id: typeof message.id === "string" ? null : message.id,
        content: message.content,
        username: message.username,
        avatar: message.avatar,
        embeds
    }

    return newMessage;
}

function convertEmbedsToSendableData(embed: EmbedFormikInitialValue, index: number): Embed {
    return {
        id: typeof embed.id === "string" ? null : embed.id,
        index: index,
        description: embed.description,
        title: embed.title?.text,
        titleUrl: embed.title?.url,
        author: embed.author?.text,
        authorUrl: embed.author?.url,
        authorIcon: embed.author?.icon,
        color: embed.color,
        image: embed.image,
        footer: embed.footer?.text,
        footerIcon: embed.footer?.icon,
        thumbnail: embed.thumbnail,
        addTimestamp: embed.addTimestamp,
        fields: embed.fields.map((field) => ({ ...field, id: typeof field.id === "string" ? null : field.id }))
    }
}

export function convertEmbedToFormikData(embed: Embed): EmbedFormikInitialValue {
    return {
        id: embed.id,
        index: embed.index,
        description: embed.description || "",
        title: {
            text: embed.title || "",
            url: embed.titleUrl || ""
        },
        author: {
            text: embed.author || "",
            url: embed.authorUrl || "",
            icon: embed.authorIcon || ""
        },
        color: embed.color,
        image: embed.image,
        footer: {
            text: embed.footer || "",
            icon: embed.footerIcon || ""
        },
        thumbnail: embed.thumbnail,
        addTimestamp: embed.addTimestamp,
        fields: embed.fields
    }
}

export function makeid(length: number) {
    let result = '';
    const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    const charactersLength = characters.length;
    let counter = 0;
    while (counter < length) {
        result += characters.charAt(Math.floor(Math.random() * charactersLength));
        counter += 1;
    }
    return result;
}

