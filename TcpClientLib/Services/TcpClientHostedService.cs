using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TcpClientLib.Interfaces;
using TcpClientLib.Models;
using TcpClientLib.Observers;
using TcpClientLib.Options;

namespace TcpClientLib.Services
{
    /// <summary>
    /// TCP客户端托管服务
    /// 负责管理TCP客户端服务的生命周期
    /// </summary>
    public class TcpClientHostedService : IHostedService
    {
        private readonly ITcpClientService _tcpClientService;
        private readonly ILogger<TcpClientHostedService> _logger;
        private readonly TcpClientOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<TcpMessageObserverBase> _observers;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tcpClientService">TCP客户端服务实例</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="options">TCP客户端配置选项</param>
        /// <param name="serviceProvider">服务提供器</param>
        /// <param name="observers">观察者列表</param>
        public TcpClientHostedService(
            ITcpClientService tcpClientService,
            ILogger<TcpClientHostedService> logger,
            IOptions<TcpClientOptions> options,
            IServiceProvider serviceProvider,
            IEnumerable<TcpMessageObserverBase> observers)
        {
            _tcpClientService = tcpClientService;
            _logger = logger;
            _options = options.Value;
            _serviceProvider = serviceProvider;
            _observers = observers;
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _tcpClientService.OnMessageReceived += async (sender, message) =>
                {
                    // 1. 使用配置的事件处理器
                    if (_options.OnMessageReceivedHandler != null)
                    {
                        _options.OnMessageReceivedHandler(sender, message);
                    }

                    // 2. 使用注册的订阅器
                    using var scope = _serviceProvider.CreateScope();
                    var subscribers = scope.ServiceProvider.GetServices<ITcpMessageSubscriber>();
                    foreach (var subscriber in subscribers)
                    {
                        try
                        {
                            await subscriber.HandleMessageAsync(sender, message);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "消息订阅器处理消息时发生错误");
                        }
                    }

                    // 3. 使用观察者
                    foreach (var observer in _observers)
                    {
                        try
                        {
                            observer.OnNext(message);
                        } catch (Exception ex)
                        {
                            ((IObserver<TcpMessage>) observer).OnError(ex);
                        } finally
                        {
                            ((IObserver<TcpMessage>) observer).OnCompleted();
                        }
                    }

                    // 4. 默认的日志记录
                    if (_options.OnMessageReceivedHandler == null && !subscribers.Any() && !_observers.Any())
                    {
                        _logger.LogInformation("收到数据: {Message}", message);
                    }
                };

                await _tcpClientService.StartAsync();
                _logger.LogInformation("TCP客户端服务已启动");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动TCP客户端服务时发生错误");
                throw;
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _tcpClientService.StopAsync();
                _logger.LogInformation("TCP客户端服务已停止");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止TCP客户端服务时发生错误");
                throw;
            }
        }
    }
} 
