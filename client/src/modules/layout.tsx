import { Outlet, useLocation } from "react-router-dom";
import Header from "../components/header";
import type { User } from "../types/types";
import { useEffect } from "react";
import { BackgroundBeams } from "../components/background-beams";

export default function Layout({ user }: { user: User }) {
    const { pathname } = useLocation();

    useEffect(() => {
        window.scrollTo(0, 0);
    }, [pathname]);
    return (
        <>
            <Header user={user} />
            <main className="relative z-10 h-full flex-auto flex flex-col">
                <Outlet context={user} />
            </main>
            <BackgroundBeams />
        </>
    )
}