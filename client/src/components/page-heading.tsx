import { ReactElement } from "react";

interface Props {
    title: string;
    subtitle?: string;
    icon?: ReactElement;
    children?: ReactElement;
}

export default function PageHeader({ children, title, subtitle, icon }: Props) {
    return (
        <div className="flex flex-col md:flex-row space-y-2 md:space-y-0 w-full p-5 text-white border-b-white" style={{ borderWidth: '0 0 0.5px 0' }}>
            <div className="flex flex-col">
                <div className="flex">
                    {icon}
                    <h2>{title}</h2>
                </div>
                {subtitle && <h3 className="text-gray-400 pt-2">{subtitle}</h3>}
            </div>
            {children}
        </div>
    )
}