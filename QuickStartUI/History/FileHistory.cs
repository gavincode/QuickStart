using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace QuickStartUI
{
    class FileHistory
    {
        const String file = @"history.txt";

        static FileHistory()
        {
            if (!File.Exists(file))
            {
                File.CreateText(file);
            }
        }

        public static void Write(IEnumerable<String> files)
        {
            using (var writer = File.CreateText(file))
            {
                foreach (var item in files)
                {
                    writer.WriteLine(item);
                }
            }
        }

        public static IEnumerable<String> Read()
        {
            return File.ReadAllLines(file);
        }
    }
}
