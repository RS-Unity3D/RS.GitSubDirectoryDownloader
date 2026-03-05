using System.Text.RegularExpressions;

namespace RS.GitSubDirectoryDownloader
{
    /// <summary>
    /// Git URL 解析器 - 支持 Unity Package Manager 风格的 URL 格式
    /// 支持格式：
    /// - https://github.com/user/repo.git
    /// - https://github.com/user/repo.git#v1.0.0
    /// - https://github.com/user/repo.git#main
    /// - https://github.com/user/repo.git#commit_hash
    /// - https://github.com/user/repo.git?path=Assets/Plugins
    /// - https://github.com/user/repo.git#v1.0.0?path=Assets/Plugins
    /// - https://github.com/user/repo/tree/main/Assets/Folder (GitHub Web URL)
    /// </summary>
    public class GitUrlParser
    {
        /// <summary>
        /// 解析 Git URL
        /// </summary>
        public static GitUrlInfo Parse(string url)
        {
            var info = new GitUrlInfo { OriginalUrl = url };

            if (string.IsNullOrWhiteSpace(url))
            {
                info.IsValid = false;
                return info;
            }

            url = url.Trim();

            // 检查是否为 GH-Proxy 格式
            if (ProxyHelper.IsGhProxyUrl(url))
            {
                url = ProxyHelper.ExtractOriginalUrl(url);
                info.IsGhProxyUrl = true;
            }

            // 检查是否为 .git 格式 (Unity Package Manager 风格)
            if (url.Contains(".git"))
            {
                ParseGitStyleUrl(url, info);
            }
            // 检查是否为 GitHub Web URL 格式
            else if (url.Contains("github.com"))
            {
                ParseGitHubWebUrl(url, info);
            }
            else
            {
                info.IsValid = false;
                info.ErrorMessage = "不支持的URL格式";
            }

            return info;
        }

        /// <summary>
        /// 解析 .git 风格 URL
        /// 格式: https://github.com/user/repo.git#branch?path=folder
        /// </summary>
        private static void ParseGitStyleUrl(string url, GitUrlInfo info)
        {
            try
            {
                // 提取基础 URL（到 .git 为止）
                var gitIndex = url.IndexOf(".git");
                var baseUrl = url.Substring(0, gitIndex + 4);

                // 解析基础 URL
                ParseGitHubBaseUrl(baseUrl, info);

                // 获取剩余部分（#branch?path=folder）
                var remaining = url.Substring(gitIndex + 4);

                // 解析 # 和 ?
                if (!string.IsNullOrEmpty(remaining))
                {
                    // 分离 ref 和 path
                    var hashIndex = remaining.IndexOf('#');
                    var queryIndex = remaining.IndexOf('?');

                    if (hashIndex >= 0)
                    {
                        var endIndex = queryIndex > hashIndex ? queryIndex : remaining.Length;
                        info.Ref = remaining.Substring(hashIndex + 1, endIndex - hashIndex - 1);

                        // 判断 Ref 类型
                        if (Regex.IsMatch(info.Ref, @"^[0-9a-f]{40}$", RegexOptions.IgnoreCase))
                        {
                            info.RefType = GitRefType.Commit;
                        }
                        else if (info.Ref.StartsWith("v") || Regex.IsMatch(info.Ref, @"^\d"))
                        {
                            info.RefType = GitRefType.Tag;
                        }
                        else
                        {
                            info.RefType = GitRefType.Branch;
                        }
                    }

                    if (queryIndex >= 0)
                    {
                        var query = remaining.Substring(queryIndex + 1);
                        var pathMatch = Regex.Match(query, @"path=([^&]+)");
                        if (pathMatch.Success)
                        {
                            info.SubDirectory = pathMatch.Groups[1].Value;
                        }
                    }
                }

                info.IsValid = true;
                info.UrlType = GitUrlType.GitPackage;
            }
            catch (Exception ex)
            {
                info.IsValid = false;
                info.ErrorMessage = $"解析失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 解析 GitHub Web URL
        /// 格式: https://github.com/user/repo/tree/main/Assets/Folder
        /// </summary>
        private static void ParseGitHubWebUrl(string url, GitUrlInfo info)
        {
            try
            {
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                if (segments.Length < 2)
                {
                    info.IsValid = false;
                    info.ErrorMessage = "URL格式错误";
                    return;
                }

                info.Owner = segments[0];
                info.RepoName = segments[1].Replace(".git", "");
                info.UrlType = GitUrlType.GitHubWeb;

                // 查找 tree 或 blob 关键字
                int treeIndex = Array.FindIndex(segments, s => s == "tree" || s == "blob");
                if (treeIndex >= 0 && treeIndex + 1 < segments.Length)
                {
                    info.Ref = segments[treeIndex + 1];
                    info.RefType = GitRefType.Branch; // 默认为分支

                    // 提取子目录
                    if (treeIndex + 2 < segments.Length)
                    {
                        info.SubDirectory = string.Join("/", segments, treeIndex + 2, segments.Length - treeIndex - 2);
                    }
                }

                info.IsValid = true;
            }
            catch (Exception ex)
            {
                info.IsValid = false;
                info.ErrorMessage = $"解析失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 解析基础 GitHub URL
        /// </summary>
        private static void ParseGitHubBaseUrl(string url, GitUrlInfo info)
        {
            var cleanUrl = url.Replace(".git", "");
            var uri = new Uri(cleanUrl);
            var segments = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length >= 2)
            {
                info.Owner = segments[0];
                info.RepoName = segments[1];
            }
        }

        /// <summary>
        /// 构建 Git 克隆 URL
        /// </summary>
        public static string BuildGitCloneUrl(GitUrlInfo info, string? ghProxyPrefix = null)
        {
            var url = $"https://github.com/{info.Owner}/{info.RepoName}.git";

            if (!string.IsNullOrEmpty(ghProxyPrefix))
            {
                url = $"{ghProxyPrefix.TrimEnd('/')}/{url}";
            }

            return url;
        }

        /// <summary>
        /// 构建 GitHub API 内容 URL
        /// </summary>
        public static string BuildApiUrl(GitUrlInfo info)
        {
            return $"https://api.github.com/repos/{info.Owner}/{info.RepoName}";
        }

        /// <summary>
        /// 构建 GitHub Web URL
        /// </summary>
        public static string BuildWebUrl(GitUrlInfo info)
        {
            var refPart = string.IsNullOrEmpty(info.Ref) ? "" : $"/tree/{info.Ref}";
            var pathPart = string.IsNullOrEmpty(info.SubDirectory) ? "" : $"/{info.SubDirectory}";
            return $"https://github.com/{info.Owner}/{info.RepoName}{refPart}{pathPart}";
        }
    }

    /// <summary>
    /// Git URL 信息
    /// </summary>
    public class GitUrlInfo
    {
        public string OriginalUrl { get; set; } = "";
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = "";
        public GitUrlType UrlType { get; set; }
        public bool IsGhProxyUrl { get; set; }

        // 仓库信息
        public string Owner { get; set; } = "";
        public string RepoName { get; set; } = "";

        // 引用信息
        public string Ref { get; set; } = "";
        public GitRefType RefType { get; set; } = GitRefType.Branch;

        // 子目录
        public string SubDirectory { get; set; } = "";

        /// <summary>
        /// 是否有子目录
        /// </summary>
        public bool HasSubDirectory => !string.IsNullOrEmpty(SubDirectory);

        /// <summary>
        /// 是否有引用（分支/标签/提交）
        /// </summary>
        public bool HasRef => !string.IsNullOrEmpty(Ref);

        /// <summary>
        /// 获取显示名称
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(Owner) || string.IsNullOrEmpty(RepoName))
                    return OriginalUrl;
                return $"{Owner}/{RepoName}";
            }
        }

        /// <summary>
        /// 获取完整描述
        /// </summary>
        public string FullDescription
        {
            get
            {
                var parts = new List<string> { DisplayName };
                if (HasRef) parts.Add($"[{RefType}: {Ref}]");
                if (HasSubDirectory) parts.Add($"路径: {SubDirectory}");
                return string.Join(" | ", parts);
            }
        }
    }

    /// <summary>
    /// Git URL 类型
    /// </summary>
    public enum GitUrlType
    {
        Unknown,
        GitPackage,     // .git 格式 (Unity Package Manager 风格)
        GitHubWeb       // GitHub Web URL
    }

    /// <summary>
    /// Git 引用类型
    /// </summary>
    public enum GitRefType
    {
        Branch,     // 分支
        Tag,        // 标签
        Commit      // 提交哈希
    }
}
