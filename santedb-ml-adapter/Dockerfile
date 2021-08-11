#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["santedb-ml-adapter/santedb-ml-adapter.csproj", "santedb-ml-adapter/"]
RUN dotnet restore "santedb-ml-adapter/santedb-ml-adapter.csproj"
COPY . .
WORKDIR "/src/santedb-ml-adapter"
RUN dotnet build "santedb-ml-adapter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "santedb-ml-adapter.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "santedb-ml-adapter.dll"]