import type { EventType } from "../util/enums";

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
  events: Record<string, EventResponse>
}

export interface EventResponse {
  id: number;
  eventType: EventType;
  message: Message;
}

export interface Message {
  content?: string;
  embeds: Embed[];
  username?: string;
  avatar?: string;
}

export interface Embed {
  id: number;
  description?: string;
  title?: string;
  titleUrl?: string;
  author?: string;
  authorUrl?: string;
  authorIcon?: string;
  color?: string;
  image?: string;
  footer?: string;
  footerIcon?: string;
  thumbnail?: string;
  addTimestamp: boolean;
  fields: EmbedField[];
}

export interface EmbedField {
  id: number;
  name: string;
  value: string;
  inline: boolean;
}

export interface MessageFormikInitialValue {
  content?: string;
  username?: string;
  avatar?: string;
  embeds: Embed[];
}

export interface EventFormikInitialValue {
  message: MessageFormikInitialValue
}

export interface FormikInitialValue {
  webhookUrl?: string;
  url?: string;
  events: Record<string, EventFormikInitialValue>
}