import { defer, redirect } from "react-router-dom";

export default function getAllSubscriptions({ params }: { params: Record<string, string | undefined> }) {
    const { serverId, provider } = params;
    if (!['youtube', 'twitch'].includes(provider!)) return redirect("/");
    return defer({ subscriptions: fetch(`/api/subscriptions/${serverId}/${Provider[provider as keyof typeof Provider]}`).then((r) => r.json()).catch(() => null) })
}

enum Provider {
    "youtube",
    "twitch"
}