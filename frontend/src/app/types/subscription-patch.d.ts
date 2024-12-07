import { Message } from "../schemas/subscription.schema";

export interface SubscriptionPatch {
    webhookUrl?: string;
    messages?: Message[];
    source?: string;
    webhookUsername?: string;
    webhookAvatar?: string;
    clearWebhookUsername?: boolean;
    clearWebhookAvatar?: boolean;
}