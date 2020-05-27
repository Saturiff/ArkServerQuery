using SourceQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace ServerListGen
{
    class MainProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine("該專案用於生成最新的官服列表，大約需要半分鐘的時間..案下Enter開始");
            Console.Read();
            
            int[] port = new int[] { 27010, 27013, 27015, 27017, 27019, 27021, 27023, 27025, 27028, 27031 };

            // 取得伺服器IP列表
            WebRequest webRequest = WebRequest.Create(@"http://arkdedicated.com/officialservers.ini");
            webRequest.Method = "GET";
            WebResponse webResponse = webRequest.GetResponse();
            StreamReader sr = new StreamReader(webResponse.GetResponseStream());
            string[] resultIP = sr.ReadToEnd().Split('\n');
            webResponse.Close();

            Thread searchThread;
            // 寫入回應的伺服器資訊
            foreach (string ip in resultIP)
            {
                string onlyIP = ip;
                if (ip.Contains("/")) onlyIP = onlyIP.Split('/')[0];
                if (ip.Contains(" ")) onlyIP = onlyIP.Split(' ')[0];
                if (ip.Contains("\r")) onlyIP = onlyIP.Split('\r')[0];
                foreach (int p in port)
                {
                    searchThread = new Thread(() => SearchServerInfo(onlyIP, p));
                    searchThread.Start();
                }
            }
            Thread.Sleep(1000);
            Console.WriteLine("寫入檔案...");
            using (StreamWriter sw = new StreamWriter("ServerList.txt")) svList.ForEach(_sv => sw.Write(_sv));
            sr.Close();
            Console.WriteLine("\n-- 清單創建完成 --");

            Console.Read();
        }

        private static List<string> svList = new List<string>();
        // private static void SearchServerInfo(string onlyIP, string p)
        private static void SearchServerInfo(string onlyIP, int p)
        {
            Console.WriteLine("only ip = {0}, port = {1}", onlyIP, p);
            GameServer sv = new GameServer();
            try
            {
                sv = new GameServer(new IPEndPoint(IPAddress.Parse(onlyIP), p));
                // 紀錄ip port name
                string name = sv.name;

                // 篩選不需要的伺服器種類後新增
                if ( !(name.Contains("PVE")
                    || name.Contains("Tek")
                    || name.Contains("Raid")
                    || name.Contains("Small")
                    || name.Contains("CrossArk")
                    || name.Contains("PrimPlus")
                    || name.Contains("Hardcore")
                    || name.Contains("Classic")
                    || name.Contains("pocalypse")
                    || name.Contains("LEGACY")
                    || name.Contains("Asia"))
                    )
                    svList.Add(onlyIP + ',' + p + ',' + name + ',');
            }
            catch { }
        }
    }
}
