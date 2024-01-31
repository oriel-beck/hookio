export default async function getUser({ request }: { request: Request }) {
    const url = new URL(request.url);
    const code = url.searchParams.get("code");
    if (code) {
        const validated = await fetch(`/api/users/authenticate/${code}`, { method: "POST" }).then((r) => r.json()).catch(() => null);
        console.log(validated)
        if (validated) {
            return validated;
        }
    }
    return await fetch("/api/users/current");
}