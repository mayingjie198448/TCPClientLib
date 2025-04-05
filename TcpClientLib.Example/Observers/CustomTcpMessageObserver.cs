using TcpClientLib.Models;
using TcpClientLib.Observers;

namespace TcpClientLib.Example.Observers
{
    /// <summary>
    /// 自定义TCP消息观察者
    /// 必须实现基类的抽象方法来处理消息
    /// </summary>
    public class CustomTcpMessageObserver(ILogger<CustomTcpMessageObserver> logger)
        : TcpMessageObserverBase(logger)
    {
        protected override void OnMessageReceived(TcpMessage message)
        {
            // 自定义消息处理逻辑
            Console.WriteLine($"[自定义观察者] 收到消息: {message}");
        }

        protected override void OnError(Exception error)
        {
            // 自定义错误处理逻辑
            Console.WriteLine($"[自定义观察者] 发生错误: {error.Message}");
        }

        protected override void OnCompleted()
        {
            // 自定义完成处理逻辑
            Console.WriteLine("[自定义观察者] 完成");
        }
    }
} 
