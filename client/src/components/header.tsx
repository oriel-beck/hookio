import { useNavigate } from "react-router-dom";
import { User } from "../types/types"

export default function Header({ user, showLogin = true }: { user: User | null, showLogin?: boolean }) {
    const navigate = useNavigate();
    const discordAuthUrl = import.meta.env.VITE_DISCORD_LOGIN_URL;

    const onNavClick = (href: string) => navigate(href)
    const logOut = async () => {
        await fetch("/api/users/logout", { method: "POST" });
        navigate("/?logout=true")
    }

    return (
        <header className="w-full h-14 flex flex-row items-center bg-neutral-950 bg-opacity-70 text-white z-10 relative flex-initial">
            <h1 className="ml-3 text-2xl font-bold">
                <a href="/">
                    Hookio
                </a>
            </h1>
            {user &&
                <nav role="navigation">
                    <ul className="ml-10">
                        <li onClick={() => onNavClick("/servers")} className="cursor-pointer py-1.5 px-3 hover:bg-white hover:bg-opacity-20 rounded ext-white font-semibold text-md">
                            Servers
                        </li>
                    </ul>
                </nav>
            }
            <span aria-hidden='true' className="flex flex-1"></span>
            <div className="pr-5">
                {user
                    ?
                    <div className="flex space-x-3 justify-center">
                        <div>
                            <div className="flex flex-row items-center space-x-3 font-bold">
                                <img src={user.avatar} alt={user.username} height={40} width={40} className="rounded-full" />
                            </div>
                        </div>
                        <button aria-label="Log Out" onClick={logOut} className="p-2 rounded-full hover:bg-white hover:bg-opacity-30">
                            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none"
                                stroke="#6366f1" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"
                                className="feather feather-log-out">
                                <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path>
                                <polyline points="16 17 21 12 16 7"></polyline>
                                <line x1="21" y1="12" x2="9" y2="12"></line>
                            </svg>
                        </button>
                    </div>
                    :
                    <>
                        {showLogin &&
                            <a href={discordAuthUrl} className="p-[2px] relative" style={{ appearance: 'button' }}>
                                <div className="absolute inset-0 bg-gradient-to-r from-indigo-500 to-purple-500 rounded-lg" />
                                <div className="px-5 py-1.5 bg-black rounded-[6px] relative group transition duration-200 text-white hover:bg-transparent">
                                    Login
                                </div>
                            </a>
                        }
                    </>
                }
            </div>
        </header >
    )
}