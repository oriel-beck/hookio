import { defer, redirect } from "react-router-dom";

export default async function getSubscription({ params }: { params: Record<string, unknown> }) {
    return defer({ subscription: fetch(`/api/subscriptions/${params.subscriptionId}`).then((r) => r.json()).catch(() => redirect("/")) })
}