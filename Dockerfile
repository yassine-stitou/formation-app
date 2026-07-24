# --- Étape de build --------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/FormationApp/FormationApp.csproj ./FormationApp/
RUN dotnet restore ./FormationApp/FormationApp.csproj

COPY src/FormationApp/ ./FormationApp/
WORKDIR /src/FormationApp
RUN dotnet publish -c Release -o /app/publish --no-restore

# --- Étape finale (image d'exécution allégée) -------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Railway injecte une variable d'environnement PORT dynamique au démarrage du conteneur ;
# on adapte Kestrel pour écouter dessus (avec 8080 comme valeur par défaut en local).
EXPOSE 8080
ENTRYPOINT ["/bin/sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet FormationApp.dll"]
