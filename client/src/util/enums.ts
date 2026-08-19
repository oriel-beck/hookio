export enum Provider {
    youtube = 1,
    twitch = 2
}

export enum EventType {
    'Video Uploaded' = 1,
    'Video Edited' = 2,
    'Stream Started' = 3,
    'Stream Updated' = 4,
    'Stream Ended' = 5
}

export enum APIEvents {
    YoutubeVideoUploaded = 1,
    YoutubeVideoEdited = 2,
    TwitchStreamStarted = 3,
    TwitchStreamUpdated = 4,
    TwitchStreamEnded = 5
}
