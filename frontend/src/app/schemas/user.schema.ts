import { z } from "zod";

export type User = z.TypeOf<typeof userData>;

export const guild = z.object({
  id: z.bigint().or(z.number()),
  name: z.string(),
  iconUrl: z.optional(z.string().url()).or(z.null())
});

export const user = z.object({
  id: z.bigint().or(z.number()),
  username: z.string(),
  globalName: z.optional(z.string()),
  avatarUrl: z.optional(z.string().url()).or(z.null())
});

export const userData = z.object({
  user: user,
  guilds: z.array(guild),
});
