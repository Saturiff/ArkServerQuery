using SourceQuery;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Timer = System.Windows.Forms.Timer;

namespace ARKServerQuery
{
    // ServerLabel 繼承自 Label ，實例化時即可用來保存伺服器資訊
    // 建構子必要參數:
    // WatchLocalizedStringString -> 由查詢介面接收而來的伺服器字串，預設格式為 " IP:PORT,伺服器名稱 "
    // ClickDrag   -> 拖曳視窗事件
    // ChangeSize  -> 改變文字大小事件
    // gFontSize   -> 欲顯示的文字大小
    // ----------------------------------------------------------------------
    // 流程:
    // 實例化 GameServer 嘗試訪問目標伺服器
    // 設定顯示內容與格式
    // 若 GameServer 為 null ，則顯示訪問失敗字串; 若不為 null 則顯示實時玩家人數
    // 依照玩家人數改變字體顏色(0-29, 30-59, 60-maxPlayer)
    public class ServerLabel : Label
    {
        public ServerLabel(int interval, MouseButtonEventHandler ClickDrag, MouseWheelEventHandler ChangeSize, double fontSize)
        {
            InitTimer(interval);

            HorizontalAlignment = HorizontalAlignment.Left;
            HorizontalContentAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            Margin = new Thickness(45, 0, 492, 0);

            Opacity = 1;
            Background = new SolidColorBrush(Colors.Transparent);
            BorderBrush = new SolidColorBrush(Colors.Black);

            FontSize = fontSize;
            FontStretch = FontStretches.SemiCondensed;
            FontWeight = FontWeights.Bold;

            MouseLeftButtonDown += ClickDrag;
            MouseWheel += ChangeSize;
        }

        private Timer t;
        private void InitTimer(int interval)
        {
            t = new Timer { Interval = interval };
            t.Tick += new EventHandler(UpdateTick);
            t.Start();
        }

        public ServerInfo serverInfo;
        private void UpdateTick(object sender, EventArgs e)
        {
            if (serverInfo != null) UpdateContent();
        }

        private GameServer arkServer;
        public async void UpdateContent()
        {
            arkServer = await GetGameServer();

            Content = MakeLabelString(arkServer);

            Foreground = new SolidColorBrush((arkServer != null) ? GetStatusColor(GetServerPlayerWarningLevel(arkServer), false)
                                                                 : Palette[ColorName.Green]);

            Effect = new DropShadowEffect
            {
                BlurRadius = 20,
                Color = (arkServer != null) ? GetStatusColor(GetServerPlayerWarningLevel(arkServer), true)
                                            : Palette[ColorName.Black],
                Direction = 320,
                ShadowDepth = 0,
                Opacity = 1
            };
        }

        private string MakeLabelString(GameServer sv)
        {
            if (sv != null)
                return serverInfo.name + "\n"
                    + Application.Current.Resources[LocalizationKey.PlayerQuantifierLocalizedString.ToString()] + ": "
                    + sv.currentPlayer + " / "
                    + sv.maxPlayer + "\n";
            else
                return serverInfo.name + "\n"
                    + Application.Current.Resources[LocalizationKey.QueryFailedLocalizedString.ToString()] + " !\n";
        }

        private Task<GameServer> GetGameServer() => Task.Factory.StartNew(() => TryGetGameServer());

        private GameServer TryGetGameServer()
        {
            GameServer sv;
            try { sv = new GameServer(new IPEndPoint(IPAddress.Parse(serverInfo.ip), serverInfo.port)); }
            catch { return null; }

            return sv;
        }

        private static Color GetStatusColor(ServerPlayerWarningLevel status, bool isShadow)
        {
            if (!isShadow)
            {
                if (status == ServerPlayerWarningLevel.Safe)
                    return Palette[ColorName.Green];
                else if (status == ServerPlayerWarningLevel.Warning)
                    return Palette[ColorName.Orange];
                else
                    return Palette[ColorName.Red];
            }
            else
            {
                if (status == ServerPlayerWarningLevel.Safe || status == ServerPlayerWarningLevel.Warning)
                    return Palette[ColorName.Black];
                else
                    return Palette[ColorName.Brown];
            }
        }

        private static ServerPlayerWarningLevel GetServerPlayerWarningLevel(GameServer sv)
        {
            if (sv.currentPlayer < (int)ServerPlayerWarningLevel.Warning)
                return ServerPlayerWarningLevel.Safe;
            else if (sv.currentPlayer >= (int)ServerPlayerWarningLevel.Warning && sv.currentPlayer < (int)ServerPlayerWarningLevel.Danger)
                return ServerPlayerWarningLevel.Warning;
            else
                return ServerPlayerWarningLevel.Danger;
        }

        private enum ServerPlayerWarningLevel { Safe = 0, Warning = 30, Danger = 60 }
        private enum ColorName { Black, Green, Orange, Red, Brown }

        private static readonly Dictionary<ColorName, Color> Palette = new Dictionary<ColorName, Color>()
        {
            { ColorName.Black, (Color)ColorConverter.ConvertFromString("#FF000000") } ,
            { ColorName.Green, (Color)ColorConverter.ConvertFromString("#FF00A800") } ,
            { ColorName.Orange, (Color)ColorConverter.ConvertFromString("#FFBB4D00") } ,
            { ColorName.Red, (Color)ColorConverter.ConvertFromString("#FFA80000") } ,
            { ColorName.Brown, (Color)ColorConverter.ConvertFromString("#FFA85C00") }
        };

        private enum LocalizationKey { PlayerQuantifierLocalizedString, QueryFailedLocalizedString }
    }
}
