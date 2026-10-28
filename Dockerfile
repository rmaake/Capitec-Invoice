FROM node:22-alpine AS web-build
WORKDIR /src/web
COPY web/package.json web/package-lock.json ./
RUN npm ci
COPY web/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src
COPY api/Capitec.Api/Capitec.Api.csproj api/Capitec.Api/
COPY api/Capitec.Business/Capitec.Business.csproj api/Capitec.Business/
COPY api/Capitec.DAL/Capitec.DAL.csproj api/Capitec.DAL/
RUN dotnet restore api/Capitec.Api/Capitec.Api.csproj
COPY api/ api/
RUN dotnet publish api/Capitec.Api/Capitec.Api.csproj -c Release -o /out --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
RUN mkdir -p /app/data && chown -R 1654:1654 /app
USER 1654:1654
COPY --from=api-build /out/ ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "Capitec.Api.dll"]

FROM nginx:alpine AS web
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=web-build /src/web/dist/capitec-web/browser/ /usr/share/nginx/html/
EXPOSE 80