#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Academatica.Api.Leaderboards/Academatica.Api.Leaderboards.csproj", "Academatica.Api.Leaderboards/"]
RUN dotnet restore "Academatica.Api.Leaderboards/Academatica.Api.Leaderboards.csproj"
COPY . .
WORKDIR "/src/Academatica.Api.Leaderboards"
RUN dotnet build "Academatica.Api.Leaderboards.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Academatica.Api.Leaderboards.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Academatica.Api.Leaderboards.dll"]