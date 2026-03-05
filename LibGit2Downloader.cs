using LibGit2Sharp;
using System.Net;

namespace RS.GitSubDirectoryDownloader
{
    /// <summary>
    /// LibGit2Sharp下载器 - 使用Git原生协议克隆仓库
    /// LibGit2Sharp特点：
    /// - libgit原生实现，性能优秀
    /// - 支持完整Git操作（克隆、检出、历史等）
    /// - 支持SSH和HTTPS协议
    /// - 支持稀疏检出（需额外配置）
    /// 
    /// 注意：此实现会克隆整个仓库，然后复制指定目录
    /// </summary>
    public class LibGit2Downloader : IDirectoryDownloader
    {
        private readonly string? _username;
        private readonly string? _password;
        private readonly ProxyConfig _proxyConfig;
        private readonly AccelerationConfig _accelerationConfig;
        private Action<string>? _logCallback;
        private static bool _sslConfigured = false;
        private static readonly object _sslLock = new();

        public LibGit2Downloader(string? username = null, string? password = null, ProxyConfig? proxyConfig = null, AccelerationConfig? accelerationConfig = null, Action<string>? logCallback = null)
        {
            _username = username;
            _password = password;
            _proxyConfig = proxyConfig ?? new ProxyConfig();
            _accelerationConfig = accelerationConfig ?? new AccelerationConfig();
            _logCallback = logCallback;

            // 全局配置SSL（只执行一次）
            ConfigureSsl();

            if (_proxyConfig.IsTraditionalProxy)
            {
                Log($"LibGit2Sharp代理已配置: {_proxyConfig.ProxyAddress}");
            }
            else if (_accelerationConfig.IsEnabled)
            {
                Log($"LibGit2Sharp使用GH-Proxy: {_accelerationConfig.Prefix}");
            }

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                Log($"LibGit2Sharp认证已配置: {(username == "x-token-auth" ? "Token" : "用户名密码")}");
            }
        }

        /// <summary>
        /// 配置SSL验证 - 解决证书验证错误
        /// </summary>
        private static void ConfigureSsl()
        {
            lock (_sslLock)
            {
                if (_sslConfigured) return;

                try
                {
                    // 设置环境变量
                    Environment.SetEnvironmentVariable("GIT_SSL_NO_VERIFY", "1", EnvironmentVariableTarget.Process);
                    Environment.SetEnvironmentVariable("LIBGIT2_SSL_CERTIFICATE_CHECK", "0", EnvironmentVariableTarget.Process);
                    Environment.SetEnvironmentVariable("GIT_SSL_CAINFO", "", EnvironmentVariableTarget.Process);

                    // 尝试配置全局设置
                    try
                    {
                        GlobalSettings.SetConfigSearchPaths(ConfigurationLevel.Global, Path.GetTempPath());
                    }
                    catch { }

                    _sslConfigured = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SSL配置失败: {ex.Message}");
                }
            }
        }

        private void Log(string message)
        {
            _logCallback?.Invoke(message);
        }

        /// <summary>
        /// 下载目录 - 通过克隆仓库后复制指定目录
        /// </summary>
        public async Task DownloadDirectoryAsync(string owner, string repo, string remotePath, string localPath, string branch, List<string>? fileExtensions, IProgress<DownloadProgress>? progress, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                var repoUrl = BuildRepoUrl(owner, repo);
                var filterInfo = fileExtensions?.Count > 0 ? $" | 过滤后缀: {string.Join(", ", fileExtensions)}" : "";
                Log($"LibGit2Sharp下载 - 仓库: {repoUrl}, 分支: {branch}, 目录: {remotePath}{filterInfo}");

                var tempDir = Path.Combine(Path.GetTempPath(), $"GitDownload_{Guid.NewGuid():N}");
                Directory.CreateDirectory(tempDir);
                Log($"临时目录: {tempDir}");

                try
                {
                    Log("正在克隆仓库...");

                    var cloneOptions = CreateCloneOptions();
                    var clonedRepoPath = Repository.Clone(repoUrl, tempDir, cloneOptions);
                    Log($"克隆完成: {clonedRepoPath}");

                    // 切换到指定分支或commit
                    using var repository = new Repository(clonedRepoPath);
                    
                    // 检查是否为commit SHA（7位或40位十六进制字符串）
                    bool isCommitSha = IsCommitSha(branch);
                    
                    if (isCommitSha)
                    {
                        // 直接检出到指定commit
                        try
                        {
                            var commit = repository.Lookup<Commit>(branch);
                            if (commit != null)
                            {
                                Commands.Checkout(repository, commit);
                                Log($"已切换到Commit: {branch}");
                            }
                            else
                            {
                                Log($"警告: 未找到Commit {branch}，使用默认分支");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log($"警告: 切换到Commit {branch} 失败: {ex.Message}，使用默认分支");
                        }
                    }
                    else
                    {
                        // 查找分支
                        var branchObj = repository.Branches[$"origin/{branch}"];
                        if (branchObj != null)
                        {
                            Commands.Checkout(repository, branchObj);
                            Log($"已切换到分支: {branch}");
                        }
                        else
                        {
                            Log($"警告: 未找到分支 {branch}，使用默认分支");
                        }
                    }

                    var sourceDir = string.IsNullOrEmpty(remotePath)
                        ? tempDir
                        : Path.Combine(tempDir, remotePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                    if (!Directory.Exists(sourceDir))
                    {
                        Log($"错误: 在仓库中未找到指定目录: {remotePath}");
                        throw new DirectoryNotFoundException($"在仓库中未找到指定目录: {remotePath}");
                    }

                    if (!Directory.Exists(localPath))
                    {
                        Directory.CreateDirectory(localPath);
                    }

                    CopyDirectory(sourceDir, localPath, fileExtensions ?? new List<string>(), progress, cancellationToken);
                    Log("下载完成！");
                }
                finally
                {
                    try
                    {
                        if (Directory.Exists(tempDir))
                        {
                            Directory.Delete(tempDir, true);
                            Log("已清理临时目录");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"清理临时目录失败: {ex.Message}");
                    }
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 获取默认分支 - 需要额外API调用，这里返回默认值
        /// </summary>
        public Task<string> GetDefaultBranchAsync(string owner, string repo)
        {
            // LibGit2Sharp获取默认分支需要先克隆，这里返回常见默认值
            return Task.FromResult("main");
        }

        /// <summary>
        /// 构建仓库URL，支持GH-Proxy加速
        /// </summary>
        private string BuildRepoUrl(string owner, string repo)
        {
            var originalUrl = $"https://github.com/{owner}/{repo}.git";

            if (_accelerationConfig.IsEnabled && !string.IsNullOrWhiteSpace(_accelerationConfig.Prefix))
            {
                // 使用GH-Proxy加速克隆
                // 转换为: https://cdn.gh-proxy.org/https://github.com/user/repo.git
                var acceleratedUrl = $"{_accelerationConfig.Prefix.TrimEnd('/')}/{originalUrl}";
                Log($"使用GH-Proxy加速克隆: {acceleratedUrl}");
                return acceleratedUrl;
            }

            return originalUrl;
        }

        /// <summary>
        /// 检查字符串是否为commit SHA格式
        /// 支持短SHA（7位）和完整SHA（40位）
        /// </summary>
        private static bool IsCommitSha(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            
            // 短SHA（7位）或完整SHA（40位）
            if (input.Length == 7 || input.Length == 40)
            {
                return input.All(c => char.IsLetterOrDigit(c) && 
                    (char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')));
            }
            
            return false;
        }

        /// <summary>
        /// 创建克隆选项
        /// </summary>
        private CloneOptions CreateCloneOptions()
        {
            var options = new CloneOptions();

            // SSL证书验证 - 跳过所有证书检查
            options.FetchOptions.CertificateCheck = (certificate, host, valid) =>
            {
                Log($"SSL证书检查: {host} - 已跳过验证");
                return true;
            };

            // 配置认证
            if (!string.IsNullOrWhiteSpace(_username) && !string.IsNullOrWhiteSpace(_password))
            {
                options.FetchOptions.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials
                    {
                        Username = _username,
                        Password = _password
                    };
            }

            // 配置传统代理
            if (_proxyConfig.IsTraditionalProxy && !string.IsNullOrWhiteSpace(_proxyConfig.ProxyAddress))
            {
                options.FetchOptions.ProxyOptions.Url = _proxyConfig.ProxyAddress;
            }

            return options;
        }

        /// <summary>
        /// 复制目录
        /// </summary>
        private void CopyDirectory(string sourceDir, string destDir, List<string> fileExtensions, IProgress<DownloadProgress>? progress, CancellationToken cancellationToken)
        {
            var allFiles = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
            var files = fileExtensions.Count > 0
                ? allFiles.Where(f => fileExtensions.Contains(Path.GetExtension(f).ToLowerInvariant())).ToArray()
                : allFiles;

            var totalFiles = files.Length;
            var copiedFiles = 0;

            progress?.Report(new DownloadProgress { TotalFiles = totalFiles, Status = $"找到 {totalFiles} 个文件待复制" });
            Log($"找到 {totalFiles} 个文件待复制");

            if (totalFiles == 0)
            {
                Log("未找到匹配的文件");
                return;
            }

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var relativePath = file.Substring(sourceDir.Length).TrimStart(Path.DirectorySeparatorChar);
                var destFile = Path.Combine(destDir, relativePath);

                var destFileDir = Path.GetDirectoryName(destFile);
                if (!string.IsNullOrEmpty(destFileDir) && !Directory.Exists(destFileDir))
                {
                    Directory.CreateDirectory(destFileDir);
                }

                File.Copy(file, destFile, true);
                copiedFiles++;

                if (copiedFiles % 10 == 0 || copiedFiles == totalFiles)
                {
                    progress?.Report(new DownloadProgress
                    {
                        DownloadedFiles = copiedFiles,
                        TotalFiles = totalFiles,
                        CurrentFile = Path.GetFileName(file)
                    });
                }
            }

            Log($"文件复制完成: {copiedFiles} 个文件");
        }
    }
}
