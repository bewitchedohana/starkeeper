#syntax=docker/dockerfile:1.7.0-labs
FROM node:22.12 AS base
EXPOSE 4200
USER bookmarks

FROM base AS install
WORKDIR /src
COPY package*.json ./
RUN npm install --ignore-scripts
COPY . .

FROM install AS development
WORKDIR /src
COPY --from=install /src ./
ENTRYPOINT [ "npm", "run", "start" ]
# ENTRYPOINT [ "sleep", "5m" ]
