# Build context is the repository root, because the API pulls in three sibling projects.
#   docker build -f Planning.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Project files first: this layer only changes when a dependency changes, so the (slow) restore
# stays cached across ordinary source edits.
COPY Planning.Domain/Planning.Domain.csproj Planning.Domain/
COPY Planning.Application/Planning.Application.csproj Planning.Application/
COPY Planning.Infrastructure/Planning.Infrastructure.csproj Planning.Infrastructure/
COPY Planning.Api/Planning.Api.csproj Planning.Api/
RUN dotnet restore Planning.Api/Planning.Api.csproj

COPY Planning.Domain/ Planning.Domain/
COPY Planning.Application/ Planning.Application/
COPY Planning.Infrastructure/ Planning.Infrastructure/
COPY Planning.Api/ Planning.Api/
RUN dotnet publish Planning.Api/Planning.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# curl is here purely so the container healthcheck has something to call. Without it the
# healthcheck silently reports unhealthy and the deploy blocks on a container that is actually fine.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Organization logos are written here at runtime and are bind-mounted to a host volume in
# production. The directory must exist and be owned by the non-root app user before that mount,
# otherwise the first upload fails with a permission error.
RUN mkdir -p /app/img/logos && chown -R $APP_UID:$APP_UID /app/img

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "Planning.Api.dll"]
