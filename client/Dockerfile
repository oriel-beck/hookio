FROM node:18 as installer

WORKDIR /app
COPY package.json .
COPY package-lock.json .

RUN npm ci

FROM node:18 as builder

WORKDIR /app
COPY . .
COPY --from=installer /app/node_modules node_modules

RUN npm run build

FROM nginx as runner

COPY --from=builder /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
