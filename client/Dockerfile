FROM node:24-alpine as installer

WORKDIR /app
COPY package.json .
COPY package-lock.json .

RUN npm ci

FROM node:24-alpine as builder

WORKDIR /app
COPY . .
COPY --from=installer /app/node_modules node_modules

RUN npm run build

FROM nginx:1.30.4-alpine as runner

COPY --from=builder /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
