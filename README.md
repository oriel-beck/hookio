# hookio

## ⚠️ This is a WIP project, please do not try to run this as production ⚠️

### Required env's
- .env.postgres
```env
POSTGRES_PASSWORD=****
POSTGRES_DB=****
```

- client/.env
```env
# If using docker-compose, replace `localhost` with `server` 
API_ADDRESS=http://localhost:7109
```

- server/.env
```env
JWT_SECRET=****
DISCORD_CLIENT_SECRET=****
DISCORD_CLIENT_ID=****
PG_CONNECTION_STRING=Server={POSTGRES_HOST};Database={POSTGRES_DB};Port=5432;User Id=postgres;Password={POSTGRES_PASSWORD};
```
- ⚠️ Warning: the server env is not read as a regular env, meaning it cannot have comments, extra spaces, empty values, etc, otherwise it will break