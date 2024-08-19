using System;
using System.Windows.Forms;

namespace NCMDumpGUI
{
    public partial class WndMain : Form
    {
        public WndMain()
        {
            InitializeComponent();
        }

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
                if (result != 0) {
                    toolStripStatusLabel2.Text = "发生错误，错误代码：" + result.ToString();
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
    }
}
