using TcpClientLib.Models;

namespace TcpClientLib.Interfaces
{
    /// <summary>
    /// TCP客户端服务接口
    /// 定义了TCP客户端的基本功能，包括连接管理、消息收发等
    /// </summary>
    public interface ITcpClientService : IDisposable
    {
        /// <summary>
        /// 启动TCP客户端服务
        /// </summary>
        /// <returns>表示异步操作的任务</returns>
        /// <exception cref="InvalidOperationException">当服务已经启动时抛出</exception>
        Task StartAsync();

        /// <summary>
        /// 停止TCP客户端服务
        /// </summary>
        /// <returns>表示异步操作的任务</returns>
        Task StopAsync();

        /// <summary>
        /// 发送消息到服务器
        /// </summary>
        /// <param name="message">要发送的消息内容</param>
        /// <returns>表示异步操作的任务</returns>
        /// <exception cref="InvalidOperationException">当客户端未连接时抛出</exception>
        Task SendMessageAsync(string message);

        /// <summary>
        /// 当收到来自服务器的消息时触发的事件
        /// </summary>
        event EventHandler<TcpMessage> OnMessageReceived;
    }
} 