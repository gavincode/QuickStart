using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QuickStartUI
{
    public class FileInfos
    {
        //.sln文档的图标只能以主线程的身份读取
        public static Icon slnIcon = null;
        public static void SetSlnIcon(Icon icon)
        {
            slnIcon = icon;
        }

        public FileInfos(String file)
        {
            Name = Path.GetFileName(file);
            if (String.IsNullOrEmpty(Name)) Name = file;
            if (Path.GetExtension(file) == ".lnk") Name = Path.GetFileNameWithoutExtension(file);

            LowerName = Name.ToLower();
            NameLetters = ChineseToLetter.ToLetters(Path.GetFileNameWithoutExtension(LowerName));
            FilePath = file;
            Crdate = File.GetLastAccessTime(file);

            if (Path.GetExtension(Name) == ".sln" && slnIcon != null)
                Icon = slnIcon;
            else
                Icon = IconHandler.GetFileIcon(file);

            ShowLable = new Label();
        }

        public Icon Icon { get; set; }
        public String Name { get; set; }
        public String LowerName { get; set; }
        public String NameLetters { get; set; }
        public String FilePath { get; set; }
        public DateTime Crdate { get; set; }
        public Label ShowLable { get; set; }
    }
}
