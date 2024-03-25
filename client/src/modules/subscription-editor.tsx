import { Await, useLoaderData } from "react-router-dom";
import { Suspense } from "react";
import { Field, FieldArray, FieldProps, Formik } from "formik";
import Loader from "../components/loader";
import PageHeader from "../components/page-heading";
import ExpansionPanel from "../components/expansion-panel";
import { Input, TextArea } from "../components/input";
import MultiExpansionField from "../components/multi-expansion-field";
import type { Embed, EmbedField, Subscription } from "../types/types";


function generateNewEmbed(): Embed {
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

function generateNewField(): EmbedField {
    return {
        id: Math.random(),
        name: "",
        value: "",
        inline: false
    }
}

export default function SubscriptionEditor() {
    const data = useLoaderData() as { subscriptions: Promise<Subscription> };

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
                        <div className="flex-1 flex flex-col md:flex-row pt-5 md:max-h-[70vh] text-white">
                            {/* subscription editor and subscription embed, split screen */}
                            <div className="basis-1/2 flex-1 overflow-y-auto px-5 scrollbar-w-1 scrollbar-thumb-rounded-full scrollbar-track-rounded-full scrollbar scrollbar-thumb-gray-600 scrollbar-track-gray-300">
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
                                        <form onSubmit={handleSubmit}>
                                            <div className="space-y-4">
                                                <div>
                                                    <Field name="url" >
                                                        {(props: FieldProps) => (
                                                            <Input {...props} label="URL" placeholder={getPlaceholderByPath(location.pathname)!} />
                                                        )}
                                                    </Field>
                                                </div>
                                                <div>
                                                    <Field name="webhookUrl">
                                                        {(props: FieldProps) => (
                                                            <Input {...props} label="Webhook URL" placeholder="https://discord.com/api/webhooks/..." />
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
                                            <FieldArray
                                                name="embeds">
                                                {(helpers) => (
                                                    <MultiExpansionField helpers={helpers} max={10} label="Embed" length={values.embeds.length} generate={generateNewEmbed}>
                                                        {({ max, label, movePanelDown, movePanelUp, addPanel, removePanel }) => (
                                                            <div>
                                                                {!values.embeds.length && <button className="py-2 px-4 bg-blue-500 text-white rounded-md mt-2" onClick={(ev) => addPanel(ev)}>Add {label}</button>}
                                                                {values.embeds?.map((embed, embedIndex) => (
                                                                    <ExpansionPanel key={embed.id} max={max} label={label} length={values.embeds.length} index={embedIndex} movePanelUp={movePanelUp} movePanelDown={movePanelDown} addPanel={addPanel} removePanel={removePanel}>
                                                                        <div className="p-5 mt-2 w-full space-y-3">
                                                                            <div className="space-y-1">
                                                                                <div>
                                                                                    <ExpansionPanel label="Author">
                                                                                        <div className="p-2">
                                                                                            <Field name={`embeds.${embedIndex}.author`}>
                                                                                                {(props: FieldProps) =>
                                                                                                    <Input {...props} label="Author" placeholder="" limit={256} />
                                                                                                }
                                                                                            </Field>
                                                                                            <Field name={`embeds.${embedIndex}.authorUrl`}>
                                                                                                {(props: FieldProps) =>
                                                                                                    <Input {...props} label="Author URL" placeholder="" />
                                                                                                }
                                                                                            </Field>
                                                                                            <Field name={`embeds.${embedIndex}.authorIcon`}>
                                                                                                {(props: FieldProps) =>
                                                                                                    <Input {...props} label="Author Icon" placeholder="" />
                                                                                                }
                                                                                            </Field>
                                                                                        </div>
                                                                                    </ExpansionPanel>
                                                                                    <ExpansionPanel label="Body">
                                                                                        <div className="p-2">
                                                                                            <Field name={`embeds.${embedIndex}.title`}>
                                                                                                {(props: FieldProps) =>
                                                                                                    <Input {...props} label="Title" placeholder="" limit={256} />
                                                                                                }
                                                                                            </Field>
                                                                                            <Field name={`embeds.${embedIndex}.titleUrl`}>
                                                                                                {(props: FieldProps) =>
                                                                                                    <Input {...props} label="Title URL" placeholder="" />
                                                                                                }
                                                                                            </Field>
                                                                                            <Field name={`embeds.${embedIndex}.description`}>
                                                                                                {(props: FieldProps) =>
                                                                                                    <TextArea {...props} label="Description" placeholder="" limit={4096} />
                                                                                                }
                                                                                            </Field>
                                                                                            <Field name={`embeds.${embedIndex}.color`}>
                                                                                                {(props: FieldProps) =>
                                                                                                    // TODO: color wheel
                                                                                                    <Input {...props} label="Color" placeholder="" />
                                                                                                }
                                                                                            </Field>
                                                                                        </div>
                                                                                    </ExpansionPanel>
                                                                                    <ExpansionPanel label="Fields">
                                                                                        <FieldArray name={`embeds.${embedIndex}.fields`}>
                                                                                            {(helpers) => (
                                                                                                <MultiExpansionField helpers={helpers} max={25} label="Field" length={embed.fields.length} generate={generateNewField}>
                                                                                                    {({ max, label, movePanelDown, movePanelUp, addPanel, removePanel }) => (
                                                                                                        <div>
                                                                                                            {!embed.fields.length && <button className="py-2 px-4 bg-blue-500 text-white rounded-md mt-2" onClick={(ev) => addPanel(ev)}>Add {label}</button>}
                                                                                                            {embed.fields?.map((field, fieldIndex) => (
                                                                                                                <div key={field.id} className="px-2 mb-2">
                                                                                                                    <ExpansionPanel max={max} label={label} length={embed.fields.length} index={fieldIndex} movePanelUp={movePanelUp} movePanelDown={movePanelDown} addPanel={addPanel} removePanel={removePanel}>
                                                                                                                        <div className="p-5 mt-2 w-full space-y-3">
                                                                                                                            <div className="space-y-1">
                                                                                                                                <div className="p-2">
                                                                                                                                    <Field name={`embeds.${embedIndex}.fields.${fieldIndex}.name`}>
                                                                                                                                        {(props: FieldProps) =>
                                                                                                                                            <Input {...props} label="Name" placeholder="" limit={256} />
                                                                                                                                        }
                                                                                                                                    </Field>
                                                                                                                                    <Field name={`embeds.${embedIndex}.fields.${fieldIndex}.value`}>
                                                                                                                                        {(props: FieldProps) =>
                                                                                                                                            <Input {...props} label="Value" placeholder="" limit={1024} />
                                                                                                                                        }
                                                                                                                                    </Field>
                                                                                                                                    <Field name={`embeds.${embedIndex}.fields.${fieldIndex}.inline`}>
                                                                                                                                        {(props: FieldProps) =>
                                                                                                                                            // TODO: checkbox
                                                                                                                                            <Input {...props} label="Inline" placeholder="" />
                                                                                                                                        }
                                                                                                                                    </Field>
                                                                                                                                </div>
                                                                                                                            </div>
                                                                                                                        </div>
                                                                                                                    </ExpansionPanel>
                                                                                                                </div>
                                                                                                            ))}
                                                                                                        </div>
                                                                                                    )}
                                                                                                </MultiExpansionField>
                                                                                            )}
                                                                                        </FieldArray>
                                                                                    </ExpansionPanel>
                                                                                    <ExpansionPanel label="Images">
                                                                                        <div className="p-2">
                                                                                            <Field name={`embeds.${embedIndex}.image`}>
                                                                                                {(props: FieldProps) =>
                                                                                                    <Input {...props} label="Image" placeholder="" />
                                                                                                }
                                                                                            </Field>
                                                                                            <Field name={`embeds.${embedIndex}.thumbnail`}>
                                                                                                {(props: FieldProps) =>
                                                                                                    <Input {...props} label="Thumbnail" placeholder="" />
                                                                                                }
                                                                                            </Field>
                                                                                        </div>
                                                                                    </ExpansionPanel>
                                                                                    <ExpansionPanel label="Footer">
                                                                                        <div className="p-2">
                                                                                            <Field name={`embeds.${embedIndex}.footer`}>
                                                                                                {(props: FieldProps) =>
                                                                                                    <Input {...props} label="Footer" placeholder="" limit={2048} />
                                                                                                }
                                                                                            </Field>
                                                                                            <Field name={`embeds.${embedIndex}.footerIcon`}>
                                                                                                {(props: FieldProps) =>
                                                                                                    <Input {...props} label="Footer Icon" placeholder="" />
                                                                                                }
                                                                                            </Field>
                                                                                            <Field name={`embeds.${embedIndex}.addTimestamp`}>
                                                                                                {(props: FieldProps) =>
                                                                                                    // TODO: checkbox
                                                                                                    <Input {...props} label="Add Timestamp" placeholder="" />
                                                                                                }
                                                                                            </Field>
                                                                                        </div>
                                                                                    </ExpansionPanel>
                                                                                </div>
                                                                            </div>
                                                                        </div>
                                                                    </ExpansionPanel>
                                                                ))}
                                                            </div>
                                                        )}
                                                    </MultiExpansionField>
                                                )}
                                            </FieldArray>
                                            <button className="py-2 px-4 rounded-sm bg-green-300 text-purple-800 font-semibold" type="submit" disabled={isSubmitting}>
                                                Save
                                            </button>
                                        </form>
                                    )}
                                </Formik>
                            </div>
                            <div className="basis-1/2 flex-1 bg-[#424549] overflow-y-auto">
                                {/* Content for the second half (embed preview)*/}
                            </div>
                        </div>
                    </div>
                )}
            </Await>
        </Suspense>
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