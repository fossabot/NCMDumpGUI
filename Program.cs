using Dark.Net;

namespace NCMDumpGUI
{
    public static class GlobalVariables
    {
        public static readonly string assemblyPath = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string libncmdumpPath = assemblyPath + "libncmdump.dll";
    }

    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            IDarkNet darkNet = DarkNet.Instance;
            if (!File.Exists(GlobalVariables.libncmdumpPath))
            {
                MessageBox.Show("核心不存在\n请确认libncmdump.dll与本程序在同一目录", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            else
            {
                ApplicationConfiguration.Initialize();
                Form mainForm = new WndMain(args);
                Theme windowTheme = Theme.Auto;
                darkNet.SetWindowThemeForms(mainForm, windowTheme);
                Application.Run(mainForm);
            }
        }
    }
}