import { Outlet } from "react-router-dom";
import Header from "../components/header";
import type { User } from "../types/types";

export default function Layout({ user }: { user: User }) {
    return (
        <>
            <Header user={user} />
            <main className='bg-gray-600 p-10 h-full'>
                <Outlet context={user} />
            </main>
        </>
    )
}