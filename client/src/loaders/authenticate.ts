import { redirect } from "react-router-dom";
// import getUser from "./get-user";

export default async function authenticate({ request }: { request: Request }) {
    const url = new URL(request.url);
    const code = url.searchParams.get("code");
    if (code) {
        const validated = await fetch(`/api/users/authenticate/${code}`, { method: "POST" }).then((r) => r.json()).catch(() => null);
        if (validated) {
            if (url.searchParams.get("from")) return redirect(url.searchParams.get("from")!)
            return null //getUser({ request });
        }
    }
    return null;
}