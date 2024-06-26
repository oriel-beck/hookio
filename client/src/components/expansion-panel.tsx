import React, { SyntheticEvent, useState } from "react";
import { HiOutlineArrowCircleDown, HiOutlineArrowCircleUp, HiOutlineChevronDown, HiOutlineMinusCircle, HiOutlinePlusCircle } from "react-icons/hi";
import { IoWarningOutline } from "react-icons/io5";
import { MdOutlineContentCopy } from "react-icons/md";
import { makeid } from "../util/util";
import { AnimatePresence, motion } from "framer-motion";
import { fadeVariants } from "../animation/fade-variants";

interface Props {
    invalid?: boolean;
    label: string;
    index?: number;
    max?: number;
    length?: number;
    children: React.ReactElement;
    value?: unknown;
    movePanelUp?: (index: number, ev: SyntheticEvent<HTMLButtonElement, MouseEvent>) => unknown;
    movePanelDown?: (index: number, ev: SyntheticEvent<HTMLButtonElement, MouseEvent>) => unknown;
    removePanel?: (id: number, ev: SyntheticEvent<HTMLButtonElement, MouseEvent>) => unknown;
    addPanel?: (ev: SyntheticEvent<HTMLButtonElement, MouseEvent>) => unknown;
    duplicatePanel?: (value: unknown, ev: SyntheticEvent<HTMLButtonElement, MouseEvent>) => unknown;
}


export default function ExpansionPanel({ invalid, label, max, length, index, children, value, movePanelUp, movePanelDown, removePanel, addPanel, duplicatePanel }: Props) {
    const [isOpen, open] = useState(false);
    const id = makeid(10)

    const togglePanel = (ev: SyntheticEvent<HTMLButtonElement | HTMLDivElement, MouseEvent>) => {
        ev.preventDefault();
        open(!isOpen);
    }

    return (
        <div className="flex border flex-col mt-2 border-gray-200">
            <div className="flex flex-col items-center">
                <div className={`flex w-full p-4 ${isOpen ? ' border-b border-gray-200' : ''}`}>
                    <button aria-labelledby={id} onClick={(ev) => togglePanel(ev)} className={`transition-transform duration-300 transform ${isOpen ? 'rotate-180' : ''}`}>
                        <HiOutlineChevronDown className="w-6 h-6 text-white" />
                    </button>
                    <div className="flex-grow flex space-x-2" onClick={(ev) => togglePanel(ev)}>
                        <span id={id}>{label} {index != undefined ? index + 1 : ""}</span>
                        <AnimatePresence>
                            {invalid &&
                                <motion.span variants={fadeVariants} animate="show" initial="hide" exit="hide">
                                    <IoWarningOutline className="w-6 h-6 text-red-400" />
                                </motion.span>
                            }
                        </AnimatePresence>
                    </div>
                    {index !== undefined &&
                        <div className="flex items-center space-x-2">
                            {length! < max! && (
                                <button aria-label="Duplicate" onClick={(ev) => duplicatePanel!(value, ev)}>
                                    <MdOutlineContentCopy className="h-6 w-6 text-white" />
                                </button>
                            )}
                            {index !== 0 && (
                                <button aria-label="Move up" onClick={(ev) => movePanelUp!(index!, ev)}>
                                    <HiOutlineArrowCircleUp className="w-6 h-6 text-white" />
                                </button>
                            )}
                            {index !== length! - 1 && (
                                <button aria-label="Move down" onClick={(ev) => movePanelDown!(index!, ev)}>
                                    <HiOutlineArrowCircleDown className="w-6 h-6 text-white" />
                                </button>
                            )}
                            <button aria-label="Remove" onClick={(ev) => removePanel!(index!, ev)}>
                                <HiOutlineMinusCircle className="w-6 h-6 text-white" />
                            </button>
                            {length! < max! && index === length! - 1 && (
                                <button aria-label="Add new" onClick={addPanel}>
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