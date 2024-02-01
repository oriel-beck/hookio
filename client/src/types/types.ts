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

export interface Announcement {
  id: number;
  announcementType: 0,
  origin: string;
  message: string;
  // TODO: embeds
}