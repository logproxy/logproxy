FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["LogProxy/LogProxy.csproj", "LogProxy/"]
RUN dotnet restore "LogProxy/LogProxy.csproj"
COPY . .
WORKDIR "/src/LogProxy"
RUN dotnet build "LogProxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LogProxy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogProxy.dll"]
