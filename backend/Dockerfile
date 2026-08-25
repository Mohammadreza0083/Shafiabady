# ==============================================================================
# Multi-Stage Production Dockerfile for ShafieiAbadi .NET 9 API
# ==============================================================================

# Stage 1: Build & Publish using .NET 9 SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first to leverage Docker layer caching
COPY ShafieiAbadi.slnx ./
COPY src/ShafieiAbadi.Domain/ShafieiAbadi.Domain.csproj src/ShafieiAbadi.Domain/
COPY src/ShafieiAbadi.Application/ShafieiAbadi.Application.csproj src/ShafieiAbadi.Application/
COPY src/ShafieiAbadi.Infrastructure/ShafieiAbadi.Infrastructure.csproj src/ShafieiAbadi.Infrastructure/
COPY src/ShafieiAbadi.Api/ShafieiAbadi.Api.csproj src/ShafieiAbadi.Api/
COPY tests/ShafieiAbadi.UnitTests/ShafieiAbadi.UnitTests.csproj tests/ShafieiAbadi.UnitTests/

# Restore NuGet dependencies
RUN dotnet restore src/ShafieiAbadi.Api/ShafieiAbadi.Api.csproj

# Copy all source files
COPY . .

# Build and publish optimized release artifacts
WORKDIR /src/src/ShafieiAbadi.Api
RUN dotnet publish ShafieiAbadi.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Minimal ASP.NET Core 9.0 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Configure runtime environment variables
ENV ASPNETCORE_URLS=http://+:5000 \
    PORT=5000 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0

# Copy published binaries from build stage
COPY --from=build /app/publish .

# Expose HTTP port 5000
EXPOSE 5000

# Enforce security best practice: run as non-root user
USER $APP_UID

# Entrypoint for ASP.NET Core API application
ENTRYPOINT ["dotnet", "ShafieiAbadi.Api.dll"]
