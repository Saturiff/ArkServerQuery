using SourceQuery;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
    public static class WindowsServices // 讓鼠標忽略該軟件
    {
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            if(isA)
            {
                SetWindowLong(hwnd, GWL_EXSTYLE, oriStyle);
            }
            else
            {
                SetWindowLong(hwnd, GWL_EXSTYLE, transparentStyle);
            }
            isA = (isA) ? false : true;
        }
        public static void SetOriStyle(IntPtr hwnd)
        {
            oriStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            transparentStyle = oriStyle | WS_EX_TRANSPARENT;
            SetWindowLong(hwnd, GWL_EXSTYLE, transparentStyle);
            isA = true;
        }
        private static int oriStyle;
        private static int transparentStyle;
        private static bool isA;
    }

    public partial class MainWindow : Window
    {
        IntPtr hwnd;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetOriStyle(hwnd);
        }

        private TcpClient client;
        public BinaryReader br;
        public BinaryWriter bw;
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
            {
                ToggleManipulateWindow(KeyStates.Down);
            }
            else if ((((Keyboard.GetKeyStates(Key.OemTilde) & KeyStates.None) == 0)
                || ((Keyboard.GetKeyStates(Key.OemQuotes) & KeyStates.None) == 0))
                && canManipulateWindow)
            {
                ToggleManipulateWindow(KeyStates.None);
            }
        }

        private List<string> WatchIPList = new List<string>();

        #region 本地客戶端
        private string GetIP(string fullIP) => Convert.ToString(fullIP.Split(':')[0]);
        private int GetPort(string fullIP) => Convert.ToInt16(fullIP.Split(':')[1]);

        private void LocalClient()
        {
            client = new TcpClient("127.0.0.1", 18500);
            while (true)
            {
                Process[] processes = Process.GetProcessesByName("ARKServerQuery");
                if (processes.Length == 0)
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
                    if (receive == "_disable") WatchIPList.Clear();
                    else if (receive == "_visi") ToggleVisibility();
                    else
                    {
                        lock (WatchIPList)
                        {
                            if (!WatchIPList.Contains(receive)) WatchIPList.Add(receive);
                            else if (WatchIPList.Contains(receive)) WatchIPList.Remove(receive);
                        }
                    }
                }
                catch { /* MessageBox.Show("接收失败！");*/ }
            }
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

        private GameServer GetServerInfo(string _fullIP)
        {
            GameServer sv;
            string ip = GetIP(_fullIP);
            int port = GetPort(_fullIP);
            ip = ip.Split(' ')[0];
            try { sv = new GameServer(new IPEndPoint(IPAddress.Parse(ip), port)); }
            catch { return null; }

            return sv;
        }

        private Color GetStatusColor(ServerPlayerStatus status, bool isShadow) // 如果和原本初始化的狀態不同，閃爍並重新給予值
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
            else
            {
                if (status == ServerPlayerStatus.Safe || status == ServerPlayerStatus.Warning)
                    return (Color)ColorConverter.ConvertFromString("#FF000000"); // 黑
                else
                    return (Color)ColorConverter.ConvertFromString("#FFA85C00"); // 橘
            }
        }

        private ServerPlayerStatus GetServerPlayerStatus(GameServer sv)
        {
            if (sv.GetCurrentPlayer() < 30)                                     return ServerPlayerStatus.Safe;
            else if (sv.GetCurrentPlayer() > 29 && sv.GetCurrentPlayer() < 60)  return ServerPlayerStatus.Warning;
            else                                                                return ServerPlayerStatus.Danger;
        }

        private void ServerQuery()
        {
            lock (WatchIPList)
            {
                Dispatcher.Invoke(() =>
                {
                    if (textVisible)
                    {
                        List<Label> labelList = new List<Label>();
                        foreach (var watchString in WatchIPList)
                        {
                            string ip = watchString.Split(',')[0];
                            string name = watchString.Split(',')[1];
                            var arkSv = GetServerInfo(ip);
                            string serverContent = "";
                            Brush foregroundColor = new SolidColorBrush();
                            Color shadowColor = new Color();
                            if (arkSv != null)
                            {
                                serverContent = name + "\n人數: " + arkSv.GetCurrentPlayer() + " / " + arkSv.MaximumPlayerCount + "\n";
                                foregroundColor = new SolidColorBrush(GetStatusColor(GetServerPlayerStatus(arkSv), false));
                                shadowColor = GetStatusColor(GetServerPlayerStatus(arkSv), true);
                            }
                            else
                            {
                                serverContent = name + "\n服務器離線中 !";
                                foregroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00A800")); // 綠
                                shadowColor = (Color)ColorConverter.ConvertFromString("#FF000000"); // 黑
                            }

                            Label svLabel = new Label()
                            {
                                Content = serverContent,
                                FontSize = 20,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                HorizontalContentAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                Foreground = foregroundColor,
                                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00000000")),
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                FontStretch = FontStretches.SemiCondensed,
                                FontWeight = FontWeights.Bold,
                                Margin = new Thickness(45, 0, 492, 0),
                                Opacity = 1,
                                Effect = new DropShadowEffect
                                {
                                    BlurRadius = 20,
                                    Color = shadowColor,
                                    Direction = 320,
                                    ShadowDepth = 0,
                                    Opacity = 1
                                }
                            };
                            svLabel.MouseLeftButtonDown += ClickDrag;
                            labelList.Add(svLabel);
                            SizeToContent = SizeToContent.WidthAndHeight;
                        }
                        mainPanel.Dispatcher.Invoke(() => mainPanel.Children.Clear());
                        foreach(var label in labelList) mainPanel.Dispatcher.Invoke(() => mainPanel.Children.Add(label));
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
        int cnt;
        private void ToggleManipulateWindow(KeyStates inKeyStates)
        {
            Debug.WriteLine("g - {0}\tin - {1}", gKeyStates, inKeyStates);
            if (inKeyStates != gKeyStates) // 只檢測第一次的變更
            {
                Debug.WriteLine("-在渲染時偵測到鍵盤 - {0}", cnt);
                Debug.WriteLine("-新狀態 = {0}", inKeyStates);
                canManipulateWindow = (canManipulateWindow) ? false : true;
                //var hwnd = new WindowInteropHelper(this).Handle;
                WindowsServices.SetWindowExTransparent(hwnd);
                gKeyStates = inKeyStates;
                cnt += 1;
            }
        }

        private void ClickDrag(object sender, MouseButtonEventArgs e) => DragMove();
        #endregion
    }
}
