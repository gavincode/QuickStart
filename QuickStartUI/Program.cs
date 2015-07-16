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

            //防止多次执行   
            if (OpenedMoreThanOnce())
            {
                MessageBox.Show("程序正在在运行!   " + Constant.ShowHotKeys, "提示", MessageBoxButtons.OK);
                return;
            }

            Application.Run(new Main());
        }

        private static Boolean OpenedMoreThanOnce()
        {
            return GetSameProcess().Length >= 1;
        }

        private static Process[] GetSameProcess()
        {
            Process current = Process.GetCurrentProcess();

            var assembleName = Assembly.GetExecutingAssembly().GetName().Name;

            Process[] processes = Process.GetProcessesByName(assembleName);

            return processes.Where(p => p.Id != current.Id).ToArray();
        }
    }
}
