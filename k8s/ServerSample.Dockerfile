FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
COPY ServerSample/. .
CMD ["dotnet","Server.Sample.dll"]