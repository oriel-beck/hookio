export default async function getUser({ request }: { request: Request }) {
    const url = new URL(request.url);
    const code = url.searchParams.get("code");
    return { user: authenticateAndFallback(code) };
}

function authenticate(code: string) {
    return fetch(`/api/users/authenticate?code=${encodeURIComponent(code)}`, { method: "POST" });
}

function getCurrentUser() {
    return fetch("/api/users/current");
}

function readUser(response: Response) {
    if (!response.ok) return null;
    return response.json().catch(() => null);
}

function authenticateAndFallback(code: string | null) {
    return code
        ? authenticate(code)
            .then(readUser)
            .then((json) => json ?? getCurrentUser().then(readUser).catch(() => null))
        : getCurrentUser().then(readUser).catch(() => null);
}
