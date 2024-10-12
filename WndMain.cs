using System;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using NAudio.Wave;
using System.IO;
using System.Drawing.Text;

namespace NCMDumpGUI
{
    public partial class WndMain : Form
    {
        // 变量初始化
        private AudioFileReader audioFileReader;
        private WaveOutEvent outputDevice;
        private bool isPlaying = false;
        private string resultAudioPath = "";
        private FileSystemWatcher fileWatcher;

        // 窗口初始化
        public WndMain(string[] args)
        {
            InitializeComponent();
            toolTip.SetToolTip(fixMetaDataCheckBox, "将歌曲的详细信息添加到转换后的文件\n注意：不能保证100%正常工作，部分元数据可能无法修复！");
            toolTip.SetToolTip(convertButton, "点击开始转换文件到能被主流播放器识别的格式");
            if (args.Length > 0)
            {
                if (args[0] != "")
                {
                    if (Directory.Exists(args[0]))
                    {
                        fileFolderComboBox.SelectedIndex = 1;
                    }
                    else if (File.Exists(args[0]))
                    {
                        fileFolderComboBox.SelectedIndex = 0;
                    }
                    else
                    {
                        toolStripStatusLabel2.Text = "文件不存在";
                    }
                    filepathTextBox.Text = args[0];
                }
            }
            else
            {
                fileFolderComboBox.SelectedIndex = 0;
            }
            fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = filepathTextBox.Text;
            fileWatcher.Filter = "*.ncm";
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            fileWatcher.Created += autoDumpFolderOnChanged;
            fileWatcher.Error += autoDumpOnError;
        }

        // 窗口标题栏右键菜单
        #region fields
        private const int WM_SYSCOMMAND = 0X112;
        private const int MF_STRING = 0X0;
        private const int MF_SEPARATOR = 0X800;
        private enum SystemMenuItem : int
        {
            About,
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
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append("版本：v1.1.0.0\n基于libncmdump开发\n使用MIT许可证开源\n其他依赖：\n    · Costura.Fody\n    · NAudio\n当前.NET版本：").Append(Environment.Version.ToString());
                        TaskDialogButton[] buttons = new TaskDialogButton[]
                        {
                            TaskDialogButton.OK,
                        };
                        showTaskDialog(stringBuilder.ToString(), "NCMDumpGUI", "关于", TaskDialogIcon.Information, buttons, TaskDialogButton.OK);
                        break;
                }
            }
        }
        #endregion

        public TaskDialogButton showTaskDialog(string text, string heading, string caption, TaskDialogIcon icon, TaskDialogButton[] buttons, TaskDialogButton defaultBtn)
        {
            TaskDialogPage page = new TaskDialogPage()
            {
                Text = text,
                Heading = heading,
                Caption = caption,
                Icon = icon,
                DefaultButton = defaultBtn
            };

            foreach (var button in buttons)
            {
                page.Buttons.Add(button);
            }

            TaskDialogButton result = TaskDialog.ShowDialog(this, page);
            return result;
        }


        // “浏览”按钮被点击
        private void browseButton_Click(object sender, EventArgs e)
        {
            if (fileFolderComboBox.SelectedIndex == 0)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "NCM加密歌曲|*.ncm";
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    filepathTextBox.Text = dialog.FileName;
                }
            }
            else if (fileFolderComboBox.SelectedIndex == 1)
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.ShowNewFolderButton = true;
                dialog.RootFolder = Environment.SpecialFolder.ApplicationData;
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    filepathTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        public class FileFolderSizeRetriever
        {
            public static string GetSizeAsString(string path, bool includeSubdirectories)
            {
                long fileSizeBytes = 0;
                if (File.Exists(path))
                {
                    FileInfo fileInfo = new FileInfo(path);
                    fileSizeBytes = fileInfo.Length;
                }
                else if (Directory.Exists(path))
                {
                    DirectoryInfo directory = new DirectoryInfo(path);
                    FileInfo[] files = directory.GetFiles("*.ncm", includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                    foreach (FileInfo file in files)
                    {
                        fileSizeBytes += file.Length;
                    }
                }
                else
                {
                    return "路径不存在";
                }

                const long oneKb = 1024;
                const long oneMb = oneKb * 1024;

                if (fileSizeBytes < oneKb)
                {
                    return $"{fileSizeBytes} 字节";
                }
                else if (fileSizeBytes < oneMb)
                {
                    return $"{fileSizeBytes / oneKb} KiB";
                }
                else
                {
                    return $"{fileSizeBytes / oneMb} MiB";
                }
            }

            private static long GetDirectorySize(DirectoryInfo directory)
            {
                long totalSize = 0;
                FileInfo[] fileInfos = directory.GetFiles();
                foreach (FileInfo fileInfo in fileInfos)
                {
                    totalSize += fileInfo.Length;
                }
                DirectoryInfo[] subDirectories = directory.GetDirectories();
                foreach (DirectoryInfo subDirectory in subDirectories)
                {
                    totalSize += GetDirectorySize(subDirectory);
                }
                return totalSize;
            }
        }

        // 检查ncm二进制文件
        private bool CheckNCMBinary(string filePath)
        {
            string magicHeader = "CTENFDAM";

            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[8];
                    int bytesRead = fileStream.Read(bytes, 0, 8);

                    if (bytesRead == 8)
                    {
                        string header = Encoding.ASCII.GetString(bytes);
                        if (header == magicHeader)
                        {
                            return true;
                        }
                        else
                        {
                            if (fileFolderComboBox.SelectedIndex == 0)
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.Append("文件头为：").Append(header);
                                TaskDialogButton[] buttons = new TaskDialogButton[]
                                {
                                    TaskDialogButton.OK,
                                };
                                showTaskDialog(stringBuilder.ToString(), "不是ncm文件", "错误", TaskDialogIcon.Error, buttons, TaskDialogButton.OK);
                                toolStripStatusLabel2.Text = "不是ncm文件！";
                            }
                            return false;
                        }
                    }
                    else
                    {
                        if (fileFolderComboBox.SelectedIndex == 0)
                        {
                            TaskDialogButton[] buttons = new TaskDialogButton[]
                            {
                                TaskDialogButton.OK,
                            };
                            showTaskDialog("文件大小异常", "不是ncm文件", "错误", TaskDialogIcon.Error, buttons, TaskDialogButton.OK);
                            toolStripStatusLabel2.Text = "文件大小异常，并不是ncm文件";
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (fileFolderComboBox.SelectedIndex == 0)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("详细信息：").Append(ex.Message);
                    TaskDialogButton[] buttons = new TaskDialogButton[]
                    {
                        TaskDialogButton.OK,
                    };
                    showTaskDialog(stringBuilder.ToString(), "发生错误", "错误", TaskDialogIcon.Error, buttons, TaskDialogButton.OK);
                    toolStripStatusLabel2.Text = "载入文件时发生错误";
                }
                return false;
            }
        }

        // 检查路径合法性
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

        // 处理文件
        public int ProcessNCMFile(string path)
        {
            string savePath = "";
            NeteaseCrypt neteaseCrypt = new NeteaseCrypt(path);
            if (saveAsCheckBox.Checked) {
                savePath = saveFolderTextBox.Text;
            }
            int result = neteaseCrypt.Dump(savePath);
            if (fixMetaDataCheckBox.Checked)
            {
                neteaseCrypt.FixMetadata();
            }
            neteaseCrypt.Destroy();
            return result;
        }

        // “转换”按钮被点击
        private void convertButton_Click(object sender, EventArgs e)
        {
            audioStop();
            playGroupBox.Enabled = false;
            filesizeLabel.Text = FileFolderSizeRetriever.GetSizeAsString(filepathTextBox.Text, scanMoreFoldersCheckBox.Checked);
            if (!File.Exists(GlobalVariables.libncmdumpPath))
            {
                TaskDialogButton[] buttons = new TaskDialogButton[]
                {
                    TaskDialogButton.Retry,
                    TaskDialogButton.OK,
                };
                if (showTaskDialog("请确认libncmdump.dll与本程序在同一目录", "核心不存在", "错误", TaskDialogIcon.Error, buttons, TaskDialogButton.OK) == TaskDialogButton.Retry)
                {
                    convertButton.PerformClick();
                }
                toolStripStatusLabel2.Text = "核心不存在！请检查libncmdump.dll";
            }
            else if (filepathTextBox.Text == "")
            {
                TaskDialogButton[] buttons = new TaskDialogButton[]
                {
                    TaskDialogButton.OK,
                };
                showTaskDialog("请提供ncm文件路径", "文件路径为空！", "错误", TaskDialogIcon.Error, buttons, TaskDialogButton.OK);
                toolStripStatusLabel2.Text = "请提供文件";
            }
            else if (!IsValidFilePath(filepathTextBox.Text))
            {
                TaskDialogButton[] buttons = new TaskDialogButton[]
                {
                    TaskDialogButton.OK,
                };
                showTaskDialog("文件路径中包含非法字符！", "非法文件路径", "错误", TaskDialogIcon.Error, buttons, TaskDialogButton.OK);
                toolStripStatusLabel2.Text = "非法文件路径";
            }
            else if (!IsValidFilePath(saveFolderTextBox.Text) && saveAsCheckBox.Checked)
            {
                TaskDialogButton[] buttons = new TaskDialogButton[]
                {
                    TaskDialogButton.OK,
                };
                showTaskDialog("保存文件路径中包含非法字符！", "非法文件路径", "错误", TaskDialogIcon.Error, buttons, TaskDialogButton.OK);
                toolStripStatusLabel2.Text = "非法文件路径";
            }
            else
            {
                filepathTextBox.Enabled = false;
                browseButton.Enabled = false;
                convertButton.Enabled = false;
                fileFolderComboBox.Enabled = false;
                fixMetaDataCheckBox.Enabled = false;
                if (saveAsCheckBox.Checked)
                {
                    saveAsGroupBox.Enabled = false;
                }
                if (fileFolderComboBox.SelectedIndex == 1)
                {
                    int bypassFiles = 0;
                    int processedFiles = 0;
                    int allProcessedFiles = 0;
                    string directoryPath = filepathTextBox.Text;
                    string fileExtension = ".ncm";
                    string[] files = Directory.GetFiles(directoryPath, "*.*", scanMoreFoldersCheckBox.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Where(file => Path.GetExtension(file).ToLower() == fileExtension.ToLower()).ToArray();
                    int fileCount = Directory.GetFiles(directoryPath).Length;
                    int unprocessFiles = fileCount - files.Length;
                    toolStripProgressBar1.Maximum = files.Length;
                    foreach (var file in files)
                    {
                        if (CheckNCMBinary(file))
                        {
                            if (ProcessNCMFile(file) != 0)
                            {
                                bypassFiles += 1;
                            }
                            else
                            {
                                processedFiles += 1;
                            }
                        }
                        allProcessedFiles += 1;
                        toolStripProgressBar1.Value = allProcessedFiles;
                        toolStripStatusLabel2.Text = "已处理：" + allProcessedFiles.ToString();
                    }
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("成功：").Append(processedFiles.ToString()).Append("；失败：").Append(bypassFiles.ToString()).Append("；跳过：").Append(unprocessFiles.ToString());
                    // toolStripStatusLabel2.Text = "成功：" + processedFiles.ToString() + "；失败：" + bypassFiles.ToString() + "；跳过：" + unprocessFiles.ToString();
                    toolStripStatusLabel2.Text = stringBuilder.ToString();
                }
                else if (fileFolderComboBox.SelectedIndex == 0)
                {
                    toolStripProgressBar1.Maximum = 2;
                    if (!filepathTextBox.Text.EndsWith(".ncm"))
                    {
                        TaskDialogButton[] buttons = new TaskDialogButton[]
                        {
                            TaskDialogButton.OK,
                        };
                        showTaskDialog("请提供ncm文件路径", "文件路径为空！", "错误", TaskDialogIcon.Error, buttons, TaskDialogButton.OK);
                        toolStripStatusLabel2.Text = "请提供正确的ncm文件";
                    }
                    else if (CheckNCMBinary(filepathTextBox.Text))
                    {
                        int result = ProcessNCMFile(filepathTextBox.Text);
                        toolStripProgressBar1.Value += 1;
                        if (result != 0)
                        {
                            toolStripStatusLabel2.Text = "发生错误，返回值为：" + result.ToString();
                        }
                        else
                        {
                            resultAudioPath = Path.ChangeExtension(filepathTextBox.Text, ".flac");
                            if (!Path.Exists(resultAudioPath))
                            {
                                resultAudioPath = Path.ChangeExtension(filepathTextBox.Text, ".mp3");
                            }

                            string displayFileName = "";
                            if (Path.GetFileName(resultAudioPath).Length >= 10)
                            {
                                displayFileName = Path.GetFileName(resultAudioPath).Split(".")[0].Substring(0, 20) + "..." + Path.GetExtension(resultAudioPath);
                            }
                            else
                            {
                                displayFileName = Path.GetFileName(resultAudioPath);
                            }

                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.Append("完成！文件名：").Append(displayFileName);
                            // toolStripStatusLabel2.Text = "完成！文件名：" + displayFileName;
                            toolStripStatusLabel2.Text = stringBuilder.ToString();
                            toolStripProgressBar1.Value += 1;
                            playGroupBox.Enabled = true;
                        }
                    }
                }
                if (saveAsCheckBox.Checked)
                {
                    saveAsGroupBox.Enabled = true;
                }
                filepathTextBox.Enabled = true;
                browseButton.Enabled = true;
                convertButton.Enabled = true;
                fileFolderComboBox.Enabled = true;
                fixMetaDataCheckBox.Enabled = true;
                toolStripProgressBar1.Value = 0;
            }
        }

        // 窗口键盘事件
        public void WndMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                convertButton.PerformClick();
            }
        }

        // 文件拖入
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

        private static bool modifyByDrag = false;

        private void WndMain_DragDrop(object sender, DragEventArgs e)
        {
            string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (filePaths.Length >= 1 && convertButton.Enabled == true)
            {
                string path = filePaths[0];
                modifyByDrag = true;
                if (path.Length > 1)
                {
                    toolStripStatusLabel2.Text = "提供了多个文件，仅加载了第一个";
                }
                if (Directory.Exists(path))
                {
                    filepathTextBox.Text = path;
                    fileFolderComboBox.SelectedIndex = 1;
                }
                else if (File.Exists(path))
                {
                    filepathTextBox.Text = path;
                    fileFolderComboBox.SelectedIndex = 0;
                }
            }
        }

        private void fileFolderComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!modifyByDrag)
            {
                filepathTextBox.Text = "";
            }
            else if (modifyByDrag)
            {
                modifyByDrag = false;
            }
            if (fileFolderComboBox.SelectedIndex == 1)
            {
                scanMoreFoldersCheckBox.Visible = true;
                convertButton.ContextMenuStrip = folderConvertContextMenuStrip;
                convertButton.Text = "批量转换";
            }
            else
            {
                scanMoreFoldersCheckBox.Visible = false;
                convertButton.ContextMenuStrip = null;
                convertButton.Text = "转换";
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            TaskDialogButton[] buttons = new TaskDialogButton[]
            {
                TaskDialogButton.OK,
            };
            showTaskDialog("此应用只用于学习用途，禁止用于商业或违法用途，\n请在遵守NCM文件提供平台的服务条款下使用本应用，\n作者对商业或违法使用本软件造成的任何后果不承担任何责任！", "注意！", "免责声明", TaskDialogIcon.Information, buttons, TaskDialogButton.OK);
        }

        private void filepathTextBox_TextChanged(object sender, EventArgs e)
        {
            filesizeLabel.Text = "";
            playGroupBox.Enabled = false;
            if (isPlaying)
            {
                audioStop();
            }
        }

        private void UpdateProgress()
        {
            if (audioFileReader != null)
            {
                int currentPos = (int)(audioFileReader.CurrentTime.TotalSeconds);
                int totalLength = (int)(audioFileReader.TotalTime.TotalSeconds);

                audioProgressTrackBar.Maximum = totalLength;
                audioProgressTrackBar.Value = currentPos;

                audioProgressLabel.Text = $"{(currentPos / 60).ToString("00")}:{(currentPos % 60).ToString("00")} / {(totalLength / 60).ToString("00")}:{(totalLength % 60).ToString("00")}";

                if (isPlaying)
                {
                    timerTrackBar.Start();
                }
                else
                {
                    timerTrackBar.Stop();
                }
            }
        }

        private void audioPlay()
        {
            if (audioFileReader == null)
            {
                audioFileReader = new AudioFileReader(resultAudioPath);
                outputDevice = new WaveOutEvent();
                outputDevice.Init(audioFileReader);
            }

            if (timerTrackBar == null)
            {
                timerTrackBar = new System.Windows.Forms.Timer();
                timerTrackBar.Interval = 1000;
                timerTrackBar.Tick += timerTrackBar_Tick;
                timerTrackBar.Start();
            }

            if (outputDevice.PlaybackState != PlaybackState.Playing)
            {
                outputDevice.Play();
                isPlaying = true;
                playResumeButton.Text = "⏸️";
                UpdateProgress();
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            audioStop();
        }

        private void audioPause()
        {
            if (outputDevice != null)
            {
                outputDevice.Pause();
            }
            isPlaying = false;
            playResumeButton.Text = "▶️";
        }

        private void audioStop()
        {
            if (outputDevice != null)
            {
                outputDevice.Stop();
                outputDevice.Dispose();
                audioFileReader.Dispose();
                audioFileReader = null;
                outputDevice = null;
            }
            isPlaying = false;
            playResumeButton.Text = "▶️";
            audioProgressTrackBar.Value = 0;
            audioProgressLabel.Text = "00:00 / 00:00";
            if (timerTrackBar != null)
            {
                timerTrackBar.Stop();
                timerTrackBar.Tick -= timerTrackBar_Tick;
                timerTrackBar.Dispose();
                timerTrackBar = null;
            }
        }

        private void timerTrackBar_Tick(object sender, EventArgs e)
        {
            UpdateProgress();
        }

        private void audioProgressTrackBar_Scroll(object sender, EventArgs e)
        {
            if (audioFileReader != null && isPlaying)
            {
                audioFileReader.CurrentTime = TimeSpan.FromSeconds(audioProgressTrackBar.Value);
            }
        }

        private void playResumeButton_Click(object sender, EventArgs e)
        {
            if (isPlaying)
            {
                audioPause();
            }
            else
            {
                audioPlay();
            }
        }

        private void WndMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            audioStop();
        }

        private void openWithDefaultPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("explorer", resultAudioPath);
        }

        private void openFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("explorer", $"/select,\"{resultAudioPath}\"");
        }

        private void playMoreButton_Click(object sender, EventArgs e)
        {
            playMoreBtnContextMenuStrip.Show(playMoreButton, new Point(0, playMoreButton.Height));
        }

        private void autoDumpFolderOnChanged(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                if (CheckNCMBinary(e.FullPath))
                {
                    ProcessNCMFile(e.FullPath);
                }
            }
        }

        private void autoDumpOnError(object source, ErrorEventArgs e)
        {
            TaskDialogButton[] buttons = new TaskDialogButton[]
            {
                TaskDialogButton.OK,
            };
            showTaskDialog($"{e.GetException()}", "发生错误", "错误", TaskDialogIcon.Error, buttons, TaskDialogButton.OK);
            toolStripStatusLabel2.Text = "发生错误";
        }

        private void autoDumpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Debug.WriteLine(filepathTextBox.Text);
            if (filepathTextBox.Text == "")
            {
                TaskDialogButton[] buttons = new TaskDialogButton[]
                {
                    TaskDialogButton.OK,
                };
                showTaskDialog("请提供ncm文件路径", "文件路径为空！", "错误", TaskDialogIcon.Error, buttons, TaskDialogButton.OK);
                toolStripStatusLabel2.Text = "请提供文件";
            }
            else if (Directory.Exists(filepathTextBox.Text))
            {
                if (fileWatcher.EnableRaisingEvents)
                {
                    autoDumpToolStripMenuItem.Text = "启用 文件夹实时自动转换";
                    filepathTextBox.Enabled = true;
                    browseButton.Enabled = true;
                    convertButton.Enabled = true;
                    fileFolderComboBox.Enabled = true;
                    fixMetaDataCheckBox.Enabled = true;
                    fileWatcher.EnableRaisingEvents = false;
                }
                else
                {
                    autoDumpToolStripMenuItem.Text = "关闭 文件夹实时自动转换";
                    filepathTextBox.Enabled = false;
                    browseButton.Enabled = false;
                    convertButton.Enabled = false;
                    fileFolderComboBox.Enabled = false;
                    fixMetaDataCheckBox.Enabled = false;
                    fileWatcher.EnableRaisingEvents = true;
                }
            }
            else
            {
                TaskDialogButton[] buttons = new TaskDialogButton[]
                {
                    TaskDialogButton.OK,
                };
                showTaskDialog("请确认路径是否正确", "找不到文件", "错误", TaskDialogIcon.Error, buttons, TaskDialogButton.OK);
                toolStripStatusLabel2.Text = "请确认路径是否正确";
            }
        }

        private void saveAsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            saveAsGroupBox.Enabled = saveAsCheckBox.Checked;
        }

        private void saveAsBrowserButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            dialog.RootFolder = Environment.SpecialFolder.ApplicationData;
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                saveFolderTextBox.Text = dialog.SelectedPath;
            }
        }
    }
}
