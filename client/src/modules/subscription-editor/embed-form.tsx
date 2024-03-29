import { Field, FieldArrayRenderProps, FieldProps, FormikErrors, useFormikContext } from "formik";
import EmbedFieldsBuilder from "./embed-field-form";
import MultiExpansionField from "../../components/multi-expansion-field";
import ExpansionPanel from "../../components/expansion-panel";
import { Input, TextArea } from "../../components/input";
import type { Embed, EmbedField, EmbedFormInitialValues } from "../../types/types";

interface Props {
    helpers: FieldArrayRenderProps;
    values: EmbedFormInitialValues;
}

export default function EmbedForm({ helpers, values }: Props) {
    const formik = useFormikContext<EmbedFormInitialValues>();
    const errors = formik.errors.embeds as FormikErrors<Embed & { invalid: boolean }>[];
    return (
        <MultiExpansionField helpers={helpers} max={10} label="Embed" length={values.embeds.length} generate={generateNewEmbed}>
            {({ max, label, movePanelDown, movePanelUp, addPanel, removePanel }) => (
                <div>
                    {!values.embeds.length && <button className="py-2 px-4 bg-blue-500 text-white rounded-md mt-2" onClick={(ev) => addPanel(ev)}>Add {label}</button>}
                    {values.embeds?.map((embed, embedIndex) => (
                        <ExpansionPanel invalid={!!errors?.at(embedIndex)?.invalid} key={embed.id} max={max} label={label} length={values.embeds.length} index={embedIndex} movePanelUp={movePanelUp} movePanelDown={movePanelDown} addPanel={addPanel} removePanel={removePanel}>
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
                                                        <Input error={errors?.at(embedIndex)?.authorUrl} {...props} label="Author URL" placeholder="" />
                                                    }
                                                </Field>
                                                <Field name={`embeds.${embedIndex}.authorIcon`}>
                                                    {(props: FieldProps) =>
                                                        <Input error={errors?.at(embedIndex)?.authorIcon} {...props} label="Author Icon" placeholder="" />
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
                                                        <Input error={errors?.at(embedIndex)?.titleUrl} {...props} label="Title URL" placeholder="" />
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
                                        <EmbedFieldsBuilder embed={embed} embedIndex={embedIndex} />
                                        <ExpansionPanel label="Images">
                                            <div className="p-2">
                                                <Field name={`embeds.${embedIndex}.image`}>
                                                    {(props: FieldProps) =>
                                                        <Input error={errors?.at(embedIndex)?.image} {...props} label="Image" placeholder="" />
                                                    }
                                                </Field>
                                                <Field name={`embeds.${embedIndex}.thumbnail`}>
                                                    {(props: FieldProps) =>
                                                        <Input error={errors?.at(embedIndex)?.thumbnail} {...props} label="Thumbnail" placeholder="" />
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
                                                        <Input error={errors?.at(embedIndex)?.footerIcon} {...props} label="Footer Icon" placeholder="" />
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
    )
}

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