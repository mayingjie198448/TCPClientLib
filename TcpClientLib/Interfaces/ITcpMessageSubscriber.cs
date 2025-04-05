using TcpClientLib.Models;

namespace TcpClientLib.Interfaces
{
    /// <summary>
    /// TCP消息订阅器接口
    /// </summary>
    public interface ITcpMessageSubscriber
    {
        /// <summary>
        /// 处理接收到的消息
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="message">消息</param>
        Task HandleMessageAsync(object sender, TcpMessage message);
    }
} 