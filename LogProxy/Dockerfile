FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
COPY . .
EXPOSE 8080
ENTRYPOINT ["dotnet", "LogProxy.dll"]