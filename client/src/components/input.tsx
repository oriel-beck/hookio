import { FieldProps } from "formik";
import { AnimatePresence, motion } from "framer-motion";
import { HiOutlineXMark } from "react-icons/hi2";
import { IoWarningOutline } from "react-icons/io5";
import { fadeVariants } from "../animation/fade-variants";

export interface Props extends FieldProps {
    label: string;
    placeholder?: string;
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
        <div className="w-full">
            <div className="flex space-x-2 items-center mb-1">
                <label className="block text-white text-sm font-bold" htmlFor={field.name}>{label}</label>
                <span className="flex-1"></span>
                <Limit limit={limit} length={field.value?.length} />
            </div>
            <input
                id={field.name}
                className={`shadow appearance-none border rounded w-full py-2 px-3 text-white leading-tight focus:outline-none focus:shadow-outline bg-zinc-700 focus:border-teal-400 ${error && 'border-red-600 focus:border-red-400'}`}
                {...field}
                {...props}
                {...(limit && { maxLength: limit })}
            />
            <AnimatePresence mode="wait">
                {error &&
                    <motion.p key={error} variants={fadeVariants} animate="show" initial="hide" exit="hide" className="text-sm text-red-500 transition-all duration-200">{error}</motion.p>
                }
            </AnimatePresence>
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
        <div className="w-full">
            <div className="flex space-x-2 items-center mb-1">
                <label className="block text-white text-sm font-bold" htmlFor={field.name}>{label}</label>
                {error && <IoWarningOutline className="w-5 h-5 text-red-400" />}
                <span className="flex-1"></span>
                <Limit limit={limit} length={field.value.length} />
            </div>
            <textarea
                id={field.name}
                className={`shadow h-40 appearance-none border rounded w-full py-2 px-3 text-white leading-tight focus:outline-none focus:shadow-outline bg-zinc-700 focus:border-teal-400 ${error && 'border-red-600 focus:border-red-400'}`}
                {...field}
                {...props}
                {...(limit && { maxLength: limit })}
            />
            <AnimatePresence mode="wait">
                {error &&
                    <motion.p key={error} variants={fadeVariants} animate="show" initial="hide" exit="hide" className="text-sm text-red-500 transition-all duration-200">{error}</motion.p>
                }
            </AnimatePresence>
        </div>
    )
}

export function CheckBox({
    field, // { name, value, onChange, onBlur }
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    form, // also values, setXXXX, handleXXXX, dirty, isValid, status, etc.
    label,
    ...props
}: Props) {
    return (
        <div className="relative w-fit">
            <div className="flex space-x-1 items-center mt-8">
                <input id={field.name} type="checkbox"{...field} {...props} className="peer relative appearance-none outline-none shrink-0 w-5 h-5 border border-neutral-900 rounded-sm bg-zinc-700 hover:bg-gray-400 focus:border-teal-400" />
                <label className="block text-white text-sm font-bold" htmlFor={field.name}>{label}</label>
                <HiOutlineXMark className="right-[2.7rem] stroke-[3] absolute w-4 h-4 hidden peer-checked:block pointer-events-none" />
            </div>
        </div>
    )
}

function Limit({ limit, length }: { limit?: number, length: number }) {
    return limit ? <p className="text-gray-300 text-sm">{length}/{limit}</p> : <></>
}