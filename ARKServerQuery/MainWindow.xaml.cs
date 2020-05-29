using ARKServerQuery.Properties;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Localization = ARKServerQuery.Classes.Localization;

namespace ARKServerQuery
{
    // 查詢介面
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            InitializeLocalization();

            ServerQuery.InitializeServerList();

            InitializeWatchdog();
        }

        #region 本地化

        private void InitializeLocalization()
        {
            Localization.Load();
            CB_LanguageSwitcher.SelectedIndex = (int)Settings.Default.customLanguage;

            // 為了防止物件初始化時呼叫「更改事件」，讀取完設定後才掛鉤上事件
            CB_LanguageSwitcher.SelectionChanged += CB_LanguageSwitcher_SelectionChanged;
        }

        private void CB_LanguageSwitcher_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => Localization.Update(CB_LanguageSwitcher.SelectedIndex);

        #endregion

        #region 查詢方法

        private void SearchByName()
        {
            string inString = TB_ServerSearchBox.Dispatcher.Invoke(() => TB_ServerSearchBox.Text);

            if (inString.Length > 1)
                ServerQuery.ListSearch(inString);
            else if (TB_ServerSearchBox.Dispatcher.Invoke(() => TB_ServerSearchBox.Text == string.Empty))
                ServerQuery.arkSvCollection.Clear();

            DG_ServerSearchResultArea.Dispatcher.Invoke(() => DG_ServerSearchResultArea.ItemsSource = ArkServerCollection.collection);
        }

        private void SearchByAll()
        {
            UpdateSearchingStatus(true);

            ServerQuery.ListSearch(string.Empty);

            DG_ServerSearchResultArea.Dispatcher.Invoke(() => DG_ServerSearchResultArea.ItemsSource = ArkServerCollection.collection);

            UpdateSearchingStatus(false);
        }

        #endregion

        #region 伺服器清單

        private void JoinServer(object sender, RoutedEventArgs e)
        {
            object serverInfoObject = ((Button)sender).CommandParameter;

            Process.Start("steam://connect/"
                + ((ServerInfo)serverInfoObject).ip 
                + Convert.ToString(((ServerInfo)serverInfoObject).port));
        }

        private void UpdateSearchingStatus(bool newStatus)
        {
            TB_ServerSearchBox.Dispatcher.Invoke(() => TB_ServerSearchBox.IsEnabled = !newStatus);

            B_Start_Load.Dispatcher.Invoke(() => B_Start_Load.IsEnabled = !newStatus);

            B_Stop_Load.Dispatcher.Invoke(() => B_Stop_Load.IsEnabled = newStatus);
        }

        #endregion

        #region 伺服器人數即時監控浮動文字(watchdog)

        Watchdog watchdog;
        private void InitializeWatchdog() => watchdog = new Watchdog();

        private void B_UpdateServerMonitoringStatus_Click(object sender, RoutedEventArgs e)
        {
            object serverInfoObject = ((Button)sender).CommandParameter;

            watchdog.UpdateWatchList((ServerInfo)serverInfoObject);
        }

        #endregion

        #region 按鈕事件
        private void B_Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void B_Close_Click(object sender, RoutedEventArgs e)
        {
            watchdog.Close();

            Close();

            Environment.Exit(Environment.ExitCode);
        }

        private void ClickDrag(object sender, MouseButtonEventArgs e) => DragMove();

        private Thread searchByName;
        private void TB_ServerSearchBox_TextChanged(object sender, TextChangedEventArgs e)
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

            UpdateSearchingStatus(false);
        }

        private void ClickDisableAllWatch(object sender, RoutedEventArgs e) => watchdog.DisableAllWatch();

        private void ClickWatchVisibility(object sender, RoutedEventArgs e) => watchdog.UpdateVisibility();

        #endregion
    }
}
