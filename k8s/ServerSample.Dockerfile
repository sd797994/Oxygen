FROM microsoft/dotnet:2.2-runtime
WORKDIR /app
COPY ServerSample/. .
CMD ["dotnet","Server.Sample.dll"]