# Oxygen
Oxygen 是一款基于.netcore3.1 的针对k8s平台的分布式服务框架，这里可以下载[简易案例][1]
## 系统要求

* window10 / centos7.5 +

* docker for windows 19.03 + / linux docker-ce

* kubernetes 1.14.8 + (docker for windows) /linux kubernetes 
* dotnetcore3.1 + vs2019 + nuget
## 特色
* 基于dotnetty实现的高性能远程过程调用代理(RPC)
* 基于Messagepack实现的类型序列化/反序列化
* 采用k8s自带的dns服务实现服务注册发现
## 安装
* 创建两个默认的控制台程序，并通过nuget安装oxygen:

```bash
Install-Package Oxygen -Version 0.1.4
```

* 在根目录创建oxygen.json并注入oxygen需要的端口配置(用于服务间通讯)

```bash
{
  "Oxygen": {
    "ServerPort": 80
  }
}
```
* 在Program.cs引入下面的通用主机代码
```bash
客户端(多用于网关等不需要提供rpc服务的节点)：
 private static IConfiguration Configuration { get; set; }
        static async Task Main(string[] args)
        {
            await CreateDefaultHost(args).Build().RunAsync();
        }
        static IHostBuilder CreateDefaultHost(string[] args) => new HostBuilder()
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                //获取oxygen配置节
                config.AddJsonFile("oxygen.json");
                Configuration = config.Build();
            })
            .ConfigureContainer<ContainerBuilder>(builder =>
            {
                //注入oxygen依赖
                builder.RegisterOxygen();
            })
            .ConfigureServices(services =>
            {
                //注册oxygen配置节
                services.ConfigureOxygen(Configuration);
                services.AddLogging(configure =>
                {
                    configure.AddConsole();
                });
                services.AddHttpClient();
            })
            .UseServiceProviderFactory(new AutofacServiceProviderFactory());
服务端(提供rpc服务)：
 static IHostBuilder CreateDefaultHost(string[] args) => new HostBuilder()
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                //获取oxygen配置节
                config.AddJsonFile("oxygen.json");
                Configuration = config.Build();
            })
            .ConfigureContainer<ContainerBuilder>(builder =>
            {
                //注入oxygen依赖
                builder.RegisterOxygen();
                //注入本地业务依赖
                builder.RegisterType<UserLoginUseCase>().As<IUserLoginUseCase>().InstancePerDependency();
            })
            //注册成为oxygen服务节点
            .UseOxygenService((context, services) => {
                //注册oxygen配置
                services.ConfigureOxygen(Configuration);
                services.AddLogging(configure =>
                {
                    configure.AddConsole();
                });
                services.AddAutofac();
            })
            .UseServiceProviderFactory(new AutofacServiceProviderFactory());
```

* 创建一个接口项目，Oxygen.CsharpClientAgent
```bash
Install-Package Oxygen.CsharpClientAgent -Version 0.0.2
```
* 定义对应的服务接口，并打上暴露rpc服务的标记
```bash
[RemoteService("myserver")]//暴露到k8s的服务名
interface IXXXServices
Task<ResponseDto> GetResult(RequestModel model);
```
* 创建一个服务项目，引用上面的接口项目并实现它
```bash
public class XXXServices : IXXXServices
public asyn Task<ResponseDto> GetResult(RequestModel model)
{
      await Task.Dealy(0); //推荐使用async/await异步编程
      return new ResponseDto(){ Message = $"hello world,{model.Name}" };
}
```
* 客户端只需要引入接口项目，服务端则需要引入接口项目以及对应的实现，在客户端项目里使用如下方式即可远程调用服务

```bash
private readonly IServerProxyFactory _proxyFactory;
        public 构造函数(IServerProxyFactory proxyFactory)
        {
            _proxyFactory = proxyFactory;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
                var xxxserver = _proxyFactory.CreateProxy<IXXXServices>();//直接通过引用接口类型创建代理
                var result = await xxxserver.GetResult(new RequestModel() { Name = "admin" });
                //var remoteProxy = _proxyFactory.CreateProxy("/api/myserver/XXXServices/GetResult"); //通过url的方式创建代理
                //var result = await remoteProxy.SendAsync(new { Name = "admin" });
        }
```
* 项目整体服务注册基于k8s平台，所以需要提前准备k8s环境，可参考[简易案例][1]
## License

MIT

[1]: https://www.github.com/sd797994/oxygen "Oxygen"
[2]: https://www.jianshu.com/p/c726ed03562a "这里"
[3]: https://www.github.com/dotnetcore/cap "CAP"
[1]: https://github.com/sd797994/Oxygen-EshopSample "简易案例"