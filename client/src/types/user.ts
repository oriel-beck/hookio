export interface User {
    id: string;
    userName: string;
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
  