# 1. Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# 2. Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY BookResearchApp/BookResearchApp.csproj BookResearchApp/
WORKDIR /src/BookResearchApp
RUN dotnet restore

WORKDIR /src
COPY . .

WORKDIR /src/BookResearchApp
RUN dotnet publish -c Release -o /app/publish

# 3. Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BookResearchApp.dll"]