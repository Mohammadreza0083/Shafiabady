# ==============================================================================
# Multi-Stage Production Dockerfile for ShafieiAbadi Astro Frontend
# ==============================================================================

# Stage 1: Install Dependencies
FROM node:20-alpine AS deps
WORKDIR /app

# Install build prerequisites if required
RUN apk add --no-cache libc6-compat

# Copy package descriptors for layer caching
COPY package.json package-lock.json ./
RUN npm ci --include=dev

# Stage 2: Build Application
FROM node:20-alpine AS builder
WORKDIR /app

COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Set production build environment
ENV NODE_ENV=production \
    ASTRO_TELEMETRY_DISABLED=1

# Compile Astro project to standalone SSR bundle
RUN npm run build

# Stage 3: Minimal Production Runtime
FROM node:20-alpine AS runner
WORKDIR /app

ENV NODE_ENV=production \
    HOST=0.0.0.0 \
    PORT=4500 \
    ASTRO_TELEMETRY_DISABLED=1

# Copy build artifacts and production dependencies
COPY --from=builder /app/dist ./dist
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/package.json ./package.json
COPY --from=builder /app/public ./public

# Run with non-root node user provided by Alpine
USER node

# Expose Astro standalone server port
EXPOSE 4500

# Start standalone SSR node server
CMD ["node", "./dist/server/entry.mjs"]
