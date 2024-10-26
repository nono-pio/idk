FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

# Installer Node.js
RUN apt-get update && apt-get install -y curl && \
    curl -sL https://deb.nodesource.com/setup_21.x | bash - && \
    apt-get install -y nodejs && \
    npm install -g npm@latest

WORKDIR /src

# Copier et restaurer les projets .NET
COPY ["WebApplication1/WebApplication1.csproj", "WebApplication1/"]
COPY ["ConsoleApp1/ConsoleApp1.csproj", "ConsoleApp1/"]
RUN dotnet restore "WebApplication1/WebApplication1.csproj"

# Copier les fichiers source
COPY . .

# Installer les dépendances et construire le front-end si présent
WORKDIR "/src/WebApplication1/client-app"
RUN npm install && npm run build

# Revenir au répertoire principal et compiler le projet .NET
WORKDIR "/src/WebApplication1"
RUN dotnet build "WebApplication1.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Étape de publication
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebApplication1.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Finalisation de l'image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApplication1.dll"]
