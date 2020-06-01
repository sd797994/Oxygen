FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY ClientSample/. .
CMD ["dotnet","Server.ClientSample.dll"]