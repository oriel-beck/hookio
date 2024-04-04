import { Suspense } from "react";
import { Await, useLoaderData } from "react-router-dom"
import type { Subscription } from "../../types/types";
import Loader from "../../components/loader";
import InternalSubscriptionManager from "./manager";

export default function SubscriptionsManager() {
    const data = useLoaderData() as { subscriptions: Promise<Subscription[]> };

    return (
        <Suspense fallback={<Loader />}>
            <Await resolve={data.subscriptions}>
                <InternalSubscriptionManager/>
            </Await>
        </Suspense>
    )
}
