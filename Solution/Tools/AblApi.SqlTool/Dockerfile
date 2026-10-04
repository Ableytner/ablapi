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

# libgssapi-krb5-2: Required by the .NET runtime to resolve libgssapi_krb5.so.2.
# The minimal aspnet base image omits this Kerberos library, but .NET attempts to
# load it at startup for Kerberos/SPNEGO auth scenarios (e.g. via Npgsql).
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*

ENV DOTNET_ENVIRONMENT=PROD

# The container runs as the non-root `app` user (UID 1654, built into the aspnet base image).
USER app

RUN echo 'alias sqltool="dotnet AblApi.SqlTool.dll"' >> ~/.bashrc

ENTRYPOINT ["tail", "-f", "/dev/null"]
