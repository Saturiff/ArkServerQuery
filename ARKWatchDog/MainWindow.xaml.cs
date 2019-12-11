using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using ThreadState = System.Threading.ThreadState;
using Timer = System.Windows.Forms.Timer;

namespace ARKWatchdog
{
    public partial class MainWindow : Window
    {
        #region 視窗初始化

        public MainWindow()
        {
            InitializeComponent();
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
            Content = mainPanel;
            Thread localSvThread = new Thread(LocalClient); // Thread: 本地TCP，處理ASQ(ARKServerQuery)的訊號
            localSvThread.Start();
            InitQuery();
        }

        // 裝伺服器資訊(ServerLabel型態)的主要容器
        private StackPanel mainPanel = new StackPanel();

        private void InitQuery()
        {
            QueryTimer.Tick += new EventHandler(QueryTick);
            QueryTimer.Start();
        }

        #endregion

        #region 鍵盤綁定
        // Hook全域鍵盤，在該視窗重新渲染時執行
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            bool isKeyDown = ((Keyboard.GetKeyStates(Key.OemTilde) & KeyStates.Down) > 0) || ((Keyboard.GetKeyStates(Key.OemQuotes) & KeyStates.Down) > 0);
            bool isManipulatable = (((Keyboard.GetKeyStates(Key.OemTilde) & KeyStates.None) == 0) || ((Keyboard.GetKeyStates(Key.OemQuotes) & KeyStates.None) == 0)) && canManipulateWindow;
            if (isKeyDown)
            {
                ToggleManipulateWindow(KeyStates.Down);
            }
            else if (isManipulatable)
            {
                ToggleManipulateWindow(KeyStates.None);
            }
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

        #region 本地客戶端

        // 儲存ASQ傳來的伺服器IP位址
        private List<string> watchIPList = new List<string>();

        private void LocalClient()
        {
            TcpClient client;
            BinaryReader br;
            client = new TcpClient("127.0.0.1", 18500); // 使用本地port18500與ASQ進行TCP傳輸
            while (true)
            {
                if (Process.GetProcessesByName("ARKServerQuery").Length == 0) // ASQ關閉時自行結束
                {
                    Close();
                    Environment.Exit(Environment.ExitCode);
                }
                /* 嘗試接收ASQ的訊息
                 * _disable ASQ按下了「取消監控」 -> 清空已儲存的IP
                 * _visi    ASQ按下了「隱藏監控」 -> 隱藏文字
                 * _lang,   ASQ更改語言          -> 文字切割後改變為指定語言
                 * else     ASQ傳遞了伺服器位址   -> 不在清單內 => 新增 or 在清單內 => 移除
                 */
                try
                {
                    NetworkStream clientStream = client.GetStream();
                    br = new BinaryReader(clientStream);
                    string receive = string.Empty;
                    receive = br.ReadString();
                    if (receive == "_disable")
                    {
                        watchIPList.Clear();
                    }
                    else if (receive == "_visi") ToggleVisibility();
                    else if (receive.Substring(0, 6) == "_lang,") ServerLabel.UpdateLanguage(receive.Substring(6, 5));
                    else
                    {
                        lock (watchIPList)
                        {
                            if (!watchIPList.Contains(receive)) watchIPList.Add(receive);
                            else if (watchIPList.Contains(receive)) watchIPList.Remove(receive);
                        }
                    }
                }
                catch { }
            }
        }
        #endregion

        #region 查詢主計時器

        // 線程: 訪問陣列中的伺服器後更新文字
        Thread textUpdate;
        private void QueryTick(object sender, EventArgs e)
        {
            if (textUpdate == null || textUpdate.ThreadState != ThreadState.Running)
            {
                textUpdate = new Thread(ServerQuery);
                textUpdate.Start();
            }
        }

        Timer QueryTimer = new Timer { Interval = 1000 };

        List<ServerLabel> labelList = new List<ServerLabel>();

        double gFontSize = 20.0;

        /* 伺服器訪問步驟:
         * 1. 清空目前「需顯示的伺服器清單」
         * 2. 為每一組IP實例化一個ServerLabel類別
         * 3. 新增至「需顯示的伺服器清單」
         * 4. 清空「目前顯示的伺服器」
         * 5. 由更新後的「需顯示伺服器清單」新增至「目前顯示的伺服器」
         */
        private void ServerQuery()
        {
            lock (watchIPList)
                Dispatcher.Invoke(() =>
                {
                    if (textVisible)
                    {
                        labelList.Clear();
                        foreach (var watchString in watchIPList)
                        {
                            ServerLabel svLabel = new ServerLabel(watchString, ClickDrag, ChangeSize, gFontSize);
                            labelList.Add(svLabel);
                            SizeToContent = SizeToContent.WidthAndHeight;
                        }
                        mainPanel.Dispatcher.Invoke(() => mainPanel.Children.Clear());
                        foreach (var label in labelList) mainPanel.Dispatcher.Invoke(() => mainPanel.Children.Add(label));
                    }
                    else mainPanel.Dispatcher.Invoke(() => mainPanel.Children.Clear());
                });
        }

        #endregion

        #region 隱藏文字

        private bool textVisible = true;

        private void ToggleVisibility() => textVisible = textVisible ? false : true;

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
                else                     QueryTimer.Start();
            }
        }

        private void ClickDrag(object sender, MouseButtonEventArgs e) => DragMove();

        private void ChangeSize(object sender, MouseWheelEventArgs e)
        {
            if(e.Delta > 0) // 滾輪向上放大字體，反之縮小字體
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
