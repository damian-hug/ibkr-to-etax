FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ibkr-to-etax.csproj ./
RUN dotnet restore ibkr-to-etax.csproj

COPY . ./
RUN dotnet publish ibkr-to-etax.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./

ENV IBKR_TO_ETAX_DATA_DIR=/data

RUN mkdir -p /app/logs /data/uploads /data/outputs \
    && chown -R app:app /app/logs /data
USER app
WORKDIR /data

VOLUME ["/data"]

ENTRYPOINT ["dotnet", "/app/ibkr-to-etax.dll"]
