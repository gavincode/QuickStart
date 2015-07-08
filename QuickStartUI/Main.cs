using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace QuickStartUI
{
    public partial class Main : Form
    {
        #region 初始化

        private static readonly Object lockObj = new Object();
        private static readonly Object lockInvoke = new Object();

        public static readonly String ShowHotKeys = "切换主界面:  [ESC] 或 [Alt + X]";

        //缓存文件列表
        private static List<FileInfos> cachedFiles = new List<FileInfos>();

        //快捷键Id常量
        const Int32 hotKeyA = 987654;
        const Int32 hotKeyB = 456789;
        const Int32 hotKeyC = 756485;
        const Int32 hotKeyD = 865749;

        public Main()
        {
            InitializeComponent();
            Initializenotifyicon();
            InitText();
            RegistHotKey();
            RefreshHistory();
            LoadHistory();
        }

        private void LoadHistory()
        {
            var files = FileHistory.Read();

            cachedFiles = Convert(files);

            BindGridView(cachedFiles, false);
        }

        private void RefreshHistory()
        {
            ThreadPool.QueueUserWorkItem(p =>
            {
                var files = FileReader.ReadFiles();

                var fileInfos = Convert(files);

                lock (lockObj)
                {
                    cachedFiles = fileInfos;
                }

                InvokeMethod(() => txtSearch_TextChanged(null, null));

                FileHistory.Write(files.Where(q => q.EndsWith(".lnk")));
            });
        }

        private void InitText()
        {
            this.txtSearch.Focus();
            this.txtSearch.Enabled = true;
            this.txtSearch.SelectionStart = 0;
            this.txtSearch.SelectionLength = this.txtSearch.Text.Length;
        }

        private void Initializenotifyicon()
        {
            //定义一个MenuItem数组，并把此数组同时赋值给ContextMenu对象 
            MenuItem[] mnuItms = new MenuItem[3];
            mnuItms[0] = new MenuItem();
            mnuItms[0].Text = "显示窗口";
            mnuItms[0].Click += new System.EventHandler(this.notifyIcon_Showfrom);

            mnuItms[1] = new MenuItem("-");

            mnuItms[2] = new MenuItem();
            mnuItms[2].Text = "退出系统";
            mnuItms[2].Click += new System.EventHandler(this.notifyIcon_Exit);
            mnuItms[2].DefaultItem = true;

            this.notifyIcon.Visible = false;
            this.notifyIcon.ContextMenu = new ContextMenu(mnuItms);
        }

        #endregion

        #region 窗体事件

        protected override void WndProc(ref Message msg)
        {
            const int WM_HOTKEY = 0x0312;
            //按快捷键   
            switch (msg.Msg)
            {
                case WM_HOTKEY:
                    switch (msg.WParam.ToInt32())
                    {
                        case hotKeyA:
                        case hotKeyD:
                            ChangeWindowState();
                            break;
                        case hotKeyB:
                            OpenFile(Environment.CurrentDirectory);
                            break;
                        case hotKeyC:
                            this.notifyIcon.Visible = false;
                            Application.Exit();
                            break;
                        default:
                            break;
                    }
                    break;
            }

            base.WndProc(ref msg);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                MoveDown();
            }
            else
            {
                MoveUp();
            }

            base.OnMouseWheel(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ChangeWindowState();
            e.Cancel = true;
#if DEBUG
            e.Cancel = false;
#endif
            if (!e.Cancel)
            {
                UnRegistHotKey();
            }

            base.OnClosing(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            InitText();
            base.OnActivated(e);
        }

        #endregion

        #region 控件事件

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            if (e.Modifiers == Keys.Alt && (e.KeyCode == Keys.Up || e.KeyCode == Keys.D))
            {
                tsmiOpen_Click(null, null);
                return;
            }

            if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.R)
            {
                tsmiRefresh_Click(null, null);
                return;
            }

            if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.C)
            {
                var selectedPath = GetCurrentPath();
                Clipboard.SetText(selectedPath);
                return;
            }

            if (e.KeyCode == Keys.Up)
            {
                e.Handled = true;
                MoveUp();
                return;
            }

            if (e.KeyCode == Keys.Down)
            {
                e.Handled = true;
                MoveDown();
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                dataGridView_CellMouseDoubleClick(null, null);
                return;
            }

            if (this.txtSearch.Text.EndsWith(".."))
            {
                InitText();
                return;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            String search = txtSearch.Text.Trim().ToLower();

            if (String.IsNullOrEmpty(search) || search.StartsWith("请输入需要启动文件的关键字"))
            {
                BindGridView(cachedFiles.OrderByDescending(q => q.Crdate));
            }
            else
            {
                BindGridView(cachedFiles.Where(p => p.GetLowerName().Contains(search) || p.GetNameLetters().Contains(search))
                                        .OrderBy(p => p.GetNameLetters().Length)
                                        .ThenByDescending(p => p.Crdate));
            }
        }

        private void dataGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            this.txtSearch.Focus();
            this.txtSearch.SelectionStart = txtSearch.Text.Length;
        }

        private void dataGridView_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (this.dataGridView.CurrentRow == null) return;

            var filePath = this.dataGridView.CurrentRow.Cells["FilePath"].Value.ToString();

            OpenFile(filePath);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            dataGridView_CellMouseDoubleClick(null, null);
        }

        public void notifyIcon_Showfrom(object sender, System.EventArgs e)
        {
            ChangeWindowState();
        }

        public void notifyIcon_Exit(object sender, System.EventArgs e)
        {
            this.Close();
            this.Dispose(true);
        }

        private void tsmiIngoreFile_Click(object sender, EventArgs e)
        {
            var selectedFile = dataGridView.CurrentRow.Cells["FilePath"].Value.ToString();

            lock (lockObj)
            {
                cachedFiles.RemoveAll(p => p.FilePath == selectedFile);
            }

            txtSearch_TextChanged(null, null);

            FileFilter.AddFileIgnore(selectedFile);
        }

        private void tsmiIgnoreType_Click(object sender, EventArgs e)
        {
            var selectedFile = dataGridView.CurrentRow.Cells["FilePath"].Value.ToString();

            FileFilter.AddTypeIgnore(selectedFile);

            //RefreshHistory();
        }

        private void tsmiRefresh_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(p =>
            {
                OpenFile(Assembly.GetExecutingAssembly().Location);
            });

            Application.Exit();
        }

        private void tsmiOpen_Click(object sender, EventArgs e)
        {
            var selectedFile = dataGridView.CurrentRow.Cells["FilePath"].Value.ToString();

            if (Directory.GetParent(selectedFile) == null) return;

            OpenFile(Directory.GetParent(selectedFile).FullName);
        }

        private void txtSearch_DoubleClick(object sender, EventArgs e)
        {
            InitText();
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                ChangeWindowState();
            }
        }

        #endregion

        #region 私有方法

        private void BindGridView(IEnumerable<FileInfos> dataSource, Boolean changeText = true)
        {
            lock (lockObj)
            {
                this.dataGridView.DataSource = dataSource.ToList();
            }

            if (changeText)
                this.Text = "快速启动 - Total: " + dataSource.Count() + "         快捷键-  " + ShowHotKeys + "   关闭:[Alt + E]   刷新:[Alt + R]   打开目录:[Alt + O]";

            //dataGridView
            this.dataGridView.Columns[0].HeaderText = String.Empty;
            this.dataGridView.Columns[0].FillWeight = 3;
            this.dataGridView.Columns[1].FillWeight = 20;
            this.dataGridView.Columns[2].FillWeight = 65;
            this.dataGridView.Columns[3].FillWeight = 12;
        }

        private List<FileInfos> Convert(IEnumerable<String> files)
        {
            List<FileInfos> fileInfos = new List<FileInfos>();

            ConvertTo(files, fileInfos);

            return fileInfos.OrderBy(p => p.Name).ToList();
        }

        private void ConvertTo(IEnumerable<String> files, List<FileInfos> fileInfos)
        {
            //.sln文档的图标只能以主线程的身份读取
            if (FileInfos.slnIcon == null)
            {
                var sln = files.FirstOrDefault(q => q.EndsWith(".sln"));
                InvokeMethod(() =>
                {
                    if (sln == null) return;
                    FileInfos.SetSlnIcon(new FileInfos(sln).Icon);
                });
            }

            foreach (var item in files)
            {
                //if (File.Exists(item) || Directory.Exists(item))
                fileInfos.Add(new FileInfos(item));
            }
        }

        private String GetCurrentPath()
        {
            if (this.dataGridView.CurrentRow == null) return null;

            return this.dataGridView.CurrentRow.Cells["FilePath"].Value.ToString();
        }

        private Boolean OpenFile(String path)
        {
            Boolean opened = false;

            if (File.Exists(path) || Directory.Exists(path))
            {
                ThreadPool.QueueUserWorkItem(p =>
                {
                    try
                    {
                        Process.Start(path);

                        InvokeMethod(() => ChangeWindowState());
                    }
                    catch (Exception)
                    {
                        InvokeMethod(() => MessageBox.Show("[启动失败]该文件已被删除或已失效!", "提示", MessageBoxButtons.OK));
                    }
                });
                opened = true;
            }

            return opened;
        }

        private void InvokeMethod(Action action)
        {
            lock (lockInvoke)
            {
                Invoke(action);
            }
        }

        private void MoveDown()
        {
            if (this.dataGridView.CurrentRow == null) return;
            Int32 index = this.dataGridView.CurrentRow.Index + 1;

            if (index >= dataGridView.Rows.Count) return;

            this.dataGridView.Rows[index].Selected = true;
            this.dataGridView.CurrentCell = this.dataGridView.Rows[index].Cells[0];
        }

        private void MoveUp()
        {
            if (this.dataGridView.CurrentRow == null) return;
            Int32 index = this.dataGridView.CurrentRow.Index - 1;

            if (index < 0) return;

            this.dataGridView.Rows[index].Selected = true;
            this.dataGridView.CurrentCell = this.dataGridView.Rows[index].Cells[0];
        }

        private void ChangeWindowState()
        {
            UnRegistHotKey();

            Boolean isNormal = this.WindowState == FormWindowState.Normal;

            //设置控件状态
            this.WindowState = isNormal ? FormWindowState.Minimized : FormWindowState.Normal;
            this.ShowInTaskbar = !isNormal;
            this.notifyIcon.Visible = isNormal;

            if (!isNormal)
            {
                this.Select();
            }

            RegistHotKey();
        }

        private void RegistHotKey()
        {
            HotKeyHelper.RegisterHotKey(this.Handle, hotKeyA, KeyModifiers.Alt, Keys.X);
            HotKeyHelper.RegisterHotKey(this.Handle, hotKeyB, KeyModifiers.Alt, Keys.O);
            HotKeyHelper.RegisterHotKey(this.Handle, hotKeyC, KeyModifiers.Alt, Keys.E);
            HotKeyHelper.RegisterHotKey(this.Handle, hotKeyD, KeyModifiers.None, Keys.Escape);
        }

        private void UnRegistHotKey()
        {
            HotKeyHelper.UnregisterHotKey(this.Handle, hotKeyA);
            HotKeyHelper.UnregisterHotKey(this.Handle, hotKeyB);
            HotKeyHelper.UnregisterHotKey(this.Handle, hotKeyC);
            HotKeyHelper.UnregisterHotKey(this.Handle, hotKeyD);
        }

        #endregion
    }
}