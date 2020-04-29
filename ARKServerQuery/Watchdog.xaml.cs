using System;
using System.Collections.Generic;
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
        
        #region 與主UI的通訊
        
        // 儲存ASQ傳來的伺服器IP位址
        private List<string> watchIPList = new List<string>();
        
        public void Message(string msg)
        {
            if (msg == "_disable")
            {
                watchIPList.Clear();
            }
            else if (msg == "_visi") ToggleVisibility();
            else if (msg.Substring(0, 6) == "_lang,") ServerLabel.UpdateLanguage(msg.Substring(6, 5));
            else
            {
                lock (watchIPList)
                {
                    if (!watchIPList.Contains(msg)) watchIPList.Add(msg);
                    else if (watchIPList.Contains(msg)) watchIPList.Remove(msg);
                }
            }
        }

        private bool textVisible = true;
        private void ToggleVisibility()
        {
            if (textVisible) Show();
            else Hide();
            textVisible = !textVisible;
        }

        #endregion

        #region 查詢主計時器

        // 每秒訪問一次清單中的伺服器
        Timer QueryTimer = new Timer { Interval = 1000 };
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
