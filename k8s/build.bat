cd ../test/Server.Sample
dotnet publish -c release -o ../../k8s/ServerSample
cd ../../test/Client.Sample
dotnet publish -c release -o ../../k8s/ClientSample
cd ../../ApiGateWay
dotnet publish -c release -o ../k8s/ApiGateWay
cd ../k8s
docker build -f E:/dotnet_project/Oxygen/k8s/ServerSample.Dockerfile . -t registry.cn-chengdu.aliyuncs.com/gmmy/oxygen-service-sample:latest
docker build -f E:/dotnet_project/Oxygen/k8s/ClientSample.Dockerfile . -t registry.cn-chengdu.aliyuncs.com/gmmy/oxygen-client-sample:latest
docker build -f E:/dotnet_project/Oxygen/k8s/ApiGateway.Dockerfile . -t registry.cn-chengdu.aliyuncs.com/gmmy/oxygen-apigateway-sample:latest
docker push registry.cn-chengdu.aliyuncs.com/gmmy/oxygen-service-sample:latest
docker push registry.cn-chengdu.aliyuncs.com/gmmy/oxygen-client-sample:latest
docker push registry.cn-chengdu.aliyuncs.com/gmmy/oxygen-apigateway-sample:latest
docker rm 1registry.cn-chengdu.aliyuncs.com/gmmy/oxygen-service-sample:latest registry.cn-chengdu.aliyuncs.com/gmmy/oxygen-client-sample:latest registry.cn-chengdu.aliyuncs.com/gmmy/oxygen-apigateway-sample:latest