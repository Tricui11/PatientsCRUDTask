# Используем официальный образ для ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Используем образ для сборки проекта
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Копируем только csproj и восстанавливаем зависимости
COPY PatientApi/PatientApi.csproj /app/PatientApi.csproj
RUN dotnet restore "/app/PatientApi.csproj"

# Копируем весь проект
COPY PatientApi/ /app/PatientApi/

WORKDIR "/app/PatientApi"
RUN dotnet build "PatientApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PatientApi.csproj" -c Release -o /app/publish

# Окончательная сборка с результатами публикации
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish . 
ENTRYPOINT ["dotnet", "PatientApi.dll"]
