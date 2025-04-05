using TcpClientLib.Interfaces;
using TcpClientLib.Models;

namespace TcpClientLib.Example.Subscribers
{
    /// <summary>
    /// 控制台消息订阅器
    /// 将接收到的消息输出到控制台
    /// </summary>
    public class ConsoleMessageSubscriber : ITcpMessageSubscriber
    {
        public Task HandleMessageAsync(object sender, TcpMessage message)
        {
            Console.WriteLine($"[控制台订阅器] 收到消息: {message}");
            return Task.CompletedTask;
        }
    }
} 