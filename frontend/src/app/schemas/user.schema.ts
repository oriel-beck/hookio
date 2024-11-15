/*
User:
public string Email { get; private set; }

public bool IsVerified { get; private set; }

public bool IsMfaEnabled { get; private set; }

public UserProperties Flags { get; private set; }

public PremiumType PremiumType { get; private set; }

public string Locale { get; private set; }

Guild:

public string Name { get; private set; }

public bool IsOwner { get; private set; }

public GuildPermissions Permissions { get; private set; }

public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

public string IconUrl => CDN.GetGuildIconUrl(base.Id, _iconId, 2048);

public GuildFeatures Features { get; private set; }

public int? ApproximateMemberCount { get; private set; }

public int? ApproximatePresenceCount { get; private set; }
*/

import { z } from "zod";

// TODO: complete this after seeing what I get

export const guild = z.object({
    name: z.string(),
    isOwner: z.boolean(),
})

export const userData = z.object({
    // user: ,
    // guilds: ,
});
