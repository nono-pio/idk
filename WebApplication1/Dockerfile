# Étape 1 : Utiliser l'image de base ASP.NET Core pour exécuter l'application
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

# Étape 2 : Utiliser l'image SDK pour construire l'application
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["AppWeb/AppWeb.csproj", "AppWeb/"]
RUN dotnet restore "AppWeb/AppWeb.csproj"
COPY . .
WORKDIR "/src/AppWeb"
RUN dotnet build "AppWeb.csproj" -c Release -o /app/build

# Étape 3 : Publier l'application
FROM build AS publish
RUN dotnet publish "AppWeb.csproj" -c Release -o /app/publish

# Étape 4 : Préparer l'image finale pour exécuter l'application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AppWeb.dll"]
