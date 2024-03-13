import { useOutletContext } from "react-router-dom";
import Guild from "../components/guild";
import type { User } from "../types/types";
import PageHeader from "../components/page-heading";

export default function Guilds() {
    const user = useOutletContext() as User;
    return (
        <div className="flex flex-col m-5">
            <PageHeader
                title="Server List"
                subtitle="Choose a server from the list to continue"
                icon={
                    <svg className="w-6 h-6 mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 6.75h12M8.25 12h12m-12 5.25h12M3.75 6.75h.007v.008H3.75V6.75Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0ZM3.75 12h.007v.008H3.75V12Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm-.375 5.25h.007v.008H3.75v-.008Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Z" />
                    </svg>
                }
            />
            <ul className="flex-wrap justify-center flex-grow flex pt-5">
                {user?.guilds?.map((guild) => <Guild key={guild.name} guild={guild} />)}
            </ul>
        </div>
    )
}