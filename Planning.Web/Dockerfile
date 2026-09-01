# Build context is Planning.Web.
#   docker build -f Planning.Web/Dockerfile Planning.Web
FROM node:22-alpine AS build
WORKDIR /src

RUN corepack enable && corepack prepare pnpm@10.33.0 --activate

# --ignore-scripts because package.json's postinstall runs `nuxt prepare`, which needs the app
# source that has not been copied yet. `nuxt build` below does its own prepare, so nothing is lost.
COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile --ignore-scripts

COPY . .
RUN pnpm run build

FROM node:22-alpine AS final
WORKDIR /app

# Nitro's output is fully self-contained: no node_modules, no package.json, no install step.
COPY --from=build /src/.output ./

ENV NODE_ENV=production \
    HOST=0.0.0.0 \
    PORT=3000

EXPOSE 3000
USER node

CMD ["node", "server/index.mjs"]
