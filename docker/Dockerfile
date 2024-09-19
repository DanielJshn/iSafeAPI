# Используем .NET SDK 7.0 для этапа сборки
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Обновляем пакеты и устанавливаем необходимые зависимости в промежуточном контейнере
RUN apt-get update && apt-get upgrade -y && apt-get clean && rm -rf /var/lib/apt/lists/*

# Копируем проект и восстанавливаем зависимости
COPY ["TestAPI.csproj", "./"]
RUN dotnet restore "TestAPI.csproj"

# Копируем оставшиеся файлы и собираем проект
COPY . .
RUN dotnet build --configuration Release --output /app/build

# Используем .NET SDK 7.0 для этапа публикации
FROM build AS publish
RUN dotnet publish --configuration Release --output /app/publish

# Используем .NET Runtime 7.0 для финального этапа
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app

# Обновляем пакеты в финальном контейнере
RUN apt-get update && apt-get upgrade -y && apt-get clean && rm -rf /var/lib/apt/lists/*

# Копируем опубликованные файлы из предыдущего этапа
COPY --from=publish /app/publish .

# Указываем порт, на котором будет работать приложение
EXPOSE 80

# Определяем команду для запуска приложения
ENTRYPOINT ["dotnet", "TestAPI.dll"]
