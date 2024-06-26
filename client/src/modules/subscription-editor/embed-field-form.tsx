import { Field, FieldArray, FieldProps, FormikErrors, useFormikContext } from "formik";
import ExpansionPanel from "../../components/expansion-panel";
import MultiExpansionField from "../../components/multi-expansion-field";
import { CheckBox, Input, TextArea } from "../../components/input";
import { EmbedField, EmbedFormikInitialValue, EventFormikInitialValue, FormikInitialValue } from "../../types/types";
import { EventType } from "../../util/enums";
import { generateNewField } from "../../util/util";

interface Props {
    embed: EmbedFormikInitialValue;
    embedIndex: number;
    eventType: EventType;
}

export default function EmbedFieldsBuilder({ embed, embedIndex, eventType }: Props) {
    const formik = useFormikContext<FormikInitialValue>();
    const errors = ((formik.errors.events?.[eventType.toString()] as FormikErrors<EventFormikInitialValue>)?.message?.embeds?.at(embedIndex) as FormikErrors<EmbedFormikInitialValue & { invalid: boolean }>)?.fields as FormikErrors<EmbedField>[];

    return (
        <ExpansionPanel label="Fields">
            <FieldArray name={`events.${eventType}.message.embeds.${embedIndex}.fields`}>
                {(helpers) => (
                    <MultiExpansionField helpers={helpers} max={25} label="Field" length={embed.fields.length} generate={generateNewField}>
                        {({ max, label, ...props }) => (
                            <div>
                                {!embed.fields.length && <button className="py-2 px-4 bg-blue-500 text-white rounded-md mt-2" onClick={(ev) => props.addPanel(ev)}>Add {label}</button>}
                                {embed.fields?.map((field, fieldIndex) => (
                                    <div key={field.id as string} className="px-2 mb-2">
                                        <ExpansionPanel
                                            invalid={!!errors?.at(fieldIndex)?.name || !!errors?.at(fieldIndex)?.value}
                                            max={max}
                                            label={label}
                                            length={embed.fields.length}
                                            index={fieldIndex}
                                            value={field}
                                            {...props}>
                                            <div className="p-5 mt-2 w-full space-y-3">
                                                <div className="space-y-1">
                                                    <div className="p-2 space-y-2">
                                                        <div className="flex space-x-2 items-center">
                                                            <Field name={`events.${eventType}.message.embeds.${embedIndex}.fields.${fieldIndex}.name`}>
                                                                {(props: FieldProps) =>
                                                                    <Input {...props} label="Name" error={errors?.at(fieldIndex)?.name} limit={256} />
                                                                }
                                                            </Field>
                                                            <span className="flex-1"></span>
                                                            <Field name={`events.${eventType}.message.embeds.${embedIndex}.fields.${fieldIndex}.inline`}>
                                                                {(props: FieldProps) =>
                                                                    <CheckBox {...props} label="Inline" />
                                                                }
                                                            </Field>
                                                        </div>
                                                        <Field name={`events.${eventType}.message.embeds.${embedIndex}.fields.${fieldIndex}.value`}>
                                                            {(props: FieldProps) =>
                                                                <TextArea {...props} label="Value" error={errors?.at(fieldIndex)?.value} limit={1024} />
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
    )
}

