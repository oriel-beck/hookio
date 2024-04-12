import { Field, FieldArrayRenderProps, FieldProps, FormikErrors, useFormikContext } from "formik";
import EmbedFieldsBuilder from "./embed-field-form";
import MultiExpansionField from "../../components/multi-expansion-field";
import ExpansionPanel from "../../components/expansion-panel";
import { CheckBox, Input, TextArea } from "../../components/input";
import type { EmbedAuthorFormikInitialValue, EmbedFooterFormikInitialValue, EmbedFormikInitialValue, EmbedTitleFormikInitialValue, EventFormikInitialValue, FormikInitialValue, MessageFormikInitialValue } from "../../types/types";
import { EventType } from "../../util/enums";
import { generateNewEmbed } from "../../util/util";

interface Props {
    helpers: FieldArrayRenderProps;
    values: MessageFormikInitialValue;
    eventType: EventType;
}

export default function EmbedForm({ helpers, values, eventType }: Props) {
    const formik = useFormikContext<FormikInitialValue>();
    const errors = (formik.errors.events?.[eventType.toString()] as FormikErrors<EventFormikInitialValue>)?.message?.embeds as FormikErrors<EmbedFormikInitialValue>[];
    const titleErrors = (idx: number) => errors?.at(idx)?.title as FormikErrors<EmbedTitleFormikInitialValue>;
    const authorErrors = (idx: number) => errors?.at(idx)?.author as FormikErrors<EmbedAuthorFormikInitialValue>;
    const footerErrors = (idx: number) => errors?.at(idx)?.footer as FormikErrors<EmbedFooterFormikInitialValue>;

    return (
        <MultiExpansionField helpers={helpers} max={10} label="Embed" length={values.embeds.length} generate={generateNewEmbed}>
            {({ max, label, ...props }) => (
                <div>
                    {!values.embeds.length && <button className="py-2 px-4 bg-blue-500 text-white rounded-md mt-2" onClick={(ev) => props.addPanel(ev)}>Add {label}</button>}
                    {values.embeds?.map((embed, embedIndex) => (
                        <ExpansionPanel
                            invalid={!!errors?.at(embedIndex)}
                            key={embed.id as string}
                            max={max}
                            label={label}
                            length={values.embeds.length}
                            index={embedIndex}
                            {...props}
                            value={embed}
                        >
                            <div className="p-5 mt-2 w-full space-y-3">
                                <div className="space-y-1">
                                    <div>
                                        <ExpansionPanel invalid={!!authorErrors(embedIndex)} label="Author">
                                            <div className="p-2 space-y-2">
                                                <Field name={`events.${eventType}.message.embeds.${embedIndex}.author.text`}>
                                                    {(props: FieldProps) =>
                                                        <Input  error={authorErrors(embedIndex)?.text} {...props} label="Author" limit={256} />
                                                    }
                                                </Field>
                                                <Field name={`events.${eventType}.message.embeds.${embedIndex}.author.url`}>
                                                    {(props: FieldProps) =>
                                                        <Input error={authorErrors(embedIndex)?.url} {...props} label="Author URL" />
                                                    }
                                                </Field>
                                                <Field name={`events.${eventType}.message.embeds.${embedIndex}.author.icon`}>
                                                    {(props: FieldProps) =>
                                                        <Input error={authorErrors(embedIndex)?.icon} {...props} label="Author Icon" />
                                                    }
                                                </Field>
                                            </div>
                                        </ExpansionPanel>
                                        <ExpansionPanel invalid={!!titleErrors(embedIndex)} label="Body">
                                            <div className="p-2 space-y-2">
                                                <Field name={`events.${eventType}.message.embeds.${embedIndex}.title.text`}>
                                                    {(props: FieldProps) =>
                                                        <Input error={titleErrors(embedIndex)?.text} {...props} label="Title" limit={256} />
                                                    }
                                                </Field>
                                                <Field name={`events.${eventType}.message.embeds.${embedIndex}.title.url`}>
                                                    {(props: FieldProps) =>
                                                        // TODO: fix
                                                        <Input error={titleErrors(embedIndex)?.url} {...props} label="Title URL" />
                                                    }
                                                </Field>
                                                <Field name={`events.${eventType}.message.embeds.${embedIndex}.description`}>
                                                    {(props: FieldProps) =>
                                                        <TextArea {...props} label="Description" limit={4096} />
                                                    }
                                                </Field>
                                                <Field name={`events.${eventType}.message.embeds.${embedIndex}.color`}>
                                                    {(props: FieldProps) =>
                                                        // TODO: color wheel
                                                        <Input {...props} label="Color" />
                                                    }
                                                </Field>
                                            </div>
                                        </ExpansionPanel>
                                        <EmbedFieldsBuilder eventType={eventType} embed={embed} embedIndex={embedIndex} />
                                        <ExpansionPanel invalid={!!errors?.at(embedIndex)?.image || !!errors?.at(embedIndex)?.thumbnail} label="Images">
                                            <div className="p-2 space-y-2">
                                                <Field name={`events.${eventType}.message.embeds.${embedIndex}.image`}>
                                                    {(props: FieldProps) =>
                                                        <Input error={errors?.at(embedIndex)?.image} {...props} label="Image" />
                                                    }
                                                </Field>
                                                <Field name={`events.${eventType}.message.embeds.${embedIndex}.thumbnail`}>
                                                    {(props: FieldProps) =>
                                                        <Input error={errors?.at(embedIndex)?.thumbnail} {...props} label="Thumbnail" />
                                                    }
                                                </Field>
                                            </div>
                                        </ExpansionPanel>
                                        <ExpansionPanel invalid={!!footerErrors(embedIndex)} label="Footer">
                                            <div className="p-2 space-y-2">
                                                <div className="flex space-x-2">
                                                    <Field name={`events.${eventType}.message.embeds.${embedIndex}.footer.text`}>
                                                        {(props: FieldProps) =>
                                                            <Input error={footerErrors(embedIndex)?.text} {...props} label="Footer" limit={2048} />
                                                        }
                                                    </Field>
                                                    <Field name={`events.${eventType}.message.embeds.${embedIndex}.addTimestamp`}>
                                                        {(props: FieldProps) =>
                                                            <CheckBox {...props} label="Add Timestamp" />
                                                        }
                                                    </Field>
                                                </div>
                                                <Field name={`events.${eventType}.message.embeds.${embedIndex}.footer.icon`}>
                                                    {(props: FieldProps) =>
                                                        <Input error={footerErrors(embedIndex)?.icon} {...props} label="Footer Icon" />
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
    )
}

