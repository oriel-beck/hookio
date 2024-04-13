import { defer } from "react-router-dom";

export default async function getSubscription({ params }: { params: Record<string, unknown> }) {
    return defer({ subscription: fetch(`/api/subscriptions/${params['serverId']}/${params['subscriptionId']}`).then((r) => r.json().catch(() => null)) })
}