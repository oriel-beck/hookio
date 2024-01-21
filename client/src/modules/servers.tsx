import { redirect, useLoaderData } from "react-router-dom";

export default function Servers() {
    const servers = useLoaderData();
    console.log(servers);
    if (!servers) redirect("/login")
    return (
        <>
            This is the servers component
        </>
    )
}