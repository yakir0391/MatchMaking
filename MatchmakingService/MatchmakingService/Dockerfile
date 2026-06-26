# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MatchmakingService/MatchmakingService/MatchmakingService.csproj", "MatchmakingService/MatchmakingService/"]
COPY ["Shared.Contracts/Shared.Contracts.csproj", "Shared.Contracts/"]

RUN dotnet restore "MatchmakingService/MatchmakingService/MatchmakingService.csproj"

COPY . .

WORKDIR "/src/MatchmakingService/MatchmakingService"

RUN dotnet publish "MatchmakingService.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MatchmakingService.dll"]