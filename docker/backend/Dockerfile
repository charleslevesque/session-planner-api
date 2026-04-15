FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/SessionPlanner.Api/SessionPlanner.Api.csproj src/SessionPlanner.Api/
COPY src/SessionPlanner.Core/SessionPlanner.Core.csproj src/SessionPlanner.Core/
COPY src/SessionPlanner.Infrastructure/SessionPlanner.Infrastructure.csproj src/SessionPlanner.Infrastructure/
RUN dotnet restore src/SessionPlanner.Api/SessionPlanner.Api.csproj

COPY src/ src/
RUN dotnet publish src/SessionPlanner.Api/SessionPlanner.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN mkdir -p /app/data
COPY --from=build /app/publish .

EXPOSE 5290
ENV ASPNETCORE_URLS=http://+:5290

ENTRYPOINT ["dotnet", "SessionPlanner.Api.dll"]
