import { useRouteLoaderData } from "react-router-dom";
import Guild from "../components/server";
import type { User } from "../types/types";

export default function Guilds() {
    const user = useRouteLoaderData("root") as User;
    return (
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 grid-rows-3 md:grid-rows-2 gap-4 h-5/6 w-4/5 mx-auto rounded-md">
            {user.guilds?.map((server) => <Guild key={server.name} icon={server.icon} name={server.name} id={server.id} />)}
        </div>
    )
}