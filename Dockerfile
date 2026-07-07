FROM node:18-alpine AS frontend-builder
WORKDIR /app
COPY Clients/coraltime.clients.web/package*.json ./
RUN npm ci
COPY Clients/coraltime.clients.web/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y nodejs

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/CoralTime.Services.API/CoralTime/CoralTime.Services.API.csproj", "Services/CoralTime.Services.API/CoralTime/"]
COPY ["Services/CoralTime.Services.API/CoralTime.BL/CoralTime.BL.csproj", "Services/CoralTime.Services.API/CoralTime.BL/"]
COPY ["Services/CoralTime.Services.API/CoralTime.Common/CoralTime.Common.csproj", "Services/CoralTime.Services.API/CoralTime.Common/"]
COPY ["Services/CoralTime.Services.API/CoralTime.ViewModels/CoralTime.ViewModels.csproj", "Services/CoralTime.Services.API/CoralTime.ViewModels/"]
COPY ["Services/CoralTime.Services.API/CoralTime.DAL/CoralTime.DAL.csproj", "Services/CoralTime.Services.API/CoralTime.DAL/"]
COPY ["Services/CoralTime.Services.API/CoralTime.MySqlMigrations/CoralTime.MySqlMigrations.csproj", "Services/CoralTime.Services.API/CoralTime.MySqlMigrations/"]
RUN dotnet restore "Services/CoralTime.Services.API/CoralTime/CoralTime.Services.API.csproj"
COPY . .
RUN dotnet publish "Services/CoralTime.Services.API/CoralTime/CoralTime.Services.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish  --self-contained false  --no-restore  /p:UseAppHost=false 

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=frontend-builder /app/dist/browser /app/wwwroot
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "CoralTime.Services.API.dll"]
