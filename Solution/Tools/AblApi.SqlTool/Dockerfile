FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "Tools/AblApi.SqlTool/AblApi.SqlTool.csproj"

RUN dotnet publish "Tools/AblApi.SqlTool/AblApi.SqlTool.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV DOTNET_ENVIRONMENT=PROD

# The container runs as the non-root `app` user (UID 1654, built into the aspnet base image).
USER app

RUN echo 'alias sqltool="dotnet AblApi.SqlTool.dll"' >> ~/.bashrc

ENTRYPOINT ["tail", "-f", "/dev/null"]
