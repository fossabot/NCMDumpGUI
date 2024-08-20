using System;
using System.Runtime.InteropServices;
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
                        MessageBox.Show("NCMDumpGUI v1.0.0.1\n基于libncmdump开发", "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void convertButton_Click(object sender, EventArgs e)
        {
            if (filepathTextBox.Text == "")
            {
                MessageBox.Show("文件路径为空！\n请提供ncm文件路径", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!filepathTextBox.Text.EndsWith(".ncm"))
            {
                MessageBox.Show("这似乎并不是ncm文件！\n请提供ncm文件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!File.Exists("libncmdump.dll"))
            {
                MessageBox.Show("核心不存在\n请确认libncmdump.dll与本程序在同一目录", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
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
