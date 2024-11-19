import { MessageType } from "./schemas/subscription.schema";

export function toReadableFormat(key: keyof typeof MessageType, prefix = ""): string {
    const withoutPrefix = key.replace(prefix, "");
    const readable = withoutPrefix.replace(/([A-Z])/g, ' $1').trim();

    return readable;
}