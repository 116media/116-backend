# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files (cached layer - only rebuilds if .csproj changes)
COPY *.sln ./
COPY src/Api/*.csproj ./src/Api/
COPY src/Shared/Shared/*.csproj ./src/Shared/Shared/
COPY src/Shared/Shared.Contracts/*.csproj ./src/Shared/Shared.Contracts/
COPY src/BuildingBlocks/*.csproj ./src/BuildingBlocks/
COPY src/Modules/Core/Core/*.csproj ./src/Modules/Core/Core/
COPY src/Modules/Identity/Identity/*.csproj ./src/Modules/Identity/Identity/
COPY src/Modules/Content/Content/*.csproj ./src/Modules/Content/Content/
COPY tests/Unit/*.csproj ./tests/Unit/
COPY tests/Fixtures/*.csproj ./tests/Fixtures/

# Restore dependencies (cached layer - only rebuilds if dependencies change)
# Use BuildKit cache mount for faster restores
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore

# Copy source code (this layer changes frequently)
COPY src/ ./src/

# Build and publish with BuildKit cache mounts
RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/src/obj \
    --mount=type=cache,target=/src/bin \
    dotnet publish src/Api/Api.csproj -c Release -o /app/publish

# Runtime stage (smallest possible image)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Copy only published output
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

# Run as non-root user
USER app

# Run the application
ENTRYPOINT ["dotnet", "Api.dll"]
