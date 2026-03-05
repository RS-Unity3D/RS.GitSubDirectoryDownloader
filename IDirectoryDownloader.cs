namespace RS.GitSubDirectoryDownloader
{
    /// <summary>
    /// GitHub目录下载器统一接口
    /// 所有下载实现都应遵循此接口
    /// </summary>
    public interface IDirectoryDownloader
    {
        /// <summary>
        /// 递归下载目录
        /// </summary>
        /// <param name="owner">仓库所有者</param>
        /// <param name="repo">仓库名称</param>
        /// <param name="remotePath">远程目录路径</param>
        /// <param name="localPath">本地保存路径</param>
        /// <param name="branch">分支名称</param>
        /// <param name="fileExtensions">文件后缀过滤（空列表表示下载全部）</param>
        /// <param name="progress">进度报告</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task DownloadDirectoryAsync(string owner, string repo, string remotePath, string localPath, string branch, List<string>? fileExtensions, IProgress<DownloadProgress>? progress, CancellationToken cancellationToken);

        /// <summary>
        /// 获取仓库默认分支
        /// </summary>
        Task<string> GetDefaultBranchAsync(string owner, string repo);
        
        /// <summary>
        /// 获取仓库所有分支列表
        /// </summary>
        Task<List<string>> GetBranchesAsync(string owner, string repo) => Task.FromResult(new List<string>());
        
        /// <summary>
        /// 获取仓库所有标签列表
        /// </summary>
        Task<List<string>> GetTagsAsync(string owner, string repo) => Task.FromResult(new List<string>());
        
        /// <summary>
        /// 检查仓库是否为私有仓库
        /// </summary>
        Task<bool> IsPrivateRepoAsync(string owner, string repo) => Task.FromResult(false);
    }

    /// <summary>
    /// 下载进度信息
    /// </summary>
    public class DownloadProgress
    {
        public int DownloadedFiles { get; set; }
        public int TotalFiles { get; set; }
        public string CurrentFile { get; set; } = "";
        public string Status { get; set; } = "";
    }

    /// <summary>
    /// 下载计数器
    /// </summary>
    internal class DownloadCounter
    {
        public int TotalFiles { get; set; }
        public int DownloadedFiles { get; set; }
    }

    /// <summary>
    /// GitHub项目信息
    /// </summary>
    public class GitHubRepoInfo
    {
        public string Owner { get; set; } = "";
        public string Repo { get; set; } = "";
        public string Branch { get; set; } = "";
        public string Folder { get; set; } = "";
    }
}
