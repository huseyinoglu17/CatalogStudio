FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY CatalogStudio.csproj ./
RUN dotnet restore CatalogStudio.csproj
COPY . ./
RUN dotnet publish CatalogStudio.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
USER root
RUN apt-get update && apt-get install -y --no-install-recommends fonts-dejavu-core gosu \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish ./
COPY docker-entrypoint.sh /usr/local/bin/catalog-entrypoint
RUN sed -i 's/\r$//' /usr/local/bin/catalog-entrypoint && chmod 755 /usr/local/bin/catalog-entrypoint
ENV ASPNETCORE_ENVIRONMENT=Production \
    APP_DATA_PATH=/app/App_Data \
    ASPNETCORE_HTTP_PORTS=8080 \
    Catalog__FontPath=/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf
EXPOSE 8080
ENTRYPOINT ["/usr/local/bin/catalog-entrypoint"]
