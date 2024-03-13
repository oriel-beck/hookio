import { ReactElement } from "react";

interface Props {
    title: string;
    subtitle?: string;
    icon?: ReactElement;
}

export default function PageHeader({ title, subtitle, icon }: Props) {
    return (
        <div className="w-full p-5 text-white border-b-white" style={{ borderWidth: '0 0 0.5px 0' }}>
            <div className="flex">
                {icon}
                <h2>{title}</h2>
            </div>
            {subtitle && <h3 className="text-gray-400 pt-2">{subtitle}</h3>}
        </div>
    )
}