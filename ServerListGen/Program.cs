﻿using SourceQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace ServerListGen
{
    class Program
    {
        public class ServerInfo
        {
            public ServerInfo() { }
            public ServerInfo(string ip, string port, string name)
            {
                this.ip = ip;
                this.port = port;
                this.name = name;
            }

            public string ip { get; set; }
            public string port { get; set; }
            public string name { get; set; }
        };

        static void Main(string[] args)
        {
            Console.WriteLine("該專案用於生成最新的官服列表，大約需要半分鐘的時間..案下Enter開始");
            Console.Read();
            
            string[] port = new string[4] { "27015", "27017", "27019", "27021" };

            // 取得服務器IP列表
            WebRequest webRequest = WebRequest.Create(@"http://arkdedicated.com/officialservers.ini");
            webRequest.Method = "GET";
            WebResponse webResponse = webRequest.GetResponse();
            StreamReader sr = new StreamReader(webResponse.GetResponseStream());
            string[] resultIP = sr.ReadToEnd().Split('\n');
            webResponse.Close();

            Thread searchThread;
            // 寫入回應的服務器資訊
            foreach (string ip in resultIP)
            {
                string onlyIP = ip;
                if (ip.Contains("/")) onlyIP = onlyIP.Split('/')[0];
                if (ip.Contains(" ")) onlyIP = onlyIP.Split(' ')[0];
                if (ip.Contains("\r")) onlyIP = onlyIP.Split('\r')[0];
                foreach (string p in port)
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

            Console.ReadLine();
        }

        private static List<string> svList = new List<string>();
        private static void SearchServerInfo(string onlyIP, string p)
        {
            Console.WriteLine("only ip = {0}, port = {1}", onlyIP, Convert.ToInt16(p));
            GameServer sv = new GameServer();
            try
            {
                sv = new GameServer(new IPEndPoint(IPAddress.Parse(onlyIP), Convert.ToInt16(p)));
                // 紀錄ip port name
                string name = sv.name;
                if (!(name.Contains("PVE")
                    || name.Contains("Tek")
                    || name.Contains("Raid")
                    || name.Contains("Small")
                    || name.Contains("CrossArk")
                    || name.Contains("PrimPlus")
                    || name.Contains("Hardcore")
                    || name.Contains("Classic")
                    || name.Contains("pocalypse")
                    || name.Contains("LEGACY")))
                {
                    svList.Add(onlyIP + ',' + p + ',' + name + ',');
                }
            }
            catch { }
        }
    }
}
