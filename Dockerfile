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
COPY ["Train.Search.WebClient/Train.Search.WebClient.Infrastructure/Train.Search.WebClient.Infrastructure.csproj", "Train.Search.WebClient/Train.Search.WebClient.Infrastructure/"]

# Frontend Build Stage
FROM node:18 AS frontend-build
WORKDIR /src/frontend
COPY ["Train.Search.WebClient/Train.Search.WebClient.Infrastructure/", "."]
RUN npm install
RUN npm run build

# Back to .NET build
RUN dotnet restore "Train.Search.WebApplication/Train.Search.WebApplication.csproj"
COPY . .
WORKDIR "/src/Train.Search.WebApplication"
RUN dotnet build "Train.Search.WebApplication.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Train.Search.WebApplication.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
# Copy both backend and frontend builds
COPY --from=publish /app/publish .
COPY --from=frontend-build /src/frontend/dist /app/wwwroot
ENTRYPOINT ["dotnet", "Train.Search.WebApplication.dll"]