import { defer } from "react-router-dom";

export default async function getUser({ request }: { request: Request }) {
    const url = new URL(request.url);
    const code = url.searchParams.get("code");
    if (code) {
        return defer({ user: authenticateAndFallback(code) })
    }
    return defer({ user: authenticateAndFallback(code) });
}

function authenticate(code: string) {
    return fetch(`/api/users/authenticate/${code}`, { method: "POST" })
}

function getCurrentUser() {
    return fetch("/api/users/current");
}

function authenticateAndFallback(code: string | null) {
    // if there is a code, try to authenticate, if not, get the current user
    return code ? authenticate(code)
        .then((response) => {
            return response.json().catch(() => null);
        })
        .then((json) => {
            // if auth returned false, try to get the currently authenticated user anyways
            return json ? json : getCurrentUser()
                .then((response) => {
                    return response.json().catch(() => null);
                })
                .catch(() => null)
        }) : getCurrentUser()
            .then((response) => {
                return response.json().catch(() => null);
            })
            .catch(() => null);
}