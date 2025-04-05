using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using TcpClientLib.Interfaces;
using TcpClientLib.Options;
using TcpClientLib.Services;

namespace TcpClientLib.Extensions
{
    /// <summary>
    /// 服务集合扩展方法
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加TCP客户端服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configureOptions">配置选项的委托</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddTcpClientService(
            this IServiceCollection services,
            Action<TcpClientOptions>? configureOptions = null)
        {
            // 配置选项
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            // 注册服务
            services.AddSingleton<ITcpClientService>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<TcpClientOptions>>();
                return new TcpClientService(options);
            });

            // 注册托管服务
            services.AddHostedService<TcpClientHostedService>();

            return services;
        }
        
        /// <summary>
        /// 添加TCP客户端服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="serverIp">服务器IP地址</param>
        /// <param name="serverPort">服务器端口号</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddTcpClientService(
            this IServiceCollection services,
            string serverIp,
            int serverPort)
        {
            return services.AddTcpClientService(options =>
            {
                options.ServerIp = serverIp;
                options.ServerPort = serverPort;
            });
        }

        /// <summary>
        /// 从配置文件添加TCP客户端服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <param name="sectionName">配置节点名称，默认为"TcpClient"</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddTcpClientServiceFromConfiguration(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = "TcpClient")
        {
            // 绑定配置
            services.Configure<TcpClientOptions>(configuration.GetSection(sectionName));

            // 注册TCP客户端服务
            services.AddSingleton<ITcpClientService>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<TcpClientOptions>>();
                return new TcpClientService(options);
            });

            // 注册托管服务
            services.AddHostedService<TcpClientHostedService>();

            return services;
        }

        /// <summary>
        /// 从配置添加TCP客户端服务
        /// </summary>
        public static IServiceCollection AddTcpClientServiceFromConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<TcpClientOptions>(configuration.GetSection("TcpClient"));
            return services.AddTcpClientService();
        }
    }
} 
