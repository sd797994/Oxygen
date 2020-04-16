cd ../test/Server.Sample
dotnet publish -c release -o ../../k8s/ServerSample
cd ../Client.Sample
dotnet publish -c release -o ../../k8s/ClientSample
cd ../../ApiGateWay
dotnet publish -c release -o ../k8s/ApiGateWay
cd ../k8s
docker build -f E:/dotnet_project/Oxygen/k8s/ServerSample.Dockerfile . -t oxygen-service-sample:latest
docker build -f E:/dotnet_project/Oxygen/k8s/ClientSample.Dockerfile . -t roxygen-client-sample:latest
docker build -f E:/dotnet_project/Oxygen/k8s/ApiGateway.Dockerfile . -t oxygen-apigateway-sample:latest