FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MES.Web/MES.Web.csproj", "MES.Web/"]
COPY ["MES.Core/MES.Core.csproj", "MES.Core/"]

RUN dotnet restore "MES.Web/MES.Web.csproj"

COPY . .

RUN dotnet publish "MES.Web/MES.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MES.Web.dll"]