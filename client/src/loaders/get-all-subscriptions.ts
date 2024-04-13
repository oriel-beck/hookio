import { defer, redirect } from "react-router-dom";
import { Provider } from "../util/enums";

export default function getAllSubscriptions({ params }: { params: Record<string, string | undefined> }) {
    const { serverId, provider } = params;
    if (!['youtube', 'twitch'].includes(provider!)) return redirect(`/servers/${serverId}`);
    return defer({ subscriptions: fetch(`/api/subscriptions/${serverId}?subscriptionType=${Provider[provider as keyof typeof Provider]}&withCounts=true`).then((r) => r.json().catch(() => null)).catch(() => null) })
}