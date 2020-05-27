using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using ThreadState = System.Threading.ThreadState;
using Timer = System.Windows.Forms.Timer;

namespace ARKServerQuery
{
    // 監控介面
    public partial class Watchdog : Window
    {
        #region 視窗初始化

        public Watchdog()
        {
            InitializeComponent();
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering); // 用於鍵盤綁定
            Content = mainPanel;
            InitQuery();
        }

        // 裝伺服器資訊(ServerLabel型態)的主要容器
        private StackPanel mainPanel = new StackPanel();

        #endregion

        #region 鍵盤綁定
        // Hook全域鍵盤，在該視窗重新渲染時執行
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            bool isKeyDown = ((Keyboard.GetKeyStates(Key.OemTilde) & KeyStates.Down) > 0) ||
                ((Keyboard.GetKeyStates(Key.OemQuotes) & KeyStates.Down) > 0);

            bool isManipulatable = (((Keyboard.GetKeyStates(Key.OemTilde) & KeyStates.None) == 0) ||
                ((Keyboard.GetKeyStates(Key.OemQuotes) & KeyStates.None) == 0)) && canManipulateWindow;

            if (isKeyDown) ToggleManipulateWindow(KeyStates.Down);
            else if (isManipulatable) ToggleManipulateWindow(KeyStates.None);
        }

        // 初始化時將目前視窗參數儲存
        private IntPtr hwnd;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetOriStyle(hwnd);
        }

        #endregion

        #region 與查詢介面的通訊

        // 儲存查詢介面傳來的伺服器IP位址
        private List<string> watchIPList = new List<string>();

        public bool IsWatchListEmpty()
        {
            return GetServerListCount() == 0;
        }

        public void AddWatchList(string serverInfo)
        {
            lock (watchIPList)
            {
                if (!watchIPList.Contains(serverInfo)) watchIPList.Add(serverInfo);
                else if (watchIPList.Contains(serverInfo)) watchIPList.Remove(serverInfo);
            }
        }

        public void DisableAllWatch()
        {
            watchIPList.Clear();
        }

        #endregion

        #region 查詢主計時器

        // 每秒訪問一次清單中的伺服器
        private Timer QueryTimer = new Timer { Interval = 1000 };
        private void InitQuery()
        {
            QueryTimer.Tick += new EventHandler(QueryTick);
            QueryTimer.Start();
        }

        // 線程: 訪問陣列中的伺服器後更新文字
        Thread mainQueryThread;
        private void QueryTick(object sender, EventArgs e)
        {
            if (mainQueryThread == null || mainQueryThread.ThreadState != ThreadState.Running)
            {
                mainQueryThread = new Thread(ServerQuery);
                mainQueryThread.Start();
            }
        }

        public void EnableQueryTimer(bool enable)
        {
            if (enable) QueryTimer.Start();
            else QueryTimer.Stop();
        }

        double gFontSize = 20.0;

        // 目前顯示的數量與目前清單的數量
        private int GetServerDisplayCount()
        {
            return mainPanel.Dispatcher.Invoke(() => mainPanel.Children.Count);
        }

        private int GetServerListCount()
        {
            return watchIPList.Count;
        }

        /* 監控顯示步驟
         * 1. 檢查已顯示與未顯示的物件數量差距
         * 2. 更新數量
         * 3. 更新已顯示的伺服器資訊
         */
        private void ServerQuery()
        {
            lock (watchIPList)
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        int offset = GetServerListCount() - GetServerDisplayCount();
                        if (offset > 0)
                            for (int i = 0; i < offset; i++)
                                mainPanel.Dispatcher.Invoke(() => mainPanel.Children.Add(new ServerLabel(string.Empty, ClickDrag, ChangeSize, gFontSize)));
                        else if (offset < 0)
                            for (int i = 0; i < Math.Abs(offset); i++)
                                mainPanel.Dispatcher.Invoke(() => mainPanel.Children.RemoveAt(GetServerListCount() - 1));

                        int cnt = 0;
                        foreach (ServerLabel child in mainPanel.Dispatcher.Invoke(() => mainPanel.Children))
                            child.UpdateInfo(watchIPList.ElementAt(cnt++));
                        SizeToContent = SizeToContent.WidthAndHeight;
                    } catch { }
                });
        }

        #endregion

        #region 鍵盤/滑鼠與程式間的交互

        private bool canManipulateWindow = false;

        private KeyStates gKeyStates = KeyStates.None;

        private void ToggleManipulateWindow(KeyStates inKeyStates)
        {
            /* None -> Down, Down -> None : 改變可操縱視窗狀態並保存目前狀態，視窗可移動時將停止伺服器訪問以增進使用者體驗
             * None -> None, Down -> Down : 不做任何事
             */
            if (inKeyStates != gKeyStates) // 狀態改變則致能
            {
                canManipulateWindow = (canManipulateWindow) ? false : true;
                WindowsServices.SetWindowExTransparent(hwnd);
                gKeyStates = inKeyStates;
                if (canManipulateWindow) QueryTimer.Stop();
                else QueryTimer.Start();
            }
        }

        private void ClickDrag(object sender, MouseButtonEventArgs e) => DragMove();

        private void ChangeSize(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) // 滾輪向上放大字體，反之縮小字體
            {
                foreach (ServerLabel child in mainPanel.Dispatcher.Invoke(() => mainPanel.Children))
                {
                    if (child.FontSize <= 2) break;
                    child.FontSize += 5;
                    gFontSize = child.FontSize;
                }
            }
            else
            {
                foreach (ServerLabel child in mainPanel.Dispatcher.Invoke(() => mainPanel.Children))
                {
                    if (child.FontSize >= int.MaxValue) break;
                    child.FontSize -= 5;
                    gFontSize = child.FontSize;
                }
            }
        }

        #endregion
    }
}
