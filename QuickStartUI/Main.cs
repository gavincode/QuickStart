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

        //缓存列表读取锁
        private static readonly Object lockObj = new Object();

        //缓存文件列表
        private static List<FileInfos> cachedFiles = new List<FileInfos>();

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
            StartNewTast(() =>
            {
                var files = FileReader.ReadFiles();

                var fileInfos = Convert(files);

                SyncFiles(fileInfos);

                FileHistory.Write(files.Where(q => q.EndsWith(Constant.LinkExtension)));
            });
        }

        private void SyncFiles(List<FileInfos> fileInfos)
        {
            lock (lockObj)
            {
                cachedFiles = fileInfos;
            }

            InvokeMethod(() => txtSearch_TextChanged(null, null));
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
                        case Constant.hotKeyAltX:
                        case Constant.hotKeyEscape:
                            ChangeWindowState();
                            break;
                        case Constant.hotKeyAltO:
                            OpenFile(Environment.CurrentDirectory);
                            break;
                        case Constant.hotKeyAltE:
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
                MoveDown();
            else
                MoveUp();

            base.OnMouseWheel(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {

#if !DEBUG
            e.Cancel = true;
#endif

            ChangeWindowState();

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
                MoveUp();
                return;
            }

            if (e.KeyCode == Keys.Down)
            {
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

            if (String.IsNullOrEmpty(search) || search.EndsWith(".."))
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
            OpenFile(GetCurrentPath());
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
            var selectedFile = GetCurrentPath();

            lock (lockObj)
            {
                cachedFiles.RemoveAll(p => p.FilePath == selectedFile);
            }

            txtSearch_TextChanged(null, null);

            FileFilter.AddFileIgnore(selectedFile);
        }

        private void tsmiIgnoreType_Click(object sender, EventArgs e)
        {
            FileFilter.AddTypeIgnore(GetCurrentPath());
        }

        private void tsmiRefresh_Click(object sender, EventArgs e)
        {
            StartNewTast(() =>
            {
                OpenFile(Assembly.GetExecutingAssembly().Location);
            });

            Application.Exit();
        }

        private void tsmiOpen_Click(object sender, EventArgs e)
        {
            var selectedFile = GetCurrentPath();

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
                this.Text = "快速启动 - Total: " + dataSource.Count() + "         快捷键-  " + Constant.ShowHotKeys + "   关闭:[Alt + E]   刷新:[Alt + R]   打开目录:[Alt + O]";

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
            LoadSlnIconSync(files);

            foreach (var item in files)
            {
                fileInfos.Add(new FileInfos(item));
            }
        }

        private void LoadSlnIconSync(IEnumerable<String> files)
        {
            //.sln文档的图标只能以主线程的身份读取
            if (FileInfos.slnIcon == null)
            {
                var sln = files.FirstOrDefault(q => q.EndsWith(Constant.SlnExtension));
                if (sln == null) return;

                InvokeMethod(() =>
                {
                    FileInfos.SetSlnIcon(new FileInfos(sln).Icon);
                });
            }
        }

        private String GetCurrentPath()
        {
            if (this.dataGridView.CurrentRow == null) return null;

            return this.dataGridView.CurrentRow.Cells["FilePath"].Value.ToString();
        }

        private void OpenFile(String path)
        {
            try
            {
                ChangeWindowState();

                Process.Start(path);
            }
            catch (Exception)
            {
                MessageBox.Show("[启动失败]该文件已被删除或已失效!", "提示", MessageBoxButtons.OK);
            }
        }

        private void InvokeMethod(Action action)
        {
            BeginInvoke(action);
        }

        private void StartNewTast(Action action)
        {
            ThreadPool.QueueUserWorkItem(p =>
            {
                action();
            });
        }

        private void MoveUp()
        {
            MoveRow(-1);
        }

        private void MoveDown()
        {
            MoveRow(1);
        }

        private void MoveRow(Int32 moveIndex)
        {
            if (this.dataGridView.CurrentRow == null) return;

            Int32 nextIndex = this.dataGridView.CurrentRow.Index + moveIndex;

            if (nextIndex >= dataGridView.Rows.Count || nextIndex < 0) return;

            this.dataGridView.Rows[nextIndex].Selected = true;
            this.dataGridView.CurrentCell = this.dataGridView.Rows[nextIndex].Cells[0];

        }

        private void ChangeWindowState()
        {
            UnRegistHotKey();

            Boolean isNormal = this.WindowState == FormWindowState.Normal;

            //设置控件状态
            this.WindowState = isNormal ? FormWindowState.Minimized : FormWindowState.Normal;
            this.ShowInTaskbar = !isNormal;
            this.notifyIcon.Visible = isNormal;

            RegistHotKey();
        }

        private void RegistHotKey()
        {
            HotKeyHelper.RegisterHotKey(this.Handle, Constant.hotKeyAltX, KeyModifiers.Alt, Keys.X);
            HotKeyHelper.RegisterHotKey(this.Handle, Constant.hotKeyAltO, KeyModifiers.Alt, Keys.O);
            HotKeyHelper.RegisterHotKey(this.Handle, Constant.hotKeyAltE, KeyModifiers.Alt, Keys.E);
            HotKeyHelper.RegisterHotKey(this.Handle, Constant.hotKeyEscape, KeyModifiers.None, Keys.Escape);
        }

        private void UnRegistHotKey()
        {
            HotKeyHelper.UnregisterHotKey(this.Handle, Constant.hotKeyAltX);
            HotKeyHelper.UnregisterHotKey(this.Handle, Constant.hotKeyAltO);
            HotKeyHelper.UnregisterHotKey(this.Handle, Constant.hotKeyAltE);
            HotKeyHelper.UnregisterHotKey(this.Handle, Constant.hotKeyEscape);
        }

        #endregion
    }
}