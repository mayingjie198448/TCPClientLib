using System.Reactive.Subjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TcpClientLib.Models;

namespace TcpClientLib.Observers
{
    /// <summary>
    /// TCP消息观察者基类
    /// 用户必须继承此类并实现抽象方法来处理消息
    /// </summary>
    public abstract class TcpMessageObserverBase : IObserver<TcpMessage>
    {
        private readonly Subject<TcpMessage> _messageSubject = new();
        private readonly ILogger<TcpMessageObserverBase> _logger;

        protected TcpMessageObserverBase(ILogger<TcpMessageObserverBase> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取消息流
        /// </summary>
        public IObservable<TcpMessage> MessageStream => _messageSubject;

        /// <summary>
        /// 处理接收到的消息
        /// </summary>
        /// <param name="message">消息</param>
        protected abstract void OnMessageReceived(TcpMessage message);

        /// <summary>
        /// 处理错误
        /// </summary>
        /// <param name="error">错误</param>
        protected abstract void OnError(Exception error);

        /// <summary>
        /// 处理完成
        /// </summary>
        protected abstract void OnCompleted();

        /// <summary>
        /// 处理下一个消息
        /// </summary>
        /// <param name="value">消息</param>
        public void OnNext(TcpMessage value)
        {
            OnMessageReceived(value);
            _messageSubject.OnNext(value);
        }

        void IObserver<TcpMessage>.OnError(Exception error)
        {
            OnError(error);
            _messageSubject.OnError(error);
        }

        void IObserver<TcpMessage>.OnCompleted()
        {
            OnCompleted();
            _messageSubject.OnCompleted();
        }
    }

    /// <summary>
    /// 服务注册扩展方法
    /// </summary>
    public static class TcpMessageObserverExtensions
    {
        /// <summary>
        /// 添加TCP消息观察者
        /// </summary>
        /// <typeparam name="TObserver">观察者类型</typeparam>
        public static IServiceCollection AddTcpMessageObserver<TObserver>(
            this IServiceCollection services)
            where TObserver : TcpMessageObserverBase
        {
            // 注册具体观察者类型
            services.AddSingleton<TObserver>();
            // 同时注册为基类类型，这样可以通过 IEnumerable<TcpMessageObserverBase> 获取所有观察者
            services.AddSingleton<TcpMessageObserverBase>(sp => sp.GetRequiredService<TObserver>());
            return services;
        }
    }
} 
