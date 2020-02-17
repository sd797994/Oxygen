FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
COPY ClientSample/. .
CMD ["dotnet","Client.Sample.dll"]