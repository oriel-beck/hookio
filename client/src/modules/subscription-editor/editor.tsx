import { Await, useLoaderData, useParams } from "react-router-dom";
import { Suspense, useState } from "react";
import { Field, FieldArray, FieldProps, Formik, useFormikContext } from "formik";
import EmbedForm from "./embed-form";
import EmbedPreview from "./preview";
import Loader from "../../components/loader";
import PageHeader from "../../components/page-heading";
import { Input, TextArea } from "../../components/input";
import { EventType, Provider } from "../../util/enums";
import { generateDefaultEvents, getEventTypes } from "../../util/util";
import * as Yup from 'yup';
import type { Embed, EmbedField, EventFormikInitialValue, FormikInitialValue, MessageFormikInitialValue, Subscription } from "../../types/types";
import ExpansionPanel from "../../components/expansion-panel";

const webhookRegex = new RegExp("https:\\/\\/(?:canary\\.)?discord(?:app)?\\.com\\/api\\/webhooks\\/\\d+\\/[a-zA-Z0-9_-]+", "s");
const twitchRegex = new RegExp("https?:\\/\\/(?:www\\.)?twitch\\.tv\\/([a-zA-Z0-9_]{4,25})", "s");
const youtubeRegex = new RegExp("https?:\\/\\/(?:www\\.)?youtube\\.com\\/channel\\/[a-zA-Z0-9_-]{22}", "s");

export default function SubscriptionEditor() {
    const data = useLoaderData() as { subscriptions: Promise<Subscription> };
    const params = useParams();
    const availableEvents = getEventTypes(Provider[params['provider'] as keyof typeof Provider]);
    const [eventType, setEventType] = useState<EventType>(availableEvents[0])

    const fieldSchema: Yup.ObjectSchema<EmbedField> = Yup.object({
        id: Yup.number().required(),
        name: Yup.string().required(),
        value: Yup.string().required(),
        inline: Yup.boolean().required(),
    })

    const embedSchema: Yup.ObjectSchema<Embed> = Yup.object({
        id: Yup.number().required(),
        description: Yup.string().optional(),
        title: Yup.string().optional(),
        titleUrl: Yup.string().optional().url(),
        author: Yup.string().optional(),
        authorUrl: Yup.string().optional().url(),
        authorIcon: Yup.string().optional().url(),
        color: Yup.string().optional(),
        image: Yup.string().optional().url(),
        footer: Yup.string().optional(),
        footerIcon: Yup.string().optional().url(),
        thumbnail: Yup.string().optional().url(),
        addTimestamp: Yup.boolean().required(),
        fields: Yup.array().of(fieldSchema).required()
    }).test({
        test(embed, ctx) {
            if (!embed.description && !embed.title && !embed.author && !embed.fields?.length && !embed.image && !embed.thumbnail && !embed.footer) return ctx.createError({ message: { invalid: true } });
            return true;
        }
    })

    const messageSchema: Yup.ObjectSchema<MessageFormikInitialValue> = Yup.object({
        content: Yup.string().optional(),
        username: Yup.string().optional(),
        avatar: Yup.string().optional().url(),
        embeds: Yup.array().of(embedSchema).required()
    });

    const eventSchema: Yup.ObjectSchema<EventFormikInitialValue> = Yup.object({
        message: messageSchema
    })

    const validationSchema: Yup.ObjectSchema<FormikInitialValue> = Yup.object({
        webhookUrl: Yup.string().url().test({
            test(url, ctx) {
                if (!webhookRegex.test(url || "")) return ctx.createError({ message: "Invalid webhook URL" });
                return true;
            }
        }),
        url: Yup.string().url().required().test({
            test(url, ctx) {
                switch (params['provider']) {
                    // youtube
                    case 'youtube':
                        if (!youtubeRegex.test(url || "")) return ctx.createError({ message: "Invalid channel URL" })
                        break;
                    // twitch
                    case 'twitch':
                        if (!twitchRegex.test(url || "")) return ctx.createError({ message: "Invalid channel URL" })
                        break;
                }
                return true;
            }
        }),
        events: Yup.object({
            // YouTube video create
            "0": eventSchema.optional(),
            // YouTube video edit (title/description changed)
            "1": eventSchema.optional(),

            // Twitch stream created
            "2": eventSchema.optional(),
            // Twitch stream updated
            "3": eventSchema.optional(),
            // Twitch stream ended
            "4": eventSchema.optional(),
        })
    });

    return (
        <Suspense
            fallback={<Loader />}
        >
            <Await
                resolve={data?.subscriptions}
            >
                {(subscription?: Subscription) => (
                    <Formik
                        initialValues={{
                            webhookUrl: "",
                            url: "",
                            events: subscription?.events || generateDefaultEvents(Provider[params['provider'] as keyof typeof Provider])
                        }}
                        onSubmit={(values, ctx) => {
                            setTimeout(() => {
                                console.log(values);
                                ctx.setSubmitting(false);
                            }, 5000);
                        }}
                        validationSchema={validationSchema}
                    >
                        {({
                            values,
                            handleSubmit,
                            isSubmitting,
                        }) => (
                            <div className="flex flex-col m-5 flex-1">
                                <PageHeader
                                    title="Subscriptions"
                                    subtitle={subscription ? "Edit your subscription below" : "Create a new subscription"}
                                    icon={
                                        <svg className="w-6 h-6 mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                                            <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 6.75h12M8.25 12h12m-12 5.25h12M3.75 6.75h.007v.008H3.75V6.75Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0ZM3.75 12h.007v.008H3.75V12Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm-.375 5.25h.007v.008H3.75v-.008Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Z" />
                                        </svg>
                                    }
                                >
                                    <>
                                        <span className="flex-auto"></span>
                                        <ul className="flex items-center space-x-5">
                                            {availableEvents.map((ev) => (
                                                <li key={ev} className={`cursor-pointer pb-1 relative w-fit block after:block after:content-[''] after:absolute after:h-[3px] after:bg-white after:w-full after:scale-y-0 after:hover:scale-y-100 after:transition after:duration-200 after:origin-left ${ev === eventType ? 'after:scale-y-100' : ''}`}>
                                                    <button onClick={() => setEventType(ev)}>
                                                        {EventType[ev]}
                                                    </button>
                                                </li>
                                            ))}
                                        </ul>
                                    </>
                                </PageHeader>
                                {/* subscription editor and subscription embed, split screen */}
                                <div className="flex-1 flex flex-col md:flex-row pt-5 md:max-h-[77vh] text-white">
                                    {/* Content for the first half (embed inputs) */}
                                    <div className="basis-1/2 flex-1 overflow-y-auto px-5 scrollbar-w-1 scrollbar-thumb-rounded-full scrollbar-track-rounded-full scrollbar scrollbar-thumb-gray-600 scrollbar-track-gray-300">
                                        <form onSubmit={handleSubmit}>
                                            <SubscriptionAndContentFields eventType={eventType} />
                                            <FieldArray
                                                name={`events.${eventType}.message.embeds`}>
                                                {(helpers) => (
                                                    <EmbedForm eventType={eventType} helpers={helpers} values={values.events[eventType.toString()].message} />
                                                )}
                                            </FieldArray>
                                            {/* TODO: actually make a good button */}
                                            <button className="py-2 px-4 rounded-sm bg-green-300 text-purple-800 font-semibold" type="submit" disabled={isSubmitting}>
                                                Save
                                            </button>
                                        </form>
                                    </div>
                                    <div className="basis-1/2 flex-1 bg-[#313338] overflow-y-auto scrollbar-w-1 scrollbar-thumb-rounded-full scrollbar-track-rounded-full scrollbar scrollbar-thumb-gray-600 scrollbar-track-gray-300 min-h-[700px] md:min-h-0">
                                        {/* Content for the second half (embed preview)*/}
                                        <EmbedPreview eventType={eventType} />
                                    </div>
                                </div>
                            </div>
                        )}
                    </Formik>
                )}
            </Await>
        </Suspense>
    )
}


function SubscriptionAndContentFields({ eventType }: { eventType: EventType }) {
    const formik = useFormikContext<FormikInitialValue>();
    const errors = formik.errors;
    return (
        <div className="space-y-4">
            <div>
                <Field name={`url`} >
                    {(props: FieldProps) => (
                        <Input error={errors.url} {...props} label="URL" placeholder={getPlaceholderByPath(location.pathname)!} />
                    )}
                </Field>
            </div>
            <ExpansionPanel label="Webhook">
                <div className="p-4">
                    <div>
                        <Field name={`webhookUrl`}>
                            {(props: FieldProps) => (
                                <Input error={errors.webhookUrl} {...props} label="Webhook URL" placeholder="https://discord.com/api/webhooks/..." />
                            )}
                        </Field>
                    </div>
                    <div>
                        <Field name={`events.${eventType}.message.username`}>
                            {(props: FieldProps) => (
                                <Input {...props} label="Username" placeholder="" />
                            )}
                        </Field>
                    </div>
                    <div>
                        <Field name={`events.${eventType}.message.avatar`}>
                            {(props: FieldProps) => (
                                <Input {...props} label="Avatar" placeholder="" />
                            )}
                        </Field>
                    </div>
                </div>
            </ExpansionPanel>
            <div>
                <Field name={`events.${eventType}.message.content`}>
                    {(props: FieldProps) => (
                        <TextArea {...props} label="Content" placeholder="..." limit={2000} />
                    )}
                </Field>
            </div>
        </div>
    )
}

function getPlaceholderByPath(path: string) {
    switch (true) {
        case path.includes("youtube"):
            return "https://www.youtube.com/channel/..."
        case path.includes("twitch"):
            return "https://www.twitch.tv/..."
    }
}