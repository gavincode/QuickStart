using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QuickStartUI
{
    class FileReader
    {
        const String file = "Folders.txt";

        public static List<String> ReadFiles()
        {
            var folders = GetFolders().OrderByDescending(p => p.Split('\\').Length).ThenByDescending(q => q.Length);

            List<String> files = new List<String>();

            foreach (var item in folders)
            {
                files.AddRange(FileForeachHelper.GetAllFiles(item));
            }

            FileForeachHelper.ClearHistory();

            return files;
        }

        private static IEnumerable<String> GetFolders()
        {
            return File.ReadAllLines(file);
        }
    }
}
