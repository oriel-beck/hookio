export interface User {
  id: string;
  username: string;
  discriminator: string;
  premium: number;
  avatar: string;
  guilds: Guild[];
}

export interface Guild {
  name: string;
  icon: string;
  id: string;
}

export interface Subscription {
  id: number;
  subscriptionType: number;
  channelId: string;
}

export interface Message {
  // TODO
}