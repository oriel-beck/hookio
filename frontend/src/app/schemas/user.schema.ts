import { z } from "zod";

export type User = z.TypeOf<typeof userData>;

export const guild = z.object({
  id: z.bigint(),
  name: z.string(),
  iconUrl: z.string().url().optional()
});

export const user = z.object({
  id: z.bigint(),
  username: z.string(),
  globalName: z.string().optional(),
  avatarUrl: z.string().url().optional()
});

export const userData = z.object({
  user: user,
  guilds: z.array(guild),
});
