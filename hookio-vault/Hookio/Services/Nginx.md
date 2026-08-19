---
date: 2026-08-20
tags:
  - hookio
  - service
project: Hookio
---

# Nginx

The `client` Compose service: nginx 1.30.4 Alpine serving the Vite production build and reverse-proxying the API.

## Current behavior

- `listen 80` only in `client/nginx.conf`. SPA `root /usr/share/nginx/html` with `try_files $uri $uri/ /index.html`.
- Gzip for JS/CSS/JSON/XML.
- `location /api` → `http://server:8080/api`
- `location /healthz` and `location /health` → Kestrel ([[Hookio/Features/Health]])
- Host publishes **`80:80`**. `443:443` is commented out until certs exist.

Optional TLS snippet: `client/nginx.tls.conf.example` (`listen 443 ssl`, certs at `/etc/nginx/certs/fullchain.pem` and `privkey.pem`). Required for Twitch EventSub without a tunnel. See [[Hookio/Infrastructure]] and [[Hookio/Features/Twitch EventSub]].

## How it is wired

| Piece | Path |
| --- | --- |
| Dockerfile | `client/Dockerfile` — Node 24 Alpine `npm ci` + `npm run build`, then copy `dist` + `nginx.conf` into `nginx:1.30.4-alpine` |
| Config | `client/nginx.conf` |
| TLS example | `client/nginx.tls.conf.example` |
| SPA | React 18 + Vite 6 (`client/src/`), routes in `client/src/main.tsx` |
| Compose | service `client`, `env_file: client/.env.production` (Vite vars baked at **image build** time) |

`depends_on: server` with `service_healthy`.

Local development does **not** use this nginx: `npm run dev` uses Vite’s `/api` proxy (`client/vite.config.ts`).

## Env vars (names only)

Build-time: `VITE_API_ADDRESS`, `VITE_DISCORD_LOGIN_URL`. Compose file `client/.env.production`.

## Links

- [[Hookio/Infrastructure]]
- [[Hookio/Services/Hookio API]]
- [[Hookio/Features/Auth]] (login URL is a Vite env)
- [[Hookio/Features/Health]]

## Official docs

- [nginx](https://nginx.org/en/docs/)
- [`proxy_pass`](https://nginx.org/en/docs/http/ngx_http_proxy_module.html)
- [Vite](https://vite.dev/guide/)
- [React Router](https://reactrouter.com/)
