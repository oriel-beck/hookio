export default async function getSubscription({ params }: { params: Record<string, unknown> }) {
    return { subscription: fetch(`/api/subscriptions/${params['serverId']}/${params['subscriptionId']}`).then((r) => r.json().catch(() => null)) }
}
