using System.Text;

namespace RS.GitSubDirectoryDownloader
{
    internal static class Program
    {
        private const string LogFileName = "download.log";
        private static StreamWriter? _logWriter;
        private static readonly object _logLock = new();

        /// <summary>
        /// 应用程序主入口点
        /// 支持GUI模式和命令行模式
        /// 
        /// 命令行用法:
        ///   RS.GitSubDirectoryDownloader --url <GitHub URL> [选项]
        /// 
        /// 示例:
        ///   RS.GitSubDirectoryDownloader --url https://github.com/user/repo/tree/main/src --output ./download
        ///   RS.GitSubDirectoryDownloader -u https://github.com/user/repo -o ./download -m octokit -p ghproxy:hongkong
        /// </summary>
        [STAThread]
        static async Task<int> Main(string[] args)
        {
            // 如果没有参数或请求帮助，启动GUI
            if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
            {
                if (args.Contains("--help") || args.Contains("-h"))
                {
                    ShowHelp();
                    return 0;
                }

                ApplicationConfiguration.Initialize();
                Application.Run(new Form1());
                return 0;
            }

            // 命令行模式 - 初始化日志文件
            InitializeLogFile();

            try
            {
                var result = await RunCommandLineAsync(args);
                return result;
            }
            finally
            {
                CloseLogFile();
            }
        }

        /// <summary>
        /// 初始化日志文件
        /// </summary>
        private static void InitializeLogFile()
        {
            try
            {
                _logWriter = new StreamWriter(LogFileName, true, Encoding.UTF8) { AutoFlush = true };
                _logWriter.WriteLine("");
                _logWriter.WriteLine($"========== 命令行下载 {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==========");
            }
            catch { }
        }

        /// <summary>
        /// 关闭日志文件
        /// </summary>
        private static void CloseLogFile()
        {
            try
            {
                _logWriter?.WriteLine($"========== 下载结束 {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==========");
                _logWriter?.Close();
                _logWriter?.Dispose();
            }
            catch { }
        }

        /// <summary>
        /// 日志输出 - 同时输出到控制台和文件
        /// </summary>
        private static void Log(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var fullLogLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            var shortLogLine = $"[{timestamp}] {message}";

            Console.WriteLine(shortLogLine);

            try
            {
                lock (_logLock)
                {
                    _logWriter?.WriteLine(fullLogLine);
                }
            }
            catch { }
        }

        /// <summary>
        /// 显示帮助信息
        /// </summary>
        private static void ShowHelp()
        {
            var help = @"
GitHub仓库目录下载器 - 命令行模式
================================

用法:
  RS.GitSubDirectoryDownloader --url <GitHub URL> [选项]

必需参数:
  --url, -u <URL>        GitHub目录地址
                         例如: https://github.com/user/repo/tree/main/src

可选参数:
  --output, -o <路径>    本地保存路径 (默认: 当前目录)
  --method, -m <方式>    下载方式 (默认: octokit)
                         octokit  - Octokit官方库 (推荐)
                         libgit  - LibGit2Sharp (完整克隆)
  --acceleration, -a <加速> 加速服务设置 (默认: none)
                         none           - 不使用加速
                         ghproxy        - GH-Proxy加速(默认节点)
                         ghproxy:节点   - GH-Proxy指定节点
                                          cloudflare, hongkong, fastly, edgeone
  --proxy, -p <代理>     传统代理设置 (默认: none)
                         none          - 不使用代理
                         system        - 使用系统代理
                         custom:地址   - 自定义代理地址
                                        例如: http://127.0.0.1:7890
  --branch, -b <分支>    分支名称 (默认: 自动检测)
  --subdir, -s <目录>    子目录路径 (可选，覆盖URL中的路径)
  --extensions, -e <后缀> 文件后缀过滤，逗号分隔
                         例如: .cs,.json,.txt
  --token, -t <token>    GitHub Token (可选，用于私有仓库或提高速率限制)
  --help, -h             显示此帮助信息

注意:
  --acceleration 是加速服务(GH-Proxy)，通过URL重定向加速GitHub下载
  --proxy 是传统网络代理(HTTP/SOCKS5)，用于网络代理访问

示例:
  # 基本用法
  RS.GitSubDirectoryDownloader --url https://github.com/octokit/octokit.net/tree/main/Octokit

  # 指定输出目录
  RS.GitSubDirectoryDownloader -u https://github.com/user/repo -o ./download

  # 使用GH-Proxy香港节点加速
  RS.GitSubDirectoryDownloader -u https://github.com/user/repo -a ghproxy:hongkong

  # 使用传统代理访问
  RS.GitSubDirectoryDownloader -u https://github.com/user/repo -p custom:http://127.0.0.1:7890

  # 同时使用加速服务和代理
  RS.GitSubDirectoryDownloader -u https://github.com/user/repo -a ghproxy -p system

  # 指定子目录
  RS.GitSubDirectoryDownloader -u https://github.com/user/repo -s src/components

  # 过滤特定文件类型
  RS.GitSubDirectoryDownloader -u https://github.com/user/repo -e .cs,.json

  # 使用Token
  RS.GitSubDirectoryDownloader -u https://github.com/user/repo -t ghp_xxxxx

";
            Console.WriteLine(help);
        }

        /// <summary>
        /// 解析命令行参数
        /// </summary>
        private static CommandLineOptions ParseArgs(string[] args)
        {
            var options = new CommandLineOptions();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--url":
                    case "-u":
                        if (i + 1 < args.Length) options.Url = args[++i];
                        break;

                    case "--output":
                    case "-o":
                        if (i + 1 < args.Length) options.Output = args[++i];
                        break;

                    case "--method":
                    case "-m":
                        if (i + 1 < args.Length) options.Method = args[++i].ToLowerInvariant();
                        break;

                    case "--acceleration":
                    case "-a":
                        if (i + 1 < args.Length) options.Acceleration = args[++i].ToLowerInvariant();
                        break;

                    case "--proxy":
                    case "-p":
                        if (i + 1 < args.Length) options.Proxy = args[++i].ToLowerInvariant();
                        break;

                    case "--branch":
                    case "-b":
                        if (i + 1 < args.Length) options.Branch = args[++i];
                        break;

                    case "--subdir":
                    case "-s":
                        if (i + 1 < args.Length) options.SubDir = args[++i];
                        break;

                    case "--extensions":
                    case "-e":
                        if (i + 1 < args.Length) options.Extensions = args[++i];
                        break;

                    case "--token":
                    case "-t":
                        if (i + 1 < args.Length) options.Token = args[++i];
                        break;
                }
            }

            return options;
        }

        /// <summary>
        /// 命令行模式执行
        /// </summary>
        private static async Task<int> RunCommandLineAsync(string[] args)
        {
            var options = ParseArgs(args);

            // 验证必需参数
            if (string.IsNullOrWhiteSpace(options.Url))
            {
                Console.Error.WriteLine("错误: 必须指定 --url 参数");
                Console.Error.WriteLine("使用 --help 查看帮助信息");
                return 1;
            }

            // 配置SSL
            Environment.SetEnvironmentVariable("GIT_SSL_NO_VERIFY", "1", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("LIBGIT2_SSL_CERTIFICATE_CHECK", "0", EnvironmentVariableTarget.Process);

            int exitCode = 0;
            try
            {
                await ExecuteDownloadAsync(options);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"下载失败: {ex.Message}");
                Log($"下载失败: {ex.Message}");
                exitCode = 1;
            }
            
            // 命令行执行完成后自动退出
            return exitCode;
        }

        /// <summary>
        /// 执行下载
        /// </summary>
        private static async Task ExecuteDownloadAsync(CommandLineOptions options)
        {
            // 解析GitHub URL - 使用新的GitUrlParser支持Unity Package Manager风格
            if (string.IsNullOrWhiteSpace(options.Url))
            {
                throw new ArgumentException("GitHub URL不能为空");
            }

            var urlInfo = GitUrlParser.Parse(options.Url!);
            if (!urlInfo.IsValid)
            {
                throw new ArgumentException($"URL解析失败: {urlInfo.ErrorMessage}");
            }

            var owner = urlInfo.Owner;
            var repo = urlInfo.RepoName;
            var branch = urlInfo.Ref ?? options.Branch ?? "main";
            var folder = urlInfo.SubDirectory;
            
            // 如果命令行指定了子目录，覆盖URL中的目录
            if (!string.IsNullOrWhiteSpace(options.SubDir))
            {
                folder = options.SubDir;
            }

            // 解析下载方式
            // 根据URL类型自动选择下载方式（如果未明确指定）
            DownloadMethod downloadMethod;
            if (options.Method == "libgit")
            {
                downloadMethod = DownloadMethod.LibGit2Sharp;
            }
            else if (options.Method == "octokit")
            {
                downloadMethod = DownloadMethod.Octokit;
            }
            else
            {
                // 未指定时根据URL类型自动选择
                // Unity Package Manager风格(.git) -> LibGit2Sharp
                // GitHub Web URL -> Octokit
                downloadMethod = urlInfo.UrlType == GitUrlType.GitPackage 
                    ? DownloadMethod.LibGit2Sharp 
                    : DownloadMethod.Octokit;
            }

            // 解析加速服务配置 (GH-Proxy)
            var accelerationConfig = ParseAccelerationOption(options.Acceleration);

            // 解析代理配置 (传统代理)
            var proxyConfig = ParseProxyOption(options.Proxy);

            // 解析文件后缀
            var extensions = ParseExtensions(options.Extensions);

            // 确定输出路径
            var localPath = string.IsNullOrWhiteSpace(options.Output)
                ? Path.Combine(Directory.GetCurrentDirectory(), repo, folder.Replace('/', '_').Trim('_'))
                : Path.GetFullPath(options.Output);

            // 输出参数信息
            Log("========== 下载参数 ==========");
            Log($"GitHub URL: {options.Url}");
            Log($"URL类型: {urlInfo.UrlType}");
            Log($"本地路径: {localPath}");
            Log($"下载方式: {downloadMethod}");
            Log($"Owner: {owner}");
            Log($"Repo: {repo}");
            Log($"分支: {branch}");
            Log($"目录: {(string.IsNullOrEmpty(folder) ? "根目录" : folder)}");
            Log($"文件后缀过滤: {(extensions.Count > 0 ? string.Join(", ", extensions) : "无(下载全部)")}");
            Log($"加速服务: {(accelerationConfig.IsEnabled ? $"GH-Proxy ({accelerationConfig.Node})" : "未启用")}");
            Log($"代理模式: {proxyConfig.Mode}");
            Log($"代理地址: {proxyConfig.ProxyAddress ?? "无"}");
            Log($"认证方式: {(string.IsNullOrEmpty(options.Token) ? "无认证" : "Token认证")}");
            Log("================================");
            Log("开始下载...");

            // 创建下载器
            IDirectoryDownloader downloader;

            if (downloadMethod == DownloadMethod.LibGit2Sharp)
            {
                downloader = new LibGit2Downloader(
                    string.IsNullOrEmpty(options.Token) ? null : "x-token-auth",
                    options.Token,
                    proxyConfig,
                    accelerationConfig,
                    Log);
            }
            else
            {
                downloader = new OctokitDownloader(options.Token, proxyConfig, accelerationConfig, Log);
            }

            // 创建本地目录
            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
                Log($"创建本地目录: {localPath}");
            }

            // 尝试获取默认分支
            if (string.IsNullOrEmpty(options.Branch))
            {
                try
                {
                    var defaultBranch = await downloader.GetDefaultBranchAsync(owner, repo);
                    if (!string.IsNullOrEmpty(defaultBranch) && branch != defaultBranch)
                    {
                        branch = defaultBranch;
                        Log($"检测到默认分支: {branch}");
                    }
                }
                catch
                {
                    // 忽略错误，使用默认分支名
                }
            }

            // 创建进度报告
            var progress = new Progress<DownloadProgress>(p =>
            {
                if (!string.IsNullOrEmpty(p.CurrentFile))
                {
                    Log($"下载: {p.CurrentFile} ({p.DownloadedFiles}/{p.TotalFiles})");
                }
            });

            // 执行下载
            await downloader.DownloadDirectoryAsync(owner, repo, folder, localPath, branch, extensions, progress, CancellationToken.None);

            Log("下载完成！");
            Log($"文件已保存到: {localPath}");
        }

        /// <summary>
        /// 解析加速服务选项 (GH-Proxy)
        /// </summary>
        private static AccelerationConfig ParseAccelerationOption(string? accelerationOption)
        {
            if (string.IsNullOrWhiteSpace(accelerationOption) || accelerationOption == "none")
            {
                return new AccelerationConfig { IsEnabled = false };
            }

            // ghproxy 或 ghproxy:节点名
            if (accelerationOption.StartsWith("ghproxy"))
            {
                if (accelerationOption == "ghproxy")
                {
                    // 使用默认节点
                    return ProxyHelper.ParseAccelerationConfig(ProxyHelper.DefaultGhProxyNode);
                }
                
                // 解析节点名: ghproxy:hongkong
                var parts = accelerationOption.Split(':');
                if (parts.Length >= 2)
                {
                    var node = parts[1];
                    return ProxyHelper.ParseAccelerationConfig(node);
                }
            }

            return new AccelerationConfig { IsEnabled = false };
        }

        /// <summary>
        /// 解析代理选项 (传统代理)
        /// </summary>
        private static ProxyConfig ParseProxyOption(string? proxyOption)
        {
            if (string.IsNullOrWhiteSpace(proxyOption) || proxyOption == "none")
            {
                return ProxyHelper.ParseProxyConfig(ProxyMode.None);
            }

            if (proxyOption == "system")
            {
                return ProxyHelper.ParseProxyConfig(ProxyMode.System);
            }

            if (proxyOption.StartsWith("custom:"))
            {
                var address = proxyOption.Substring(7);
                return ProxyHelper.ParseProxyConfig(ProxyMode.Custom, address);
            }

            // 默认当作自定义代理地址
            return ProxyHelper.ParseProxyConfig(ProxyMode.Custom, proxyOption);
        }

        /// <summary>
        /// 解析文件后缀
        /// </summary>
        private static List<string> ParseExtensions(string? extensions)
        {
            if (string.IsNullOrWhiteSpace(extensions))
                return new List<string>();

            return extensions.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim().ToLowerInvariant())
                .Where(e => !string.IsNullOrEmpty(e))
                .Select(e => e.StartsWith(".") ? e : "." + e)
                .Distinct()
                .ToList();
        }
    }

    /// <summary>
    /// 命令行选项
    /// </summary>
    internal class CommandLineOptions
    {
        public string? Url { get; set; }
        public string? Output { get; set; }
        public string? Method { get; set; }  // null表示自动选择
        public string? Acceleration { get; set; }  // 加速服务 (GH-Proxy)
        public string? Proxy { get; set; }          // 传统代理
        public string? Branch { get; set; }
        public string? SubDir { get; set; }         // 子目录
        public string? Extensions { get; set; }
        public string? Token { get; set; }
    }
}
