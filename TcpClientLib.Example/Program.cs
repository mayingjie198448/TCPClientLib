using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TcpClientLib.Example.Observers;
using TcpClientLib.Example.Subscribers;
using TcpClientLib.Extensions;
using TcpClientLib.Interfaces;
using TcpClientLib.Models;
using TcpClientLib.Observers;

var builder = Host.CreateDefaultBuilder(args);

// 配置日志
builder.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

// 配置服务
builder.ConfigureServices((hostContext, services) =>
{
    // 1. 从配置文件添加TCP客户端服务
    services.AddTcpClientServiceFromConfiguration(hostContext.Configuration);

    // 2. 注册消息订阅器
    services.AddScoped<ITcpMessageSubscriber, ConsoleMessageSubscriber>();
    services.AddScoped<ITcpMessageSubscriber, FileMessageSubscriber>();

    // 3. 注册自定义观察者
    services.AddTcpMessageObserver<CustomTcpMessageObserver>();
});

var host = builder.Build();
await host.RunAsync();
