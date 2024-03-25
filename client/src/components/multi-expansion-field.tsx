import { FieldArrayRenderProps } from "formik";
import { SyntheticEvent } from "react";

interface Props {
    length: number;
    label: string;
    max: number;
    helpers: FieldArrayRenderProps;
    generate: () => unknown
    children: (props: ChildrenProps) => React.ReactElement;
}

interface ChildrenProps {
    max: number;
    label: string;
    addPanel: (ev: SyntheticEvent<HTMLButtonElement, MouseEvent>) => void;
    removePanel: (index: number, ev: SyntheticEvent<HTMLButtonElement, MouseEvent>) => void;
    movePanelUp: (index: number, ev: SyntheticEvent<HTMLButtonElement, MouseEvent>) => void;
    movePanelDown: (index: number, ev: SyntheticEvent<HTMLButtonElement, MouseEvent>) => void;
}

export default function MultiExpansionField({ children, max, label, length, helpers, generate }: Props) {
    const addPanel = (ev: SyntheticEvent<HTMLButtonElement, MouseEvent>) => {
        ev.preventDefault();
        if (length < max) {
            // unique id to force React to re-render when the order changes
            helpers.push(generate())
        }
    };

    const removePanel = (index: number, ev: SyntheticEvent<HTMLButtonElement, MouseEvent>) => {
        ev.preventDefault();
        helpers.remove(index);
    };

    const movePanelUp = (index: number, ev: SyntheticEvent<HTMLButtonElement, MouseEvent>) => {
        ev.preventDefault();
        if (index > 0) {
            helpers.swap(index, index - 1);
        }
    };

    const movePanelDown = (index: number, ev: SyntheticEvent<HTMLButtonElement, MouseEvent>) => {
        ev.preventDefault();
        if (index < length + 1) {
            helpers.swap(index, index + 1);
        }
    };

    return children({ max, label, addPanel, removePanel, movePanelDown, movePanelUp })
}