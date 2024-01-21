FROM node:18 as installer

WORKDIR /app
COPY package.json .
COPY package-lock.json .

RUN npm ci

FROM node:18 as builder

WORKDIR /app
COPY . .
COPY --from=installer . .

RUN npm run build

FROM nginx as runner

# TODO: move to the html dir of nginx
COPY --from=builder dist dist
# TODO: create nginx.conf
COPY nginx.conf .
