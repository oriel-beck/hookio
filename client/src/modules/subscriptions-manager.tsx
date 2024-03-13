import { Suspense } from "react";
import { Await, useLoaderData, useNavigate } from "react-router-dom"
import type { Subscription } from "../types/types";
import Loader from "../components/loader";
import PageHeader from "../components/page-heading";

export default function SubscriptionsManager() {
    const navigate = useNavigate();
    const data = useLoaderData() as { subscriptions: Promise<Subscription[]> };
    const onClick = (id?: number) => navigate(id?.toString() || "new");

    return (
        <Suspense
            fallback={<Loader />}
        >
            <Await
                resolve={data.subscriptions}
            >
                {(subscriptions: Subscription[]) => (
                    <div className="flex flex-col m-5">
                        <PageHeader
                            title="YouTube subscriptions"
                            subtitle="Select a youtube subsciptions to edit or create a new one"
                            icon={
                                <svg className="w-6 h-6 mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 6.75h12M8.25 12h12m-12 5.25h12M3.75 6.75h.007v.008H3.75V6.75Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0ZM3.75 12h.007v.008H3.75V12Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm-.375 5.25h.007v.008H3.75v-.008Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Z" />
                                </svg>
                            }
                        />
                        <div className="flex flex-col justify-center items-center space-y-4 p-5 text-white">
                            {/* subscription list */}
                            <div>
                                {subscriptions.map((sub) => (
                                    <>
                                        {sub.id}
                                    </>
                                ))}
                            </div>

                            {/* Create subscription button */}
                            {
                                subscriptions.length
                                    ?
                                    // If there are any subs, move to the bottom left
                                    <div className="flex justify-end w-full">
                                        <button onClick={() => onClick()}>
                                            Create new Subscription
                                        </button>
                                    </div>
                                    :
                                    // If there are no subs, go to the center of the screen
                                    <div className="flex">
                                        <button onClick={() => onClick()}>
                                            Create new Subscription
                                        </button>
                                    </div>
                            }

                        </div>
                    </div>
                )}
            </Await>
        </Suspense>
    )
}
/*



*/