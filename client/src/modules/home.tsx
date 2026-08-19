import { useEffect } from "react";
import { Link, useOutletContext, useSearchParams } from "react-router-dom";
import type { User } from "../types/types";

export default function Home() {
    const [searchParams, setSearchParams] = useSearchParams();
    const user = useOutletContext<User | null>();
    const discordAuthUrl = import.meta.env.VITE_DISCORD_LOGIN_URL as string | undefined;

    useEffect(() => {
        if (searchParams.has("code")) {
            searchParams.delete("code");
            setSearchParams(searchParams, { replace: true });
        }
    }, [searchParams, setSearchParams]);

    return (
        <div className="text-white flex-auto flex flex-col items-center justify-center px-6 py-16 text-center">
            <h2 className="text-4xl font-bold mb-4">Announce YouTube and Twitch to Discord</h2>
            <p className="max-w-2xl text-lg text-zinc-300 mb-8">
                Hookio uses Discord webhooks to announce YouTube videos being released or updated, and Twitch streams starting, updating, or ending. Customize the message with an embed builder, then send it through your own webhook.
            </p>
            {user
                ? <Link to="/servers" className="px-5 py-2 rounded bg-indigo-600 hover:bg-indigo-500 font-semibold">Choose a server</Link>
                : discordAuthUrl
                    ? <a href={discordAuthUrl} className="px-5 py-2 rounded bg-indigo-600 hover:bg-indigo-500 font-semibold">Log in with Discord</a>
                    : <p className="text-zinc-400">Set VITE_DISCORD_LOGIN_URL to enable Discord login.</p>
            }
        </div>
    );
}
