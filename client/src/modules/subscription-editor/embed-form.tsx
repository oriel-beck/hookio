import { Field, FieldArrayRenderProps, FieldProps, FormikErrors, useFormikContext } from "formik";
import EmbedFieldsBuilder from "./embed-field-form";
import MultiExpansionField from "../../components/multi-expansion-field";
import ExpansionPanel from "../../components/expansion-panel";
import { CheckBox, Input, TextArea } from "../../components/input";
import type { Embed, EventFormikInitialValue, FormikInitialValue, MessageFormikInitialValue } from "../../types/types";
import { EventType } from "../../util/enums";
import { generateNewEmbed } from "../../util/util";

interface Props {
    helpers: FieldArrayRenderProps;
    values: MessageFormikInitialValue;
    eventType: EventType;
}

export default function EmbedForm({ helpers, values, eventType }: Props) {
    const formik = useFormikContext<FormikInitialValue>();
    const errors = (formik.errors.events?.[eventType.toString()] as FormikErrors<EventFormikInitialValue>)?.message?.embeds as FormikErrors<Embed & { invalid: boolean }>[];
    return (
        <MultiExpansionField helpers={helpers} max={10} label="Embed" length={values.embeds.length} generate={generateNewEmbed}>
            {({ max, label, ...props }) => (
                <div>
                    {!values.embeds.length && <button className="py-2 px-4 bg-blue-500 text-white rounded-md mt-2" onClick={(ev) => props.addPanel(ev)}>Add {label}</button>}
                    {values.embeds?.map((embed, embedIndex) => (
                        <ExpansionPanel
                            invalid={!!errors?.at(embedIndex)?.invalid}
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
                                        <ExpansionPanel label="Author">
                                            <div className="p-2 space-y-2">
                                                <Field name={`events.${eventType}.message.embeds.${embedIndex}.author`}>
                                                    {(props: FieldProps) =>
                                                        <Input {...props} label="Author" limit={256} />
                                                    }
                                                </Field>
                                                <Field name={`events.${eventType}.message.embeds.${embedIndex}.authorUrl`}>
                                                    {(props: FieldProps) =>
                                                        <Input error={errors?.at(embedIndex)?.authorUrl} {...props} label="Author URL" />
                                                    }
                                                </Field>
                                                <Field name={`events.${eventType}.message.embeds.${embedIndex}.authorIcon`}>
                                                    {(props: FieldProps) =>
                                                        <Input error={errors?.at(embedIndex)?.authorIcon} {...props} label="Author Icon" />
                                                    }
                                                </Field>
                                            </div>
                                        </ExpansionPanel>
                                        <ExpansionPanel label="Body">
                                            <div className="p-2 space-y-2">
                                                <Field name={`events.${eventType}.message.embeds.${embedIndex}.title`}>
                                                    {(props: FieldProps) =>
                                                        <Input {...props} label="Title" limit={256} />
                                                    }
                                                </Field>
                                                <Field name={`events.${eventType}.message.embeds.${embedIndex}.titleUrl`}>
                                                    {(props: FieldProps) =>
                                                        <Input error={errors?.at(embedIndex)?.titleUrl} {...props} label="Title URL" />
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
                                        <ExpansionPanel label="Images">
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
                                        <ExpansionPanel label="Footer">
                                            <div className="p-2 space-y-2">
                                                <div className="flex space-x-2">
                                                    <Field name={`events.${eventType}.message.embeds.${embedIndex}.footer`}>
                                                        {(props: FieldProps) =>
                                                            <Input {...props} label="Footer" limit={2048} />
                                                        }
                                                    </Field>
                                                    <Field name={`events.${eventType}.message.embeds.${embedIndex}.addTimestamp`}>
                                                        {(props: FieldProps) =>
                                                            <CheckBox {...props} label="Add Timestamp" />
                                                        }
                                                    </Field>
                                                </div>
                                                <Field name={`events.${eventType}.message.embeds.${embedIndex}.footerIcon`}>
                                                    {(props: FieldProps) =>
                                                        <Input error={errors?.at(embedIndex)?.footerIcon} {...props} label="Footer Icon" />
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

