FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY . .
EXPOSE 8080
ENTRYPOINT ["dotnet", "LogProxy.dll"]