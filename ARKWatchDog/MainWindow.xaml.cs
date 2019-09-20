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
        private IntPtr hwnd;
        // 初始化時將目前視窗參數儲存
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetOriStyle(hwnd);
        }

        private StackPanel mainPanel = new StackPanel();

        public MainWindow()
        {
            InitializeComponent();
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
            Content = mainPanel;
            Thread localSvThread = new Thread(LocalClient);
            localSvThread.Start();
            InitQuery();
        }

        // Hook全域鍵盤
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (((Keyboard.GetKeyStates(Key.OemTilde) & KeyStates.Down) > 0)
                || ((Keyboard.GetKeyStates(Key.OemQuotes) & KeyStates.Down) > 0))
                ToggleManipulateWindow(KeyStates.Down);
            else if ((((Keyboard.GetKeyStates(Key.OemTilde) & KeyStates.None) == 0)
                || ((Keyboard.GetKeyStates(Key.OemQuotes) & KeyStates.None) == 0)) && canManipulateWindow)
                ToggleManipulateWindow(KeyStates.None);
        }

        private List<string> watchIPList = new List<string>();

        #region 本地客戶端
       
        private void LocalClient()
        {
            TcpClient client;
            BinaryReader br;
            client = new TcpClient("127.0.0.1", 18500);
            while (true)
            {
                if (Process.GetProcessesByName("ARKServerQuery").Length == 0)
                {
                    Close();
                    Environment.Exit(Environment.ExitCode);
                }
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
        
        private bool textVisible = true;
        private void ToggleVisibility() => textVisible = textVisible ? false : true;

        #region 查詢主計時器
        Timer QueryTimer = new Timer { Interval = 1000 };
        List<ServerLabel> labelList = new List<ServerLabel>();
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
                            ServerLabel svLabel = new ServerLabel(watchString, ClickDrag);
                            labelList.Add(svLabel);
                            SizeToContent = SizeToContent.WidthAndHeight;
                        }
                        mainPanel.Dispatcher.Invoke(() => mainPanel.Children.Clear());
                        foreach (var label in labelList) mainPanel.Dispatcher.Invoke(() => mainPanel.Children.Add(label));
                    }
                    else mainPanel.Dispatcher.Invoke(() => mainPanel.Children.Clear());
                });
        }

        Thread textUpdate;
        private void QueryTick(object sender, EventArgs e)
        {
            if (textUpdate == null || textUpdate.ThreadState != ThreadState.Running)
            {
                textUpdate = new Thread(ServerQuery);
                textUpdate.Start();
            }
        }

        private void InitQuery()
        {
            QueryTimer.Tick += new EventHandler(QueryTick);
            QueryTimer.Start();
        }
        #endregion

        private bool canManipulateWindow = false;
        private KeyStates gKeyStates = KeyStates.None;
        private void ToggleManipulateWindow(KeyStates inKeyStates)
        {
            if (inKeyStates != gKeyStates) // 只檢測第一次的變更
            {
                canManipulateWindow = (canManipulateWindow) ? false : true;
                WindowsServices.SetWindowExTransparent(hwnd);
                gKeyStates = inKeyStates;
                if (canManipulateWindow)    QueryTimer.Stop();
                else                        QueryTimer.Start();
            }
        }

        private void ClickDrag(object sender, MouseButtonEventArgs e) => DragMove();
    }
}
