#if DISABLE_GITHUB_API_CLIENT
// GitHubApiClient已被禁用，OctokitDownloader提供了更完整的功能
// 如需恢复，将第一行的 #if DISABLE_GITHUB_API_CLIENT 改为 #if false

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;

namespace RS.GitSubDirectoryDownloader
{
    /// <summary>
    /// GitHub API客户端下载器 - 使用GitHub REST API下载文件
    /// GitHubApi特点：
    /// - 轻量级，无需额外依赖
    /// - 直接通过API获取文件内容
    /// - 支持Rate Limit处理
    /// - 支持GH-Proxy加速下载
    /// </summary>
    public class GitHubApiClient : IDirectoryDownloader
    {
        private readonly HttpClient _httpClient;
        private readonly string? _token;
        private readonly ProxyConfig _proxyConfig;
        private Action<string>? _logCallback;

        public GitHubApiClient(string? token = null, string? proxyAddress = null, Action<string>? logCallback = null)
        {
            _token = token;
            _proxyConfig = ProxyHelper.ParseProxyAddress(proxyAddress);
            _logCallback = logCallback;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            // 配置传统代理
            if (_proxyConfig.IsTraditionalProxy && !string.IsNullOrWhiteSpace(_proxyConfig.ProxyAddress))
            {
                handler.Proxy = new WebProxy(_proxyConfig.ProxyAddress, false);
                handler.UseProxy = true;
                Log($"HttpClient代理已配置: {_proxyConfig.ProxyAddress}");
            }

            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GitHubDownloader", "1.0"));

            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);
                Log("Token认证已配置");
            }

            if (_proxyConfig.IsGhProxy)
            {
                Log($"GH-Proxy加速已启用: {_proxyConfig.GhProxyPrefix}");
            }
        }

        private void Log(string message)
        {
            _logCallback?.Invoke(message);
        }

        /// <summary>
        /// 获取仓库目录内容
        /// </summary>
        private async Task<List<GitHubItem>> GetDirectoryContentsAsync(string owner, string repo, string path, string branch = "main")
        {
            // API请求不使用GH-Proxy，直接访问GitHub API
            var url = $"https://api.github.com/repos/{owner}/{repo}/contents/{path}?ref={branch}";
            Log($"API请求: {url}");
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            Log($"API响应长度: {json.Length} 字节");

            // 尝试解析为数组
            try
            {
                var items = JsonSerializer.Deserialize<List<GitHubItem>>(json);
                if (items != null && items.Count > 0)
                {
                    Log($"API返回 {items.Count} 个项目: {string.Join(", ", items.Select(i => $"{i.Name}({i.Type})"))}");
                    return items;
                }
            }
            catch (Exception ex)
            {
                Log($"解析数组失败: {ex.Message}");
            }

            // 尝试解析为单个对象（文件）
            try
            {
                var singleItem = JsonSerializer.Deserialize<GitHubItem>(json);
                if (singleItem != null)
                {
                    Log($"API返回单个文件: {singleItem.Name}");
                    return new List<GitHubItem> { singleItem };
                }
            }
            catch (Exception ex)
            {
                Log($"解析单个对象失败: {ex.Message}");
            }

            // 可能是错误响应
            Log($"API响应内容: {json.Substring(0, Math.Min(500, json.Length))}...");
            return new List<GitHubItem>();
        }

        /// <summary>
        /// 下载文件 - 支持GH-Proxy加速
        /// </summary>
        private async Task<byte[]> DownloadFileAsync(Uri? downloadUrl)
        {
            if (downloadUrl == null)
                return Array.Empty<byte>();

            var url = downloadUrl.ToString();

            // 如果启用了GH-Proxy，转换URL
            if (_proxyConfig.IsGhProxy && !string.IsNullOrWhiteSpace(_proxyConfig.GhProxyPrefix))
            {
                url = $"{_proxyConfig.GhProxyPrefix}{url}";
            }

            // 使用流式下载，更高效
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// 递归下载目录
        /// </summary>
        public async Task DownloadDirectoryAsync(string owner, string repo, string remotePath, string localPath, string branch = "main", List<string>? fileExtensions = null, IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            fileExtensions ??= new List<string>();
            Log($"开始统计文件数量...");
            var totalFiles = await CountFilesAsync(owner, repo, remotePath, branch, fileExtensions);
            Log($"找到 {totalFiles} 个匹配文件");
            var counter = new DownloadCounter { TotalFiles = totalFiles };

            await DownloadDirectoryRecursiveAsync(owner, repo, remotePath, localPath, branch, fileExtensions, progress, counter, cancellationToken);
        }

        private async Task DownloadDirectoryRecursiveAsync(string owner, string repo, string remotePath, string localPath, string branch, List<string> fileExtensions, IProgress<DownloadProgress>? progress, DownloadCounter counter, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var items = await GetDirectoryContentsAsync(owner, repo, remotePath, branch);

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // URL解码文件名，处理特殊字符
                var itemName = HttpUtility.UrlDecode(item.Name);
                var itemLocalPath = Path.Combine(localPath, itemName);

                if (item.Type == "dir")
                {
                    // 如果有文件后缀过滤，需要检查子目录是否包含匹配的文件
                    if (fileExtensions.Count > 0)
                    {
                        // 先递归检查是否有匹配的文件，没有则跳过
                        var hasMatchingFiles = await HasMatchingFilesAsync(owner, repo, item.Path, branch, fileExtensions);
                        if (!hasMatchingFiles) continue;
                    }

                    if (!Directory.Exists(itemLocalPath))
                    {
                        Directory.CreateDirectory(itemLocalPath);
                    }
                    await DownloadDirectoryRecursiveAsync(owner, repo, item.Path, itemLocalPath, branch, fileExtensions, progress, counter, cancellationToken);
                }
                else if (item.Type == "file")
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

                    var fileContent = await DownloadFileAsync(item.DownloadUrl);
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

        private async Task<int> CountFilesAsync(string owner, string repo, string path, string branch, List<string> fileExtensions)
        {
            var count = 0;
            var items = await GetDirectoryContentsAsync(owner, repo, path, branch);

            foreach (var item in items)
            {
                if (item.Type == "dir")
                {
                    count += await CountFilesAsync(owner, repo, item.Path, branch, fileExtensions);
                }
                else if (item.Type == "file")
                {
                    // 检查文件后缀是否匹配
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

            return count;
        }

        /// <summary>
        /// 检查目录中是否有匹配后缀的文件
        /// </summary>
        private async Task<bool> HasMatchingFilesAsync(string owner, string repo, string path, string branch, List<string> fileExtensions)
        {
            var items = await GetDirectoryContentsAsync(owner, repo, path, branch);

            foreach (var item in items)
            {
                if (item.Type == "dir")
                {
                    if (await HasMatchingFilesAsync(owner, repo, item.Path, branch, fileExtensions))
                        return true;
                }
                else if (item.Type == "file")
                {
                    var ext = Path.GetExtension(item.Name).ToLowerInvariant();
                    if (fileExtensions.Contains(ext))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取仓库默认分支 - 通过API获取
        /// </summary>
        public async Task<string> GetDefaultBranchAsync(string owner, string repo)
        {
            try
            {
                var url = $"https://api.github.com/repos/{owner}/{repo}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var repoInfo = JsonSerializer.Deserialize<GitHubRepoInfoResponse>(json);
                return repoInfo?.DefaultBranch ?? "main";
            }
            catch
            {
                return "main";
            }
        }

        /// <summary>
        /// 检查仓库是否为私有仓库
        /// </summary>
        public async Task<bool> IsPrivateRepoAsync(string owner, string repo)
        {
            try
            {
                var url = $"https://api.github.com/repos/{owner}/{repo}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var repoInfo = JsonSerializer.Deserialize<GitHubRepoInfoResponse>(json);
                return repoInfo?.Private ?? false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取仓库所有分支列表
        /// </summary>
        public async Task<List<string>> GetBranchesAsync(string owner, string repo)
        {
            try
            {
                var branches = new List<string>();
                var url = $"https://api.github.com/repos/{owner}/{repo}/branches?per_page=100";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var branchList = JsonSerializer.Deserialize<List<GitHubBranch>>(json);
                
                if (branchList != null)
                {
                    branches = branchList.Where(b => !string.IsNullOrEmpty(b.Name)).Select(b => b.Name!).ToList();
                }
                
                return branches;
            }
            catch (Exception ex)
            {
                Log($"获取分支列表失败: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// 获取仓库所有标签列表
        /// </summary>
        public async Task<List<string>> GetTagsAsync(string owner, string repo)
        {
            try
            {
                var tags = new List<string>();
                var url = $"https://api.github.com/repos/{owner}/{repo}/tags?per_page=100";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var tagList = JsonSerializer.Deserialize<List<GitHubTag>>(json);
                
                if (tagList != null)
                {
                    tags = tagList.Where(t => !string.IsNullOrEmpty(t.Name)).Select(t => t.Name!).ToList();
                }
                
                return tags;
            }
            catch (Exception ex)
            {
                Log($"获取标签列表失败: {ex.Message}");
                return new List<string>();
            }
        }
    }

    /// <summary>
    /// GitHub API返回的项目信息
    /// </summary>
    internal class GitHubRepoInfoResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("default_branch")]
        public string? DefaultBranch { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("private")]
        public bool Private { get; set; }
    }

    /// <summary>
    /// GitHub分支信息
    /// </summary>
    internal class GitHubBranch
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    /// <summary>
    /// GitHub标签信息
    /// </summary>
    internal class GitHubTag
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("commit")]
        public GitHubCommitRef? Commit { get; set; }
    }

    /// <summary>
    /// GitHub提交引用
    /// </summary>
    internal class GitHubCommitRef
    {
        [System.Text.Json.Serialization.JsonPropertyName("sha")]
        public string? Sha { get; set; }
    }

    /// <summary>
    /// GitHub API返回的目录项
    /// </summary>
    public class GitHubItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; } = "";
        
        [System.Text.Json.Serialization.JsonPropertyName("path")]
        public string Path { get; set; } = "";
        
        [System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; } = "";
        
        [System.Text.Json.Serialization.JsonPropertyName("download_url")]
        public Uri? DownloadUrl { get; set; }
    }
}

#endif
