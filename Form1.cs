using System.Text;
using System.Text.Json;
using LibGit2Sharp;
using Octokit;

namespace RS.GitSubDirectoryDownloader
{
    public partial class Form1 : Form
    {
        private const string TokensFileName = "tokens.txt";
        private const string ConfigFileName = "config.json";
        private const string LogFileName = "download.log";
        private const int MaxLogLines = 1000;
        private CancellationTokenSource? _cancellationTokenSource;
        private AppConfig _config = new();
        private static bool _sslConfigured = false;
        private readonly object _logLock = new();
        private SystemProxyInfo? _systemProxyInfo;
        private StreamWriter? _logWriter;
        private GitUrlInfo? _parsedUrlInfo;
        private bool _isPrivateRepo = false;
        private GitHubClient? _gitHubClient;

        public Form1()
        {
            InitializeComponent();
            ConfigureSslVerification();
        }

        /// <summary>
        /// 配置SSL验证 - 解决证书验证错误
        /// </summary>
        private static void ConfigureSslVerification()
        {
            if (_sslConfigured) return;

            try
            {
                Environment.SetEnvironmentVariable("GIT_SSL_NO_VERIFY", "1", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("LIBGIT2_SSL_CERTIFICATE_CHECK", "0", EnvironmentVariableTarget.Process);
                _sslConfigured = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SSL配置失败: {ex.Message}");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 初始化日志文件
            InitializeLogFile();

            // 检测系统代理
            _systemProxyInfo = ProxyHelper.GetSystemProxy();
            UpdateSystemProxyLink();

            LoadConfig();
            LoadTokens();
            UpdateAuthControls();

            AppendLog("程序启动完成");
            AppendLog("公开项目无需认证即可下载");

            // 默认选择不使用加速和不使用代理
            if (cmbAccelerationMode.SelectedIndex < 0)
            {
                cmbAccelerationMode.SelectedIndex = 0;
            }
            if (cmbProxyMode.SelectedIndex < 0)
            {
                cmbProxyMode.SelectedIndex = 0;
            }
            if (cmbDownloadMethod.SelectedIndex < 0)
            {
                cmbDownloadMethod.SelectedIndex = 0;
            }

            // 动态获取GH-Proxy节点（会自动初始化节点列表和设置SelectedIndex）
            _ = FetchAndUpdateGhProxyNodesAsync();
        }

        /// <summary>
        /// 异步获取并更新GH-Proxy节点列表
        /// </summary>
        private async Task FetchAndUpdateGhProxyNodesAsync()
        {
            // 获取配置中的节点名称
            var configNodeName = _config.GhProxyNode ?? "cloudflare";
            
            // 先初始化默认节点到UI
            this.SafeInvoke(() =>
            {
                cmbGhProxyNode.Items.Clear();
                foreach (var kvp in ProxyHelper.GhProxyNodes)
                {
                    cmbGhProxyNode.Items.Add($"{kvp.Key} - {kvp.Value.Description}");
                }
                if (cmbGhProxyNode.Items.Count > 0)
                {
                    // 根据配置设置选中的节点
                    var nodeIndex = GetNodeIndexByName(configNodeName);
                    cmbGhProxyNode.SelectedIndex = nodeIndex >= 0 && nodeIndex < cmbGhProxyNode.Items.Count ? nodeIndex : 0;
                }
            });

            // 然后尝试动态获取更新
            await ProxyHelper.FetchGhProxyNodesAsync(AppendLog);
            
            this.SafeInvoke(() =>
            {
                var currentNode = cmbGhProxyNode.SelectedIndex;
                cmbGhProxyNode.Items.Clear();
                foreach (var kvp in ProxyHelper.GhProxyNodes)
                {
                    cmbGhProxyNode.Items.Add($"{kvp.Key} - {kvp.Value.Description}");
                }
                if (cmbGhProxyNode.Items.Count > 0 && currentNode >= 0 && currentNode < cmbGhProxyNode.Items.Count)
                {
                    cmbGhProxyNode.SelectedIndex = currentNode;
                }
                else if (cmbGhProxyNode.Items.Count > 0)
                {
                    // 根据配置设置选中的节点
                    var nodeIndex = GetNodeIndexByName(configNodeName);
                    cmbGhProxyNode.SelectedIndex = nodeIndex >= 0 && nodeIndex < cmbGhProxyNode.Items.Count ? nodeIndex : 0;
                }
            });
        }

        /// <summary>
        /// 根据节点名称获取在列表中的索引
        /// </summary>
        private int GetNodeIndexByName(string nodeName)
        {
            var index = 0;
            foreach (var kvp in ProxyHelper.GhProxyNodes)
            {
                if (kvp.Key.Equals(nodeName, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
                index++;
            }
            return 0;
        }

        /// <summary>
        /// 初始化日志文件
        /// </summary>
        private void InitializeLogFile()
        {
            try
            {
                _logWriter = new StreamWriter(LogFileName, true, Encoding.UTF8) { AutoFlush = true };
                _logWriter.WriteLine("");
                _logWriter.WriteLine($"========== 程序启动 {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==========");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"日志文件初始化失败: {ex.Message}");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            SaveConfig();

            // 关闭日志文件
            try
            {
                _logWriter?.WriteLine($"========== 程序关闭 {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==========");
                _logWriter?.Close();
                _logWriter?.Dispose();
            }
            catch { }
        }

        #region 配置管理

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigFileName))
                {
                    var json = File.ReadAllText(ConfigFileName);
                    _config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();

                    txtGithubUrl.Text = _config.LastGithubUrl ?? "";
                    txtLocalPath.Text = _config.LastLocalPath ?? "";
                    txtUsername.Text = _config.LastUsername ?? "";
                    txtFileExtensions.Text = _config.FileExtensions ?? "";

                    // 加速模式 - 使用 AccelerationMode 枚举
                    if (cmbAccelerationMode.Items.Count > 0)
                    {
                        cmbAccelerationMode.SelectedIndex = (int)_config.AccelerationMode;
                    }
                    
                    // GH-Proxy节点 - 在FetchAndUpdateGhProxyNodesAsync中设置
                    // 因为此时Items可能还没有被填充

                    // 代理模式
                    if (cmbProxyMode.Items.Count > 0)
                    {
                        cmbProxyMode.SelectedIndex = (int)_config.ProxyMode;
                    }
                    txtProxyAddress.Text = _config.ProxyAddress ?? "";

                    switch (_config.AuthMode)
                    {
                        case AuthMode.Token:
                            rbToken.Checked = true;
                            break;
                        case AuthMode.Password:
                            rbPassword.Checked = true;
                            break;
                        default:
                            rbNoAuth.Checked = true;
                            break;
                    }

                    // 设置下载方式 (0=Octokit, 1=LibGit2Sharp)
                    if (cmbDownloadMethod.Items.Count > 0)
                    {
                        cmbDownloadMethod.SelectedIndex = _config.DownloadMethod == DownloadMethod.LibGit2Sharp ? 1 : 0;
                    }
                }
                else
                {
                    if (cmbDownloadMethod.Items.Count > 0) cmbDownloadMethod.SelectedIndex = 0;
                    if (cmbAccelerationMode.Items.Count > 0) cmbAccelerationMode.SelectedIndex = 0;
                    if (cmbProxyMode.Items.Count > 0) cmbProxyMode.SelectedIndex = 0;
                }
            }
            catch
            {
                if (cmbDownloadMethod.Items.Count > 0) cmbDownloadMethod.SelectedIndex = 0;
                if (cmbAccelerationMode.Items.Count > 0) cmbAccelerationMode.SelectedIndex = 0;
                if (cmbProxyMode.Items.Count > 0) cmbProxyMode.SelectedIndex = 0;
            }

            // 更新UI状态
            UpdateAccelerationControls();
            UpdateProxyControls();
            
            // 加载完成后更新加速URL
            UpdateAcceleratedUrl();
        }

        private void SaveConfig()
        {
            try
            {
                _config.LastGithubUrl = txtGithubUrl.Text.Trim();
                _config.LastLocalPath = txtLocalPath.Text.Trim();
                _config.ProxyMode = (ProxyMode)cmbProxyMode.SelectedIndex;
                _config.ProxyAddress = txtProxyAddress.Text.Trim();
                _config.LastUsername = txtUsername.Text.Trim();
                _config.FileExtensions = txtFileExtensions.Text.Trim();
                
                // 下载方式
                _config.DownloadMethod = cmbDownloadMethod.SelectedIndex == 1 ? DownloadMethod.LibGit2Sharp : DownloadMethod.Octokit;
                
                // 加速服务 - 使用 AccelerationMode 枚举
                _config.AccelerationMode = (AccelerationMode)cmbAccelerationMode.SelectedIndex;

                // GH-Proxy节点
                _config.GhProxyNode = cmbGhProxyNode.SelectedIndex switch
                {
                    1 => "cloudflare-v6",
                    2 => "hongkong",
                    3 => "fastly",
                    4 => "edgeone",
                    _ => "cloudflare"
                };

                if (rbToken.Checked)
                    _config.AuthMode = AuthMode.Token;
                else if (rbPassword.Checked)
                    _config.AuthMode = AuthMode.Password;
                else
                    _config.AuthMode = AuthMode.None;

                var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFileName, json);
            }
            catch { }
        }

        private void SaveTokenImmediately(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return;

            try
            {
                var tokens = new HashSet<string>();
                if (File.Exists(TokensFileName))
                {
                    tokens = new HashSet<string>(File.ReadAllLines(TokensFileName)
                        .Where(t => !string.IsNullOrWhiteSpace(t)));
                }

                tokens.Add(token);
                File.WriteAllLines(TokensFileName, tokens);
            }
            catch { }
        }

        #endregion

        #region Token管理

        private void LoadTokens()
        {
            try
            {
                if (File.Exists(TokensFileName))
                {
                    var tokens = File.ReadAllLines(TokensFileName)
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Distinct()
                        .ToList();
                    cmbToken.Items.Clear();
                    foreach (var token in tokens)
                    {
                        cmbToken.Items.Add(token);
                    }
                    if (cmbToken.Items.Count > 0)
                    {
                        cmbToken.SelectedIndex = 0;
                    }
                }
            }
            catch { }
        }

        #endregion

        #region 认证模式切换

        private void RbAuth_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAuthControls();
        }

        private void UpdateAuthControls()
        {
            lblUsername.Enabled = txtUsername.Enabled = false;
            lblPassword.Enabled = txtPassword.Enabled = false;
            lblToken.Enabled = cmbToken.Enabled = false;

            if (rbToken.Checked)
            {
                lblToken.Enabled = cmbToken.Enabled = true;
                AppendLog("已选择Token认证 - 将以 x-token-auth 作为用户名");
            }
            else if (rbPassword.Checked)
            {
                lblUsername.Enabled = txtUsername.Enabled = true;
                lblPassword.Enabled = txtPassword.Enabled = true;
                AppendLog("已选择账号密码认证");
            }
            else
            {
                AppendLog("已选择无认证模式 - 公开项目无需认证");
            }
        }

        #endregion

        #region 加速服务设置

        private void CmbAccelerationMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateAccelerationControls();
            UpdateAcceleratedUrl();
        }

        private void UpdateAccelerationControls()
        {
            var useAcceleration = cmbAccelerationMode.SelectedIndex == 1;

            lblGhProxyNode.Enabled = cmbGhProxyNode.Enabled = useAcceleration;
            txtAcceleratedUrl.ReadOnly = !useAcceleration;
            
            if (!useAcceleration)
            {
                txtAcceleratedUrl.Text = "";
                txtAcceleratedUrl.PlaceholderText = "启用加速后显示最终URL，可手动修改";
            }

            if (useAcceleration)
            {
                AppendLog($"已启用GH-Proxy加速 - 节点: {cmbGhProxyNode.Text}");
            }
            else
            {
                AppendLog("已禁用加速服务");
            }
        }

        /// <summary>
        /// GH-Proxy节点选择变化时更新加速URL
        /// </summary>
        private void CmbGhProxyNode_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateAcceleratedUrl();
        }

        /// <summary>
        /// 根据当前设置更新加速后的URL
        /// </summary>
        private void UpdateAcceleratedUrl()
        {
            if (cmbAccelerationMode.SelectedIndex != 1)
            {
                txtAcceleratedUrl.Text = "";
                return;
            }

            var githubUrl = txtGithubUrl.Text.Trim();
            if (string.IsNullOrEmpty(githubUrl))
            {
                txtAcceleratedUrl.Text = "";
                txtAcceleratedUrl.PlaceholderText = "请先输入GitHub URL";
                return;
            }

            // 获取当前选择的节点URL
            var nodeKey = cmbGhProxyNode.SelectedIndex switch
            {
                1 => "cloudflare-v6",
                2 => "hongkong",
                3 => "fastly",
                4 => "edgeone",
                _ => "cloudflare"
            };

            var proxyPrefix = ProxyHelper.GetNodeUrl(nodeKey);
            var acceleratedUrl = $"{proxyPrefix.TrimEnd('/')}/{githubUrl}";
            txtAcceleratedUrl.Text = acceleratedUrl;
            txtAcceleratedUrl.PlaceholderText = "";
            AppendLog($"加速URL已生成: {acceleratedUrl}");
        }

        /// <summary>
        /// 获取当前加速配置
        /// </summary>
        private AccelerationConfig GetAccelerationConfig()
        {
            if (cmbAccelerationMode.SelectedIndex != 1)
            {
                return new AccelerationConfig { IsEnabled = false };
            }

            var acceleratedUrl = txtAcceleratedUrl.Text.Trim();
            if (string.IsNullOrEmpty(acceleratedUrl))
            {
                return new AccelerationConfig { IsEnabled = false };
            }

            // 从加速URL中提取前缀
            return ProxyHelper.ParseAccelerationConfig(acceleratedUrl);
        }

        #endregion

        #region 代理设置

        private void CmbProxyMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateProxyControls();
        }

        private void UpdateProxyControls()
        {
            var mode = (ProxyMode)cmbProxyMode.SelectedIndex;

            // 重置所有控件状态 - 全部隐藏
            lblProxyAddress.Visible = txtProxyAddress.Visible = false;
            lblSystemProxyStatus.Visible = lnkSystemProxy.Visible = false;

            switch (mode)
            {
                case ProxyMode.System:
                    // 系统代理：显示系统代理状态
                    lblSystemProxyStatus.Visible = lnkSystemProxy.Visible = true;
                    lnkSystemProxy.Enabled = true;
                    if (_systemProxyInfo?.IsEnabled == true && !string.IsNullOrEmpty(_systemProxyInfo.ProxyAddress))
                    {
                        AppendLog($"已选择系统代理: {_systemProxyInfo.ProxyAddress}");
                    }
                    else
                    {
                        AppendLog("已选择系统代理，但未检测到系统代理设置");
                    }
                    break;

                case ProxyMode.Custom:
                    // 自定义代理：显示代理地址输入框
                    lblProxyAddress.Visible = txtProxyAddress.Visible = true;
                    lblProxyAddress.Enabled = txtProxyAddress.Enabled = true;
                    AppendLog("已选择自定义代理 - 请输入代理地址");
                    break;

                default:
                    AppendLog("已选择不使用代理");
                    break;
            }
        }

        private void UpdateSystemProxyLink()
        {
            if (_systemProxyInfo?.IsEnabled == true && !string.IsNullOrEmpty(_systemProxyInfo.ProxyAddress))
            {
                lnkSystemProxy.Text = $"检测到系统代理({_systemProxyInfo.ProxyAddress}) 点击复制到自定义代理";
                lnkSystemProxy.LinkColor = Color.Green;
            }
            else
            {
                lnkSystemProxy.Text = "未检测到系统代理设置";
                lnkSystemProxy.LinkColor = Color.Gray;
            }
        }

        private void LnkSystemProxy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_systemProxyInfo?.IsEnabled == true && !string.IsNullOrEmpty(_systemProxyInfo.ProxyAddress))
            {
                // 点击后切换到自定义代理并填入系统代理地址
                cmbProxyMode.SelectedIndex = (int)ProxyMode.Custom;
                txtProxyAddress.Text = _systemProxyInfo.ProxyAddress;
                AppendLog($"已将系统代理地址填入自定义代理: {_systemProxyInfo.ProxyAddress}");
            }
            else
            {
                MessageBox.Show("未检测到系统代理设置。\n\n您可以在系统设置 > 网络 > 代理 中配置代理后重试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 获取当前代理配置
        /// </summary>
        private ProxyConfig GetProxyConfig()
        {
            var mode = (ProxyMode)cmbProxyMode.SelectedIndex;

            switch (mode)
            {
                case ProxyMode.System:
                    return ProxyHelper.ParseProxyConfig(ProxyMode.System);

                case ProxyMode.Custom:
                    return ProxyHelper.ParseProxyConfig(ProxyMode.Custom, txtProxyAddress.Text.Trim());

                default:
                    return ProxyHelper.ParseProxyConfig(ProxyMode.None);
            }
        }

        #endregion

        #region URL解析

        /// <summary>
        /// 解析URL按钮点击事件
        /// </summary>
        private async void BtnParseUrl_Click(object sender, EventArgs e)
        {
            var url = txtGithubUrl.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("请输入GitHub URL", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnParseUrl.Enabled = false;
            btnParseUrl.Text = "解析中...";

            try
            {
                // 使用新的解析器
                _parsedUrlInfo = GitUrlParser.Parse(url);

                if (!_parsedUrlInfo.IsValid)
                {
                    lblParsedInfo.Text = $"❌ {_parsedUrlInfo.ErrorMessage}";
                    lblParsedInfo.ForeColor = Color.Red;
                    lblParsedInfo.Visible = true;
                    ClearRepoInfo();
                    chkDownloadSubDir.Visible = false;
                    return;
                }

                // 更新UI显示解析结果
                UpdateParsedInfoUI(_parsedUrlInfo);

                // 尝试获取仓库信息
                await FetchRepositoryInfoAsync(_parsedUrlInfo);

                AppendLog($"URL解析成功: {_parsedUrlInfo.FullDescription}");
            }
            catch (Exception ex)
            {
                lblParsedInfo.Text = $"❌ 解析失败: {ex.Message}";
                lblParsedInfo.ForeColor = Color.Red;
                lblParsedInfo.Visible = true;
                AppendLog($"URL解析失败: {ex.Message}");
            }
            finally
            {
                btnParseUrl.Enabled = true;
                btnParseUrl.Text = "解析URL";
            }
        }

        /// <summary>
        /// 更新解析信息UI
        /// </summary>
        private void UpdateParsedInfoUI(GitUrlInfo info)
        {
            // 更新仓库信息
            txtOwner.Text = info.Owner;
            txtRepoName.Text = info.RepoName;
            txtFolder.Text = info.SubDirectory;
            cmbBranch.Text = info.Ref ?? "";

            // 显示解析信息标签
            var infoText = $"✓ {info.DisplayName}";
            if (info.HasRef)
            {
                infoText += $" [{info.RefType}: {info.Ref}]";
            }
            if (info.HasSubDirectory)
            {
                infoText += $" → {info.SubDirectory}";
                chkDownloadSubDir.Visible = true;
                chkDownloadSubDir.Text = $"仅下载子目录: {info.SubDirectory}";
            }
            else
            {
                chkDownloadSubDir.Visible = false;
            }

            lblParsedInfo.Text = infoText;
            lblParsedInfo.ForeColor = Color.FromArgb(0, 122, 204);
            lblParsedInfo.Visible = true;

            // 根据URL类型自动选择下载方式
            // Unity Package Manager风格(.git) -> LibGit2Sharp
            // GitHub Web URL -> Octokit
            AutoSelectDownloadMethod(info);
            
            // 更新加速URL（如果已启用加速）
            UpdateAcceleratedUrl();
        }

        /// <summary>
        /// 根据URL类型和子目录选项自动选择下载方式
        /// </summary>
        private void AutoSelectDownloadMethod(GitUrlInfo info)
        {
            // 如果有子目录且勾选了"仅下载子目录"，必须使用Octokit
            var downloadSubDirOnly = chkDownloadSubDir.Checked && chkDownloadSubDir.Visible;
            
            if (downloadSubDirOnly)
            {
                // Octokit支持单独下载子目录
                if (cmbDownloadMethod.SelectedIndex != 0)
                {
                    cmbDownloadMethod.SelectedIndex = 0; // Octokit
                    AppendLog("仅下载子目录模式，自动选择Octokit");
                }
                return;
            }
            
            // 没有子目录或未勾选"仅下载子目录"，根据URL类型选择
            if (info.UrlType == GitUrlType.GitPackage)
            {
                // Unity Package Manager风格 -> LibGit2Sharp（完整克隆）
                if (cmbDownloadMethod.SelectedIndex != 1)
                {
                    cmbDownloadMethod.SelectedIndex = 1; // LibGit2Sharp
                    AppendLog("Unity Package Manager风格URL，自动选择LibGit2Sharp");
                }
            }
            else
            {
                // GitHub Web URL -> Octokit
                if (cmbDownloadMethod.SelectedIndex != 0)
                {
                    cmbDownloadMethod.SelectedIndex = 0; // Octokit
                    AppendLog("GitHub Web URL，自动选择Octokit");
                }
            }
        }

        /// <summary>
        /// 更新下载方式选项 - 根据子目录选项自动切换
        /// </summary>
        private void UpdateDownloadMethodOptions()
        {
            if (_parsedUrlInfo == null) return;
            
            // 重新根据当前状态选择下载方式
            AutoSelectDownloadMethod(_parsedUrlInfo);
        }

        /// <summary>
        /// 子目录下载复选框状态改变
        /// </summary>
        private void ChkDownloadSubDir_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDownloadMethodOptions();
        }

        /// <summary>
        /// 分支选择变化时获取Commits列表
        /// </summary>
        private async void CmbBranch_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedBranch = cmbBranch.Text.Trim();
            if (string.IsNullOrEmpty(selectedBranch) || selectedBranch.StartsWith("──"))
            {
                return;
            }

            if (_parsedUrlInfo == null || string.IsNullOrEmpty(_parsedUrlInfo.Owner) || string.IsNullOrEmpty(_parsedUrlInfo.RepoName))
            {
                return;
            }

            await FetchCommitsAsync(_parsedUrlInfo.Owner, _parsedUrlInfo.RepoName, selectedBranch);
        }

        /// <summary>
        /// 获取仓库信息（分支、标签等）
        /// </summary>
        private async Task FetchRepositoryInfoAsync(GitUrlInfo info)
        {
            try
            {
                var token = cmbToken.Text.Trim();
                _gitHubClient = string.IsNullOrEmpty(token) 
                    ? new GitHubClient(new ProductHeaderValue("GitHubDownloader", "1.0"))
                    : new GitHubClient(new ProductHeaderValue("GitHubDownloader", "1.0"))
                    {
                        Credentials = new Octokit.Credentials(token)
                    };

                // 获取仓库信息
                var repository = await _gitHubClient.Repository.Get(info.Owner, info.RepoName);
                
                // 检查是否为私有仓库
                _isPrivateRepo = repository.Private;
                if (_isPrivateRepo == true)
                {
                    AppendLog("⚠️ 检测到私有仓库，需要认证才能下载");
                    rbToken.Checked = true;
                }

                // 获取默认分支
                var defaultBranch = repository.DefaultBranch;
                if (!string.IsNullOrEmpty(defaultBranch))
                {
                    AppendLog($"检测到默认分支: {defaultBranch}");
                }

                // 获取分支列表
                var branches = await _gitHubClient.Repository.Branch.GetAll(info.Owner, info.RepoName);
                if (branches.Count > 0)
                {
                    cmbBranch.Items.Clear();
                    foreach (var branch in branches)
                    {
                        cmbBranch.Items.Add(branch.Name);
                    }
                    AppendLog($"获取到 {branches.Count} 个分支");
                }

                // 获取标签列表
                var tags = await _gitHubClient.Repository.GetAllTags(info.Owner, info.RepoName);
                if (tags.Count > 0)
                {
                    // 在分支后添加分隔和标签
                    if (cmbBranch.Items.Count > 0)
                    {
                        cmbBranch.Items.Add("── 标签 ──");
                    }
                    foreach (var tag in tags)
                    {
                        cmbBranch.Items.Add(tag.Name);
                    }
                    AppendLog($"获取到 {tags.Count} 个标签");
                }

                // 设置当前选中的分支
                if (!string.IsNullOrEmpty(info.Ref))
                {
                    cmbBranch.Text = info.Ref;
                }
                else if (!string.IsNullOrEmpty(defaultBranch))
                {
                    cmbBranch.Text = defaultBranch;
                }
            }
            catch (Exception ex)
            {
                AppendLog($"获取仓库信息失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取指定分支的Commits列表
        /// </summary>
        private async Task FetchCommitsAsync(string owner, string repo, string branch)
        {
            try
            {
                if (_gitHubClient == null) return;

                AppendLog($"正在获取分支 {branch} 的Commits列表...");

                var commits = await _gitHubClient.Repository.Commit.GetAll(owner, repo, new CommitRequest
                {
                    Sha = branch
                }, new ApiOptions { PageSize = 20, PageCount = 1 });

                cmbCommit.Items.Clear();
                foreach (var commit in commits)
                {
                    var shortSha = commit.Sha.Substring(0, 7);
                    var message = commit.Commit.Message.Split('\n')[0];
                    if (message.Length > 40) message = message.Substring(0, 40) + "...";
                    cmbCommit.Items.Add($"{shortSha} - {message}");
                }

                if (cmbCommit.Items.Count > 0)
                {
                    cmbCommit.SelectedIndex = 0;
                }

                AppendLog($"获取到 {commits.Count} 个Commits");
            }
            catch (Exception ex)
            {
                AppendLog($"获取Commits失败: {ex.Message}");
            }
        }

        private void ClearRepoInfo()
        {
            txtOwner.Text = "";
            txtRepoName.Text = "";
            txtFolder.Text = "";
            cmbBranch.Text = "";
            cmbBranch.Items.Clear();
            cmbCommit.Text = "";
            cmbCommit.Items.Clear();
        }

        private (string owner, string repo, string branch, string folder) ParseGitHubUrlInfo(string url)
        {
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 2)
                throw new ArgumentException("URL格式错误");

            var owner = segments[0];
            var repo = segments[1];
            string branch = "";
            string folder = "";

            int treeIndex = Array.FindIndex(segments, s => s == "tree" || s == "blob");
            if (treeIndex >= 0 && treeIndex + 1 < segments.Length)
            {
                branch = segments[treeIndex + 1];
                if (treeIndex + 2 < segments.Length)
                {
                    folder = string.Join("/", segments, treeIndex + 2, segments.Length - treeIndex - 2);
                }
            }

            return (owner, repo, branch, folder);
        }

        #endregion

        #region UI事件

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                txtLocalPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private async void BtnDownload_Click(object sender, EventArgs e)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                btnDownload.SafeSetText("开始下载");
                btnDownload.SafeSetBackColor(Color.FromArgb(0, 122, 204));
                _cancellationTokenSource = null;
                return;
            }

            // 在UI线程获取所有需要的值
            var githubUrl = txtGithubUrl.Text.Trim();
            var localPath = txtLocalPath.Text.Trim();
            var selectedBranch = cmbBranch.Text.Trim();
            var selectedCommit = cmbCommit.Text.Trim();
            var isTokenMode = rbToken.Checked;
            var isPasswordMode = rbPassword.Checked;
            var token = cmbToken.Text.Trim();
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text;
            var downloadMethod = cmbDownloadMethod.SelectedIndex == 1 ? DownloadMethod.LibGit2Sharp : DownloadMethod.Octokit;
            var fileExtensions = txtFileExtensions.Text.Trim();
            var downloadSubDirOnly = chkDownloadSubDir.Checked && chkDownloadSubDir.Visible;

            // 获取加速配置
            var accelerationConfig = GetAccelerationConfig();

            // 获取代理配置
            var proxyConfig = GetProxyConfig();

            // 验证输入
            if (string.IsNullOrWhiteSpace(githubUrl))
            {
                MessageBox.Show("请输入GitHub目录地址", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(localPath))
            {
                MessageBox.Show("请选择本地保存路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (proxyConfig.Mode == ProxyMode.Custom && string.IsNullOrWhiteSpace(proxyConfig.ProxyAddress))
            {
                MessageBox.Show("请输入代理地址", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 如果没有解析过URL，先解析
            if (_parsedUrlInfo == null || _parsedUrlInfo.OriginalUrl != githubUrl)
            {
                _parsedUrlInfo = GitUrlParser.Parse(githubUrl);
                if (!_parsedUrlInfo.IsValid)
                {
                    MessageBox.Show($"URL格式错误: {_parsedUrlInfo.ErrorMessage}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // 获取认证信息
            string? authUsername = null;
            string? authPassword = null;

            if (isTokenMode)
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    MessageBox.Show("请输入GitHub Token", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                authUsername = "x-token-auth";
                authPassword = token;

                SaveTokenImmediately(token);
                LoadTokens();
            }
            else if (isPasswordMode)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    MessageBox.Show("请输入GitHub用户名", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("请输入GitHub密码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                authUsername = username;
                authPassword = password;
            }

            SaveConfig();

            _cancellationTokenSource = new CancellationTokenSource();
            btnDownload.SafeSetText("取消");
            btnDownload.SafeSetBackColor(Color.FromArgb(220, 53, 69));

            try
            {
                await DownloadGitHubDirectoryAsync(_parsedUrlInfo, localPath, authUsername, authPassword, accelerationConfig, proxyConfig, selectedBranch, selectedCommit, downloadMethod, fileExtensions, downloadSubDirOnly, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                AppendLog("下载已取消");
            }
            catch (Exception ex)
            {
                AppendLog($"下载失败: {ex.Message}");
                MessageBox.Show($"下载失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _cancellationTokenSource = null;
                btnDownload.SafeSetText("开始下载");
                btnDownload.SafeSetBackColor(Color.FromArgb(0, 122, 204));
                progressBar.SafeSetValue(0);
            }
        }

        #endregion

        #region GitHub下载逻辑

        private async Task DownloadGitHubDirectoryAsync(GitUrlInfo urlInfo, string localPath, string? username, string? password, AccelerationConfig accelerationConfig, ProxyConfig proxyConfig, string selectedBranch, string selectedCommit, DownloadMethod downloadMethod, string fileExtensions, bool downloadSubDirOnly, CancellationToken cancellationToken)
        {
            var owner = urlInfo.Owner;
            var repo = urlInfo.RepoName;
            
            // 确定分支/提交
            string branch;
            if (!string.IsNullOrEmpty(selectedCommit) && selectedCommit.Contains(" - "))
            {
                // 从Commit选项中提取SHA
                branch = selectedCommit.Split(' ')[0];
            }
            else
            {
                branch = urlInfo.Ref ?? selectedBranch;
            }
            
            var folder = downloadSubDirOnly ? urlInfo.SubDirectory : "";

            if (string.IsNullOrEmpty(branch)) branch = "main";

            // 解析文件后缀过滤
            var extensions = ParseFileExtensions(fileExtensions);

            // 构建代理地址字符串（用于文件下载）
            string? proxyAddress = null;
            string? accelerationPrefix = null;
            
            if (proxyConfig.HasProxy)
            {
                proxyAddress = proxyConfig.ProxyAddress;
            }
            
            if (accelerationConfig.IsEnabled)
            {
                accelerationPrefix = accelerationConfig.Prefix;
            }

            // ========== 输出所有下载参数 ==========
            AppendLog("========== 下载参数 ==========");
            AppendLog($"GitHub URL: {urlInfo.OriginalUrl}");
            AppendLog($"本地路径: {localPath}");
            AppendLog($"下载方式: {downloadMethod}");
            AppendLog($"Owner: {owner}");
            AppendLog($"Repo: {repo}");
            AppendLog($"分支/Commit: {branch}");
            AppendLog($"子目录: {(string.IsNullOrEmpty(folder) ? "无(克隆整个仓库)" : folder)}");
            AppendLog($"仅下载子目录: {downloadSubDirOnly}");
            AppendLog($"文件后缀过滤: {(extensions.Count > 0 ? string.Join(", ", extensions) : "无(下载全部)")}");
            AppendLog($"加速服务: {(accelerationConfig.IsEnabled ? $"GH-Proxy ({accelerationConfig.Node})" : "未启用")}");
            AppendLog($"代理模式: {proxyConfig.Mode}");
            AppendLog($"代理地址: {proxyAddress ?? "无"}");
            AppendLog($"认证方式: {(string.IsNullOrEmpty(password) ? "无认证" : (username == "x-token-auth" ? "Token认证" : "账号密码认证"))}");
            AppendLog("================================");

            // 如果没有子目录或不需要仅下载子目录，使用 LibGit2Sharp 克隆整个仓库
            if (string.IsNullOrEmpty(folder) && downloadMethod == DownloadMethod.LibGit2Sharp)
            {
                await CloneEntireRepositoryAsync(urlInfo, localPath, username, password, accelerationPrefix, proxyAddress, branch, cancellationToken);
                return;
            }

            // 创建下载器实例 (只使用Octokit)
            IDirectoryDownloader downloader;
            
            downloader = new OctokitDownloader(password, proxyConfig, accelerationConfig, AppendLog);

            AppendLog($"开始下载...");

            try
            {
                if (!Directory.Exists(localPath))
                {
                    Directory.CreateDirectory(localPath);
                    AppendLog($"创建本地目录: {localPath}");
                }

                // 尝试获取默认分支
                if (string.IsNullOrEmpty(selectedBranch) || selectedBranch == "main" || selectedBranch == "master")
                {
                    try
                    {
                        var defaultBranch = await downloader.GetDefaultBranchAsync(owner, repo);
                        if (!string.IsNullOrEmpty(defaultBranch) && branch != defaultBranch)
                        {
                            branch = defaultBranch;
                            AppendLog($"检测到默认分支: {branch}");
                        }
                    }
                    catch
                    {
                        // 忽略错误，使用默认分支名
                    }
                }

                var progress = new Progress<DownloadProgress>(p =>
                {
                    progressBar.SafeSetMaximum(p.TotalFiles);
                    progressBar.SafeSetValue(p.DownloadedFiles);
                    if (!string.IsNullOrEmpty(p.CurrentFile))
                    {
                        AppendLog($"下载: {p.CurrentFile} ({p.DownloadedFiles}/{p.TotalFiles})");
                    }
                });

                await downloader.DownloadDirectoryAsync(owner, repo, folder, localPath, branch, extensions, progress, cancellationToken);

                AppendLog("下载完成！");
                this.SafeInvoke(() => MessageBox.Show($"下载完成！\n文件已保存到: {localPath}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information));
            }
            catch (Exception ex)
            {
                AppendLog($"{downloadMethod}下载失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 克隆整个仓库
        /// </summary>
        private async Task CloneEntireRepositoryAsync(GitUrlInfo urlInfo, string localPath, string? username, string? password, string? accelerationPrefix, string? proxyAddress, string branch, CancellationToken cancellationToken)
        {
            AppendLog("使用LibGit2Sharp克隆整个仓库...");

            // 构建仓库URL（支持加速服务）
            var repoUrl = $"https://github.com/{urlInfo.Owner}/{urlInfo.RepoName}.git";
            
            // 如果启用加速服务，转换URL
            if (!string.IsNullOrEmpty(accelerationPrefix))
            {
                repoUrl = $"{accelerationPrefix.TrimEnd('/')}/{repoUrl}";
                AppendLog($"已应用加速服务: {repoUrl}");
            }
            
            AppendLog($"仓库地址: {repoUrl}");

            await Task.Run(() =>
            {
                try
                {
                    var tempDir = localPath;

                    if (LibGit2Sharp.Repository.IsValid(localPath))
                    {
                        AppendLog($"目录已存在Git仓库，将进行更新...");
                        using var repo = new LibGit2Sharp.Repository(localPath);
                        Commands.Pull(repo, new LibGit2Sharp.Signature("downloader", "downloader@local", DateTimeOffset.Now), new PullOptions());
                        AppendLog("仓库更新完成！");
                    }
                    else
                    {
                        AppendLog($"正在克隆仓库...");

                        var cloneOptions = new CloneOptions();

                        // SSL证书验证 - 跳过所有证书检查
                        cloneOptions.FetchOptions.CertificateCheck = (certificate, host, valid) =>
                        {
                            AppendLog($"SSL证书检查: {host} - 已跳过验证");
                            return true;
                        };

                        // 配置认证
                        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                        {
                            cloneOptions.FetchOptions.CredentialsProvider = (_url, _user, _cred) =>
                                new UsernamePasswordCredentials
                                {
                                    Username = username,
                                    Password = password
                                };
                        }

                        // 配置传统代理（注意：不使用加速服务URL作为代理）
                        if (!string.IsNullOrEmpty(proxyAddress))
                        {
                            cloneOptions.FetchOptions.ProxyOptions.Url = proxyAddress;
                        }

                        // 指定分支
                        cloneOptions.BranchName = branch;

                        var clonedRepoPath = LibGit2Sharp.Repository.Clone(repoUrl, tempDir, cloneOptions);
                        AppendLog($"克隆完成: {clonedRepoPath}");
                    }

                    AppendLog("下载完成！");
                    this.SafeInvoke(() => MessageBox.Show($"下载完成！\n仓库已克隆到: {localPath}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information));
                }
                catch (Exception ex)
                {
                    AppendLog($"克隆失败: {ex.Message}");
                    throw;
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 解析文件后缀字符串为列表
        /// </summary>
        private List<string> ParseFileExtensions(string extensions)
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

        #endregion

        #region 日志

        /// <summary>
        /// 追加日志消息 - 同时输出到界面和文件
        /// </summary>
        private void AppendLog(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logLine = $"[{timestamp}] {message}";
            var fullLogLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

            // 写入日志文件
            try
            {
                _logWriter?.WriteLine(fullLogLine);
            }
            catch { }

            this.SafeInvoke(() =>
            {
                lock (_logLock)
                {
                    txtLog.AppendText(logLine + Environment.NewLine);

                    // 限制日志行数
                    var lines = txtLog.Lines;
                    if (lines.Length > MaxLogLines)
                    {
                        var newLines = new string[MaxLogLines];
                        Array.Copy(lines, lines.Length - MaxLogLines, newLines, 0, MaxLogLines);
                        txtLog.Lines = newLines;
                    }

                    // 滚动到底部
                    txtLog.SelectionStart = txtLog.Text.Length;
                    txtLog.ScrollToCaret();
                }
            });
        }

        #endregion
    }

    public enum AuthMode
    {
        None,
        Token,
        Password
    }

    public class AppConfig
    {
        public string? LastGithubUrl { get; set; }
        public string? LastLocalPath { get; set; }
        public ProxyMode ProxyMode { get; set; }
        public string? ProxyAddress { get; set; }
        public string? GhProxyNode { get; set; }
        public AccelerationMode AccelerationMode { get; set; } = AccelerationMode.None;
        public AuthMode AuthMode { get; set; }
        public string? LastUsername { get; set; }
        public DownloadMethod DownloadMethod { get; set; } = DownloadMethod.Octokit;
        public string? FileExtensions { get; set; }
    }

    public enum DownloadMethod
    {
        Octokit,
        LibGit2Sharp
    }
}
