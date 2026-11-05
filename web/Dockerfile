# syntax=docker/dockerfile:1
# Image du front : build Angular, puis nginx non root qui sert les fichiers et relaie /api.

ARG NODE_VERSION=26
FROM node:${NODE_VERSION}-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginxinc/nginx-unprivileged:stable-alpine AS final
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY securite.conf /etc/nginx/snippets/securite.conf
COPY --from=build /app/dist/web/browser /usr/share/nginx/html
EXPOSE 8080
