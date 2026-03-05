namespace RS.GitSubDirectoryDownloader
{
    /// <summary>
    /// Control安全访问扩展方法
    /// 解决跨线程访问WinForm控件问题
    /// </summary>
    public static class ControlExtensions
    {
        /// <summary>
        /// 线程安全地执行操作
        /// </summary>
        /// <param name="control">控件</param>
        /// <param name="action">要执行的操作</param>
        public static void SafeInvoke(this Control control, Action action)
        {
            if (control == null) return;

            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// 线程安全地获取控件属性值
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="control">控件</param>
        /// <param name="func">获取值的函数</param>
        /// <returns>属性值</returns>
        public static T SafeGet<T>(this Control control, Func<T> func)
        {
            if (control == null) return default!;

            if (control.InvokeRequired)
            {
                return (T)control.Invoke(func);
            }
            return func();
        }

        /// <summary>
        /// 线程安全地设置文本
        /// </summary>
        public static void SafeSetText(this TextBox textBox, string text)
        {
            textBox.SafeInvoke(() => textBox.Text = text);
        }

        /// <summary>
        /// 线程安全地获取文本
        /// </summary>
        public static string SafeGetText(this TextBox textBox)
        {
            return textBox.SafeGet(() => textBox.Text);
        }

        /// <summary>
        /// 线程安全地设置标签文本
        /// </summary>
        public static void SafeSetText(this Label label, string text)
        {
            label.SafeInvoke(() => label.Text = text);
        }

        /// <summary>
        /// 线程安全地设置进度条值
        /// </summary>
        public static void SafeSetValue(this ProgressBar progressBar, int value)
        {
            progressBar.SafeInvoke(() => progressBar.Value = value);
        }

        /// <summary
        /// 线程安全地设置进度条最大值
        /// </summary>
        public static void SafeSetMaximum(this ProgressBar progressBar, int maximum)
        {
            progressBar.SafeInvoke(() => progressBar.Maximum = maximum);
        }

        /// <summary>
        /// 线程安全地设置按钮文本
        /// </summary>
        public static void SafeSetText(this Button button, string text)
        {
            button.SafeInvoke(() => button.Text = text);
        }

        /// <summary>
        /// 线程安全地设置按钮背景色
        /// </summary>
        public static void SafeSetBackColor(this Button button, Color color)
        {
            button.SafeInvoke(() => button.BackColor = color);
        }

        /// <summary>
        /// 线程安全地获取ComboBox文本
        /// </summary>
        public static string SafeGetText(this ComboBox comboBox)
        {
            return comboBox.SafeGet(() => comboBox.Text);
        }

        /// <summary>
        /// 线程安全地设置ComboBox文本
        /// </summary>
        public static void SafeSetText(this ComboBox comboBox, string text)
        {
            comboBox.SafeInvoke(() => comboBox.Text = text);
        }
    }
}
