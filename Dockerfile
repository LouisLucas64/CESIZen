# Étape 1 : Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copie tout
COPY . .

# Restore
RUN dotnet restore CESIZenAppli.sln

# Build et publish
RUN dotnet publish CesiNewsBackOfficeMVC/CesiNewsBackOfficeMVC.csproj -c Release -o /app/publish

# Étape 2 : Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "CesiNewsBackOfficeMVC.dll"]