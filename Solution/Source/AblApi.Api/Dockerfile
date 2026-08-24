# Build from the solution root (Solution); requires BuildKit (default since Docker 23):
#   docker build -f Source/AblApi.Api/Dockerfile -t ablapi-api .

# --platform=$BUILDPLATFORM: the publish output is framework-dependent IL (UseAppHost=false below),
# i.e. architecture-neutral, so a multi-platform build (--platform linux/amd64,linux/arm64) runs
# restore/publish exactly once, natively on the build host, and shares the output across all
# target platforms.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy every csproj this project transitively needs so restore can resolve ProjectReferences
# without pulling the full source tree yet (keeps restore cached when only .cs files change).
COPY ["Source/AblApi.Api/AblApi.Api.csproj", "Source/AblApi.Api/"]
COPY ["Source/AblApi.DataAccess/AblApi.DataAccess.csproj", "Source/AblApi.DataAccess/"]
COPY ["Source/AblApi.Repositories/AblApi.Repositories.csproj", "Source/AblApi.Repositories/"]
COPY ["Source/AblApi.GTNH/AblApi.GTNH.csproj", "Source/AblApi.GTNH/"]
COPY ["Source/AblApi.Common/AblApi.Common.csproj", "Source/AblApi.Common/"]
COPY ["Source/AblApi.Core/AblApi.Core.csproj", "Source/AblApi.Core/"]
RUN dotnet restore "Source/AblApi.Api/AblApi.Api.csproj"

COPY . .
# --no-restore: publish reuses the assets restored above (.dockerignore excludes local obj/, so
# COPY cannot clobber them).
RUN dotnet publish "Source/AblApi.Api/AblApi.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# The container runs as the non-root `app` user (UID 1654, built into the aspnet base image).
USER app

ENTRYPOINT ["dotnet", "AblApi.Api.dll"]
