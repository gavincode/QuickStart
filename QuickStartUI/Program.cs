using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace QuickStartUI
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Process current = Process.GetCurrentProcess();

            Process[] processes = Process.GetProcessesByName(Assembly.GetExecutingAssembly().GetName().Name);
            processes = processes.Where(p => p.Id != current.Id).ToArray();

            //防止多次执行   
            if (processes.Length >= 1)
            {
                MessageBox.Show("程序正在在运行!   " + QuickStartUI.Main.ShowHotKeys, "提示", MessageBoxButtons.OK);
            }
            else
            {
                Application.Run(new Main());
            }
        }
    }
}
