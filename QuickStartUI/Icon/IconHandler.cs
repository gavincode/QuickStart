using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace QuickStartUI
{
    public class IconHandler
    {
        static readonly uint m_uflagFile = (uint)(SHGFI.SHGFI_ICON | SHGFI.SHGFI_SMALLICON | SHGFI.SHGFI_USEFILEATTRIBUTES);
        static readonly uint m_uflagDirectory = (uint)(SHGFI.SHGFI_ICON | SHGFI.SHGFI_SMALLICON);

        static Icon m_FolderIcon = null;

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        /// <summary>  
        /// 返回系统设置的图标  
        /// </summary>  
        /// <param name="pszPath">文件路径 如果为""  返回文件夹的</param>  
        /// <param name="dwFileAttributes">0</param>  
        /// <param name="psfi">结构体</param>  
        /// <param name="cbSizeFileInfo">结构体大小</param>  
        /// <param name="uFlags">枚举类型</param>  
        /// <returns>-1失败</returns>  
        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref   SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        public enum SHGFI
        {
            SHGFI_ICON = 0x100,
            SHGFI_LARGEICON = 0x0,
            SHGFI_SMALLICON = 0x1,
            SHGFI_USEFILEATTRIBUTES = 0x10
        }

        /// <summary>  
        /// 获取文件图标 
        /// </summary>  
        /// <param name="p_Path">文件全路径</param>  
        /// <returns>图标</returns>  
        public static Icon GetIcon(string p_Path)
        {
            if (Directory.Exists(p_Path))
            {
                if (m_FolderIcon == null) m_FolderIcon = GetDirectoryIcon();

                return m_FolderIcon;
            }

            if (File.Exists(p_Path))
            {
                return GetFileIcon(p_Path);
            }

            return null;
        }

        /// <summary>  
        /// 获取文件图标 
        /// </summary>  
        /// <param name="p_Path">文件全路径</param>  
        /// <returns>图标</returns>  
        public static Icon GetFileIcon(string p_Path)
        {
            SHFILEINFO _SHFILEINFO = new SHFILEINFO();

            IntPtr _IconIntPtr = SHGetFileInfo(p_Path, 0, ref _SHFILEINFO, (uint)Marshal.SizeOf(_SHFILEINFO), m_uflagFile);

            if (_IconIntPtr.Equals(IntPtr.Zero) || _SHFILEINFO.hIcon.Equals(IntPtr.Zero)) return null;

            return Icon.FromHandle(_SHFILEINFO.hIcon);
        }

        /// <summary>  
        /// 获取文件夹图标
        /// </summary>  
        /// <returns>图标</returns>  
        public static Icon GetDirectoryIcon()
        {
            SHFILEINFO _SHFILEINFO = new SHFILEINFO();

            IntPtr _IconIntPtr = SHGetFileInfo(@"", 0, ref _SHFILEINFO, (uint)Marshal.SizeOf(_SHFILEINFO), m_uflagDirectory);

            if (_IconIntPtr.Equals(IntPtr.Zero) || _SHFILEINFO.hIcon.Equals(IntPtr.Zero)) return null;

            return Icon.FromHandle(_SHFILEINFO.hIcon);
        }
    }
}
