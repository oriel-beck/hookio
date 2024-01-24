import { redirect } from "react-router-dom";

export default async function getUser({ request }: { request: Request }) {
    console.log(request)
    const currentUser = await fetch("/api/users/current");
    if (request.url.endsWith("/login") && currentUser) return redirect("/servers");
    return currentUser;
}