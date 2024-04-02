import { defer, redirect } from "react-router-dom";
import { Provider } from "../util/enums";

export default async function getSubscription({ params }: { params: Record<string, unknown> }) {
    return defer({ subscription: fetch(`/api/subscriptions/${params.subscriptionId}`).then((r) => r.json()).catch(() => redirect("/")), eventTypes: fetch(`/api/values/${Provider[params.provider as keyof typeof Provider]}`).then((r) => r.json()).catch(() => redirect(`servers/${params.serverId}/${params.provider}`)) })
}