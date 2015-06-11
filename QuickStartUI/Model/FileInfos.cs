using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QuickStartUI
{
    public class FileInfos
    {
        public FileInfos(String file)
        {
            Name = Path.GetFileName(file);
            if (String.IsNullOrEmpty(Name)) Name = file;
            if (Path.GetExtension(file) == ".lnk") Name = Path.GetFileNameWithoutExtension(file);

            LowerName = Name.ToLower();
            NameLetters = ChineseToLetter.ToLetters(Path.GetFileNameWithoutExtension(LowerName));
            FilePath = file;
            Crdate = File.GetLastAccessTime(file);
        }

        public String Name { get; set; }
        public String LowerName { get; set; }
        public String NameLetters { get; set; }
        public String FilePath { get; set; }
        public DateTime Crdate { get; set; }
    }
}
