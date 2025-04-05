using TcpClientLib.Models;

namespace TcpClientLib.Options
{
    /// <summary>
    /// TCP客户端服务配置选项
    /// </summary>
    public class TcpClientOptions
    {
        /// <summary>
        /// 服务器IP地址
        /// </summary>
        public string ServerIp { get; set; } 

        /// <summary>
        /// 服务器端口号
        /// </summary>
        public int ServerPort { get; set; } = 4001;

        /// <summary>
        /// 消息超时时间（毫秒），超过此时间没有新数据则认为消息完整
        /// </summary>
        public int MessageTimeoutMs { get; set; } = 100;

        /// <summary>
        /// 消息最大长度限制
        /// </summary>
        public int MaxMessageLength { get; set; } = 1024;

        /// <summary>
        /// 消息接收事件处理器
        /// </summary>
        public Action<object, TcpMessage>? OnMessageReceivedHandler { get; set; }
    }
} 
