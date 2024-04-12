import { Await, useAsyncValue, useLoaderData, useNavigate, useParams } from "react-router-dom";
import { Suspense, useEffect, useState } from "react";
import { Field, FieldArray, FieldProps, Formik, useFormikContext } from "formik";
import { MdOutlineContentCopy } from "react-icons/md";
import EmbedPreview from "./preview";
import EmbedForm from "./embed-form";
import CopyModal from "./copy-modal";
import { convertEmbedToFormikData, generateDefaultEvent, generateDefaultEvents, getEventTypes, submitSubscription } from "../../util/util";
import { APIEvents, EventType, Provider } from "../../util/enums";
import PageHeader from "../../components/page-heading";
import ExpansionPanel from "../../components/expansion-panel";
import Loader from "../../components/loader";
import { Input, TextArea } from "../../components/input";
import type { EmbedField, EmbedFormikInitialValue, EventFormikInitialValue, FormikInitialValue, MessageFormikInitialValue, Subscription } from "../../types/types";
import * as Yup from 'yup';
import { IoWarningOutline } from "react-icons/io5";
import { AnimatePresence, motion } from "framer-motion";
import { fadeVariants } from "../../animation/fade-variants";

const webhookRegex = new RegExp("https:\\/\\/(?:canary\\.)?discord(?:app)?\\.com\\/api\\/webhooks\\/\\d+\\/[a-zA-Z0-9_-]+", "s");
const twitchRegex = new RegExp("https?:\\/\\/(?:www\\.)?twitch\\.tv\\/([a-zA-Z0-9_]{4,25})", "s");
const youtubeRegex = new RegExp("https?:\\/\\/(?:www\\.)?youtube\\.com\\/channel\\/[a-zA-Z0-9_-]{22}", "s");
const imageExtensionRegex = new RegExp("\\.(png|jpe?g|gif|bmp|webp)$");

const invalidUrlError = "This is not a valid URL";
const invalidDirectLinkError = "Please provide a direct link";

export default function SubscriptionEditor() {
    const data = useLoaderData() as { subscription: Promise<Subscription> };

    return (
        <Suspense fallback={<Loader />}>
            <Await resolve={data?.subscription}>
                <FormikForm />
            </Await>
        </Suspense>
    )
}

function FormikForm() {
    const params = useParams();
    const availableEvents = getEventTypes(Provider[params['provider'] as keyof typeof Provider]);
    const [eventType, setEventType] = useState<EventType>(availableEvents[0]);
    const subscription = useAsyncValue() as Subscription;

    const navigate = useNavigate();

    const [copyData, setCopyData] = useState<{ origin: EventType, data: EventFormikInitialValue } | null>(null);
    const openModal = (origin: EventType, data: EventFormikInitialValue) => setCopyData({ origin, data });
    const closeModal = () => setCopyData(null);

    const isValidImageUrl = (url: string) => imageExtensionRegex.test(url);

    const fieldSchema: Yup.ObjectSchema<EmbedField> = Yup.object({
        id: Yup.mixed().required(),
        index: Yup.number().optional(),
        name: Yup.string().required("Please provide a field name"),
        value: Yup.string().required("Please provide a field value"),
        inline: Yup.boolean().required(),
    })

    const embedSchema: Yup.ObjectSchema<EmbedFormikInitialValue> = Yup.object({
        id: Yup.mixed().required(),
        index: Yup.number().optional(),
        description: Yup.string().optional(),
        title: Yup.object({
            text: Yup.string().optional(),
            url: Yup.string().optional().url(invalidUrlError)
        }).test({
            test(val, ctx) {
                if (val.url && !val.text) return ctx.createError({ message: { text: "Please supply a title" } });
                return true;
            }
        }),
        author: Yup.object({
            text: Yup.string().optional(),
            url: Yup.string().url(invalidUrlError),
            icon: Yup.string().optional().url(invalidUrlError).test({
                test: (val, ctx) => {
                    if (!val) return true;
                    if (!isValidImageUrl(val)) return ctx.createError({ message: invalidDirectLinkError });
                    return true;
                }
            })
        }).test({
            test: (val, ctx) => {
                if ((val.url || val.icon) && !val.text) return ctx.createError({ message: { text: "Please supply an author" } });
                return true;
            }
        }),

        color: Yup.string().optional(),
        image: Yup.string().optional().url(invalidUrlError).test({
            test: (val, ctx) => {
                if (!val) return true;
                if (!isValidImageUrl(val)) return ctx.createError({ message: invalidDirectLinkError });
                return true
            }
        }),
        footer: Yup.object({
            text: Yup.string().optional(),
            icon: Yup.string().optional().url(invalidUrlError).test({
                test: (val, ctx) => {
                    if (!val) return true;
                    if (!isValidImageUrl(val)) return ctx.createError({ message: invalidDirectLinkError });
                    return true
                }
            })
        }).test({
            test: (val, ctx) => {
                if (val.icon && !val.text) return ctx.createError({ message: { text: "Please supply a footer" } });
                return true;
            }
        }),
        thumbnail: Yup.string().optional().url(invalidUrlError).test({
            test: (val, ctx) => {
                if (!val) return true;
                if (!isValidImageUrl(val)) return ctx.createError({ message: invalidDirectLinkError });
                return true
            }
        }),
        addTimestamp: Yup.boolean().required(),
        fields: Yup.array().of(fieldSchema).required()
    }).test({
        test(embed, ctx) {
            if (!embed.description && !embed.title.text && !embed.author.text && !embed.fields?.length && !embed.image && !embed.thumbnail && !embed.footer.text) return ctx.createError({ message: { invalid: true } });
            return true;
        }
    })

    const messageSchema: Yup.ObjectSchema<MessageFormikInitialValue> = Yup.object({
        id: Yup.mixed().optional(),
        content: Yup.string().optional(),
        username: Yup.string().optional().test({
            test(str, ctx) {
                if (!str) return true;
                if (/(?:discord|clyde|```|system message|everyone|here)/i.test(str)) return ctx.createError({ message: "Username contains forbidden characters" });
                return true;
            }
        }),
        avatar: Yup.string().optional().url(),
        embeds: Yup.array().of(embedSchema).required()
    });

    const eventSchema: Yup.ObjectSchema<EventFormikInitialValue> = Yup.object({
        id: Yup.mixed().optional(),
        message: messageSchema.required().test({
            test: (val, ctx) => {
                if (!val.content && !val.embeds.length) return ctx.createError({ message: "Invalid message" });
                return true;
            }
        }).test({
            test: (val, ctx) => {
                const count = (val.content?.length || 0) + val.embeds.reduce((embedsAcc, currentEmbed) => embedsAcc + (currentEmbed.description?.length || 0) + (currentEmbed.title?.text?.length || 0) + (currentEmbed.author?.text?.length || 0) + (currentEmbed.footer?.text?.length || 0) + currentEmbed.fields.reduce((fieldsAcc, currentField) => fieldsAcc + (currentField.name?.length || 0) + (currentField.value?.length || 0), 0), 0);
                if (count > 6000) return ctx.createError({ message: "Content over 6000" });
                return true;
            }
        })
    })

    const validationSchema: Yup.ObjectSchema<FormikInitialValue> = Yup.object({
        webhookUrl: getWebhookUrl(),
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
        events: getValidationEvents()
    });

    function getValidationEvents() {
        switch (params['provider']) {
            case 'youtube':
                return Yup.object({
                    // YouTube video create
                    "0": eventSchema.required(),
                    // YouTube video edit (title/description changed)
                    "1": eventSchema.required(), // TODO: maybe add an `edit message` option which when turned off will simply send a new message instead of editing the old one
                })
            case 'twitch':
                return Yup.object({
                    // Twitch stream created
                    "2": eventSchema.required(),
                    // Twitch stream updated (title/description/game changed)
                    "3": eventSchema.required(), // TODO: maybe add an `edit message` option which when turned off will simply send a new message instead of editing the old one
                    // Twitch stream ended
                    "4": eventSchema.required(), // TODO: maybe add a `delete message` option for stream ended, need to see how to implement that
                })
            default:
                // Should not get here
                return Yup.object({})
        }
    }

    function getWebhookUrl() {
        // If this is not a new subscription then make webhook optional since it will not return from the backend
        let schema = Yup.string()
        if (params['subscriptionId']) schema = schema.optional();
        else schema = schema.required();
        schema.url().test({
            test(url, ctx) {
                if (!webhookRegex.test(url || "")) return ctx.createError({ message: "Invalid webhook URL" });
                return true;
            }
        });
        return schema;
    }

    useEffect(() => {
        if (params['subscriptionId'] && 'status' in subscription) return navigate(`/servers/${params['serverId']}/${params['provider']}`, { replace: true })
    });

    return (
        <Formik
            initialValues={{
                // Webhook url is not returned, schema is optional if this is an edit and not a new subscription
                webhookUrl: "",
                url: `${subscription?.url || ''}`,
                events: subscription?.events ? convertAPIEventsToFront(subscription.events) : generateDefaultEvents(Provider[params['provider'] as keyof typeof Provider])
            }}
            onSubmit={async (values, ctx) => {
                await submitSubscription(values, Provider[params['provider'] as keyof typeof Provider], params['serverId']!, subscription?.id);
                ctx.setSubmitting(false);
            }}
            validationSchema={validationSchema}
            validateOnMount={true}
        >
            {({
                values,
                handleSubmit,
                isSubmitting,
                setFieldValue,
                errors
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
                                        <AnimatePresence>
                                            {errors.events?.[ev.toString()]?.message &&
                                                <motion.span variants={fadeVariants} animate="show" initial="hide" exit="hide" className="cursor-default absolute -left-6 top-1" >
                                                    <IoWarningOutline className="w-5 h-5 text-red-400" />
                                                </motion.span>
                                            }
                                        </AnimatePresence>
                                        <div className="flex items-center space-x-1">
                                            <button onClick={() => setEventType(ev)}>
                                                {EventType[ev]}
                                            </button>
                                            {ev != eventType &&
                                                <button aria-label={`Copy from ${EventType[ev]}`} onClick={() => openModal(ev, values.events[ev])}>
                                                    <MdOutlineContentCopy className="h-5 w-5" />
                                                </button>
                                            }
                                        </div>
                                    </li>
                                ))}
                            </ul>
                        </>
                    </PageHeader>
                    {/* subscription editor and subscription embed, split screen */}
                    <div className="flex-1 flex flex-col md:flex-row pt-5 md:max-h-[77vh] text-white">
                        {/* Content for the first half (embed inputs) */}
                        <div className="pb-4 md:pb-0 md:basis-1/2 md:flex-1 overflow-y-auto md:px-5 scrollbar-w-1 scrollbar-thumb-rounded-full scrollbar-track-rounded-full scrollbar scrollbar-thumb-gray-600 scrollbar-track-gray-300">
                            <form onSubmit={handleSubmit}>
                                <SubscriptionAndContentFields eventType={eventType} />
                                <FieldArray
                                    name={`events.${eventType}.message.embeds`}>
                                    {(helpers) => (
                                        <EmbedForm eventType={eventType} helpers={helpers} values={values.events[eventType.toString()].message} />
                                    )}
                                </FieldArray>
                                <div className="w-full flex mt-2 space-x-2">
                                    <button className="w-full text-lg font-semibold py-2 px-4 rounded-md border border-red-500 text-red-500 hover:opacity-90" onClick={() => setFieldValue(`events.${eventType}`, generateDefaultEvent())}>
                                        Clear
                                    </button>
                                    <button className="w-full text-lg py-2 px-4 rounded-md bg-[#5865F2] border border-white text-white font-semibold disabled:bg-opacity-50 disabled:cursor-default hover:shadow-md hover:shadow-purple-900 disabled:shadow-none" type="submit" disabled={isSubmitting || !!Object.keys(errors).length}>
                                        Save
                                    </button>
                                </div>
                            </form>
                        </div>
                        <div className="mb-4 md:mb-0 md:basis-1/2 md:flex-1 bg-[#313338] overflow-y-auto scrollbar-w-1 scrollbar-thumb-rounded-full scrollbar-track-rounded-full scrollbar scrollbar-thumb-gray-600 scrollbar-track-gray-300 min-h-[700px] md:min-h-0">
                            {/* Content for the second half (embed preview)*/}
                            <EmbedPreview eventType={eventType} />
                        </div>
                    </div>
                    {copyData && <CopyModal originEventType={copyData.origin} targetEventType={eventType} targetId={values.events[eventType.toString()].id} data={copyData.data} closeModal={closeModal} />}
                </div>
            )}
        </Formik>
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
            <ExpansionPanel invalid={!!errors.webhookUrl} label="Webhook">
                <div className="p-4 space-y-2">
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
                                <Input {...props} label="Username" error={errors.events?.[eventType]?.message?.username} limit={80} />
                            )}
                        </Field>
                    </div>
                    <div>
                        <Field name={`events.${eventType}.message.avatar`}>
                            {(props: FieldProps) => (
                                <Input {...props} label="Avatar" />
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

function convertAPIEventsToFront(events: Subscription['events']): { [eventType: string]: EventFormikInitialValue } {
    return Object.entries(events).reduce((acc, [eventKey, { message, id }]) => ({ ...acc, [APIEvents[eventKey as keyof typeof APIEvents]]: { message: { ...message, embeds: message.embeds.map((e) => convertEmbedToFormikData(e)) }, id } }), {})
}