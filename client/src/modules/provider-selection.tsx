import { useNavigate } from "react-router-dom";
import ConfigureButton from "../components/configure-button";

export default function ProviderSelection() {
    const navigate = useNavigate();
    function selectProvider(provider: string) {
        navigate(provider);
    }
    return (
        <div className="flex flex-col m-5 mt-10 bg-zinc-700 pb-5">
            <div className="w-full p-5 bg-zinc-800 text-white">
                <div className="flex">
                    <svg className="w-6 h-6 mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 6.75h12M8.25 12h12m-12 5.25h12M3.75 6.75h.007v.008H3.75V6.75Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0ZM3.75 12h.007v.008H3.75V12Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm-.375 5.25h.007v.008H3.75v-.008Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Z" />
                    </svg>
                    <h2>Subscriptions</h2>
                </div>
                <h3 className="text-gray-400 pt-2">Select a streaming service to subscribe to</h3>
            </div>
            <div className="flex flex-col justify-center items-center space-y-4 p-5 text-white">
                <div className="bg-zinc-800 p-5 rounded flex items-center space-x-4 w-full h-24">
                    <img
                        src="/youtube.webp"
                        width={100}
                    />
                    <h3 className="text-4xl font-bold" style={{ color: '#ff0000' }}>Youtube</h3>
                    <span className="flex flex-1"></span>
                    <div className="flex justify-end mt-2">
                        <ConfigureButton onClick={() => selectProvider("youtube")} />
                    </div>
                </div>
                <div className="bg-zinc-800 p-5 rounded flex items-center space-x-4 w-full h-24">
                    <img
                        src="/twitch.png"
                        width={80}
                        className="mr-5"
                    />
                    <h3 className="text-4xl font-bold" style={{ color: '#6441A5' }}>Twitch</h3>
                    <span className="flex flex-1"></span>
                    <div className="flex justify-end mt-2">
                        <ConfigureButton onClick={() => selectProvider("twitch")} />
                    </div>
                </div>
            </div>
        </div>
    )
}