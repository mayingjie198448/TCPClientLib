using System.Net.Sockets;
using System.Text;
using System.Timers;
using Microsoft.Extensions.Options;
using TcpClientLib.Interfaces;
using TcpClientLib.Models;
using TcpClientLib.Options;

namespace TcpClientLib.Services
{
    /// <summary>
    /// TCP客户端服务实现类
    /// 实现了与TCP服务器的连接、数据收发等功能
    /// 使用异步操作和事件机制处理数据接收
    /// </summary>
    public class TcpClientService : ITcpClientService
    {
        private readonly TcpClientOptions _options;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private readonly byte[] _buffer;
        private readonly StringBuilder _messageBuffer;
        private DateTime _lastReceiveTime;
        private DateTime _lastOutputTime;
        private System.Timers.Timer? _timer;
        private readonly SemaphoreSlim _bufferLock = new SemaphoreSlim(1, 1);
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _isDisposed;

        /// <summary>
        /// 当收到来自服务器的消息时触发的事件
        /// </summary>
        public event EventHandler<TcpMessage>? OnMessageReceived;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options">配置选项</param>
        public TcpClientService(IOptions<TcpClientOptions> options)
        {
            _options = options.Value;
            _buffer = new byte[_options.MaxMessageLength];
            _messageBuffer = new StringBuilder();
            _lastReceiveTime = DateTime.Now;
            _lastOutputTime = DateTime.Now;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 启动TCP客户端服务
        /// </summary>
        public async Task StartAsync()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(TcpClientService));

            try
            {
                await ConnectAsync();
                InitializeTimer();
                _ = Task.Run(() => ReceiveDataAsync(_cancellationTokenSource.Token));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to start TCP client service", ex);
            }
        }

        /// <summary>
        /// 停止TCP客户端服务
        /// </summary>
        public async Task StopAsync()
        {
            if (_isDisposed)
                return;

            _cancellationTokenSource.Cancel();
            await CleanupAsync();
        }

        /// <summary>
        /// 发送消息到服务器
        /// </summary>
        public async Task SendMessageAsync(string message)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(TcpClientService));

            if (_stream == null || _client == null || !_client.Connected)
            {
                throw new InvalidOperationException("Client is not connected");
            }

            var data = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(data, 0, data.Length);
        }

        private async Task ConnectAsync()
        {
            _client = new TcpClient();
            await _client.ConnectAsync(_options.ServerIp, _options.ServerPort);
            _stream = _client.GetStream();
        }

        private void InitializeTimer()
        {
            _timer = new System.Timers.Timer(_options.MessageTimeoutMs);
            _timer.Elapsed += async (sender, e) =>
            {
                await _bufferLock.WaitAsync();
                try
                {
                    if (_messageBuffer.Length > 0 && 
                        (DateTime.Now - _lastReceiveTime).TotalMilliseconds > _options.MessageTimeoutMs)
                    {
                        var message = new TcpMessage(_messageBuffer.ToString(), _lastReceiveTime);
                        _messageBuffer.Clear();
                        _lastOutputTime = DateTime.Now;
                        OnMessageReceived?.Invoke(this, message);
                    }
                }
                finally
                {
                    _bufferLock.Release();
                }
            };
            _timer.Start();
        }

        private async Task ReceiveDataAsync(CancellationToken cancellationToken)
        {
            if (_stream == null)
                throw new InvalidOperationException("Stream is not initialized");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var bytesRead = await _stream.ReadAsync(_buffer, 0, _buffer.Length, cancellationToken);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    var newData = Encoding.UTF8.GetString(_buffer, 0, bytesRead);
                    _lastReceiveTime = DateTime.Now;

                    await _bufferLock.WaitAsync(cancellationToken);
                    try
                    {
                        _messageBuffer.Append(newData);

                        if (_messageBuffer.Length > _options.MaxMessageLength)
                        {
                            _messageBuffer.Clear();
                        }
                    }
                    finally
                    {
                        _bufferLock.Release();
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        private async Task CleanupAsync()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }

            if (_stream != null)
            {
                await _stream.DisposeAsync();
                _stream = null;
            }

            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _timer?.Dispose();
                _stream?.Dispose();
                _client?.Dispose();
                _bufferLock.Dispose();
            }

            _isDisposed = true;
        }
    }
} 