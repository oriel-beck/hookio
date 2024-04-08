import { useFormikContext } from "formik";
import { EventFormikInitialValue, FormikInitialValue } from "../../types/types";
import { EventType } from "../../util/enums";
import { HiXMark } from "react-icons/hi2";

interface Props {
    origin: EventType;
    target: EventType;
    data: EventFormikInitialValue;
    closeModal: () => unknown;
}

export default function CopyModal({ origin, target, data, closeModal }: Props) {
    const formik = useFormikContext<FormikInitialValue>();
    const copyDataToCurrentEvent = () => {
        formik.setValues({
            ...formik.values,
            events: {
                ...formik.values.events,
                [target.toString()]: data
            },
        });
        closeModal();
    }

    return (
        <>
            {/* Backdrop */}
            <div className="h-full w-full absolute top-0 left-0 bg-gray-500 bg-opacity-50 z-40" onClick={() => closeModal()}></div>
            <div className=" absolute top-1/4 left-1/4 w-auto my-6 mx-auto max-w-3xl z-50 border border-white rounded text-white bg-gray-400 bg-opacity-90">
                {/*content*/}
                <div className="border-0 rounded-lg shadow-lg relative flex flex-col w-full bg-transparent outline-none focus:outline-none">
                    {/*header*/}
                    <div className="flex items-start justify-between p-5 rounded-t">
                        <h3 className="text-3xl font-semibold">
                            Copying data from {EventType[origin]}
                        </h3>
                        <button
                            className="p-1 ml-auto bg-transparent border-0 text-black float-right text-3xl leading-none font-semibold outline-none focus:outline-none"
                            onClick={() => closeModal()}
                        >
                            <HiXMark className="w-5 h-5" />
                        </button>
                    </div>
                    {/*body*/}
                    <div className="relative p-6 flex-auto">
                        <p className="my-4 text-blueGray-500 text-lg leading-relaxed">
                            Copying data from {EventType[origin]} will override all data in {EventType[target]}, are you sure?
                        </p>
                    </div>
                    {/*footer*/}
                    <div className="flex items-center justify-end p-6 rounded-b">
                        <button
                            className="text-red-500 font-bold uppercase px-6 py-2 text-md outline-none focus:outline-none mr-1 mb-1 ease-linear transition-all duration-150"
                            type="button"
                            onClick={() => closeModal()}
                        >
                            No
                        </button>
                        <button
                            className="bg-emerald-500 text-white active:bg-emerald-600 font-bold uppercase text-md px-6 py-3 rounded shadow hover:shadow-lg outline-none focus:outline-none mr-1 mb-1 ease-linear transition-all duration-150"
                            type="button"
                            onClick={() => copyDataToCurrentEvent()}
                        >
                            Yes
                        </button>
                    </div>
                </div>
            </div>
        </>
    )
}