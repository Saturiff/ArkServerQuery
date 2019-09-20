using SourceQuery;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using ThreadState = System.Threading.ThreadState;
using Timer = System.Windows.Forms.Timer;

namespace ARKWatchdog
{
    public partial class MainWindow : Window
    {
        IntPtr hwnd;
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

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (((Keyboard.GetKeyStates(Key.OemTilde) & KeyStates.Down) > 0)
                || ((Keyboard.GetKeyStates(Key.OemQuotes) & KeyStates.Down) > 0))
                ToggleManipulateWindow(KeyStates.Down);
            else if ((((Keyboard.GetKeyStates(Key.OemTilde) & KeyStates.None) == 0)
                || ((Keyboard.GetKeyStates(Key.OemQuotes) & KeyStates.None) == 0)) && canManipulateWindow)
                ToggleManipulateWindow(KeyStates.None);
        }

        private List<string> WatchIPList = new List<string>();

        #region 本地客戶端
        private static string GetIP(string fullIP) => Convert.ToString(fullIP.Split(':')[0]);
        private static int GetPort(string fullIP) => Convert.ToInt16(fullIP.Split(':')[1]);

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
                        WatchIPList.Clear();
                    }
                    else if (receive == "_visi") ToggleVisibility();
                    else if (receive.Substring(0, 6) == "_lang,") UpdateLanguage(receive.Substring(6, 5));
                    else
                    {
                        lock (WatchIPList)
                        {
                            if (!WatchIPList.Contains(receive)) WatchIPList.Add(receive);
                            else if (WatchIPList.Contains(receive)) WatchIPList.Remove(receive);
                        }
                    }
                }
                catch { }
            }
        }
        private static string currentLanguage = "zh_tw";
        private static readonly Dictionary<string, string> mutiLangText_PlayerText = new Dictionary<string, string>()
        {
            { "zh_tw", "人數" },
            { "zh_cn", "人数" },
            { "en_us", "Players" }
        };
        private static readonly Dictionary<string, string> mutiLangText_QueryFailed = new Dictionary<string, string>()
        {
            { "zh_tw", "服務器訪問失敗" },
            { "zh_cn", "服务器访问失败" },
            { "en_us", "Server query failed" }
        };
        private void UpdateLanguage(string lang)
        {
            currentLanguage = lang;
        }
        
        private bool textVisible = true;
        private void ToggleVisibility()
        {
            if (textVisible)    textVisible = false;
            else                textVisible = true;
        }
        #endregion

        #region 查詢主計時器
        Timer QueryTimer = new Timer { Interval = 1000 };
        
        private enum ServerPlayerStatus { Safe, Warning, Danger }

        private static GameServer GetServerInfo(string _fullIP)
        {
            GameServer sv;
            string ip = GetIP(_fullIP);
            int port = GetPort(_fullIP);
            ip = ip.Split(' ')[0];
            try { sv = new GameServer(new IPEndPoint(IPAddress.Parse(ip), port)); }
            catch { return null; }

            return sv;
        }

        private static Color GetStatusColor(ServerPlayerStatus status, bool isShadow)
        {
            if (!isShadow)
            {
                if (status == ServerPlayerStatus.Safe)
                    return (Color)ColorConverter.ConvertFromString("#FF00A800"); // 綠
                else if (status == ServerPlayerStatus.Warning)
                    return (Color)ColorConverter.ConvertFromString("#FFBB4D00"); // 橘
                else
                    return (Color)ColorConverter.ConvertFromString("#FFA80000"); // 紅
            }
            else return (Color)ColorConverter.ConvertFromString((status == ServerPlayerStatus.Safe || status == ServerPlayerStatus.Warning) ? "#FF000000" : "#FFA85C00");
        }

        private static ServerPlayerStatus GetServerPlayerStatus(GameServer sv)
        {
            if (sv.currentPlayer < 30)                                  return ServerPlayerStatus.Safe;
            else if (sv.currentPlayer > 29 && sv.currentPlayer < 60)    return ServerPlayerStatus.Warning;
            else                                                        return ServerPlayerStatus.Danger;
        }
        private class ServerLabel : Label
        {
            public ServerLabel(string watchString)
            {
                string[] ipAndName = watchString.Split(',');
                GameServer arkServer = GetServerInfo(ipAndName[0]);
                string name = ipAndName[1];
                Brush foregroundColor = new SolidColorBrush();

                Content = (arkServer != null)
                    ? name + "\n" + mutiLangText_PlayerText[currentLanguage] + ": " + arkServer.currentPlayer + " / " + arkServer.maxPlayer + "\n"
                    : name + "\n" + mutiLangText_QueryFailed[currentLanguage] + " !\n";
                FontSize = 20;
                HorizontalAlignment = HorizontalAlignment.Left;
                HorizontalContentAlignment = HorizontalAlignment.Left;
                VerticalAlignment = VerticalAlignment.Top;
                Foreground = new SolidColorBrush((arkServer != null)
                    ? GetStatusColor(GetServerPlayerStatus(arkServer), false)
                    : (Color)ColorConverter.ConvertFromString("#FF00A800")), // 綠
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00000000"));
                BorderBrush = new SolidColorBrush(Colors.Black);
                FontStretch = FontStretches.SemiCondensed;
                FontWeight = FontWeights.Bold;
                Margin = new Thickness(45, 0, 492, 0);
                Opacity = 1;
                Effect = new DropShadowEffect
                {
                    BlurRadius = 20,
                    Color = (arkServer != null)
                    ? GetStatusColor(GetServerPlayerStatus(arkServer), true)
                    : (Color)ColorConverter.ConvertFromString("#FF000000"), // 黑,
                Direction = 320,
                    ShadowDepth = 0,
                    Opacity = 1
                };
            }
        }

        List<Label> labelList = new List<Label>();
        private void ServerQuery()
        {
            lock (WatchIPList)
            {
                Dispatcher.Invoke(() =>
                {
                    if (textVisible)
                    {
                        labelList.Clear();
                        foreach (var watchString in WatchIPList)
                        {
                            ServerLabel svLabel = new ServerLabel(watchString);
                            svLabel.MouseLeftButtonDown += ClickDrag;
                            labelList.Add(svLabel);
                            SizeToContent = SizeToContent.WidthAndHeight;
                        }
                        mainPanel.Dispatcher.Invoke(() => mainPanel.Children.Clear());
                        foreach (var label in labelList) mainPanel.Dispatcher.Invoke(() => mainPanel.Children.Add(label));
                    }
                    else mainPanel.Dispatcher.Invoke(() => mainPanel.Children.Clear());
                });
            }
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

        #region 基礎功能
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
        #endregion
    }
}
