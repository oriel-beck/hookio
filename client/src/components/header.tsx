import { User } from "../types/types"
import { useCallback, useEffect, useState } from "react";

export default function Header({ user, showLogin = true }: { user: User | null, showLogin?: boolean }) {
    const discordAuthUrl = "https://discord.com/api/oauth2/authorize?client_id=1198355601990893688&response_type=code&redirect_uri=http%3A%2F%2Flocalhost%3A5173%2F&scope=identify+guilds+email";

    const [open, setOpen] = useState(false);

    const handleOutsideClick = useCallback((event: MouseEvent) => {
        if ((event.target as HTMLElement).parentElement?.id === "clickable") return;
        setOpen(false)
    }, []);

    useEffect(() => {
        window.addEventListener("click", handleOutsideClick);
        return () => {
            window.removeEventListener("click", handleOutsideClick);
        }
    })

    return (
        <header className="w-full h-14 flex flex-row items-center bg-gray-800 text-white">
            <h1 className="ml-3 text-xl">Hookio</h1>
            <span aria-hidden='true' className="flex flex-1"></span>
            <div className="pr-5">
                {user
                    ?
                    <div>
                        <div aria-haspopup='true' aria-expanded={open} id="clickable" onClick={() => setOpen(!open)} className="cursor-pointer flex flex-row items-center space-x-3 font-bold" typeof="button">
                            <span>{user.username}</span>
                            <img src={user.avatar} alt={user.username} height={40} width={40} className="rounded-full" />

                        </div>
                        {open &&
                            <div id="clickable" className="relative left-0">
                                <div id="clickable" className="absolute rounded-md mt-0.5 w-32 bg-slate-800">
                                    <h2 className="flex rounded-t-md border-b-slate-600 border-t-0 border-l-0 border-r-0 border-2 justify-center p-2 bg-slate-900">Actions</h2>
                                    <ul id="clickable">
                                        <li className="cursor-pointer px-3 p-2 hover:bg-slate-700">
                                            <a className="flex items-center" href="/servers">
                                                <svg className="w-6 h-6 mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                                                    <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 6.75h12M8.25 12h12m-12 5.25h12M3.75 6.75h.007v.008H3.75V6.75Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0ZM3.75 12h.007v.008H3.75V12Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm-.375 5.25h.007v.008H3.75v-.008Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Z" />
                                                </svg>
                                                Servers
                                            </a>
                                        </li>
                                        <li className="cursor-pointer px-3 p-2 hover:bg-slate-700">
                                            <a className="flex items-center" href="/logout">
                                                <svg className="mr-2 h-6 w-6" fill="none" strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} viewBox="0 0 24 24" stroke="currentColor">
                                                    <path d="M17 16L21 12M21 12L17 8M21 12L7 12M13 16V17C13 18.6569 11.6569 20 10 20H6C4.34315 20 3 18.6569 3 17V7C3 5.34315 4.34315 4 6 4H10C11.6569 4 13 5.34315 13 7V8"></path>
                                                </svg>
                                                Log out
                                            </a>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        }
                    </div>
                    :
                    <>
                        {showLogin &&
                            <a href={discordAuthUrl} className="bg-transparent hover:bg-indigo-900 font-semibold py-1.5 px-4 border border-white hover:border-transparent rounded">Log in</a>
                        }
                    </>
                }
            </div>
        </header >
    )
}