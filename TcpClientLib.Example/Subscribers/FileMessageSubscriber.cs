using TcpClientLib.Interfaces;
using TcpClientLib.Models;

namespace TCPClientTest.Subscribers
{
    /// <summary>
    /// 文件消息订阅器
    /// 将接收到的消息写入文件
    /// </summary>
    public class FileMessageSubscriber : ITcpMessageSubscriber
    {
        private readonly string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "messages.log");

        public async Task HandleMessageAsync(object sender, TcpMessage message)
        {
            await File.AppendAllTextAsync(_logPath, $"[文件订阅器] {message}{Environment.NewLine}");
        }
    }
} 
