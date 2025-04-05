namespace TcpClientLib.Models
{
    /// <summary>
    /// TCP消息类，用于封装从服务器接收到的数据
    /// </summary>
    public class TcpMessage
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// 接收时间
        /// </summary>
        public DateTime ReceiveTime { get; }

        /// <summary>
        /// 消息长度
        /// </summary>
        public int Length => Content.Length;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="receiveTime">接收时间，默认为当前时间</param>
        public TcpMessage(string content, DateTime? receiveTime = null)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            ReceiveTime = receiveTime ?? DateTime.Now;
        }

        /// <summary>
        /// 获取消息的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"[{ReceiveTime:yyyy-MM-dd HH:mm:ss}] {Content}";
        }

        /// <summary>
        /// 检查消息是否为空
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Content);
    }
} 