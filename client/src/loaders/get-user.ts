import { redirect } from "react-router-dom";

export default async function getUser({ request }: { request: Request }) {
    const currentUser = await fetch("/api/users/current");
    console.log(currentUser.ok)
    if (!request.url.endsWith("/login") && !currentUser.ok) return redirect("/login");
    if (request.url.endsWith("/login") && currentUser.ok) return redirect("/servers")
    return currentUser;
}