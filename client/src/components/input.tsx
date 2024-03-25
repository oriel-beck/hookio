import { FieldProps } from "formik";

export interface Props extends FieldProps {
    label: string;
    placeholder: string;
    limit?: number;
}

export function Input({
    field, // { name, value, onChange, onBlur }
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    form, // also values, setXXXX, handleXXXX, dirty, isValid, status, etc.
    label,
    limit,
    ...props
}: Props) {
    return (
        <div className="mb-2">
            <div className="flex space-x-2 items-center mb-1">
                <label className="block text-white text-sm font-bold" id={field.name}>{label}</label>
                {limit && <p>{field.value.length}/{limit}</p>}
            </div>
            <input
                className="shadow appearance-none border rounded w-full py-2 px-3 text-white leading-tight focus:outline-none focus:shadow-outline bg-gray-600"
                {...field}
                {...props}
                {...(limit && { maxLength: limit })}
            />
        </div>
    )
}

export function TextArea({
    field, // { name, value, onChange, onBlur }
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    form, // also values, setXXXX, handleXXXX, dirty, isValid, status, etc.
    label,
    limit,
    ...props
}: Props) {
    return (
        <div className="mb-2">
            <div className="flex space-x-2 items-center mb-1">
                <label className="block text-white text-sm font-bold" htmlFor={field.name}>{label}</label>
                {limit && <p>{field.value.length}/{limit}</p>}
            </div>
            <textarea
                className="shadow appearance-none border rounded w-full py-2 px-3 text-white leading-tight focus:outline-none focus:shadow-outline bg-gray-600"
                {...field}
                {...props}
                {...(limit && { maxLength: limit })}
            />
        </div>
    )
}