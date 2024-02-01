import { useNavigate } from "react-router-dom";
import type { Guild as GuildType } from "../types/types";

interface Props {
    guild: GuildType
}

export default function Guild({ guild }: Props) {
    const navigte = useNavigate();
    function onClick() {
        navigte(`/servers/${guild.id}`);
    }

    const fallback = "data:image/png;base64,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"
    return (
        <div>
            <div className="flex flex-col space-y-2 px-3 pb-2 max-w-28 md:max-w-40 justify-center items-center rounded-md">
                <img
                    typeof="button"
                    aria-labelledby={`label-for-${guild.id}`}
                    onClick={() => onClick()}
                    className="cursor-pointer rounded-full border-gray-500 border-2 hover:border-green-300"
                    src={guild.icon === "fallback" ? fallback : guild.icon}
                    width={125}
                    height={125}
                />
                <h2 id={`label-for-${guild.id}`} className="text-white flex text-center">{guild.name}</h2>
            </div>
        </div>
    )
}