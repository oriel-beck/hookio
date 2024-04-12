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
  url: string;
  events: Record<string, EventResponse>
}

export interface AllSubscriptionsResponse {
  count: number;
  subscriptions: Subscription[];
}

export interface EventResponse {
  id: unknown;
  eventType: EventType;
  message: Message;
}

export interface Message {
  id?: unknown;
  content?: string;
  embeds: Embed[];
  username?: string;
  avatar?: string;
}

export interface Embed {
  /** String when generated locally, number when returned from server */
  id?: unknown;
  /** Required only in fetch */
  index?: number;
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
  /** String when generated locally, number when returned from server */
  id?: unknown;
  /** Required only in fetch */
  index?: number;
  name: string;
  value: string;
  inline: boolean;
}

export interface EmbedTitleFormikInitialValue {
  text?: string;
  url?: string;
}

export interface EmbedAuthorFormikInitialValue {
  text?: string;
  url?: string;
  icon?: string;
}

export interface EmbedFooterFormikInitialValue {
  text?: string;
  icon?: string;
}

export interface EmbedFormikInitialValue {
    /** String when generated locally, number when returned from server */
    id?: unknown;
    /** Required only in fetch */
    index?: number;
    description?: string;
    title?: EmbedTitleFormikInitialValue;
    author?: EmbedAuthorFormikInitialValue;
    color?: string;
    image?: string;
    footer?: EmbedFooterFormikInitialValue;
    thumbnail?: string;
    addTimestamp: boolean;
    fields: EmbedField[];
}

export interface MessageFormikInitialValue {
  id?: unknown;
  content?: string;
  username?: string;
  avatar?: string;
  embeds: EmbedFormikInitialValue[];
}

export interface EventFormikInitialValue {
  id?: unknown;
  message: MessageFormikInitialValue
}

export interface FormikInitialValue {
  webhookUrl?: string;
  url?: string;
  events: Record<string, EventFormikInitialValue>
}