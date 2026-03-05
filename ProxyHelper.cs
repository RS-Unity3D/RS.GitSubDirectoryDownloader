using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace RS.GitSubDirectoryDownloader
{
    /// <summary>
    /// 代理帮助类 - 支持代理和加速服务
    /// 
    /// 重要区分：
    /// - 加速服务(GH-Proxy): https://gh-proxy.com/ 提供的GitHub文件下载加速，通过URL重定向实现
    /// - 代理(Proxy): 传统网络代理，如HTTP/SOCKS5代理
    /// </summary>
    public static class ProxyHelper
    {
        #region GH-Proxy 加速服务
        
        /// <summary>
        /// GH-Proxy 支持的加速节点 - 来自 https://gh-proxy.com/
        /// 动态获取后会更新此列表
        /// </summary>
        public static Dictionary<string, GhProxyNodeInfo> GhProxyNodes { get; private set; } = new()
        {
            { "cloudflare", new GhProxyNodeInfo { Name = "cloudflare", Url = "https://gh-proxy.org/", Description = "主站，全球加速！", IsAvailable = true } },
            { "cloudflare-v6", new GhProxyNodeInfo { Name = "cloudflare-v6", Url = "https://v6.gh-proxy.org/", Description = "国内优选和V6支持", IsAvailable = true } },
            { "hongkong", new GhProxyNodeInfo { Name = "hongkong", Url = "https://hk.gh-proxy.org/", Description = "国内线路优化", IsAvailable = true } },
            { "fastly", new GhProxyNodeInfo { Name = "fastly", Url = "https://fastly.gh-proxy.org/", Description = "Fastly CDN", IsAvailable = true } },
            { "edgeone", new GhProxyNodeInfo { Name = "edgeone", Url = "https://edgeone.gh-proxy.org/", Description = "EdgeOne CDN", IsAvailable = true } }
        };

        /// <summary>
        /// 默认加速节点
        /// </summary>
        public const string DefaultGhProxyNode = "cloudflare";

        /// <summary>
        /// 上次动态获取节点的时间
        /// </summary>
        private static DateTime _lastFetchTime = DateTime.MinValue;

        /// <summary>
        /// 节点缓存有效期（分钟）
        /// </summary>
        private const int CacheDurationMinutes = 30;

        /// <summary>
        /// 动态获取GH-Proxy可用节点
        /// 从 https://gh-proxy.com/ 页面解析可用节点
        /// </summary>
        public static async Task FetchGhProxyNodesAsync(Action<string>? logCallback = null)
        {
            if ((DateTime.Now - _lastFetchTime).TotalMinutes < CacheDurationMinutes)
            {
                logCallback?.Invoke($"使用缓存的GH-Proxy节点列表（{_lastFetchTime:HH:mm:ss}获取）");
                return;
            }

            try
            {
                logCallback?.Invoke("正在从 gh-proxy.com 获取可用节点...");

                using var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                using var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("Mozilla/5.0", ""));

                var html = await client.GetStringAsync("https://gh-proxy.com/");
                
                var newNodes = ParseGhProxyNodesFromHtml(html);
                
                if (newNodes.Count > 0)
                {
                    foreach (var node in newNodes)
                    {
                        if (GhProxyNodes.ContainsKey(node.Key))
                        {
                            GhProxyNodes[node.Key].Url = node.Value.Url;
                            GhProxyNodes[node.Key].Description = node.Value.Description;
                            GhProxyNodes[node.Key].IsAvailable = true;
                        }
                        else
                        {
                            GhProxyNodes[node.Key] = node.Value;
                        }
                    }
                    
                    _lastFetchTime = DateTime.Now;
                    logCallback?.Invoke($"成功获取 {newNodes.Count} 个GH-Proxy节点");
                }
                else
                {
                    logCallback?.Invoke("未能从页面解析到节点，使用默认节点列表");
                }
            }
            catch (Exception ex)
            {
                logCallback?.Invoke($"获取GH-Proxy节点失败: {ex.Message}，使用默认节点列表");
            }
        }

        /// <summary>
        /// 从HTML页面解析GH-Proxy节点
        /// </summary>
        private static Dictionary<string, GhProxyNodeInfo> ParseGhProxyNodesFromHtml(string html)
        {
            var nodes = new Dictionary<string, GhProxyNodeInfo>();

            var urlPattern = @"https?://([a-zA-Z0-9\-]+\.)?gh-proxy\.org/?";
            var matches = Regex.Matches(html, urlPattern, RegexOptions.IgnoreCase);

            var seenUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Match match in matches)
            {
                var url = match.Value.TrimEnd('/');
                if (seenUrls.Contains(url)) continue;
                seenUrls.Add(url);

                var host = new Uri(url).Host;
                string name;
                string description = "";

                if (host.StartsWith("v6."))
                {
                    name = "cloudflare-v6";
                    description = "国内优选和V6支持";
                }
                else if (host.StartsWith("hk."))
                {
                    name = "hongkong";
                    description = "国内线路优化";
                }
                else if (host.StartsWith("fastly."))
                {
                    name = "fastly";
                    description = "Fastly CDN";
                }
                else if (host.StartsWith("edgeone."))
                {
                    name = "edgeone";
                    description = "EdgeOne CDN";
                }
                else if (host.Equals("gh-proxy.org", StringComparison.OrdinalIgnoreCase))
                {
                    name = "cloudflare";
                    description = "主站，全球加速！";
                }
                else
                {
                    var subdomain = host.Split('.')[0];
                    name = subdomain;
                    description = $"{subdomain} 节点";
                }

                nodes[name] = new GhProxyNodeInfo
                {
                    Name = name,
                    Url = url + "/",
                    Description = description,
                    IsAvailable = true
                };
            }

            return nodes;
        }

        /// <summary>
        /// 获取节点URL
        /// </summary>
        public static string GetNodeUrl(string nodeName)
        {
            if (GhProxyNodes.TryGetValue(nodeName, out var node) && node.IsAvailable)
            {
                return node.Url;
            }
            return GhProxyNodes[DefaultGhProxyNode].Url;
        }

        /// <summary>
        /// 检测URL是否为GH-Proxy格式
        /// </summary>
        public static bool IsGhProxyUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            return Regex.IsMatch(url, @"https?://[^/]*\.gh-proxy\.org/", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 从GH-Proxy URL中提取原始GitHub URL
        /// </summary>
        public static string ExtractOriginalUrl(string proxyUrl)
        {
            if (string.IsNullOrWhiteSpace(proxyUrl)) return proxyUrl;

            var match = Regex.Match(proxyUrl, @"https?://[^/]*\.gh-proxy\.org/(https?://.+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return proxyUrl;
        }

        /// <summary>
        /// 将GitHub URL转换为GH-Proxy加速链接
        /// </summary>
        public static string ConvertToGhProxyUrl(string githubUrl, string node = DefaultGhProxyNode)
        {
            if (string.IsNullOrWhiteSpace(githubUrl)) return githubUrl;
            if (IsGhProxyUrl(githubUrl)) return githubUrl;
            if (!githubUrl.Contains("github.com", StringComparison.OrdinalIgnoreCase)) return githubUrl;

            var proxyPrefix = GetNodeUrl(node);
            return $"{proxyPrefix}{githubUrl}";
        }
        
        #endregion

        #region 传统代理
        
        /// <summary>
        /// 获取系统代理设置
        /// </summary>
        public static SystemProxyInfo GetSystemProxy()
        {
            var info = new SystemProxyInfo();

            try
            {
                // 从IE/Windows获取系统代理设置
                var proxy = WebRequest.DefaultWebProxy;
                if (proxy != null)
                {
                    // 检查是否有代理
                    var testUrl = new Uri("https://github.com");
                    var proxyUri = proxy.GetProxy(testUrl);
                    if (proxyUri != null && proxyUri.AbsoluteUri != testUrl.AbsoluteUri)
                    {
                        info.ProxyAddress = proxyUri.AbsoluteUri.TrimEnd('/');
                        info.IsEnabled = !proxy.IsBypassed(testUrl);
                    }
                }

                // 从注册表获取更详细的代理信息
                info.RegistryProxy = GetRegistryProxy();
                if (!string.IsNullOrEmpty(info.RegistryProxy))
                {
                    info.ProxyAddress = info.RegistryProxy;
                    info.IsEnabled = true;
                }
            }
            catch
            {
                // 忽略错误
            }

            return info;
        }

        /// <summary>
        /// 从注册表获取代理设置
        /// </summary>
        private static string? GetRegistryProxy()
        {
            try
            {
                // Windows注册表路径
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Internet Settings");

                if (key != null)
                {
                    var proxyEnable = key.GetValue("ProxyEnable");
                    if (proxyEnable != null && proxyEnable.Equals(1))
                    {
                        var proxyServer = key.GetValue("ProxyServer")?.ToString();
                        if (!string.IsNullOrEmpty(proxyServer))
                        {
                            // 如果代理地址不包含协议前缀，添加http://
                            if (!proxyServer.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                                !proxyServer.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                                !proxyServer.StartsWith("socks", StringComparison.OrdinalIgnoreCase))
                            {
                                return $"http://{proxyServer}";
                            }
                            return proxyServer;
                        }
                    }
                }
            }
            catch
            {
                // 忽略错误
            }

            return null;
        }
        
        #endregion

        #region 配置解析

        /// <summary>
        /// 解析加速服务配置
        /// 支持格式：
        /// - 节点名: "cloudflare", "hongkong" 等
        /// - 完整加速URL: "https://hk.gh-proxy.org/https://github.com/user/repo"
        /// - 前缀URL: "https://hk.gh-proxy.org/"
        /// </summary>
        public static AccelerationConfig ParseAccelerationConfig(string? accelerationInput)
        {
            var config = new AccelerationConfig();
            
            if (string.IsNullOrWhiteSpace(accelerationInput) || accelerationInput == "none")
            {
                config.IsEnabled = false;
                return config;
            }
            
            // 检查是否为GH-Proxy节点名
            if (GhProxyNodes.TryGetValue(accelerationInput.ToLowerInvariant(), out var nodeInfo))
            {
                config.IsEnabled = true;
                config.Node = accelerationInput.ToLowerInvariant();
                config.Prefix = nodeInfo.Url;
                return config;
            }
            
            // 检查是否为GH-Proxy URL格式
            if (accelerationInput.Contains("gh-proxy", StringComparison.OrdinalIgnoreCase))
            {
                config.IsEnabled = true;
                
                // 尝试匹配已知节点并提取前缀
                foreach (var kvp in GhProxyNodes)
                {
                    if (accelerationInput.Contains(kvp.Value.Url.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                    {
                        config.Node = kvp.Key;
                        config.Prefix = kvp.Value.Url;
                        return config;
                    }
                }
                
                // 如果没有匹配到已知节点，提取前缀部分
                // 例如从 "https://custom.gh-proxy.org/https://github.com/..." 提取 "https://custom.gh-proxy.org/"
                var match = Regex.Match(accelerationInput, @"(https?://[^/]*gh-proxy[^/]*/)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    config.Prefix = match.Groups[1].Value;
                }
                else
                {
                    // 回退：使用整个URL作为前缀（确保以/结尾）
                    config.Prefix = accelerationInput.TrimEnd('/') + "/";
                }
            }
            
            return config;
        }

        /// <summary>
        /// 解析代理配置
        /// </summary>
        public static ProxyConfig ParseProxyConfig(ProxyMode mode, string? customProxyAddress = null)
        {
            var config = new ProxyConfig { Mode = mode };

            switch (mode)
            {
                case ProxyMode.None:
                    config.Type = ProxyType.None;
                    break;

                case ProxyMode.System:
                    var systemProxy = GetSystemProxy();
                    if (systemProxy.IsEnabled && !string.IsNullOrEmpty(systemProxy.ProxyAddress))
                    {
                        config.Type = ProxyType.Traditional;
                        config.ProxyAddress = systemProxy.ProxyAddress;
                        config.IsSystemProxy = true;
                    }
                    else
                    {
                        config.Type = ProxyType.None;
                    }
                    break;

                case ProxyMode.Custom:
                    if (!string.IsNullOrWhiteSpace(customProxyAddress))
                    {
                        var address = customProxyAddress.Trim();
                        
                        // 传统代理格式
                        if (address.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                            address.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                            address.StartsWith("socks5://", StringComparison.OrdinalIgnoreCase) ||
                            address.StartsWith("socks4://", StringComparison.OrdinalIgnoreCase))
                        {
                            config.Type = ProxyType.Traditional;
                            config.ProxyAddress = address;
                        }
                        else
                        {
                            // 默认作为HTTP代理
                            config.Type = ProxyType.Traditional;
                            config.ProxyAddress = $"http://{address}";
                        }
                    }
                    else
                    {
                        config.Type = ProxyType.None;
                    }
                    break;
            }

            return config;
        }

        /// <summary>
        /// 创建配置了代理的HttpClientHandler
        /// </summary>
        public static HttpClientHandler CreateHttpClientHandler(ProxyConfig config)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            if (config.Type == ProxyType.Traditional && !string.IsNullOrWhiteSpace(config.ProxyAddress))
            {
                handler.Proxy = new WebProxy(config.ProxyAddress, false);
                handler.UseProxy = true;
            }

            return handler;
        }
        
        #endregion
    }

    #region 数据类

    /// <summary>
    /// GH-Proxy节点信息
    /// </summary>
    public class GhProxyNodeInfo
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsAvailable { get; set; } = true;
    }

    /// <summary>
    /// 系统代理信息
    /// </summary>
    public class SystemProxyInfo
    {
        public bool IsEnabled { get; set; }
        public string? ProxyAddress { get; set; }
        public string? RegistryProxy { get; set; }
    }

    /// <summary>
    /// 加速服务配置 - GH-Proxy加速服务
    /// </summary>
    public class AccelerationConfig
    {
        public bool IsEnabled { get; set; }
        public string? Node { get; set; }
        public string? Prefix { get; set; }
    }

    /// <summary>
    /// 代理配置 - 仅用于传统代理
    /// 注意：加速服务(GH-Proxy)使用 AccelerationConfig 单独配置
    /// </summary>
    public class ProxyConfig
    {
        public ProxyType Type { get; set; } = ProxyType.None;
        public ProxyMode Mode { get; set; } = ProxyMode.None;
        public string? ProxyAddress { get; set; }
        public bool IsSystemProxy { get; set; }

        public bool IsTraditionalProxy => Type == ProxyType.Traditional;
        public bool HasProxy => Type != ProxyType.None;
    }

    /// <summary>
    /// 代理类型 - 仅传统代理
    /// </summary>
    public enum ProxyType
    {
        None,
        Traditional
    }

    /// <summary>
    /// 代理模式
    /// </summary>
    public enum ProxyMode
    {
        None,       // 不使用代理
        System,     // 使用系统代理
        Custom      // 自定义代理
    }

    /// <summary>
    /// 加速服务模式
    /// </summary>
    public enum AccelerationMode
    {
        None,       // 不使用加速
        GhProxy     // 使用GH-Proxy加速
    }
    
    #endregion
}
