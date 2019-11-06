FROM mcr.microsoft.com/dotnet/core/runtime:2.2
WORKDIR /app
COPY ClientSample/. .
CMD ["dotnet","Client.Sample.dll"]