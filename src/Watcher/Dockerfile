# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "RealTimeMongoDashboard.Watcher/RealTimeMongoDashboard.Watcher.csproj" --use-current-runtime
RUN dotnet publish "RealTimeMongoDashboard.Watcher/RealTimeMongoDashboard.Watcher.csproj" -c Release -o /app/publish /p:UseAppHost=false

# run
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
VOLUME ["/app/logs"]
ENTRYPOINT ["dotnet", "RealTimeMongoDashboard.Watcher.dll"]
