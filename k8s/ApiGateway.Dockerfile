FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY ApiGateWay/. .
CMD ["dotnet","ApiGateWay.dll"]