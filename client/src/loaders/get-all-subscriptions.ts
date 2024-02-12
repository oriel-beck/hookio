import { defer } from "react-router-dom";

export default function getAllSubscriptions({ params }: { params: Record<string, unknown> }) {
    const { serverId, provider } = params;
    return defer({ subscriptions: fetch(`/api/subscriptions/${serverId}/${Provider[provider as keyof typeof Provider]}`).then((r) => r.json()).catch(() => null) })
}

enum Provider {
    "youtube",
    "twitch"
}