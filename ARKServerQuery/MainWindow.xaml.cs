﻿using ARKServerQuery.Properties;
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

        private void LoadLanguageFile(string languagefileName) => Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary()
        {
            Source = new Uri(languagefileName, UriKind.RelativeOrAbsolute)
        };

        private void InitializeLanguageString()
        {
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
                ServerQuery.ListSearch(inString);
            else if (TB_ServerID.Dispatcher.Invoke(() => TB_ServerID.Text == string.Empty))
                ServerQuery.arkSvCollection.Clear();

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
        private void InitWatchdog() => watchdog = new Watchdog();

        // 對監控介面傳遞伺服器資訊
        private void ToggleSpecificServerWatchStatus(object sender, RoutedEventArgs e)
        {
            object serverInfoObject = ((Button)sender).CommandParameter;
            watchdog.UpdateWatchList((ServerInfo)serverInfoObject);

            UpdateWatchdogLanguage();
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

        private void ClickDisableAllWatch(object sender, RoutedEventArgs e) => watchdog.DisableAllWatch();

        private void ClickWatchVisibility(object sender, RoutedEventArgs e) => watchdog.UpdateVisibility();

        #endregion
    }
}
