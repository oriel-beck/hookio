import { Await, useLoaderData } from "react-router-dom";
import { Suspense } from "react";
import Loader from "../../components/loader";
import type { Subscription } from "../../types/types";
import { FormikForm } from "./formik";

export default function SubscriptionEditor() {
    const data = useLoaderData() as { subscription: Promise<Subscription> };

    return (
        <Suspense fallback={<Loader />}>
            <Await resolve={data?.subscription}>
                <FormikForm />
            </Await>
        </Suspense>
    )
}

