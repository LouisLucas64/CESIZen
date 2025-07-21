# Étape 1 : Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier les fichiers de solution et de projets pour le restore
COPY ["CESIZenAppli.sln", "./"]
COPY ["CesiNewsBackOfficeMVC/CESIZenBackOfficeMVC.csproj", "CesiNewsBackOfficeMVC/"]
COPY ["CesiNewsModel/CESIZenModel.csproj", "CesiNewsModel/"]
COPY ["CESIZen.Tests/CESIZen.Tests.csproj", "CESIZen.Tests/"]

# Restore des dépendances
RUN dotnet restore "CESIZenAppli.sln"

# Copier tout le code source
COPY . .

# Build et publish du projet spécifique
WORKDIR "/src/CesiNewsBackOfficeMVC"
RUN dotnet build "CesiNewsBackOfficeMVC.csproj" -c Release -o /app/build
RUN dotnet publish "CesiNewsBackOfficeMVC.csproj" -c Release -o /app/publish --no-restore

# Étape 2 : Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "CesiNewsBackOfficeMVC.dll"]