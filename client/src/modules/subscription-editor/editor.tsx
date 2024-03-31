import { Await, useLoaderData, useParams } from "react-router-dom";
import { Suspense } from "react";
import { Field, FieldArray, FieldProps, Formik, useFormikContext } from "formik";
import EmbedForm from "./embed-form";
import EmbedPreview from "./preview";
import Loader from "../../components/loader";
import PageHeader from "../../components/page-heading";
import { Input, TextArea } from "../../components/input";
import type { EmbedFormInitialValues, Subscription } from "../../types/types";
import * as Yup from 'yup';
import * as sanitizeHtml from 'sanitize-html';

const webhookRegex = new RegExp("https:\\/\\/(?:canary\\.)?discord(?:app)?\\.com\\/api\\/webhooks\\/\\d+\\/[a-zA-Z0-9_-]+", "s");
const twitchRegex = new RegExp("https?:\\/\\/(?:www\\.)?twitch\\.tv\\/([a-zA-Z0-9_]{4,25})", "s");
const youtubeRegex = new RegExp("https?:\\/\\/(?:www\\.)?youtube\\.com\\/channel\\/[a-zA-Z0-9_-]{22}", "s");

export default function SubscriptionEditor() {
    const data = useLoaderData() as { subscriptions: Promise<Subscription> };
    const params = useParams();

    const validationSchema: Yup.ObjectSchema<EmbedFormInitialValues> = Yup.object({
        webhookUrl: Yup.string().transform(sanitizeHtml).url().test({
            test(url, ctx) {
                if (!webhookRegex.test(url || "")) return ctx.createError({ message: "Invalid webhook URL" });
                return true;
            }
        }),
        url: Yup.string().transform(sanitizeHtml).url().required().test({
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
        content: Yup.string().transform(sanitizeHtml).optional(),
        username: Yup.string().transform(sanitizeHtml).optional(),
        avatar: Yup.string().transform(sanitizeHtml).optional().url(),
        embeds: Yup.array().of(
            Yup.object({
                id: Yup.number().required(),
                description: Yup.string().transform(sanitizeHtml).optional(),
                title: Yup.string().transform(sanitizeHtml).optional(),
                titleUrl: Yup.string().transform(sanitizeHtml).optional().url(),
                author: Yup.string().transform(sanitizeHtml).optional(),
                authorUrl: Yup.string().transform(sanitizeHtml).optional().url(),
                authorIcon: Yup.string().transform(sanitizeHtml).optional().url(),
                color: Yup.string().transform(sanitizeHtml).optional(),
                image: Yup.string().transform(sanitizeHtml).optional().url(),
                footer: Yup.string().transform(sanitizeHtml).optional(),
                footerIcon: Yup.string().transform(sanitizeHtml).optional().url(),
                thumbnail: Yup.string().transform(sanitizeHtml).optional().url(),
                addTimestamp: Yup.boolean().required(),
                fields: Yup.array().of(
                    Yup.object({
                        id: Yup.number().required(),
                        name: Yup.string().transform(sanitizeHtml).required(),
                        value: Yup.string().transform(sanitizeHtml).required(),
                        inline: Yup.boolean().required(),
                    })
                ).required()
            }).test({
                test(embed, ctx) {
                    if (!embed.description && !embed.title && !embed.author && !embed.fields?.length && !embed.image && !embed.thumbnail && !embed.footer) return ctx.createError({ message: { invalid: true } });
                    return true;
                }
            })
        ).required()
    })

    return (
        <Suspense
            fallback={<Loader />}
        >
            <Await
                resolve={data?.subscriptions}
            >
                {(subscription?: Subscription) => (
                    <div className="flex flex-col m-5 flex-1">
                        <PageHeader
                            title="Subscriptions"
                            subtitle={subscription ? "Edit your subscription below" : "Create a new subscription"}
                            icon={
                                <svg className="w-6 h-6 mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 6.75h12M8.25 12h12m-12 5.25h12M3.75 6.75h.007v.008H3.75V6.75Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0ZM3.75 12h.007v.008H3.75V12Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm-.375 5.25h.007v.008H3.75v-.008Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Z" />
                                </svg>
                            }
                        />
                        {/* subscription editor and subscription embed, split screen */}
                        <div className="flex-1 flex flex-col md:flex-row pt-5 md:max-h-[77vh] text-white">
                            {/* Content for the first half (embed inputs) */}
                            <Formik
                                // TODO: implement the event tabs so it will use the correct message data for every event
                                initialValues={{
                                    webhookUrl: "",
                                    url: "",
                                    content: subscription?.messages[0].content || "",
                                    username: subscription?.messages[0].username || "",
                                    avatar: subscription?.messages[0].avatar || "",
                                    embeds: subscription?.messages[0].embeds || [],
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
                                    // errors,
                                    // touched,
                                    // handleChange,
                                    // handleBlur,
                                    handleSubmit,
                                    isSubmitting,
                                }) => (
                                    <>
                                        <div className="basis-1/2 flex-1 overflow-y-auto px-5 scrollbar-w-1 scrollbar-thumb-rounded-full scrollbar-track-rounded-full scrollbar scrollbar-thumb-gray-600 scrollbar-track-gray-300">
                                            <form onSubmit={handleSubmit}>
                                                <SubscriptionFields />
                                                <FieldArray
                                                    name="embeds">
                                                    {(helpers) => (
                                                        <EmbedForm helpers={helpers} values={values} />
                                                    )}
                                                </FieldArray>
                                                <button className="py-2 px-4 rounded-sm bg-green-300 text-purple-800 font-semibold" type="submit" disabled={isSubmitting}>
                                                    Save
                                                </button>
                                            </form>
                                        </div>
                                        <div className="basis-1/2 flex-1 bg-[#313338] overflow-y-auto scrollbar-w-1 scrollbar-thumb-rounded-full scrollbar-track-rounded-full scrollbar scrollbar-thumb-gray-600 scrollbar-track-gray-300 min-h-[700px]">
                                            {/* Content for the second half (embed preview)*/}
                                            <EmbedPreview />
                                        </div>
                                    </>
                                )}
                            </Formik>

                        </div>
                    </div>
                )}
            </Await>
        </Suspense>
    )
}


function SubscriptionFields() {
    const formik = useFormikContext<EmbedFormInitialValues>();
    const errors = formik.errors;
    return (
        <div className="space-y-4">
            <div>
                <Field name="url" >
                    {(props: FieldProps) => (
                        <Input error={errors.url} {...props} label="URL" placeholder={getPlaceholderByPath(location.pathname)!} />
                    )}
                </Field>
            </div>
            <div>
                <Field name="webhookUrl">
                    {(props: FieldProps) => (
                        <Input error={errors.webhookUrl} {...props} label="Webhook URL" placeholder="https://discord.com/api/webhooks/..." />
                    )}
                </Field>
            </div>
            <div>
                <Field name="content">
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