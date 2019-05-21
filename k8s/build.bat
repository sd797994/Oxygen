cd ../test/Server.Sample
dotnet publish -c release -o ../../k8s/ServerSample
cd ../../ApiGateWay
dotnet publish -c release -o ../k8s/ApiGateWay
cd ../k8s
docker build -f F:/git/Oxygen/k8s/ServerSample.Dockerfile . -t registry.cn-shenzhen.aliyuncs.com/gmmy/k8sapisample:oxysvrsmp
docker build -f F:/git/Oxygen/k8s/ApiGateway.Dockerfile . -t registry.cn-shenzhen.aliyuncs.com/gmmy/k8sapisample:oxyapiway
docker push registry.cn-shenzhen.aliyuncs.com/gmmy/k8sapisample:oxysvrsmp
docker push registry.cn-shenzhen.aliyuncs.com/gmmy/k8sapisample:oxyapiway