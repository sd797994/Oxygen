FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY ServerSample/. .
CMD ["dotnet","Server.Sample.dll"]