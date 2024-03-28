import React, { SyntheticEvent, useState } from "react";
import { HiOutlineArrowCircleDown, HiOutlineArrowCircleUp, HiOutlineChevronDown, HiOutlineMinusCircle, HiOutlinePlusCircle } from "react-icons/hi";
import { IoWarningOutline } from "react-icons/io5";

interface Props {
    invalid?: boolean;
    label: string;
    index?: number;
    max?: number;
    length?: number;
    children: React.ReactElement;
    movePanelUp?: (index: number, ev: SyntheticEvent<HTMLButtonElement, MouseEvent>) => unknown;
    movePanelDown?: (index: number, ev: SyntheticEvent<HTMLButtonElement, MouseEvent>) => unknown;
    removePanel?: (id: number, ev: SyntheticEvent<HTMLButtonElement, MouseEvent>) => unknown;
    addPanel?: (ev: SyntheticEvent<HTMLButtonElement, MouseEvent>) => unknown;
}


export default function ExpansionPanel({ invalid, label, max, length, index, children, movePanelUp, movePanelDown, removePanel, addPanel }: Props) {
    const [isOpen, open] = useState(false);

    const togglePanel = (ev: SyntheticEvent<HTMLButtonElement | HTMLDivElement, MouseEvent>) => {
        ev.preventDefault();
        open(!isOpen);
    }

    return (
        <div className="flex flex-col mt-2 border-gray-200">
            <div className="border flex flex-col items-center">
                <div className={'flex w-full p-4' + (isOpen ? ' border-b border-gray-200' : '')}>
                    <button onClick={(ev) => togglePanel(ev)} className={`transition-transform duration-300 transform ${isOpen ? 'rotate-180' : ''}`}>
                        <HiOutlineChevronDown className="w-6 h-6 text-white" />
                    </button>
                    <div className="flex-grow flex space-x-2" onClick={(ev) => togglePanel(ev)}>
                        <span>{label} {index != undefined ? index + 1 : ""}</span>
                        {invalid && <IoWarningOutline className="w-6 h-6 text-red-400" />}
                    </div>
                    {index !== undefined &&
                        <div className="flex items-center space-x-2">
                            {index !== 0 && (
                                <button onClick={(ev) => movePanelUp!(index!, ev)}>
                                    <HiOutlineArrowCircleUp className="w-6 h-6 text-white" />
                                </button>
                            )}
                            {index !== length! - 1 && (
                                <button onClick={(ev) => movePanelDown!(index!, ev)}>
                                    <HiOutlineArrowCircleDown className="w-6 h-6 text-white" />
                                </button>
                            )}
                            <button onClick={(ev) => removePanel!(index!, ev)}>
                                <HiOutlineMinusCircle className="w-6 h-6 text-white" />
                            </button>
                            {length! < max! && index === length! - 1 && (
                                <button onClick={addPanel}>
                                    <HiOutlinePlusCircle className="w-6 h-6 text-white" />
                                </button>
                            )}
                        </div>
                    }
                </div>
                <div className="w-full">
                    {isOpen && children}
                </div>
            </div>
        </div>
    )
}