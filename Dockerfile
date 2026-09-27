# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# One copy of src/ rather than a line per project: the project list changes with every module split, and
# Docker flattens a `**/*.csproj` glob unless the labs `COPY --parents` is available, which CI cannot assume.
COPY Directory.Build.props Directory.Packages.props ./
COPY src/ ./src/

# Restore only what the host needs. The test projects are not copied, so restoring the solution would fail.
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore src/host/Api/Api.csproj

# Build and publish with BuildKit cache mounts
RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/src/obj \
    --mount=type=cache,target=/src/bin \
    dotnet publish src/host/Api/Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage (smallest possible image)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime

# Install ICU libraries for globalization support (localization, cultures)
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

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
