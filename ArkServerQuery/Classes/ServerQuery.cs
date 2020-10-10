using SourceQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace ArkServerQuery.Classes
{
    public static class ServerQuery
    {
        // 判斷傳入字串是否為IP格式
        private static bool IsIP(string IPStr) => IPAddress.TryParse(IPStr, out _) && IPStr.Contains(".");

        // 查詢伺服器是否開啟，若開啟則回傳GameServer類型值，否則回傳null
        private static GameServer GetServerInfo(string ip, int port)
        {
            GameServer arkServer;

            try { arkServer = new GameServer(new IPEndPoint(IPAddress.Parse(ip), port)); }
            catch { return null; }

            if (arkServer != null && arkServer.connectStatus == false) return null;

            return arkServer;
        }

        // 將回傳的GameServer寫入Collection
        private static void SearchServerInfo(ServerInfo sv) => arkSvCollection.Add(sv, GetServerInfo(sv.ip, sv.port));

        // 依照傳入字串對記憶體內的伺服器名稱進行搜索
        public static void ListSearch(string inString)
        {
            if (searchThreadList.Count != 0) searchThreadList.ForEach(c => c.Abort());

            searchThreadList.Clear();
            arkSvCollection.Clear();

            Thread searchThread;
            bool isEmptyString = inString == string.Empty;

            serverInfoList.ForEach(sv =>
            {
                if (isEmptyString || sv.name.ToLower().Contains(inString.ToLower()))
                {
                    searchThread = new Thread(() => SearchServerInfo(sv));

                    searchThreadList.Add(searchThread);

                    searchThread.Start();
                }
            });
        }

        // 從生成出的文件初始化伺服器列表並儲存
        public static void InitializeServerList()
        {
            int serverIndex = 0;

            StreamReader sr = new StreamReader("./bin/ServerList.txt");

            string[] allServer = sr.ReadToEnd().Split(',');

            foreach (var svIP in allServer)
            {
                if (IsIP(svIP))
                    serverInfoList.Add(new ServerInfo(svIP, Convert.ToInt16(allServer[serverIndex + 1]), allServer[serverIndex + 2]));

                serverIndex += 1;
            }
        }

        // 所有搜尋中的執行緒
        private static List<Thread> searchThreadList = new List<Thread>();

        // 所有搜尋到的伺服器資訊清單
        private static List<ServerInfo> serverInfoList = new List<ServerInfo>();

        // 所有搜尋到的伺服器GameServer實體集合
        public static ArkServerCollection arkSvCollection
        {
            get => _arkSvCollection;
            set => _arkSvCollection = value;
        }
        private static ArkServerCollection _arkSvCollection = new ArkServerCollection();
    }
}
