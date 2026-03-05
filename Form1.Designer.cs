namespace RS.GitSubDirectoryDownloader
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblGithubUrl = new Label();
            txtGithubUrl = new TextBox();
            btnParseUrl = new Button();
            chkDownloadSubDir = new CheckBox();
            lblParsedInfo = new Label();
            grpRepoInfo = new GroupBox();
            lblCommit = new Label();
            cmbCommit = new ComboBox();
            lblBranch = new Label();
            cmbBranch = new ComboBox();
            lblFolder = new Label();
            txtFolder = new TextBox();
            lblRepoName = new Label();
            txtRepoName = new Label();
            txtOwner = new Label();
            lblOwner = new Label();
            lblLocalPath = new Label();
            txtLocalPath = new TextBox();
            btnBrowse = new Button();
            grpDownloadOptions = new GroupBox();
            lblDownloadMethod = new Label();
            cmbDownloadMethod = new ComboBox();
            lblFileExtensions = new Label();
            txtFileExtensions = new TextBox();
            grpAuth = new GroupBox();
            cmbToken = new ComboBox();
            lblToken = new Label();
            txtPassword = new TextBox();
            lblPassword = new Label();
            txtUsername = new TextBox();
            lblUsername = new Label();
            rbPassword = new RadioButton();
            rbToken = new RadioButton();
            rbNoAuth = new RadioButton();
            grpAcceleration = new GroupBox();
            lblAcceleratedUrl = new Label();
            txtAcceleratedUrl = new TextBox();
            lblGhProxyNode = new Label();
            cmbGhProxyNode = new ComboBox();
            lblAccelerationMode = new Label();
            cmbAccelerationMode = new ComboBox();
            grpProxy = new GroupBox();
            lblSystemProxyStatus = new Label();
            lnkSystemProxy = new LinkLabel();
            lblProxyMode = new Label();
            cmbProxyMode = new ComboBox();
            txtProxyAddress = new TextBox();
            lblProxyAddress = new Label();
            btnDownload = new Button();
            progressBar = new ProgressBar();
            folderBrowserDialog = new FolderBrowserDialog();
            grpLog = new GroupBox();
            txtLog = new TextBox();
            grpRepoInfo.SuspendLayout();
            grpDownloadOptions.SuspendLayout();
            grpAuth.SuspendLayout();
            grpAcceleration.SuspendLayout();
            grpProxy.SuspendLayout();
            grpLog.SuspendLayout();
            SuspendLayout();
            // 
            // lblGithubUrl
            // 
            lblGithubUrl.AutoSize = true;
            lblGithubUrl.Location = new Point(18,15);
            lblGithubUrl.Name = "lblGithubUrl";
            lblGithubUrl.Size = new Size(99,17);
            lblGithubUrl.TabIndex = 0;
            lblGithubUrl.Text = "GitHub目录地址:";
            // 
            // txtGithubUrl
            // 
            txtGithubUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtGithubUrl.Location = new Point(18,36);
            txtGithubUrl.Name = "txtGithubUrl";
            txtGithubUrl.PlaceholderText = "例如: https://github.com/user/repo.git#v1.0.0?path=Assets/Folder";
            txtGithubUrl.Size = new Size(560,23);
            txtGithubUrl.TabIndex = 1;
            // 
            // btnParseUrl
            // 
            btnParseUrl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnParseUrl.Location = new Point(585,34);
            btnParseUrl.Name = "btnParseUrl";
            btnParseUrl.Size = new Size(90,26);
            btnParseUrl.TabIndex = 2;
            btnParseUrl.Text = "解析URL";
            btnParseUrl.UseVisualStyleBackColor = true;
            btnParseUrl.Click += BtnParseUrl_Click;
            // 
            // chkDownloadSubDir
            // 
            chkDownloadSubDir.AutoSize = true;
            chkDownloadSubDir.Checked = true;
            chkDownloadSubDir.CheckState = CheckState.Checked;
            chkDownloadSubDir.Location = new Point(496,73);
            chkDownloadSubDir.Name = "chkDownloadSubDir";
            chkDownloadSubDir.Size = new Size(99,21);
            chkDownloadSubDir.TabIndex = 8;
            chkDownloadSubDir.Text = "仅下载子目录";
            chkDownloadSubDir.UseVisualStyleBackColor = true;
            chkDownloadSubDir.Visible = false;
            chkDownloadSubDir.CheckedChanged += ChkDownloadSubDir_CheckedChanged;
            // 
            // lblParsedInfo
            // 
            lblParsedInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblParsedInfo.AutoSize = true;
            lblParsedInfo.Font = new Font("Segoe UI",8F,FontStyle.Italic);
            lblParsedInfo.ForeColor = Color.FromArgb(0,122,204);
            lblParsedInfo.Location = new Point(18,62);
            lblParsedInfo.Name = "lblParsedInfo";
            lblParsedInfo.Size = new Size(0,13);
            lblParsedInfo.TabIndex = 3;
            lblParsedInfo.Visible = false;
            // 
            // grpRepoInfo
            // 
            grpRepoInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpRepoInfo.Controls.Add(chkDownloadSubDir);
            grpRepoInfo.Controls.Add(lblCommit);
            grpRepoInfo.Controls.Add(cmbCommit);
            grpRepoInfo.Controls.Add(lblBranch);
            grpRepoInfo.Controls.Add(cmbBranch);
            grpRepoInfo.Controls.Add(lblFolder);
            grpRepoInfo.Controls.Add(txtFolder);
            grpRepoInfo.Controls.Add(lblRepoName);
            grpRepoInfo.Controls.Add(txtRepoName);
            grpRepoInfo.Controls.Add(txtOwner);
            grpRepoInfo.Controls.Add(lblOwner);
            grpRepoInfo.Location = new Point(18,78);
            grpRepoInfo.Name = "grpRepoInfo";
            grpRepoInfo.Size = new Size(660,109);
            grpRepoInfo.TabIndex = 4;
            grpRepoInfo.TabStop = false;
            grpRepoInfo.Text = "仓库信息";
            // 
            // lblCommit
            // 
            lblCommit.AutoSize = true;
            lblCommit.Location = new Point(12,50);
            lblCommit.Name = "lblCommit";
            lblCommit.Size = new Size(56,17);
            lblCommit.TabIndex = 8;
            lblCommit.Text = "Commit:";
            // 
            // cmbCommit
            // 
            cmbCommit.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbCommit.FormattingEnabled = true;
            cmbCommit.Location = new Point(70,47);
            cmbCommit.Name = "cmbCommit";
            cmbCommit.Size = new Size(420,25);
            cmbCommit.TabIndex = 9;
            // 
            // lblBranch
            // 
            lblBranch.AutoSize = true;
            lblBranch.Location = new Point(366,22);
            lblBranch.Name = "lblBranch";
            lblBranch.Size = new Size(51,17);
            lblBranch.TabIndex = 6;
            lblBranch.Text = "Branch:";
            // 
            // cmbBranch
            // 
            cmbBranch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmbBranch.FormattingEnabled = true;
            cmbBranch.Location = new Point(423,19);
            cmbBranch.Name = "cmbBranch";
            cmbBranch.Size = new Size(227,25);
            cmbBranch.TabIndex = 7;
            cmbBranch.SelectedIndexChanged += CmbBranch_SelectedIndexChanged;
            // 
            // lblFolder
            // 
            lblFolder.AutoSize = true;
            lblFolder.Location = new Point(12,75);
            lblFolder.Name = "lblFolder";
            lblFolder.Size = new Size(70,17);
            lblFolder.TabIndex = 4;
            lblFolder.Text = "SubFolder:";
            // 
            // txtFolder
            // 
            txtFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFolder.Font = new Font("Segoe UI",9F,FontStyle.Bold);
            txtFolder.Location = new Point(85,72);
            txtFolder.Name = "txtFolder";
            txtFolder.PlaceholderText = "可选: 输入子目录路径";
            txtFolder.Size = new Size(405,23);
            txtFolder.TabIndex = 5;
            // 
            // lblRepoName
            // 
            lblRepoName.AutoSize = true;
            lblRepoName.Location = new Point(150,22);
            lblRepoName.Name = "lblRepoName";
            lblRepoName.Size = new Size(81,17);
            lblRepoName.TabIndex = 2;
            lblRepoName.Text = "Repo Name:";
            // 
            // txtRepoName
            // 
            txtRepoName.AutoSize = true;
            txtRepoName.Font = new Font("Segoe UI",9F,FontStyle.Bold);
            txtRepoName.Location = new Point(251,22);
            txtRepoName.Name = "txtRepoName";
            txtRepoName.Size = new Size(0,15);
            txtRepoName.TabIndex = 3;
            // 
            // txtOwner
            // 
            txtOwner.AutoSize = true;
            txtOwner.Font = new Font("Segoe UI",9F,FontStyle.Bold);
            txtOwner.Location = new Point(65,22);
            txtOwner.Name = "txtOwner";
            txtOwner.Size = new Size(0,15);
            txtOwner.TabIndex = 1;
            // 
            // lblOwner
            // 
            lblOwner.AutoSize = true;
            lblOwner.Location = new Point(12,22);
            lblOwner.Name = "lblOwner";
            lblOwner.Size = new Size(49,17);
            lblOwner.TabIndex = 0;
            lblOwner.Text = "Owner:";
            // 
            // lblLocalPath
            // 
            lblLocalPath.AutoSize = true;
            lblLocalPath.Location = new Point(18,192);
            lblLocalPath.Name = "lblLocalPath";
            lblLocalPath.Size = new Size(59,17);
            lblLocalPath.TabIndex = 5;
            lblLocalPath.Text = "本地路径:";
            // 
            // txtLocalPath
            // 
            txtLocalPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtLocalPath.Location = new Point(18,214);
            txtLocalPath.Name = "txtLocalPath";
            txtLocalPath.PlaceholderText = "选择本地保存目录";
            txtLocalPath.Size = new Size(573,23);
            txtLocalPath.TabIndex = 6;
            // 
            // btnBrowse
            // 
            btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowse.Location = new Point(608,212);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(70,26);
            btnBrowse.TabIndex = 7;
            btnBrowse.Text = "浏览...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += BtnBrowse_Click;
            // 
            // grpDownloadOptions
            // 
            grpDownloadOptions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpDownloadOptions.Controls.Add(lblDownloadMethod);
            grpDownloadOptions.Controls.Add(cmbDownloadMethod);
            grpDownloadOptions.Controls.Add(lblFileExtensions);
            grpDownloadOptions.Controls.Add(txtFileExtensions);
            grpDownloadOptions.Location = new Point(18,246);
            grpDownloadOptions.Name = "grpDownloadOptions";
            grpDownloadOptions.Size = new Size(660,78);
            grpDownloadOptions.TabIndex = 8;
            grpDownloadOptions.TabStop = false;
            grpDownloadOptions.Text = "下载选项";
            // 
            // lblDownloadMethod
            // 
            lblDownloadMethod.AutoSize = true;
            lblDownloadMethod.Location = new Point(12,26);
            lblDownloadMethod.Name = "lblDownloadMethod";
            lblDownloadMethod.Size = new Size(59,17);
            lblDownloadMethod.TabIndex = 0;
            lblDownloadMethod.Text = "下载方式:";
            // 
            // cmbDownloadMethod
            // 
            cmbDownloadMethod.FormattingEnabled = true;
            cmbDownloadMethod.Items.AddRange(new object[] { "Octokit (推荐)","LibGit2Sharp (完整克隆)" });
            cmbDownloadMethod.Location = new Point(89,23);
            cmbDownloadMethod.Name = "cmbDownloadMethod";
            cmbDownloadMethod.Size = new Size(180,25);
            cmbDownloadMethod.TabIndex = 1;
            // 
            // lblFileExtensions
            // 
            lblFileExtensions.AutoSize = true;
            lblFileExtensions.Location = new Point(12,51);
            lblFileExtensions.Name = "lblFileExtensions";
            lblFileExtensions.Size = new Size(59,17);
            lblFileExtensions.TabIndex = 2;
            lblFileExtensions.Text = "文件后缀:";
            // 
            // txtFileExtensions
            // 
            txtFileExtensions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFileExtensions.Location = new Point(89,48);
            txtFileExtensions.Name = "txtFileExtensions";
            txtFileExtensions.PlaceholderText = "例如: .cs,.json,.txt (留空下载全部)";
            txtFileExtensions.Size = new Size(559,23);
            txtFileExtensions.TabIndex = 3;
            // 
            // grpAuth
            // 
            grpAuth.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpAuth.Controls.Add(cmbToken);
            grpAuth.Controls.Add(lblToken);
            grpAuth.Controls.Add(txtPassword);
            grpAuth.Controls.Add(lblPassword);
            grpAuth.Controls.Add(txtUsername);
            grpAuth.Controls.Add(lblUsername);
            grpAuth.Controls.Add(rbPassword);
            grpAuth.Controls.Add(rbToken);
            grpAuth.Controls.Add(rbNoAuth);
            grpAuth.Location = new Point(18,320);
            grpAuth.Name = "grpAuth";
            grpAuth.Size = new Size(660,100);
            grpAuth.TabIndex = 8;
            grpAuth.TabStop = false;
            grpAuth.Text = "认证方式";
            // 
            // cmbToken
            // 
            cmbToken.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbToken.Enabled = false;
            cmbToken.FormattingEnabled = true;
            cmbToken.Location = new Point(77,72);
            cmbToken.Name = "cmbToken";
            cmbToken.Size = new Size(571,25);
            cmbToken.TabIndex = 8;
            // 
            // lblToken
            // 
            lblToken.AutoSize = true;
            lblToken.Enabled = false;
            lblToken.Location = new Point(12,75);
            lblToken.Name = "lblToken";
            lblToken.Size = new Size(47,17);
            lblToken.TabIndex = 7;
            lblToken.Text = "Token:";
            // 
            // txtPassword
            // 
            txtPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPassword.Enabled = false;
            txtPassword.Location = new Point(343,49);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '●';
            txtPassword.Size = new Size(305,23);
            txtPassword.TabIndex = 6;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Enabled = false;
            lblPassword.Location = new Point(290,52);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(35,17);
            lblPassword.TabIndex = 5;
            lblPassword.Text = "密码:";
            // 
            // txtUsername
            // 
            txtUsername.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtUsername.Enabled = false;
            txtUsername.Location = new Point(77,49);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(200,23);
            txtUsername.TabIndex = 4;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Enabled = false;
            lblUsername.Location = new Point(12,52);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(47,17);
            lblUsername.TabIndex = 3;
            lblUsername.Text = "用户名:";
            // 
            // rbPassword
            // 
            rbPassword.AutoSize = true;
            rbPassword.Location = new Point(280,22);
            rbPassword.Name = "rbPassword";
            rbPassword.Size = new Size(98,21);
            rbPassword.TabIndex = 2;
            rbPassword.Text = "使用账号密码";
            rbPassword.UseVisualStyleBackColor = true;
            rbPassword.CheckedChanged += RbAuth_CheckedChanged;
            // 
            // rbToken
            // 
            rbToken.AutoSize = true;
            rbToken.Location = new Point(150,22);
            rbToken.Name = "rbToken";
            rbToken.Size = new Size(86,21);
            rbToken.TabIndex = 1;
            rbToken.Text = "使用Token";
            rbToken.UseVisualStyleBackColor = true;
            rbToken.CheckedChanged += RbAuth_CheckedChanged;
            // 
            // rbNoAuth
            // 
            rbNoAuth.AutoSize = true;
            rbNoAuth.Checked = true;
            rbNoAuth.Location = new Point(12,22);
            rbNoAuth.Name = "rbNoAuth";
            rbNoAuth.Size = new Size(106,21);
            rbNoAuth.TabIndex = 0;
            rbNoAuth.TabStop = true;
            rbNoAuth.Text = "无需认证(公开)";
            rbNoAuth.UseVisualStyleBackColor = true;
            rbNoAuth.CheckedChanged += RbAuth_CheckedChanged;
            // 
            // grpAcceleration
            // 
            grpAcceleration.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpAcceleration.Controls.Add(lblAcceleratedUrl);
            grpAcceleration.Controls.Add(txtAcceleratedUrl);
            grpAcceleration.Controls.Add(lblGhProxyNode);
            grpAcceleration.Controls.Add(cmbGhProxyNode);
            grpAcceleration.Controls.Add(lblAccelerationMode);
            grpAcceleration.Controls.Add(cmbAccelerationMode);
            grpAcceleration.Location = new Point(18,420);
            grpAcceleration.Name = "grpAcceleration";
            grpAcceleration.Size = new Size(660,103);
            grpAcceleration.TabIndex = 9;
            grpAcceleration.TabStop = false;
            grpAcceleration.Text = "加速服务 (GH-Proxy)";
            // 
            // lblAcceleratedUrl
            // 
            lblAcceleratedUrl.AutoSize = true;
            lblAcceleratedUrl.Location = new Point(12,75);
            lblAcceleratedUrl.Name = "lblAcceleratedUrl";
            lblAcceleratedUrl.Size = new Size(70,17);
            lblAcceleratedUrl.TabIndex = 5;
            lblAcceleratedUrl.Text = "加速后URL:";
            // 
            // txtAcceleratedUrl
            // 
            txtAcceleratedUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtAcceleratedUrl.Location = new Point(89,72);
            txtAcceleratedUrl.Name = "txtAcceleratedUrl";
            txtAcceleratedUrl.PlaceholderText = "启用加速后显示最终URL，可手动修改";
            txtAcceleratedUrl.ReadOnly = true;
            txtAcceleratedUrl.Size = new Size(559,23);
            txtAcceleratedUrl.TabIndex = 6;
            // 
            // lblGhProxyNode
            // 
            lblGhProxyNode.AutoSize = true;
            lblGhProxyNode.Location = new Point(200,26);
            lblGhProxyNode.Name = "lblGhProxyNode";
            lblGhProxyNode.Size = new Size(59,17);
            lblGhProxyNode.TabIndex = 2;
            lblGhProxyNode.Text = "加速节点:";
            // 
            // cmbGhProxyNode
            // 
            cmbGhProxyNode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGhProxyNode.FormattingEnabled = true;
            cmbGhProxyNode.Location = new Point(280,23);
            cmbGhProxyNode.Name = "cmbGhProxyNode";
            cmbGhProxyNode.Size = new Size(170,25);
            cmbGhProxyNode.TabIndex = 3;
            cmbGhProxyNode.SelectedIndexChanged += CmbGhProxyNode_SelectedIndexChanged;
            // 
            // lblAccelerationMode
            // 
            lblAccelerationMode.AutoSize = true;
            lblAccelerationMode.Location = new Point(12,26);
            lblAccelerationMode.Name = "lblAccelerationMode";
            lblAccelerationMode.Size = new Size(59,17);
            lblAccelerationMode.TabIndex = 0;
            lblAccelerationMode.Text = "加速模式:";
            // 
            // cmbAccelerationMode
            // 
            cmbAccelerationMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAccelerationMode.FormattingEnabled = true;
            cmbAccelerationMode.Items.AddRange(new object[] { "不使用加速","GH-Proxy加速" });
            cmbAccelerationMode.Location = new Point(77,23);
            cmbAccelerationMode.Name = "cmbAccelerationMode";
            cmbAccelerationMode.Size = new Size(115,25);
            cmbAccelerationMode.TabIndex = 1;
            cmbAccelerationMode.SelectedIndexChanged += CmbAccelerationMode_SelectedIndexChanged;
            // 
            // grpProxy
            // 
            grpProxy.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpProxy.Controls.Add(lblSystemProxyStatus);
            grpProxy.Controls.Add(lnkSystemProxy);
            grpProxy.Controls.Add(lblProxyMode);
            grpProxy.Controls.Add(cmbProxyMode);
            grpProxy.Controls.Add(txtProxyAddress);
            grpProxy.Controls.Add(lblProxyAddress);
            grpProxy.Location = new Point(18,529);
            grpProxy.Name = "grpProxy";
            grpProxy.Size = new Size(660,78);
            grpProxy.TabIndex = 10;
            grpProxy.TabStop = false;
            grpProxy.Text = "代理设置 (传统代理)";
            // 
            // lblSystemProxyStatus
            // 
            lblSystemProxyStatus.AutoSize = true;
            lblSystemProxyStatus.Location = new Point(12,50);
            lblSystemProxyStatus.Name = "lblSystemProxyStatus";
            lblSystemProxyStatus.Size = new Size(68,17);
            lblSystemProxyStatus.TabIndex = 7;
            lblSystemProxyStatus.Text = "系统代理，";
            lblSystemProxyStatus.Visible = false;
            // 
            // lnkSystemProxy
            // 
            lnkSystemProxy.AutoSize = true;
            lnkSystemProxy.Enabled = false;
            lnkSystemProxy.Location = new Point(77,50);
            lnkSystemProxy.Name = "lnkSystemProxy";
            lnkSystemProxy.Size = new Size(111,17);
            lnkSystemProxy.TabIndex = 6;
            lnkSystemProxy.TabStop = true;
            lnkSystemProxy.Text = "检测到系统代理: 无";
            lnkSystemProxy.Visible = false;
            lnkSystemProxy.LinkClicked += LnkSystemProxy_LinkClicked;
            // 
            // lblProxyMode
            // 
            lblProxyMode.AutoSize = true;
            lblProxyMode.Location = new Point(12,26);
            lblProxyMode.Name = "lblProxyMode";
            lblProxyMode.Size = new Size(59,17);
            lblProxyMode.TabIndex = 0;
            lblProxyMode.Text = "代理模式:";
            // 
            // cmbProxyMode
            // 
            cmbProxyMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProxyMode.FormattingEnabled = true;
            cmbProxyMode.Items.AddRange(new object[] { "不使用代理","系统代理","自定义代理" });
            cmbProxyMode.Location = new Point(77,23);
            cmbProxyMode.Name = "cmbProxyMode";
            cmbProxyMode.Size = new Size(130,25);
            cmbProxyMode.TabIndex = 1;
            cmbProxyMode.SelectedIndexChanged += CmbProxyMode_SelectedIndexChanged;
            // 
            // txtProxyAddress
            // 
            txtProxyAddress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtProxyAddress.Enabled = false;
            txtProxyAddress.Location = new Point(270,47);
            txtProxyAddress.Name = "txtProxyAddress";
            txtProxyAddress.PlaceholderText = "例如: http://127.0.0.1:7890 或 socks5://127.0.0.1:1080";
            txtProxyAddress.Size = new Size(380,23);
            txtProxyAddress.TabIndex = 5;
            txtProxyAddress.Visible = false;
            // 
            // lblProxyAddress
            // 
            lblProxyAddress.AutoSize = true;
            lblProxyAddress.Enabled = false;
            lblProxyAddress.Location = new Point(200,50);
            lblProxyAddress.Name = "lblProxyAddress";
            lblProxyAddress.Size = new Size(59,17);
            lblProxyAddress.TabIndex = 4;
            lblProxyAddress.Text = "代理地址:";
            lblProxyAddress.Visible = false;
            // 
            // btnDownload
            // 
            btnDownload.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDownload.BackColor = Color.FromArgb(0,122,204);
            btnDownload.FlatStyle = FlatStyle.Flat;
            btnDownload.ForeColor = Color.White;
            btnDownload.Location = new Point(590,617);
            btnDownload.Name = "btnDownload";
            btnDownload.Size = new Size(88,32);
            btnDownload.TabIndex = 11;
            btnDownload.Text = "开始下载";
            btnDownload.UseVisualStyleBackColor = false;
            btnDownload.Click += BtnDownload_Click;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(18,617);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(560,32);
            progressBar.TabIndex = 12;
            // 
            // grpLog
            // 
            grpLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpLog.Controls.Add(txtLog);
            grpLog.Location = new Point(18,659);
            grpLog.Name = "grpLog";
            grpLog.Size = new Size(660,185);
            grpLog.TabIndex = 13;
            grpLog.TabStop = false;
            grpLog.Text = "日志";
            // 
            // txtLog
            // 
            txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtLog.BackColor = Color.FromArgb(30,30,30);
            txtLog.Font = new Font("Consolas",9F);
            txtLog.ForeColor = Color.FromArgb(220,220,220);
            txtLog.Location = new Point(6,22);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(648,157);
            txtLog.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F,17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(698,855);
            Controls.Add(grpLog);
            Controls.Add(progressBar);
            Controls.Add(btnDownload);
            Controls.Add(grpProxy);
            Controls.Add(grpAcceleration);
            Controls.Add(grpAuth);
            Controls.Add(grpDownloadOptions);
            Controls.Add(btnBrowse);
            Controls.Add(txtLocalPath);
            Controls.Add(lblLocalPath);
            Controls.Add(grpRepoInfo);
            Controls.Add(lblParsedInfo);
            Controls.Add(btnParseUrl);
            Controls.Add(txtGithubUrl);
            Controls.Add(lblGithubUrl);
            MinimumSize = new Size(627,782);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "GitHub仓库目录下载器";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            grpRepoInfo.ResumeLayout(false);
            grpRepoInfo.PerformLayout();
            grpDownloadOptions.ResumeLayout(false);
            grpDownloadOptions.PerformLayout();
            grpAuth.ResumeLayout(false);
            grpAuth.PerformLayout();
            grpAcceleration.ResumeLayout(false);
            grpAcceleration.PerformLayout();
            grpProxy.ResumeLayout(false);
            grpProxy.PerformLayout();
            grpLog.ResumeLayout(false);
            grpLog.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblGithubUrl;
        private TextBox txtGithubUrl;
        private Button btnParseUrl;
        private CheckBox chkDownloadSubDir;
        private Label lblParsedInfo;
        private GroupBox grpRepoInfo;
        private Label lblOwner;
        private Label txtOwner;
        private Label lblRepoName;
        private Label txtRepoName;
        private Label lblFolder;
        private TextBox txtFolder;
        private Label lblBranch;
        private ComboBox cmbBranch;
        private Label lblCommit;
        private ComboBox cmbCommit;
        private Label lblLocalPath;
        private TextBox txtLocalPath;
        private Button btnBrowse;
        private GroupBox grpDownloadOptions;
        private Label lblDownloadMethod;
        private ComboBox cmbDownloadMethod;
        private Label lblFileExtensions;
        private TextBox txtFileExtensions;
        private GroupBox grpAuth;
        private RadioButton rbNoAuth;
        private RadioButton rbToken;
        private RadioButton rbPassword;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private Label lblToken;
        private ComboBox cmbToken;
        private GroupBox grpAcceleration;
        private Label lblAccelerationMode;
        private ComboBox cmbAccelerationMode;
        private Label lblGhProxyNode;
        private ComboBox cmbGhProxyNode;
        private Label lblAcceleratedUrl;
        private TextBox txtAcceleratedUrl;
        private GroupBox grpProxy;
        private Label lblProxyMode;
        private ComboBox cmbProxyMode;
        private Label lblProxyAddress;
        private TextBox txtProxyAddress;
        private LinkLabel lnkSystemProxy;
        private Label lblSystemProxyStatus;
        private Button btnDownload;
        private ProgressBar progressBar;
        private FolderBrowserDialog folderBrowserDialog;
        private GroupBox grpLog;
        private TextBox txtLog;
    }
}
