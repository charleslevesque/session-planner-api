FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/SessionPlanner.Web/SessionPlanner.Web.csproj src/SessionPlanner.Web/
RUN dotnet restore src/SessionPlanner.Web/SessionPlanner.Web.csproj

COPY src/SessionPlanner.Web/ src/SessionPlanner.Web/
RUN dotnet publish src/SessionPlanner.Web/SessionPlanner.Web.csproj -c Release -o /app/publish --no-restore

FROM nginx:1.27-alpine AS runtime
COPY docker/frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

EXPOSE 80
