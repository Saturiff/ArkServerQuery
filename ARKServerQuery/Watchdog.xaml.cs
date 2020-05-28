using ARKServerQuery.Classes;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace ARKServerQuery
{
    // 監控介面
    public partial class Watchdog : Window
    {
        #region 視窗初始化

        public Watchdog()
        {
            InitializeComponent();
            windowManipulateComponent = new WindowManipulate();
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering); // 用於鍵盤綁定
            Content = mainPanel;
        }

        WindowManipulate windowManipulateComponent;

        // 裝伺服器資訊(ServerLabel型態)的主要容器
        private StackPanel mainPanel = new StackPanel();

        #endregion

        #region 鍵盤綁定
        // Hook全域鍵盤，在該視窗重新渲染時執行
        private void CompositionTarget_Rendering(object sender, EventArgs e) 
            => windowManipulateComponent.KeyDetect();

        // 初始化時將目前視窗參數儲存
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            windowManipulateComponent.Init(new WindowInteropHelper(this).Handle);
        }

        #endregion

        #region 與查詢介面的通訊

        // 儲存查詢介面傳來的伺服器IP位址
        private List<ServerInfo> serverInfoList = new List<ServerInfo>();

        public bool IsWatchListEmpty() => GetServerListCount() == 0;

        public void AddWatchList(ServerInfo serverInfo)
        {
            lock (serverInfoList)
            {
                if (!serverInfoList.Contains(serverInfo)) serverInfoList.Add(serverInfo);
                else if (serverInfoList.Contains(serverInfo)) serverInfoList.Remove(serverInfo);
            }
            UpdateServerQueryList();
        }

        public void DisableAllWatch() => serverInfoList.Clear();

        #endregion

        #region 監控標籤控制區

        private double gFontSize = (double)FontSizeValue.Default;

        // 目前顯示的數量與目前清單的數量
        private int GetServerDisplayCount() => mainPanel.Dispatcher.Invoke(() => mainPanel.Children.Count);

        private int GetServerListCount() => serverInfoList.Count;

        private Random r = new Random();

        private int RandomTimerInterval => r.Next() % 200 + 1000;

        // 監控顯示步驟
        // 1. 檢查已顯示與未顯示的物件數量差距
        // 2. 更新數量
        // 3. 更新已顯示的伺服器資訊
        private void UpdateServerQueryList()
        {
            lock (serverInfoList)
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        int offset = GetServerListCount() - GetServerDisplayCount();
                        if (offset > 0)
                        {
                            for (int i = 0; i < offset; i++)
                                mainPanel.Dispatcher.Invoke(() => mainPanel.Children.
                                    Add(new ServerLabel(RandomTimerInterval, ClickDrag, ChangeSize, gFontSize)));
                        }
                        else if (offset < 0)
                        {
                            for (int i = 0; i < Math.Abs(offset); i++)
                                mainPanel.Dispatcher.Invoke(() => mainPanel.Children.
                                    RemoveAt(GetServerListCount() - 1));
                        }
                        
                        int cnt = 0;
                        foreach (ServerLabel child in mainPanel.Dispatcher.Invoke(() => mainPanel.Children))
                            child.serverInfo = serverInfoList[cnt++];
                        SizeToContent = SizeToContent.WidthAndHeight;
                    }
                    catch { }
                });
        }

        #endregion

        #region 鍵盤/滑鼠與程式間的交互

        private void ClickDrag(object sender, MouseButtonEventArgs e) => DragMove();

        private void ChangeSize(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) // 滾輪向上放大字體，反之縮小字體
            {
                foreach (ServerLabel child in mainPanel.Dispatcher.Invoke(() => mainPanel.Children))
                {
                    if (child.FontSize <= (int)FontSizeValue.Lowerbound) break;
                    child.FontSize += (int)FontSizeValue.Add;
                    gFontSize = child.FontSize;
                }
            }
            else
            {
                foreach (ServerLabel child in mainPanel.Dispatcher.Invoke(() => mainPanel.Children))
                {
                    if (child.FontSize >= (int)FontSizeValue.UpperBound) break;
                    child.FontSize -= (int)FontSizeValue.Sub;
                    gFontSize = child.FontSize;
                }
            }
        }

        private enum FontSizeValue { Default = 20, Add = 5, Sub = 5, UpperBound = int.MaxValue , Lowerbound = 2 }

        #endregion
    }
}
