import { Await, useLoaderData } from "react-router-dom";
import type { Subscription } from "../types/types";
import { Suspense } from "react";
import Loader from "../components/loader";
import PageHeader from "../components/page-heading";

export default function SubscriptionEditor() {
    const data = useLoaderData() as { subscriptions: Promise<Subscription> };

    return (
        <Suspense
            fallback={<Loader />}
        >
            <Await
                resolve={data?.subscriptions}
            >
                {(subscription?: Subscription) => (
                    <div className="flex flex-col m-5">
                        <PageHeader
                            title="Subsctiptions"
                            subtitle={subscription ? "Edit your subscription below" : "Create a new subscription"}
                            icon={
                                <svg className="w-6 h-6 mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 6.75h12M8.25 12h12m-12 5.25h12M3.75 6.75h.007v.008H3.75V6.75Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0ZM3.75 12h.007v.008H3.75V12Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm-.375 5.25h.007v.008H3.75v-.008Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Z" />
                                </svg>
                            }
                        />
                        <div className="flex flex-row justify-center items-center space-y-4 p-5 text-white">
                            {/* subscription editor and subscription embed, split screen */}
                            <div className="basis-1/2 w-full h-52">

                            </div>
                            <div className="basis-1/2 w-full bg-zinc-800 h-52">

                            </div>
                        </div>
                    </div>
                )}
            </Await>
        </Suspense>
    )
}