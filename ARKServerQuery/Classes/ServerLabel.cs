﻿using SourceQuery;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ARKServerQuery
{
    /* ServerLabel 繼承自 Label ，實例化時即可保存伺服器資訊
     * 建構子必要參數:
     * watchString -> 由查詢介面接收而來的伺服器字串，預設格式為 " IP:PORT,伺服器名稱 "
     * ClickDrag   -> 拖曳視窗事件
     * ChangeSize  -> 改變文字大小事件
     * gFontSize   -> 欲顯示的文字大小
     * ----------------------------------------------------------------------
     * 流程:
     * 實例化 GameServer 嘗試訪問目標伺服器
     * 設定顯示內容與格式
     * 若 GameServer 為 null ，則顯示訪問失敗字串; 若不為 null 則顯示實時玩家人數
     * 依照玩家人數改變字體顏色(0-29, 29-59, 60-)
     */
    public class ServerLabel : Label
    {
        public ServerLabel(string watchString, MouseButtonEventHandler ClickDrag, MouseWheelEventHandler ChangeSize, double gFontSize)
        {
            // string[] ipAndName = watchString.Split(',');
            // GameServer arkServer = GetServerInfo(ipAndName[0]);
            // string name = ipAndName[1];
            // 
            // if (arkServer != null)
            //     Content = name + "\n" + mutiLangText_PlayerText[currentLanguage] + ": " + arkServer.currentPlayer
            //         + " / " + arkServer.maxPlayer + "\n";
            // else
            //     Content = name + "\n" + mutiLangText_QueryFailed[currentLanguage] + " !\n";

            if (watchString != string.Empty) UpdateInfo(watchString);

            HorizontalAlignment = HorizontalAlignment.Left;
            HorizontalContentAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            Margin = new Thickness(45, 0, 492, 0);

            Opacity = 1;
            // Foreground = new SolidColorBrush((arkServer != null)
            //     ? GetStatusColor(GetServerPlayerStatus(arkServer), false)
            //     : (Color)ColorConverter.ConvertFromString("#FF00A800")); // 綠
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00000000"));
            BorderBrush = new SolidColorBrush(Colors.Black);
            // Effect = new DropShadowEffect
            // {
            //     BlurRadius = 20,
            //     Color = (arkServer != null)
            //     ? GetStatusColor(GetServerPlayerStatus(arkServer), true)
            //     : (Color)ColorConverter.ConvertFromString("#FF000000"), // 黑,
            //     Direction = 320,
            //     ShadowDepth = 0,
            //     Opacity = 1
            // };

            FontSize = gFontSize;
            FontStretch = FontStretches.SemiCondensed;
            FontWeight = FontWeights.Bold;

            MouseLeftButtonDown += ClickDrag;
            MouseWheel += ChangeSize;
        }

        public void UpdateInfo(string watchString)
        {
            string[] ipAndName = watchString.Split(',');
            GameServer arkServer = GetServerInfo(ipAndName[0]);
            string name = ipAndName[1];

            if (arkServer != null)
                Content = name + "\n" + mutiLangText_PlayerText[currentLanguage] + ": " + arkServer.currentPlayer
                    + " / " + arkServer.maxPlayer + "\n";
            else
                Content = name + "\n" + mutiLangText_QueryFailed[currentLanguage] + " !\n";

            Foreground = new SolidColorBrush((arkServer != null)
                ? GetStatusColor(GetServerPlayerStatus(arkServer), false)
                : (Color)ColorConverter.ConvertFromString("#FF00A800")); // 綠

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

        #region 語言

        public static void UpdateLanguage(LanguageList lang)
        {
            currentLanguage = lang;
        }

        protected static LanguageList currentLanguage = LanguageList.zh_tw;

        protected static readonly Dictionary<LanguageList, string> mutiLangText_PlayerText = new Dictionary<LanguageList, string>()
        {
            { LanguageList.zh_tw, "人數" },
            { LanguageList.zh_cn, "人数" },
            { LanguageList.en_us, "Players" }
        };

        protected static readonly Dictionary<LanguageList, string> mutiLangText_QueryFailed = new Dictionary<LanguageList, string>()
        {
            { LanguageList.zh_tw, "伺服器訪問失敗" },
            { LanguageList.zh_cn, "服务器访问失败" },
            { LanguageList.en_us, "Server query failed" }
        };

        #endregion

        protected static string GetIP(string fullIP)
        {
            return Convert.ToString(fullIP.Split(':')[0]);
        }

        protected static int GetPort(string fullIP)
        {
            return Convert.ToInt16(fullIP.Split(':')[1]);
        }

        protected static GameServer GetServerInfo(string _fullIP)
        {
            GameServer sv;
            string ip = GetIP(_fullIP);
            int port = GetPort(_fullIP);
            ip = ip.Split(' ')[0];
            try { sv = new GameServer(new IPEndPoint(IPAddress.Parse(ip), port)); }
            catch { return null; }

            return sv;
        }

        protected static Color GetStatusColor(ServerPlayerStatus status, bool isShadow)
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
                    return (Color)ColorConverter.ConvertFromString("#FF000000");
                else
                    return (Color)ColorConverter.ConvertFromString("#FFA85C00");
            }
        }

        protected static ServerPlayerStatus GetServerPlayerStatus(GameServer sv)
        {
            if (sv.currentPlayer < 30)
                return ServerPlayerStatus.Safe;
            else if (sv.currentPlayer > 29 && sv.currentPlayer < 60)
                return ServerPlayerStatus.Warning;
            else
                return ServerPlayerStatus.Danger;
        }

        protected enum ServerPlayerStatus { Safe, Warning, Danger }
    }
}