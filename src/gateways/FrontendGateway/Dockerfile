#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/gateways/FrontendGateway/FrontendGateway.csproj", "src/gateways/FrontendGateway/"]
COPY ["src/middleware/Middleware/Middleware.csproj", "src/middleware/Middleware/"]
RUN dotnet restore "./src/gateways/FrontendGateway/FrontendGateway.csproj"
COPY . .
WORKDIR "/src/src/gateways/FrontendGateway"
RUN dotnet build "./FrontendGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FrontendGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FrontendGateway.dll"]