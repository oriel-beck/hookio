import { defer } from "react-router-dom";

export default async function getUser({ request }: { request: Request }) {
    const url = new URL(request.url);
    const code = url.searchParams.get("code");
    if (code) {
        return defer({ user: fetch(`/api/users/authenticate/${code}`, { method: "POST" }).catch(() => fetch("/api/users/current").catch(() => null)).then((r) => r?.json()) })
    }
    return defer({ user: fetch("/api/users/current").catch(() => null).then((r => r?.json())) });
}