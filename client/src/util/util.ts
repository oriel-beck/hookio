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
