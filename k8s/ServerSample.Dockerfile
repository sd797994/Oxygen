FROM mcr.microsoft.com/dotnet/core/runtime:2.2
WORKDIR /app
COPY ServerSample/. .
CMD ["dotnet","Server.Sample.dll"]