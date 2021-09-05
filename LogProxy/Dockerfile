FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY . .
EXPOSE 8080
HEALTHCHECK CMD curl --fail http://localhost:8080/health || exit
ENTRYPOINT ["dotnet", "LogProxy.dll"]