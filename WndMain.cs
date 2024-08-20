using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NCMDumpGUI
{
    public partial class WndMain : Form
    {
        public WndMain()
        {
            InitializeComponent();
        }

        #region fields
        private const int WM_SYSCOMMAND = 0X112;
        private const int MF_STRING = 0X0;
        private const int MF_SEPARATOR = 0X800;
        private enum SystemMenuItem : int
        {
            About,
            FeedBack,
        }
        #endregion

        #region GetSystemMenu
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        #endregion

        #region AppendMenu
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);
        #endregion

        #region InsertMenu
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InsertMenu(IntPtr hMenu, int uPosition, int uFlags, int uIDNewItem, string lpNewItem);
        #endregion

        #region OnHandleCreated
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            var hSysMenu = GetSystemMenu(this.Handle, false);
            AppendMenu(hSysMenu, MF_SEPARATOR, 0, String.Empty);
            AppendMenu(hSysMenu, MF_STRING, (int)SystemMenuItem.About, "关于");
        }
        #endregion

        #region WndProc
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_SYSCOMMAND)
            {
                switch ((SystemMenuItem)m.WParam)
                {
                    case SystemMenuItem.About:
                        MessageBox.Show("NCMDumpGUI v1.0.0.2\n基于libncmdump开发", "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                }
            }
        }
        #endregion

        private void browseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "NCM加密歌曲|*.ncm";
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                filepathTextBox.Text = dialog.FileName;
            }
        }

        private bool CheckNCMBinary(string filePath)
        {
            string correctHeader = "CTENFDAM";

            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[8];
                    int bytesRead = fileStream.Read(bytes, 0, 8);

                    if (bytesRead == 8)
                    {
                        string header = Encoding.ASCII.GetString(bytes);
                        if (header == correctHeader)
                        {
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("不是ncm文件\n文件头为：" + header, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            toolStripStatusLabel2.Text = "不是ncm文件！";
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("文件大小异常", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        toolStripStatusLabel2.Text = "文件大小异常，并不是ncm文件";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("发生错误: \n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel2.Text = "载入文件时发生错误";
                return false;
            }
        }

        public static bool IsValidFilePath(string path)
        {
            if (Path.GetInvalidPathChars().Any(c => path.Contains(c)))
            {
                return false;
            }
            try
            {
                string normalizedPath = Path.GetFullPath(path);
                return Path.IsPathRooted(normalizedPath);
            }
            catch
            {
                return false;
            }
        }

        private void convertButton_Click(object sender, EventArgs e)
        {
            if (filepathTextBox.Text == "")
            {
                MessageBox.Show("文件路径为空！\n请提供ncm文件路径", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel2.Text = "请提供文件";
            }
            else if (!IsValidFilePath(filepathTextBox.Text))
            {
                MessageBox.Show("非法文件路径\n文件路径中包含非法字符！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel2.Text = "非法文件路径";
            }
            else if (!filepathTextBox.Text.EndsWith(".ncm"))
            {
                MessageBox.Show("这似乎并不是ncm文件！\n请提供ncm文件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel2.Text = "请提供正确的ncm文件";
            }
            else if (!File.Exists("libncmdump.dll"))
            {
                MessageBox.Show("核心不存在\n请确认libncmdump.dll与本程序在同一目录", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel2.Text = "核心不存在！请检查libncmdump.dll";
            }
            else if (CheckNCMBinary(filepathTextBox.Text))
            {
                filepathTextBox.Enabled = false;
                browseButton.Enabled = false;
                convertButton.Enabled = false;
                NeteaseCrypt neteaseCrypt = new NeteaseCrypt(filepathTextBox.Text);
                int result = neteaseCrypt.Dump();
                if (fixMetaDataCheckBox.Checked)
                {
                    neteaseCrypt.FixMetadata();
                }
                neteaseCrypt.Destroy();
                if (result != 0)
                {
                    toolStripStatusLabel2.Text = "发生错误，返回值为：" + result.ToString();
                }
                else
                {
                    toolStripStatusLabel2.Text = "转换完成！文件在ncm歌曲同级目录下";
                }
                filepathTextBox.Enabled = true;
                browseButton.Enabled = true;
                convertButton.Enabled = true;
            }
        }

        public void WndMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                convertButton.PerformClick();
            }
        }

        private void WndMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void WndMain_DragDrop(object sender, DragEventArgs e)
        {
            filepathTextBox.Text = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
        }
    }
}
