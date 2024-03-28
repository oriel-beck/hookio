import { Field, FieldArray, FieldProps, FormikErrors, useFormikContext } from "formik";
import ExpansionPanel from "../../components/expansion-panel";
import MultiExpansionField from "../../components/multi-expansion-field";
import { Input } from "../../components/input";
import { Embed, EmbedField, EmbedFormInitialValues } from "../../types/types";

export default function EmbedFieldsBuilder({ embed, embedIndex }: { embed: Embed, embedIndex: number }) {
    const formik = useFormikContext<EmbedFormInitialValues>();
    const errors = (formik.errors.embeds?.at(embedIndex) as FormikErrors<Embed & { invalid: boolean }>)?.fields as FormikErrors<EmbedField>[];

    return (
        <ExpansionPanel label="Fields">
            <FieldArray name={`embeds.${embedIndex}.fields`}>
                {(helpers) => (
                    <MultiExpansionField helpers={helpers} max={25} label="Field" length={embed.fields.length} generate={generateNewField}>
                        {({ max, label, movePanelDown, movePanelUp, addPanel, removePanel }) => (
                            <div>
                                {!embed.fields.length && <button className="py-2 px-4 bg-blue-500 text-white rounded-md mt-2" onClick={(ev) => addPanel(ev)}>Add {label}</button>}
                                {embed.fields?.map((field, fieldIndex) => (
                                    <div key={field.id} className="px-2 mb-2">
                                        <ExpansionPanel invalid={!!errors?.at(fieldIndex)?.name || !!errors?.at(fieldIndex)?.value} max={max} label={label} length={embed.fields.length} index={fieldIndex} movePanelUp={movePanelUp} movePanelDown={movePanelDown} addPanel={addPanel} removePanel={removePanel}>
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
    )
}

function generateNewField(): EmbedField {
    return {
        id: Math.random(),
        name: "",
        value: "",
        inline: false
    }
}