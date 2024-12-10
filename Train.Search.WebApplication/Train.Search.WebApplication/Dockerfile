FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Train.Search.WebApplication/Train.Search.WebApplication.csproj", "Train.Search.WebApplication/"]
COPY ["Train.Search.WebApplication.Infrastructure/Train.Search.WebApplication.Infrastructure.csproj", "Train.Search.WebApplication.Infrastructure/"]
RUN dotnet restore "Train.Search.WebApplication/Train.Search.WebApplication.csproj"
COPY . .
WORKDIR "/src/Train.Search.WebApplication"
RUN dotnet build "Train.Search.WebApplication.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Train.Search.WebApplication.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Train.Search.WebApplication.dll"]
