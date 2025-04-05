# TcpClientLib

一个基于 .NET 8 的 TCP 客户端类库，专为扫码枪等设备设计。

## 快速开始

### 1. 安装

```bash
dotnet add package TcpClientLib
```

### 2. 配置

在 `appsettings.json` 中配置扫码枪连接信息：

```json
{
  "TcpClient": {
    "ServerIp": "127.0.0.1",    // 扫码枪IP地址
    "ServerPort": 5000,         // 扫码枪端口
    "ReconnectInterval": 5000,  // 重连间隔（毫秒）
    "BufferSize": 1024,         // 缓冲区大小
    "MessageTimeoutMs": 100     // 消息超时时间
  }
}
```

### 3. 使用方式

#### 方式一：简单使用（适合快速测试）

```csharp
var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
    // 从配置文件添加TCP客户端服务
    services.AddTcpClientServiceFromConfiguration(hostContext.Configuration);
});

var host = builder.Build();
await host.RunAsync();
```

#### 方式二：使用观察者模式（推荐）

1. 创建扫码观察者：

```csharp
public class ScannerObserver : TcpMessageObserverBase
{
    private readonly ILogger<ScannerObserver> _logger;

    public ScannerObserver(ILogger<ScannerObserver> logger)
        : base(logger)
    {
        _logger = logger;
    }

    protected override void OnMessageReceived(TcpMessage message)
    {
        var barcode = message.Content.Trim();
        _logger.LogInformation("扫描到条码: {Barcode}", barcode);
        
        // 这里添加你的业务逻辑
        // 例如：更新库存、记录扫描历史等
    }

    protected override void OnError(Exception error)
    {
        _logger.LogError(error, "处理扫码数据时发生错误");
    }

    protected override void OnCompleted()
    {
        _logger.LogInformation("扫码完成");
    }
}
```

2. 注册观察者：

```csharp
services.AddTcpMessageObserver<ScannerObserver>();
```

#### 方式三：使用消息订阅器（适合多处理场景）

1. 创建订阅器：

```csharp
public class ScannerSubscriber : ITcpMessageSubscriber
{
    private readonly ILogger<ScannerSubscriber> _logger;

    public ScannerSubscriber(ILogger<ScannerSubscriber> logger)
    {
        _logger = logger;
    }

    public async Task HandleMessageAsync(object sender, TcpMessage message)
    {
        var barcode = message.Content.Trim();
        _logger.LogInformation("处理条码: {Barcode}", barcode);
        
        // 这里添加你的业务逻辑
    }
}
```

2. 注册订阅器：

```csharp
services.AddScoped<ITcpMessageSubscriber, ScannerSubscriber>();
```

## 完整示例

```csharp
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
    // 1. 添加TCP客户端服务
    services.AddTcpClientServiceFromConfiguration(hostContext.Configuration);

    // 2. 注册观察者
    services.AddTcpMessageObserver<ScannerObserver>();

    // 3. 注册订阅器
    services.AddScoped<ITcpMessageSubscriber, ScannerSubscriber>();
});

var host = builder.Build();
await host.RunAsync();
```

## 配置说明

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| ServerIp | 扫码枪IP地址 | 127.0.0.1 |
| ServerPort | 扫码枪端口 | 5000 |
| ReconnectInterval | 重连间隔（毫秒） | 5000 |
| BufferSize | 缓冲区大小 | 1024 |
| MessageTimeoutMs | 消息超时时间 | 100 |

## 注意事项

1. 确保扫码枪的IP和端口配置正确
2. 建议设置合适的重连间隔，避免频繁重连
3. 观察者模式适合处理扫码数据
4. 订阅器模式适合需要多个处理逻辑的场景
5. 建议添加适当的错误处理逻辑

## 许可证

MIT 