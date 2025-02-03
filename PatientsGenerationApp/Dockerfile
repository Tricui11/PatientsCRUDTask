# Используем образ .NET SDK для сборки консольного приложения
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY ["PatientsGenerationApp/PatientsGenerationApp.csproj", "PatientsGenerationApp/"]
RUN dotnet restore "PatientsGenerationApp/PatientsGenerationApp.csproj"
COPY . .
WORKDIR "/app/PatientsGenerationApp"
RUN dotnet build "PatientsGenerationApp.csproj" -c Release -o /app/build

# Публикуем консольное приложение
FROM build AS publish
RUN dotnet publish "PatientsGenerationApp.csproj" -c Release -o /app/publish

# Используем базовый образ для выполнения приложения
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PatientsGenerationApp.dll"]
