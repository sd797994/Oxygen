FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY ApiGateWay/. .
CMD ["dotnet","ApiGateWay.dll"]