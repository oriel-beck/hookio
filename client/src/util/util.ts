import { Embed, EmbedField, EventFormikInitialValue, MessageFormikInitialValue } from "../types/types";
import { EventType, Provider } from "./enums";

export function getEventTypes(provider: Provider) {
    switch (provider) {
        case Provider.youtube:
            return [EventType["Video Uploaded"], EventType["Video Edited"]]
        case Provider.twitch:
            // TODO: twitch events
            return []
    }
}

export function generateNewEmbed(): Embed {
    return {
        id: Math.random(),
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
        id: Math.random(),
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
            return {}
    }
}

export function generateDefaultMessage(): MessageFormikInitialValue {
    return {
        content: "",
        username: "Hookio",
        avatar: "",
        embeds: []
    }
}