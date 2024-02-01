import { useEffect, useState } from "react";
import { useParams } from "react-router-dom"
import Announcement from "../components/announcement";
import { Announcement as AnnouncementType } from "../types/types";

export default function Dashboard() {
    const { serverId } = useParams();
    const [announcements, setAnnouncements] = useState<AnnouncementType[]>([]);

    useEffect(() => {
        async function getAnnouncements() {
            const req = await fetch(`/api/guilds/subscriptions/${serverId}`);
            const json = await req.json();
            setAnnouncements(json);
        }
        getAnnouncements()
    }, [serverId]);

    return (
        <div className="flex flex-col m-5 mt-10 bg-zinc-700 pb-5">
            <div className="w-full p-5 bg-zinc-800 text-white">
                <div className="flex">
                    <svg className="w-6 h-6 mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 6.75h12M8.25 12h12m-12 5.25h12M3.75 6.75h.007v.008H3.75V6.75Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0ZM3.75 12h.007v.008H3.75V12Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm-.375 5.25h.007v.008H3.75v-.008Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Z" />
                    </svg>
                    <h2>Subscriptions</h2>
                </div>
                <h3 className="text-gray-400 pt-2">Edit a subscription or create a new one</h3>
            </div>
            <div className="flex-wrap flex-grow flex pt-5 px-5">
                {announcements.map((announcement) => <Announcement key={announcement.id} announcement={announcement} />)}
            </div>
            <div className="flex justify-end">
                <button className="bg-green-500 hover:bg-green-600 text-white font-bold p-2 rounded mx-4">Add Subscription</button>
            </div>
        </div>
    )
}