// ****************************************
// FileName:FileFilter.cs
// Description:
// Tables:
// Author:Gavin
// Create Date:2015/5/26 9:41:25
// Revision History:
// ****************************************

using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace QuickStartUI
{
    /// <summary>
    /// 文件过滤类
    /// </summary>
    internal class FileFilter
    {
        public static void AddFileIgnore(String path)
        {
            if (File.Exists(path))
            {
                File.AppendAllText(FileForeachHelper.fnFileText, Environment.NewLine + Path.GetFileName(path));
            }
            else if (Directory.Exists(path))
            {
                File.AppendAllText(FileForeachHelper.fnFolderText, Environment.NewLine + Path.GetFileName(path));
            }
        }

        public static void AddTypeIgnore(String path)
        {
            if (!File.Exists(path)) return;

            String ext = Path.GetExtension(path);

            File.AppendAllText(FileForeachHelper.fnFileTypeText, Environment.NewLine + ext);
        }
    }
}
