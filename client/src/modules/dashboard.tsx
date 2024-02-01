import { useEffect, useState } from "react";
import { useParams } from "react-router-dom"

export default function Dashboard() {
    const { serverId } = useParams();
    const [announcements, setAnnouncements] = useState<unknown[]>([]);

    useEffect(() => {
        async function getAnnouncements() {
            const req = await fetch(`/api/guilds/subscriptions/${serverId}`);
            const json = await req.json();
            setAnnouncements(json);
        }
        getAnnouncements()
    }, [serverId]);

    return (
        <>
            {JSON.stringify(announcements)}
            This is the dashboard components
        </>
    )
}