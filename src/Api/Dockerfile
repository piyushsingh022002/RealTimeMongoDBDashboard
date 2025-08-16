# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "RealTimeMongoDashboard.API/RealTimeMongoDashboard.API.csproj" --use-current-runtime
RUN dotnet publish "RealTimeMongoDashboard.API/RealTimeMongoDashboard.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
VOLUME ["/app/logs"]
ENTRYPOINT ["dotnet", "RealTimeMongoDashboard.API.dll"]
