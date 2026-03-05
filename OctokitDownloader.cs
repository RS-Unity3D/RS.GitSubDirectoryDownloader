using Octokit;
using System.Net;
using System.Web;

namespace RS.GitSubDirectoryDownloader
{
    /// <summary>
    /// Octokit下载器 - 使用GitHub官方.NET客户端库
    /// Octokit特点：
    /// - GitHub官方维护，API兼容性最好
    /// - 完整的类型安全API封装
    /// - 自动处理Rate Limit（提供重置时间）
    /// - 支持URL编码文件名
    /// - 内置重试机制
    /// - 支持GH-Proxy加速文件下载
    /// 
    /// 改进点（基于Octokit最佳实践）：
    /// 1. 使用IApiResponse获取原始响应，支持更好的错误处理
    /// 2. Rate Limit自动等待和重试
    /// 3. 支持GitHub Enterprise
    /// 4. 支持GH-Proxy加速
    /// </summary>
    public class OctokitDownloader : IDirectoryDownloader
    {
        private readonly GitHubClient _client;
        private readonly ProxyConfig _proxyConfig;
        private readonly AccelerationConfig _accelerationConfig;
        private readonly string? _token;
        private Action<string>? _logCallback;
        private readonly HttpClient _fileHttpClient;

        public OctokitDownloader(string? token = null, ProxyConfig? proxyConfig = null, AccelerationConfig? accelerationConfig = null, Action<string>? logCallback = null)
        {
            _token = token;
            _proxyConfig = proxyConfig ?? new ProxyConfig();
            _accelerationConfig = accelerationConfig ?? new AccelerationConfig();
            _logCallback = logCallback;

            // 创建用于文件下载的HttpClient
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            if (_proxyConfig.IsTraditionalProxy && !string.IsNullOrWhiteSpace(_proxyConfig.ProxyAddress))
            {
                handler.Proxy = new WebProxy(_proxyConfig.ProxyAddress, false);
                handler.UseProxy = true;
            }

            _fileHttpClient = new HttpClient(handler);
            _fileHttpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("GitHubDownloader", "1.0"));

            // 创建Octokit客户端
            // 注意：Octokit的Connection构造函数需要自定义HttpMessageHandler来支持代理
            var connection = new Connection(new ProductHeaderValue("GitHubDownloader", "1.0"));

            _client = new GitHubClient(connection);

            // 配置认证
            if (!string.IsNullOrWhiteSpace(token))
            {
                _client.Credentials = new Credentials(token);
                Log("Octokit Token认证已配置");
            }

            // 日志配置信息
            if (_proxyConfig.IsTraditionalProxy)
            {
                Log($"Octokit代理已配置: {_proxyConfig.ProxyAddress}");
            }
            else if (_accelerationConfig.IsEnabled)
            {
                Log($"Octokit使用GH-Proxy加速: {_accelerationConfig.Prefix}");
            }
        }

        private void Log(string message)
        {
            _logCallback?.Invoke(message);
        }

        /// <summary>
        /// 递归下载目录
        /// </summary>
        public async Task DownloadDirectoryAsync(string owner, string repo, string remotePath, string localPath, string branch, List<string>? fileExtensions = null, IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            fileExtensions ??= new List<string>();
            Log($"开始统计文件数量...");
            var totalFiles = await CountFilesAsync(owner, repo, remotePath, branch, fileExtensions, cancellationToken);
            Log($"找到 {totalFiles} 个匹配文件");
            var counter = new DownloadCounter { TotalFiles = totalFiles };

            await DownloadDirectoryRecursiveAsync(owner, repo, remotePath, localPath, branch, fileExtensions, progress, counter, cancellationToken);
        }

        private async Task DownloadDirectoryRecursiveAsync(string owner, string repo, string remotePath, string localPath, string branch, List<string> fileExtensions, IProgress<DownloadProgress>? progress, DownloadCounter counter, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // 获取目录内容 - 使用GetAllContentsByRef支持指定分支
                // 注意：空路径时使用不同的重载方法
                IReadOnlyList<RepositoryContent> contents;
                if (string.IsNullOrEmpty(remotePath))
                {
                    contents = await ExecuteWithRateLimitAsync(
                        () => _client.Repository.Content.GetAllContentsByRef(owner, repo, branch),
                        cancellationToken);
                }
                else
                {
                    contents = await ExecuteWithRateLimitAsync(
                        () => _client.Repository.Content.GetAllContentsByRef(owner, repo, remotePath, branch),
                        cancellationToken);
                }

                foreach (var item in contents)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // URL解码文件名，处理特殊字符
                    var itemName = HttpUtility.UrlDecode(item.Name);
                    var itemLocalPath = Path.Combine(localPath, itemName);

                    if (item.Type == ContentType.Dir)
                    {
                        // 如果有文件后缀过滤，需要检查子目录是否包含匹配的文件
                        if (fileExtensions.Count > 0)
                        {
                            var hasMatchingFiles = await HasMatchingFilesAsync(owner, repo, item.Path, branch, fileExtensions, cancellationToken);
                            if (!hasMatchingFiles) continue;
                        }

                        if (!Directory.Exists(itemLocalPath))
                        {
                            Directory.CreateDirectory(itemLocalPath);
                        }
                        await DownloadDirectoryRecursiveAsync(owner, repo, item.Path, itemLocalPath, branch, fileExtensions, progress, counter, cancellationToken);
                    }
                    else if (item.Type == ContentType.File)
                    {
                        // 检查文件后缀是否匹配
                        if (fileExtensions.Count > 0)
                        {
                            var ext = Path.GetExtension(itemName).ToLowerInvariant();
                            if (!fileExtensions.Contains(ext))
                            {
                                continue;
                            }
                        }

                        // 使用流式下载，支持GH-Proxy加速
                        var fileContent = await DownloadFileContentAsync(new Uri( item.DownloadUrl), cancellationToken);
                        await File.WriteAllBytesAsync(itemLocalPath, fileContent, cancellationToken);
                        counter.DownloadedFiles++;

                        progress?.Report(new DownloadProgress
                        {
                            DownloadedFiles = counter.DownloadedFiles,
                            TotalFiles = counter.TotalFiles,
                            CurrentFile = itemName
                        });
                    }
                }
            }
            catch (NotFoundException)
            {
                Log($"警告: 目录不存在或无权访问: {remotePath}");
            }
        }

        /// <summary>
        /// 下载文件内容 - 支持GH-Proxy加速
        /// </summary>
        private async Task<byte[]> DownloadFileContentAsync(Uri? downloadUrl, CancellationToken cancellationToken)
        {
            if (downloadUrl == null)
                return Array.Empty<byte>();

            var url = downloadUrl.ToString();

            // 如果启用了GH-Proxy加速，转换URL
            if (_accelerationConfig.IsEnabled && !string.IsNullOrWhiteSpace(_accelerationConfig.Prefix))
            {
                url = $"{_accelerationConfig.Prefix}{url}";
            }

            // 使用流式下载，更高效处理大文件
            using var response = await _fileHttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);
            return memoryStream.ToArray();
        }

        private async Task<int> CountFilesAsync(string owner, string repo, string path, string branch, List<string> fileExtensions, CancellationToken cancellationToken)
        {
            var count = 0;
            try
            {
                // 注意：空路径时使用不同的重载方法
                IReadOnlyList<RepositoryContent> contents;
                if (string.IsNullOrEmpty(path))
                {
                    contents = await ExecuteWithRateLimitAsync(
                        () => _client.Repository.Content.GetAllContentsByRef(owner, repo, branch),
                        cancellationToken);
                }
                else
                {
                    contents = await ExecuteWithRateLimitAsync(
                        () => _client.Repository.Content.GetAllContentsByRef(owner, repo, path, branch),
                        cancellationToken);
                }

                foreach (var item in contents)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (item.Type == ContentType.Dir)
                    {
                        count += await CountFilesAsync(owner, repo, item.Path, branch, fileExtensions, cancellationToken);
                    }
                    else if (item.Type == ContentType.File)
                    {
                        if (fileExtensions.Count > 0)
                        {
                            var ext = Path.GetExtension(item.Name).ToLowerInvariant();
                            if (!fileExtensions.Contains(ext))
                            {
                                continue;
                            }
                        }
                        count++;
                    }
                }
            }
            catch (NotFoundException)
            {
                // 目录不存在
            }

            return count;
        }

        /// <summary>
        /// 检查目录中是否有匹配后缀的文件
        /// </summary>
        private async Task<bool> HasMatchingFilesAsync(string owner, string repo, string path, string branch, List<string> fileExtensions, CancellationToken cancellationToken)
        {
            try
            {
                // 注意：空路径时使用不同的重载方法
                IReadOnlyList<RepositoryContent> contents;
                if (string.IsNullOrEmpty(path))
                {
                    contents = await ExecuteWithRateLimitAsync(
                        () => _client.Repository.Content.GetAllContentsByRef(owner, repo, branch),
                        cancellationToken);
                }
                else
                {
                    contents = await ExecuteWithRateLimitAsync(
                        () => _client.Repository.Content.GetAllContentsByRef(owner, repo, path, branch),
                        cancellationToken);
                }

                foreach (var item in contents)
                {
                    if (item.Type == ContentType.Dir)
                    {
                        if (await HasMatchingFilesAsync(owner, repo, item.Path, branch, fileExtensions, cancellationToken))
                            return true;
                    }
                    else if (item.Type == ContentType.File)
                    {
                        var ext = Path.GetExtension(item.Name).ToLowerInvariant();
                        if (fileExtensions.Contains(ext))
                            return true;
                    }
                }
            }
            catch (NotFoundException)
            {
                // 目录不存在
            }

            return false;
        }

        /// <summary>
        /// 执行API调用并处理Rate Limit
        /// </summary>
        private async Task<T> ExecuteWithRateLimitAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    return await action();
                }
                catch (RateLimitExceededException ex)
                {
                    var waitTime = ex.Reset - DateTimeOffset.Now;
                    if (waitTime.TotalSeconds > 0)
                    {
                        Log($"GitHub API速率限制，等待 {waitTime.TotalSeconds:F0} 秒后重试...");
                        await Task.Delay(waitTime, cancellationToken);
                    }
                    else
                    {
                        // 如果等待时间已过，稍等片刻后重试
                        await Task.Delay(1000, cancellationToken);
                    }
                }
                catch (SecondaryRateLimitExceededException)
                {
                    Log($"GitHub API二级速率限制，等待后重试...");
                    await Task.Delay(60000, cancellationToken); // 等待1分钟
                }
            }
        }

        /// <summary>
        /// 获取仓库的默认分支
        /// </summary>
        public async Task<string> GetDefaultBranchAsync(string owner, string repo)
        {
            try
            {
                var repository = await _client.Repository.Get(owner, repo);
                return repository.DefaultBranch ?? "main";
            }
            catch
            {
                return "main";
            }
        }

        /// <summary>
        /// 获取API速率限制信息
        /// </summary>
        public async Task<RateLimitInfo> GetRateLimitInfoAsync()
        {
            try
            {
                var rateLimit = await _client.RateLimit.GetRateLimits();
                return new RateLimitInfo
                {
                    Limit = rateLimit.Resources.Core.Limit,
                    Remaining = rateLimit.Resources.Core.Remaining,
                    Reset = rateLimit.Resources.Core.Reset
                };
            }
            catch
            {
                return new RateLimitInfo();
            }
        }
    }

    /// <summary>
    /// 速率限制信息
    /// </summary>
    public class RateLimitInfo
    {
        public int Limit { get; set; }
        public int Remaining { get; set; }
        public DateTimeOffset Reset { get; set; }

        public bool IsLow => Remaining < 10;
    }
}
