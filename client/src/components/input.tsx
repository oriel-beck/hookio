import { FieldProps } from "formik";
import { IoWarningOutline } from "react-icons/io5";

export interface Props extends FieldProps {
    label: string;
    placeholder: string;
    error?: string;
    limit?: number;
}

export function Input({
    field, // { name, value, onChange, onBlur }
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    form, // also values, setXXXX, handleXXXX, dirty, isValid, status, etc.
    error,
    label,
    limit,
    ...props
}: Props) {
    return (
        <div className="mb-2">
            <div className="flex space-x-2 items-center mb-1">
                <label className="block text-white text-sm font-bold" id={field.name}>{label}</label>
                {error && <IoWarningOutline className="w-5 h-5 text-red-400" />}
                <span className="flex-1"></span>
                <Limit limit={limit} length={field.value.length} />
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
    error,
    label,
    limit,
    ...props
}: Props) {
    return (
        <div className="mb-2">
            <div className="flex space-x-2 items-center mb-1">
                <label className="block text-white text-sm font-bold" htmlFor={field.name}>{label}</label>
                {error && <IoWarningOutline className="w-5 h-5 text-red-400" />}
                <span className="flex-1"></span>
                <Limit limit={limit} length={field.value.length} />
            </div>
            <textarea
                className="shadow h-40 appearance-none border rounded w-full py-2 px-3 text-white leading-tight focus:outline-none focus:shadow-outline bg-gray-600"
                {...field}
                {...props}
                {...(limit && { maxLength: limit })}
            />
        </div>
    )
}

function Limit({ limit, length }: { limit?: number, length: number }) {
    return limit ? <p className="text-gray-300 text-sm">{length}/{limit}</p> : <></>
}