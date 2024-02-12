import { useNavigate } from "react-router-dom";
import type { Subscription as SubscriptionType } from "../../types/types";
import './subscription.scss';

export default function Subscription({ subscription }: { subscription: SubscriptionType }) {
    const navigate = useNavigate();
    function onClick() {
        navigate(`/subscriptions/edit/${subscription.id}`)
    }
    return (
        <div className="rounded bg-zinc-300">
            <div className="p-3 bg-zinc-500 rounded-t font-bold text-lg">
                <h3>{subscription.channelId}</h3>
            </div>
            <div className="flex justify-end">
                <button onClick={() => onClick()} className={`text-white font-bold py-2 px-4 rounded m-2 w-full ${getButtonClass(subscription.subscriptionType)}`}>Edit</button>
            </div>
        </div>
    )
}

function getButtonClass(announcementType: number) {
    // TODO: add colors for all announcement types
    switch (announcementType) {
        case 0:
            return "youtube";
        case 1:
            return "twitch";
        case 2:
            return "kick";
        default:
            return "";
    }
}