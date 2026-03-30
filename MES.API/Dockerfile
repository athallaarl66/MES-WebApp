FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MES.API/MES.API.csproj", "MES.API/"]
COPY ["MES.Core/MES.Core.csproj", "MES.Core/"]
COPY ["MES.Infrastructure/MES.Infrastructure.csproj", "MES.Infrastructure/"]

RUN dotnet restore "MES.API/MES.API.csproj"

COPY . .

RUN dotnet publish "MES.API/MES.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MES.API.dll"]