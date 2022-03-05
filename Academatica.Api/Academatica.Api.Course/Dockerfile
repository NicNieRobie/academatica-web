#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["../Academatica.Api.Course/Academatica.Api.Course.csproj", "Academatica.Api.Course/"]
COPY ["../Academatica.Api.Common/Academatica.Api.Common.csproj", "Academatica.Api.Common/"]
RUN dotnet restore "Academatica.Api.Course/Academatica.Api.Course.csproj"
COPY . .
WORKDIR "/src/Academatica.Api.Course"
RUN dotnet build "Academatica.Api.Course.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Academatica.Api.Course.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Academatica.Api.Course.dll"]