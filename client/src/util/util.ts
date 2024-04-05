import { randomUUID } from "crypto";
import { Embed, EmbedField, EventFormikInitialValue, FormikInitialValue, MessageFormikInitialValue } from "../types/types";
import { EventType, Provider } from "./enums";

export function getEventTypes(provider: Provider) {
    switch (provider) {
        case Provider.youtube:
            return [EventType["Video Uploaded"], EventType["Video Edited"]]
        case Provider.twitch:
            return [EventType["Stream Started"], EventType["Stream Updated"], EventType["Stream Ended"]]
    }
}

export function generateNewEmbed(): Embed {
    return {
        id: randomUUID(),
        addTimestamp: false,
        description: "",
        title: "",
        titleUrl: "",
        author: "",
        authorUrl: "",
        authorIcon: "",
        color: "",
        image: "",
        footer: "",
        footerIcon: "",
        thumbnail: "",
        fields: [] as EmbedField[]
    }
}

export function generateNewField(): EmbedField {
    return {
        id: randomUUID(),
        name: "",
        value: "",
        inline: false
    }
}

export function generateDefaultEvents(provider: Provider): { [eventType: string]: EventFormikInitialValue } {
    switch (provider) {
        case Provider.youtube:
            return {
                [EventType["Video Edited"].toString()]: {
                    message: generateDefaultMessage()
                },
                [EventType["Video Uploaded"].toString()]: {
                    message: generateDefaultMessage()
                }
            }
        case Provider.twitch:
            return {
                [EventType["Stream Started"].toString()]: {
                    message: generateDefaultMessage()
                },
                [EventType["Stream Updated"].toString()]: {
                    message: generateDefaultMessage()
                },
                [EventType["Stream Ended"].toString()]: {
                    message: generateDefaultMessage()
                }
            }
    }
}

export function generateDefaultMessage(): MessageFormikInitialValue {
    return {
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

    if (id) return await fetch(`/api/subscriptions/${guildId}/${id}`, { method: 'PATCH', body: JSON.stringify(data) });
    return await fetch(`/api/subscriptions/${guildId}`, { method: 'POST', body: JSON.stringify(data), headers: {
        'Content-Type': 'application/json'
    } });
}

const convertEventsToSendableData = (events: FormikInitialValue['events']) => Object.entries(events).reduce((acc, [eventType, { message }]) => ({ ...acc, [eventType]: { message: removeIDsFromNewEmbeds(message), eventType: +eventType } }), {} as Record<string, EventFormikInitialValue & { eventType: number }>)

function removeIDsFromNewEmbeds(message: MessageFormikInitialValue) {
    message.embeds = message.embeds.map((embed, embedIndex) => {
        // If the embed ID is string (created locally), remove it
        if (embed.id && typeof embed.id === 'string') embed.id = undefined;
        embed.index = embedIndex;
        embed.fields.map((field, fieldIndex) => {
            // If the embed field ID is string (created locally), remove it
            if (field.id && typeof field.id === 'string') field.id = undefined;
            field.index = fieldIndex;
            return field;
        });
        return embed;
    });
    return message;
}