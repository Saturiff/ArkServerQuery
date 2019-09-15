using SourceQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace ARKServerQuery.Classes
{
    public static class ServerQuery
    {
        // 判斷傳入字串是否為合法IP
        public static bool IsIP(string IPStr) => IPAddress.TryParse(IPStr, out _) && IPStr.Contains(".");
        // 從生成出的文件初始化服務器列表並儲存
        public static void InitServerList()
        {
            int serverOnIdx = 0;
            using (StreamReader sr = new StreamReader("./bin/ServerList.txt"))
            {
                string[] allServer = sr.ReadToEnd().Split(',');
                foreach (var svIP in allServer)
                {
                    if (IsIP(svIP)) serverInfoList.Add(new ServerInfo(svIP, Convert.ToInt16(allServer[serverOnIdx + 1]), allServer[serverOnIdx + 2]));
                    serverOnIdx += 1;
                }
            }
        }
        // 查詢服務器是否開啟，若開啟則回傳GameServer類型值，否則回傳null
        private static GameServer GetServerInfo(string ip, int port)
        {
            GameServer arkServer;
            try { arkServer = new GameServer(new IPEndPoint(IPAddress.Parse(ip), port)); }
            catch { return null; }
            return (arkServer.connectStatus) ? arkServer : null;
        }
        // 將回傳的GameServer寫入Collection
        private static void SearchServerInfo(ServerInfo sv) => arkSvCollection.Add(sv, GetServerInfo(sv.ip, sv.port));
        // 依照傳入字串對記憶體內的服務器名稱進行搜索
        public static void ListSearch(string inString)
        {
            if (searchThreadList.Count != 0) searchThreadList.ForEach(c => c.Abort());
            searchThreadList.Clear();
            arkSvCollection.Clear();
            Thread searchThread;
            bool emptyString = inString == string.Empty;
            serverInfoList.ForEach(sv =>
            {
                if (emptyString || sv.name.ToLower().Contains(inString.ToLower()))
                {
                    searchThread = new Thread(() => SearchServerInfo(sv));
                    searchThreadList.Add(searchThread);
                    searchThread.Start();
                }
            });
        }

        // 所有搜尋中的執行緒
        private static List<Thread> searchThreadList = new List<Thread>();
        // 所有搜尋到的服務器資訊
        private static List<ServerInfo> serverInfoList = new List<ServerInfo>();
        // 服務器Collection
        public static ArkServerCollection arkSvCollection = new ArkServerCollection();
    }
}
