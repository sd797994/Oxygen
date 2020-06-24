cd ..\src\Oxygen
dotnet build -c release -o ..\..\nugetpack\lib
cd ..\..\nugetpack\lib\
del /a /f /s /q  "*.pdb" "*.json"
copy Oxygen.CsharpClientAgent.dll ..\lib2
cd ..\
.\nugetpack.exe
nuget pack Oxygen.nuspec
nuget pack Oxygen.CsharpClientAgent.nuspec