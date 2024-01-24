import { redirect } from "react-router-dom";
import { getAuth } from "../util/get-auth";

export default async function getUser({ request }: { request: Request }) {
    console.log(request)
    const currentUser = await fetch("/api/users/current", {
        headers: {
            "Authorization": `Bearer ${getAuth()}`
        }
    });
    if (request.url.endsWith("/login") && currentUser) return redirect("/servers");
    return currentUser;
}