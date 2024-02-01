import { Outlet, useLocation } from "react-router-dom";
import Header from "../components/header";
import type { User } from "../types/types";
import { useEffect } from "react";

export default function Layout({ user }: { user: User }) {
    const { pathname } = useLocation();

    useEffect(() => {
        window.scrollTo(0, 0);
    }, [pathname]);
    return (
        <>
            <Header user={user} />
            <main className='min-h-screen bg-zinc-900 p-10 h-full'>
                <Outlet context={user} />
            </main>
        </>
    )
}