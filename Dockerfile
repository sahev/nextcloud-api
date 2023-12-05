#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
#EXPOSE 5121
#EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NextCloud.Api/NextCloud.Api.csproj", "NextCloud.Api/"]
COPY ["NextCloud.Core/NextCloud.Core.csproj", "NextCloud.Core/"]
RUN dotnet restore "NextCloud.Api/NextCloud.Api.csproj"
RUN dotnet restore "NextCloud.Core/NextCloud.Core.csproj"
COPY . .
WORKDIR "/src/NextCloud.Api"
RUN dotnet build "NextCloud.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NextCloud.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

ARG ASPNETCORE_ENVIRONMENT
ENV env_name $ASPNETCORE_ENVIRONMENT

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT dotnet Api.dll --environment=$env_name urls=http://0.0.0.0:80