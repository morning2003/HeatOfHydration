#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
RUN apt-get update; apt-get install libfontconfig1 -y

WORKDIR /app
EXPOSE 5567  #这里端口要与Program.cs一样

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["VolPro.WebApi/VolPro.WebApi.csproj", "VolPro.WebApi/"]
COPY ["VolPro.System/VolPro.Sys.csproj", "VolPro.System/"]
COPY ["VolPro.Core/VolPro.Core.csproj", "VolPro.Core/"]
COPY ["VolPro.Entity/VolPro.Entity.csproj", "VolPro.Entity/"]
COPY ["VolPro.Order/VolPro.Order.csproj", "VolPro.Order/"]
COPY ["VolPro.AppManager/VolPro.AppManager.csproj", "VolPro.AppManager/"]
COPY ["VolPro.Builder/VolPro.Builder.csproj", "VolPro.Builder/"]
RUN dotnet restore "VolPro.WebApi/VolPro.WebApi.csproj"
COPY . .
WORKDIR "/src/VolPro.WebApi"
RUN dotnet build "VolPro.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VolPro.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VolPro.WebApi.dll"]