using ARKServerQuery.Properties;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ARKServerQuery
{
    public enum LanguageList { zh_tw, zh_cn, en_us }

    // 查詢介面
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeLanguageString();
            ServerQuery.InitServerList();
            InitWatchdog();
        }

        #region 本地化

        private static LanguageList currentLanguage = LanguageList.zh_tw;

        private void UpdateWatchdogLanguage() => ServerLabel.UpdateLanguage();

        private void UpdateMapListLanguage()
        {
            if (currentLanguage == LanguageList.zh_tw)
                Lable_Maps.Content = "地圖:\nTheIsland\t孤島\tAberration\t畸變\nTheCenter\t中心島\tExtinction\t滅絕\nScorchedEarth\t焦土";
            else if (currentLanguage == LanguageList.zh_cn)
                Lable_Maps.Content = "地图:\nTheIsland\t孤岛\tAberration\t畸变\nTheCenter\t中心岛\tExtinction\t灭绝\nScorchedEarth\t焦土";
            else if (currentLanguage == LanguageList.en_us)
                Lable_Maps.Content = "Maps:\nTheIsland\t\tAberration\t\nTheCenter\t\tExtinction\t\nScorchedEarth\t";
        }

        private void LoadLanguageFile(string languagefileName)
        {
            Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary()
            {
                Source = new Uri(languagefileName, UriKind.RelativeOrAbsolute)
            };
        }

        private void InitializeLanguageString()
        {
            // setting 預設值
            currentLanguage = Settings.Default.customLanguage;
            if (currentLanguage == LanguageList.zh_tw)
            {
                LoadLanguageFile("/Lang/zh-tw.xaml");
                CB_LangList.SelectedIndex = 0;
                currentLanguage = LanguageList.zh_tw;
            }
            else if (currentLanguage == LanguageList.zh_cn)
            {
                LoadLanguageFile("/Lang/zh-cn.xaml");
                CB_LangList.SelectedIndex = 1;
                currentLanguage = LanguageList.zh_cn;
            }
            else if (currentLanguage == LanguageList.en_us)
            {
                LoadLanguageFile("/Lang/en-us.xaml");
                CB_LangList.SelectedIndex = 2;
                currentLanguage = LanguageList.en_us;
            }
            if (Lable_Maps != null) UpdateMapListLanguage();

            // 為了防止物件初始化時呼叫「更改事件」，讀取完設定後才掛鉤上事件
            CB_LangList.SelectionChanged += CB_LangList_SelectionChanged;
        }

        private void CB_LangList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CB_LangList.SelectedIndex == 0)
            {
                LoadLanguageFile("/Lang/zh-tw.xaml");
                currentLanguage = LanguageList.zh_tw;
            }
            else if (CB_LangList.SelectedIndex == 1)
            {
                LoadLanguageFile("/Lang/zh-cn.xaml");
                currentLanguage = LanguageList.zh_cn;
            }
            else if (CB_LangList.SelectedIndex == 2)
            {
                LoadLanguageFile("/Lang/en-us.xaml");
                currentLanguage = LanguageList.en_us;
            }
            if (Lable_Maps != null) UpdateMapListLanguage();

            Settings.Default.customLanguage = currentLanguage;
            Settings.Default.Save();
            UpdateWatchdogLanguage();
        }
        #endregion

        #region 查詢方法
        // 由輸入的名稱做搜尋
        private void SearchByName()
        {
            string inString = TB_ServerID.Dispatcher.Invoke(() => TB_ServerID.Text);
            if (inString.Length > 1)
            {
                ServerQuery.ListSearch(inString);
            }
            else if (TB_ServerID.Dispatcher.Invoke(() => TB_ServerID.Text == string.Empty))
            {
                ServerQuery.arkSvCollection.Clear();
            }

            DG_ServerList.Dispatcher.Invoke(() => DG_ServerList.ItemsSource = ArkServerCollection.collection);
        }

        // 搜尋所有伺服器
        private void SearchByAll()
        {
            IsSearching(true);
            ServerQuery.ListSearch(string.Empty);
            DG_ServerList.Dispatcher.Invoke(() => DG_ServerList.ItemsSource = ArkServerCollection.collection);
            IsSearching(false);
        }

        #endregion

        #region 伺服器清單
        // 加入伺服器
        private void JoinServer(object sender, RoutedEventArgs e)
        {
            object serverInfoObject = ((Button)sender).CommandParameter;
            Process.Start("steam://connect/" +
                ((ServerInfo)serverInfoObject).ip + Convert.ToString(((ServerInfo)serverInfoObject).port));
        }

        // 顯示地圖名稱
        private void ToggleMaps()
        {
            if (WP_BottomButtonWarp.Visibility == Visibility.Hidden)
                WP_BottomButtonWarp.Visibility = Visibility.Hidden;
            else
                WP_BottomButtonWarp.Visibility = Visibility.Visible;

            if (Lable_Maps.Visibility == Visibility.Hidden)
                Lable_Maps.Visibility = Visibility.Visible;
            else
                Lable_Maps.Visibility = Visibility.Hidden;
        }

        // 對搜尋狀態做按鈕的調整
        private void IsSearching(bool newStatus)
        {
            TB_ServerID.Dispatcher.Invoke(() => TB_ServerID.IsEnabled = !newStatus);
            B_Start_Load.Dispatcher.Invoke(() => B_Start_Load.IsEnabled = !newStatus);
            B_Stop_Load.Dispatcher.Invoke(() => B_Stop_Load.IsEnabled = newStatus);
        }

        // 顯示最小化按鈕
        private void ShowButton(bool show) => Min_button.Opacity = show ? 1 : 0;

        #endregion

        #region 伺服器人數即時監控浮動文字(watchdog)

        Watchdog watchdog;
        private void InitWatchdog()
        {
            watchdog = new Watchdog();
        }

        private void UpdateWatchdogVisibilityStatus()
        {
            if (watchdog.IsWatchListEmpty())
                watchdog.Visibility = Visibility.Hidden;
            else
                watchdog.Visibility = Visibility.Visible;
        }

        // 對監控介面傳遞伺服器資訊
        private void ToggleSpecificServerWatchStatus(object sender, RoutedEventArgs e)
        {
            object serverInfoObject = ((Button)sender).CommandParameter;
            watchdog.AddWatchList((ServerInfo)serverInfoObject);

            UpdateWatchdogLanguage();
            UpdateWatchdogVisibilityStatus();
        }

        #endregion

        #region 按鈕事件
        private void ClickMin(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void ClickExit(object sender, RoutedEventArgs e)
        {
            watchdog.Close();
            Close();
            Environment.Exit(Environment.ExitCode);
        }

        private void ClickDrag(object sender, MouseButtonEventArgs e) => DragMove();

        private void ClickShowButton(object sender, MouseEventArgs e) => ShowButton(true);

        private void ClickHideButton(object sender, MouseEventArgs e) => ShowButton(false);

        private Thread searchByName;
        private void TB_ServerID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchByName != null) searchByName.Abort();
            searchByName = new Thread(SearchByName);
            searchByName.Start();
        }

        private Thread searchByAll;
        private void ClickStartSearchAll(object sender, RoutedEventArgs e)
        {
            searchByAll = new Thread(SearchByAll);
            searchByAll.Start();
        }

        private void ClickStopSearchAll(object sender, RoutedEventArgs e)
        {
            searchByAll.Abort();
            IsSearching(false);
        }

        private void ClickMaps(object sender, RoutedEventArgs e) => ToggleMaps();

        private void ClickDisableAllWatch(object sender, RoutedEventArgs e) => watchdog.DisableAllWatch();

        private void ClickWatchVisibility(object sender, RoutedEventArgs e) => UpdateWatchdogVisibilityStatus();

        #endregion
    }
}
