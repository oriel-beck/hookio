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
        id: makeid(10),
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
                [EventType["Video Edited"].toString()]: {
                    id: makeid(10),
                    message: generateDefaultMessage()
                },
                [EventType["Video Uploaded"].toString()]: {
                    id: makeid(10),
                    message: generateDefaultMessage()
                }
            }
        case Provider.twitch:
            return {
                [EventType["Stream Started"].toString()]: {
                    id: makeid(10),
                    message: generateDefaultMessage()
                },
                [EventType["Stream Updated"].toString()]: {
                    id: makeid(10),
                    message: generateDefaultMessage()
                },
                [EventType["Stream Ended"].toString()]: {
                    id: makeid(10),
                    message: generateDefaultMessage()
                }
            }
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

const convertEventsToSendableData = (events: FormikInitialValue['events']) => Object.entries(events).reduce((acc, [eventType, { message, id }]) => ({ ...acc, [eventType]: { id: typeof id === 'string' ? null : id, message: removeIDsFromNewEmbeds(message), eventType: +eventType } }), {} as Record<string, EventFormikInitialValue & { eventType: number }>)

function removeIDsFromNewEmbeds(message: MessageFormikInitialValue) {
    const newMessage = structuredClone(message);
    newMessage.embeds = newMessage.embeds.map((embed, embedIndex) => {
        // If the embed ID is string (created locally), remove it
        if (embed.id && typeof embed.id === 'string') embed.id = null;
        embed.index = embedIndex;
        embed.fields.map((field, fieldIndex) => {
            // If the embed field ID is string (created locally), remove it
            if (field.id && typeof field.id === 'string') field.id = null;
            field.index = fieldIndex;
            return field;
        });
        return embed;
    });
    return newMessage;
}

function makeid(length: number) {
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