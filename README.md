# hookio

## ⚠️ This is a WIP project, please do not try to run this as production ⚠️

### Required env's

#### .env.postgres (root dir)

```env
POSTGRES_PASSWORD=****
POSTGRES_DB=****
```

#### client/.env & client/.env.production

```env
# If using docker-compose, replace `localhost` with `server`
VITE_API_ADDRESS=http://localhost:7109
VITE_DISCORD_LOGIN_URL=*****
```

#### server/Hookio/.env & server/Hookio/.env.production

```env
JWT_SECRET=****
DISCORD_CLIENT_SECRET=****
DISCORD_CLIENT_ID=****
DISCORD_REDIRECT_URI=*****
PG_CONNECTION_STRING=Server={POSTGRES_HOST};Database={POSTGRES_DB};Port=5432;User Id=postgres;Password={POSTGRES_PASSWORD};
YT_IDENTIFIER=*****
REDIS_CONNECTION_STRING=localhost
YOUTUBE_API_KEY=*****
```

`{POSTGRES_DB}` and `{POSTGRES_PASSWORD}` should be copied from .env.postgres

`{POSTGRES_HOST}` is `127.0.0.1` if used with a local DB or `postgres` if used with the compose

- ⚠️ Warning: the server env is not read as a regular env, meaning it cannot have comments, extra spaces, empty values, etc, otherwise it will break
