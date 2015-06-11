using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Reflection;

namespace AddFolder
{
    class Program
    {
        static String exeFile = String.Empty;

        static void Main(string[] args)
        {
            exeFile = Assembly.GetExecutingAssembly().Location + " \"%1\"";

            if (args.Length != 1)
            {
                SetSysEnvironment();
                return;
            }

            var addFile = args[0];

            var file = AppDomain.CurrentDomain.BaseDirectory + "\\Folders.txt";
            if (!File.Exists(file)) return;

            File.AppendAllText(file, Environment.NewLine + addFile);
        }

        /// <summary>
        /// 打开系统环境变量注册表
        /// </summary>
        /// <returns>RegistryKey</returns>
        private static RegistryKey SetSysEnvironment()
        {
            RegistryKey regLocalMachine = Registry.ClassesRoot;
            RegistryKey regDirectory = regLocalMachine.OpenSubKey("Directory", true);
            RegistryKey regShell = regDirectory.OpenSubKey("shell", true);

            RegistryKey regQuickStart = regShell.OpenSubKey("quickstart", true);
            if (regQuickStart == null)
            {
                regQuickStart = regShell.CreateSubKey("quickstart");
            }

            RegistryKey regCommand = regQuickStart.OpenSubKey("command", true);
            if (regCommand == null || regCommand.GetValue("").ToString() != exeFile)
            {
                regCommand = regQuickStart.CreateSubKey("command");
                regCommand.SetValue("", exeFile);
            }

            return regCommand;
        }
    }
}
