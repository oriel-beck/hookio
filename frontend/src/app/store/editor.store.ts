import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { tapResponse } from '@ngrx/operators';
import { pipe, switchMap } from 'rxjs';
import { SubscriptionService } from '../services/subscription/subscription.service';
import { Embed, EmbedField, Subscription } from '../schemas/subscription.schema';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { MoveDirection } from '../components/embed-editor/accordion-header/accordion-header.component';
import { EmbedFieldForm, EmbedForm, getEmbedFieldForm, getEmbedForm, getSubscriptionForm, MessageForm, SubscriptionForm } from '../modules/editor/util';
import { FormArray } from '@angular/forms';

export interface EditorState {
    loading: boolean;
    error: boolean;
    id: number | null;
    guildId: string | null;
    subscription: Subscription | null;
    form: SubscriptionForm | null;
    accordionState: {
        [messageIdx: number]: {
            activeIndexes: number[];
            embeds: {
                [embedIdx: number]: {
                    activeIndexes: number[]; // Tracks open embed tabs
                    fields: number[] // tracks embedFields
                };
            };
        };
    };
}

const initialState: EditorState = {
    loading: true,
    error: false,
    id: null,
    guildId: null,
    subscription: null,
    form: null,
    accordionState: {}
}

export const EditorStore = signalStore(
    withState(initialState),
    withMethods((
        store,
        subscriptionService = inject(SubscriptionService),
        router = inject(Router)
    ) => ({
        init(guildId: string, id: number) {
            patchState(store, { guildId, id });
        },
        fetchSubscription: rxMethod<void>(
            pipe(
                switchMap(() => subscriptionService.getSubscriptions(store.guildId()!, store.id()!)),
                tapResponse({
                    next: (subscription) => {
                        patchState(store, {
                            subscription,
                            loading: false,
                            form: getSubscriptionForm(subscription),
                            accordionState: subscription.messages.map((_, i) => ({ [i]: { activeIndexes: [], embeds: {} } }) as EditorState['accordionState'][0])
                        })
                    },
                    error: (error: HttpErrorResponse) => {
                        patchState(store, { error: true, loading: false });
                        console.error("Failed to get subscription", error);
                        if (error.status === 403) router.navigate(["servers"])
                    }
                })
            )
        ),
        addEmbed(messageIdx: number) {
            const messagesForm = store.form()?.get('messages') as FormArray<MessageForm>;
            const messageForm = messagesForm.at(messageIdx);

            const embedsForm = messageForm.get('embeds') as FormArray<EmbedForm>;
            if (embedsForm.value.length === 10) return;

            embedsForm.push(getEmbedForm());
        },
        duplicateEmbed(messageIdx: number, embedIdx: number) {
            const messagesForm = store.form()?.get('messages') as FormArray<MessageForm>;
            const messageForm = messagesForm.at(messageIdx);

            const embedsForm = messageForm.get('embeds') as FormArray<EmbedForm>;
            if (embedsForm.value.length === 10) return;

            const embed = embedsForm.at(embedIdx);
            const duplicate = getEmbedForm(embed.value as Embed);
            embedsForm.insert(embedIdx, duplicate);
        },
        removeEmbed(messageIdx: number, embedIdx: number) {
            const messagesForm = store.form()?.get('messages') as FormArray<MessageForm>;
            const messageForm = messagesForm.at(messageIdx);

            const embedsForm = messageForm.get('embeds') as FormArray<EmbedForm>;
            embedsForm.removeAt(embedIdx);
        },
        moveEmbed(messageIdx: number, embedIdx: number, direction: "up" | "down") {
            const messagesForm = store.form()?.get('messages') as FormArray<MessageForm>;
            const messageForm = messagesForm.at(messageIdx);

            const embedsForm = messageForm.get('embeds') as FormArray<EmbedForm>;

            const targetIdx = direction === "up" ? embedIdx - 1 : embedIdx + 1;

            const embed = getEmbedForm(embedsForm.at(embedIdx).value as Embed);
            const targetEmbed = getEmbedForm(embedsForm.at(targetIdx).value as Embed);

            embedsForm.setControl(embedIdx, embed);
            embedsForm.setControl(embedIdx, targetEmbed);
        },
        addEmbedField(messageIdx: number, embedIdx: number) {
            const messagesForm = store.form()?.get('messages') as FormArray<MessageForm>;
            const messageForm = messagesForm.at(messageIdx);

            const embedsForm = messageForm.get('embeds') as FormArray<EmbedForm>;
            const embedForm = embedsForm.at(embedIdx);

            const fieldsForm = embedForm.get('fields') as FormArray<EmbedFieldForm>;

            if (fieldsForm.value.length === 25) return;
            fieldsForm.push(getEmbedFieldForm());
        },
        duplicateEmbedField(messageIdx: number, embedIdx: number, fieldIdx: number) {
            const messagesForm = store.form()?.get('messages') as FormArray<MessageForm>;
            const messageForm = messagesForm.at(messageIdx);

            const embedsForm = messageForm.get('embeds') as FormArray<EmbedForm>;
            const embedForm = embedsForm.at(embedIdx);

            const fieldsForm = embedForm.get('fields') as FormArray<EmbedFieldForm>;

            if (fieldsForm.value.length === 25) return;

            const field = fieldsForm.at(fieldIdx);
            fieldsForm.push(getEmbedFieldForm(field.value as EmbedField));
        },
        removeEmbedField(messageIdx: number, embedIdx: number, fieldIdx: number) {
            const messagesForm = store.form()?.get('messages') as FormArray<MessageForm>;
            const messageForm = messagesForm.at(messageIdx);

            const embedsForm = messageForm.get('embeds') as FormArray<EmbedForm>;
            const embedForm = embedsForm.at(embedIdx);

            const fieldsForm = embedForm.get('fields') as FormArray<EmbedFieldForm>;
            fieldsForm.removeAt(fieldIdx);
        },
        moveEmbedField(messageIdx: number, embedIdx: number, fieldIdx: number, direction: MoveDirection) {
            const messagesForm = store.form()?.get('messages') as FormArray<MessageForm>;
            const messageForm = messagesForm.at(messageIdx);

            const embedsForm = messageForm.get('embeds') as FormArray<EmbedForm>;
            const embedForm = embedsForm.at(embedIdx);

            const fieldsForm = embedForm.get('fields') as FormArray<EmbedFieldForm>;

            const targetIdx = direction === "up" ? embedIdx - 1 : embedIdx + 1;

            const field = getEmbedFieldForm(fieldsForm.at(fieldIdx).value as EmbedField);
            const targetField = getEmbedFieldForm(fieldsForm.at(targetIdx).value as EmbedField);

            fieldsForm.setControl(embedIdx, field);
            fieldsForm.setControl(embedIdx, targetField);
        }
    })),
    withComputed((store) => ({
        messages: computed(() => store.form()?.get('messages') as FormArray<MessageForm>)
    }))
)
