FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/IbkrToEtax.Api/IbkrToEtax.Api.csproj ./src/IbkrToEtax.Api/
RUN dotnet restore src/IbkrToEtax.Api/IbkrToEtax.Api.csproj

COPY . ./
RUN dotnet publish src/IbkrToEtax.Api/IbkrToEtax.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM node:24-alpine AS frontend-build
WORKDIR /src/frontend

COPY frontend/package*.json ./
RUN npm ci

COPY frontend ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./
COPY --from=frontend-build /src/frontend/dist/ibkr-to-etax-frontend/browser ./frontend/

ENV IBKR_TO_ETAX_DATA_DIR=/data
ENV IBKR_TO_ETAX_FRONTEND_ROOT=/app/frontend

RUN mkdir -p /app/logs /data/uploads /data/outputs \
    && chown -R app:app /app/logs /data
USER app
WORKDIR /data

EXPOSE 8080
VOLUME ["/data"]

ENTRYPOINT ["dotnet", "/app/ibkr-to-etax.dll"]
