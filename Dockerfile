# Backend Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy solution and project files
COPY Train.Search.WebApplication/*.csproj ./Train.Search.WebApplication/
RUN dotnet restore "Train.Search.WebApplication/Train.Search.WebApplication.csproj"

# Copy entire solution
COPY . ./
RUN dotnet publish "Train.Search.WebApplication/Train.Search.WebApplication.csproj" -c Release -o out

# Frontend Build Stage
FROM node:18 AS frontend-build
WORKDIR /src/frontend
COPY Train.Search.WebClient/Train.Search.WebClient.Infrastructure ./
RUN npm install
RUN npm run build

# Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
# Copy backend build
COPY --from=build-env /app/out .
# Copy frontend build
COPY --from=frontend-build /src/frontend/dist ./wwwroot
ENTRYPOINT ["dotnet", "Train.Search.WebApplication.dll"]