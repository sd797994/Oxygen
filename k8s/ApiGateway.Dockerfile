FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY ApiGateWay/. .
CMD ["dotnet","ApiGateWay.dll"]