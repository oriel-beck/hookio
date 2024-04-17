import { Suspense, useEffect } from "react";
import { Await, useAsyncValue, useLoaderData, useNavigate, useParams } from "react-router-dom"
import type { AllSubscriptionsResponse } from "../types/types";
import Loader from "../components/loader";
import PageHeader from "../components/page-heading";
import { motion } from "framer-motion";

export default function SubscriptionsManager() {
    const data = useLoaderData() as { subscriptions: Promise<AllSubscriptionsResponse> };

    return (
        <Suspense fallback={<Loader />}>
            <Await resolve={data.subscriptions}>
                <InternalSubscriptionManager />
            </Await>
        </Suspense>
    )
}

function InternalSubscriptionManager() {
    const data = useAsyncValue() as AllSubscriptionsResponse;
    const params = useParams();
    const navigate = useNavigate();
    const onClick = (id?: number) => navigate(id?.toString() || "new");

    useEffect(() => {
        // TODO: handle 429 from the API, just in case, generally it should not happen
        if ('message' in data) return navigate(`/servers/${params['serverId']}`, { replace: true });
        if ('status' in data && (data as { status: number }).status === 401) return navigate(`/servers/${params['serverId']}`, { replace: true });
    })

    return (
        <div className="flex flex-col m-5 h-full">
            <PageHeader
                title="YouTube subscriptions"
                subtitle={`Select a ${params.provider} subsciption to edit or create a new one`}
                icon={
                    <svg className="w-6 h-6 mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 6.75h12M8.25 12h12m-12 5.25h12M3.75 6.75h.007v.008H3.75V6.75Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0ZM3.75 12h.007v.008H3.75V12Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm-.375 5.25h.007v.008H3.75v-.008Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Z" />
                    </svg>
                }
            />
            <div className="flex flex-col items-center justify-between space-y-4 p-5 text-white h-full">
                {/* subscription list TODO: design*/}
                <div className="flex space-x-4 justify-start w-full">
                    {Array.isArray(data.subscriptions) && data.subscriptions.map((sub) => (
                        <div key={sub.id} className="flex flex-col">
                            <button onClick={() => onClick(sub.id)} className={`${sub.subscriptionType === 0 ? 'bg-red-500 text-white' : sub.subscriptionType === 1 ? 'bg-purple-900 text-white' : ''} py-2 px-4 rounded border border-white hover:bg-opacity-60`}>
                                Edit {sub.subscriptionType === 0 ? 'Youtube' : sub.subscriptionType === 1 ? 'Twitch' : ''} Subscription {sub.id}
                            </button>
                        </div>
                    ))}
                </div>

                {/* Create subscription button */}
                {/* If there are any subs, move to the bottom left */}
                <div className={data?.subscriptions?.length ? "flex justify-center md:justify-end w-full" : "flex"}>
                    {/* TODO: make tooltip and button behavior depending on the user's premium tier */}
                    <div className="relative">
                        <button className={`peer border border-white rounded py-2 px-4 duration-75 ${(data?.count || 0) >= 2 ? 'opacity-70 cursor-default' : 'hover:bg-gray-400 hover:bg-opacity-60'}`} onClick={() => (data.count || 0) < 2 ? onClick() : null}>
                            Create new Subscription
                        </button>
                        {/* TODO: animate this to enter slowly and fade slowly */}
                        {(data?.count || 0) >= 2 &&
                            <motion.div className="absolute hidden bottom-12 left-1.5 peer-hover:block peer-hover:opacity-100 py-2 px-4 -mt-36 rounded w-48 h-18 text-wrap bg-zinc-500 transition-opacity duration-200 ease-in-out">
                                You have reached the limit for subscriptions, please subscribe in Patreon to be able to create more.
                            </motion.div>
                        }
                    </div>
                </div>
            </div>
        </div>
    )
}
