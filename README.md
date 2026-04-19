# RS.GitSubDirectoryDownloader

一个功能强大的 GitHub 仓库下载工具，支持窗口模式和命令行模式，支持 AOT 编译，支持子目录下载、分支选择、Release 下载、代理配置、加速服务等功能。

## 功能特性

### 🎯 核心功能

- **子目录下载**：支持只下载仓库中的指定子目录，无需克隆整个仓库
- **完整仓库克隆**：支持完整克隆仓库，支持指定分支
- **Release 下载**：支持下载仓库的 Release 文件和源代码
- **分支/Tag 选择**：支持选择不同的分支、Tag 或 Commit
- **文件过滤**：支持按文件后缀名过滤下载，支持包含/排除模式

### 🌐 网络功能

- **代理支持**：支持系统代理、HTTP/HTTPS 代理、SOCKS5 代理
- **GH-Proxy 加速**：内置 GH-Proxy 加速服务，支持自动选择最快节点
- **GitHub Token 认证**：支持使用 GitHub Personal Access Token 提高 API 限制

### 🛠️ 编译与保护

- **AOT 编译**：支持 Native AOT 编译，生成原生可执行文件
- **R2R 编译**：支持 ReadyToRun 预编译，提高启动速度
- **代码保护**：多种代码保护方案，包括裁剪、混淆等

### 🖥️ 用户界面

- **窗口模式**：直观的 Windows Forms 界面，适合普通用户
- **命令行模式**：强大的命令行工具，适合脚本和自动化

## 快速开始

### 窗口模式

直接运行 `RS.GitSubDirectoryDownloader.NET8.exe` 或 `RS.GitSubDirectoryDownloader.exe`（AOT 版本）即可打开窗口界面。

### 命令行模式

```bash
# 基本用法
RS.GitSubDirectoryDownloader.Console.exe --url https://github.com/user/repo --output ./download

# 指定分支
RS.GitSubDirectoryDownloader.Console.exe --url https://github.com/user/repo --branch main --output ./download

# 指定子目录
RS.GitSubDirectoryDownloader.Console.exe --url https://github.com/user/repo --subdir src --output ./download

# 使用加速服务
RS.GitSubDirectoryDownloader.Console.exe --url https://github.com/user/repo --acceleration ghproxy --output ./download

# 使用代理
RS.GitSubDirectoryDownloader.Console.exe --url https://github.com/user/repo --proxy http://127.0.0.1:7890 --output ./download

# 查看帮助
RS.GitSubDirectoryDownloader.Console.exe --help
```



## 编译说明

### 普通编译（.NET 8）

```bash
# 窗口模式
dotnet build RS.GitSubDirectoryDownloader.NET8.csproj -c Release

# 命令行模式
dotnet build RS.GitSubDirectoryDownloader.Console.csproj -c Release
```

### AOT 编译

```bash
# 窗口模式（AOT）
dotnet publish RS.GitSubDirectoryDownloader.csproj -c Release -r win-x64

# 命令行模式（AOT）
dotnet publish RS.GitSubDirectoryDownloader.Console.csproj -c Release -r win-x64
```




## 使用说明

### 窗口模式

1. **输入 GitHub URL**：在顶部输入框输入 GitHub 仓库 URL
2. **选择下载方式**：Octokit（推荐）或 LibGit2Sharp
3. **选择分支/Tag**：在下拉列表中选择要下载的分支或 Tag
4. **设置保存路径**：点击"浏览"按钮选择保存位置
5. **配置选项**：
   - 仅下载子目录：输入要下载的子目录路径
   - 文件后缀过滤：指定要下载的文件后缀
6. **网络配置**：
   - 代理设置：选择代理类型或输入代理地址
   - 加速服务：选择 加速节点
7. **开始下载**：点击"下载"按钮开始下载

### 命令行模式

```bash
# 完整选项列表
--url <url>              GitHub 仓库 URL（必需）
--output <path>          输出目录（必需）
--branch <name>          分支/Tag/Commit（默认：使用仓库默认分支）
--subdir <path>          仅下载指定子目录
--method <octokit|libgit2sharp>  下载方式（默认：自动选择）
--token <token>          GitHub Personal Access Token
--acceleration <ghproxy[:node]>  加速服务
--proxy <url>            代理地址
--include <ext1,ext2>    包含的文件后缀
--exclude <ext1,ext2>    排除的文件后缀
--help, -h               显示帮助信息
```

## 注意事项

### LibGit2Sharp + 代理问题

LibGit2Sharp 在使用 HTTP 代理时可能会遇到 SSL 证书验证失败的问题。这是 LibGit2Sharp 原生库的已知限制。

**推荐解决方案**：

1. 使用 Octokit 下载方式（推荐）
2. 使用 Github-Proxy 加速服务，不设置代理
3. 使用 AOT 版本（经验证兼容性更好）

### AOT 兼容性

AOT 版本使用自定义的 Octokit 库，完全兼容 AOT 编译，但部分 Octokit 的高级功能可能不可用。

## 许可证

本项目仅供学习和研究使用。本项目不允许商业，如果有需要请联系作者获得授权！

## 贡献

欢迎提交 Issue 和 Pull Request！

## 更新日志

### v2.1
- 修复浏览目录点击无响应问题
- 增强 SSL 配置，支持更多环境变量
- 修复分支选择问题，不再强制使用 main
- 窗口模式、命令行模式、控制台模式全面修复

### v2.0
- 支持 AOT 编译
- 优化代码保护方案

### v1.6
- 初始版本
- 支持子目录下载
- 支持 Release 下载
- 支持代理和加速服务
