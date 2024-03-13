import { useNavigate } from "react-router-dom";
import ConfigureButton from "../components/configure-button";
import PageHeader from "../components/page-heading";

export default function ProviderSelection() {
    const navigate = useNavigate();
    function selectProvider(provider: string) {
        navigate(provider);
    }
    return (
        <div className="flex flex-col m-5">
            <PageHeader
                title="Subscriptions"
                subtitle="Select a streaming service to subscribe to"
                icon={
                    <svg className="w-6 h-6 mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 6.75h12M8.25 12h12m-12 5.25h12M3.75 6.75h.007v.008H3.75V6.75Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0ZM3.75 12h.007v.008H3.75V12Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm-.375 5.25h.007v.008H3.75v-.008Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Z" />
                    </svg>
                }
            />
            <div className="flex flex-col justify-center items-center space-y-4 p-5 text-white">
                <div onClick={() => selectProvider("youtube")} onKeyUp={(ev) => ev.key === "Enter" || ev.key === " " ? selectProvider("youtube") : null} className="border-white border p-5 rounded flex items-center space-x-4 w-full h-24 hover:border-indigo-400 focus:border-indigo-400 focus:border-2 hover:border-1 outline-none" role="button" tabIndex={0}>
                    <img
                        src="/youtube.webp"
                        width={100}
                        aria-labelledby="youtube"
                    />
                    <h3 id="youtube" className="text-4xl font-bold" style={{ color: '#ff0000' }}>Youtube</h3>
                    <span className="flex flex-1"></span>
                    <div className="flex justify-end">
                        <ConfigureButton onClick={() => selectProvider("youtube")} />
                    </div>
                </div>
                <div onClick={() => selectProvider("youtube")} onKeyUp={(ev) => ev.key === "Enter" || ev.key === " " ? selectProvider("youtube") : null} className="border-white border-2 p-5 rounded flex items-center justify-center space-x-4 w-full h-24 hover:border-indigo-400 focus:border-indigo-400 focus:border-2 hover:border-1 outline-none" role="button" tabIndex={0}>
                    <img
                        src="/twitch.png"
                        width={80}
                        className="mr-5"
                        aria-labelledby="twitch"
                    />
                    <h3 id="twitch" className="text-4xl font-bold" style={{ color: '#6441A5' }}>Twitch</h3>
                    <span className="flex flex-1"></span>
                    <div className="flex justify-end">
                        <ConfigureButton onClick={() => selectProvider("twitch")} />
                    </div>
                </div>
            </div>
        </div>
    )
}