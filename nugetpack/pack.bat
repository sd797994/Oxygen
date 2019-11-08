cd ..\src\Oxygen
dotnet build -c release -o ..\..\nugetpack\lib
cd ..\..\nugetpack\lib\
del /a /f /s /q  "*.pdb" "*.json"
cd ..\
nuget pack Oxygen.nuspec