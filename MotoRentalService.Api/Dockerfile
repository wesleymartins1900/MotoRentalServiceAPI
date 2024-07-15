#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MotoRentalService.Api/MotoRentalService.Api.csproj", "MotoRentalService.Api/"]
COPY ["MotoRentalService.Application/MotoRentalService.Application.csproj", "MotoRentalService.Application/"]
COPY ["MotoRentalService.Domain/MotoRentalService.Domain.csproj", "MotoRentalService.Domain/"]
COPY ["MotoRentalService.CrossCutting/MotoRentalService.CrossCutting.csproj", "MotoRentalService.CrossCutting/"]
COPY ["MotoRentalService.Infrastructure/MotoRentalService.Infrastructure.csproj", "MotoRentalService.Infrastructure/"]
RUN dotnet restore "./MotoRentalService.Api/./MotoRentalService.Api.csproj"
COPY . .
WORKDIR "/src/MotoRentalService.Api"
RUN dotnet build "./MotoRentalService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MotoRentalService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MotoRentalService.Api.dll"]