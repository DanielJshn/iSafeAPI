
# Установка базового образа для ASP.NET Core 8.0
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Используем SDK образ для сборки (для .NET 8.0)
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TestAPI.csproj", "./"]
RUN dotnet restore "./TestAPI.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "TestAPI.csproj" -c Release -o /app/build

# Публикуем проект в папку /app/publish
FROM build AS publish
RUN dotnet publish "TestAPI.csproj" -c Release -o /app/publish

# Используем базовый образ для финального контейнера
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestAPI.dll"]
