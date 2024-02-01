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
            <main className=''>
                <Outlet context={user} />
            </main>
        </>
    )
}