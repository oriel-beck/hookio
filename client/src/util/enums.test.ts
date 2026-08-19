import { describe, expect, it } from "vitest";
import { APIEvents, EventType, Provider } from "./enums";
import { convertEventsToSendableData, generateDefaultEvents } from "./util";

describe("enum contract", () => {
    it("matches server provider numbers", () => {
        expect(Provider.youtube).toBe(1);
        expect(Provider.twitch).toBe(2);
    });

    it("matches server event numbers and keeps twitch", () => {
        expect(EventType["Video Uploaded"]).toBe(1);
        expect(EventType["Video Edited"]).toBe(2);
        expect(EventType["Stream Started"]).toBe(3);
        expect(EventType["Stream Updated"]).toBe(4);
        expect(EventType["Stream Ended"]).toBe(5);
        expect(APIEvents.TwitchStreamStarted).toBe(3);
    });
});

describe("convertEventsToSendableData", () => {
    it("sends EventType property matching dictionary keys", () => {
        const events = generateDefaultEvents(Provider.youtube);
        const sendable = convertEventsToSendableData(events);
        expect(sendable["1"].eventType).toBe(EventType["Video Uploaded"]);
        expect(sendable["2"].eventType).toBe(EventType["Video Edited"]);
    });
});
