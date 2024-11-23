import { FormArray, FormControl, FormGroup, Validators } from "@angular/forms";
import { urlRegex, webhookRegex } from "../../constants";
import { Embed, Message, Subscription } from "../../schemas/subscription.schema";

export type SubscriptionForm = ReturnType<typeof getSubscriptionForm>;
export type MessageForm = ReturnType<typeof getMessageForm>;
export type EmbedForm = ReturnType<typeof getEmbedForm>;
export type EmbedFieldForm = ReturnType<typeof getEmbedFieldForm>;

let id = 0;

function* getId() {
    yield id++;
}

export function getEmbedForm(embed?: Embed) {
    return new FormGroup({
        // ID for change detection
        id: new FormControl(getId().next().value),
        title: new FormControl<string | undefined>(embed?.title),
        description: new FormControl<string | undefined>(embed?.description),
        url: new FormControl<string | undefined>(embed?.url, [Validators.pattern(urlRegex)]),
        timestamp: new FormControl<Date | undefined>(embed?.timestamp ? new Date(embed.timestamp) : undefined),
        color: new FormControl<string | undefined>(embed?.color ? embed.color.toString(16) : undefined),
        footer: new FormGroup({
            text: new FormControl<string | undefined>(embed?.footer?.text),
            iconUrl: new FormControl<string | undefined>(embed?.footer?.iconUrl, [Validators.pattern(urlRegex)]),
        }),
        author: new FormGroup({
            name: new FormControl<string | undefined>(embed?.author?.name),
            url: new FormControl<string | undefined>(embed?.author?.url, [Validators.pattern(urlRegex)]),
            iconUrl: new FormControl<string | undefined>(embed?.author?.iconUrl, [Validators.pattern(urlRegex)])
        }),
        image: new FormGroup({
            url: new FormControl<string | undefined>(embed?.image?.url, [Validators.pattern(urlRegex)])
        }),
        thumbnail: new FormGroup({
            url: new FormControl<string | undefined>(embed?.thumbnail?.url, [Validators.pattern(urlRegex)])
        }),
        fields: new FormArray<ReturnType<typeof getEmbedFieldForm>>(embed?.fields.map(f => getEmbedFieldForm(f)) || [])
    })
}

export function getEmbedFieldForm(field?: Embed['fields'][0]) {
    return new FormGroup({
        // ID for change detection
        id: new FormControl(getId().next().value),
        name: new FormControl<string | undefined>(field?.name, [Validators.required, Validators.maxLength(256)]),
        value: new FormControl<string | undefined>(field?.value, [Validators.required, Validators.maxLength(1024)]),
        inline: new FormControl<boolean>(field?.inline || false)
    })
}

export function getSubscriptionForm(subscription?: Subscription) {
    return new FormGroup({
        webhookUrl: new FormControl<string | undefined>(undefined, [Validators.pattern(webhookRegex)]),
        source: new FormControl<string | undefined>(subscription?.source, [Validators.required]),
        messages: new FormArray<ReturnType<typeof getMessageForm>>(subscription?.messages.map(m => getMessageForm(m)) || [])
        // maybe allow to change type with a warning that it resets the source
    })
}

export function getMessageForm(message?: Message) {
    return new FormGroup({
        content: new FormControl<string | undefined>(message?.content || undefined, [Validators.maxLength(2048)]),
        embeds: new FormArray<ReturnType<typeof getEmbedForm>>(message?.embeds.map(e => getEmbedForm(e)) || []),
    })
}