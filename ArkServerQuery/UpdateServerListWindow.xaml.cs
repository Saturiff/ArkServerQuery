using ArkServerQuery.Classes;
using SourceQuery;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;

namespace ArkServerQuery
{
    public partial class UpdateServerListWindow : Window
    {
        // 更新:
        // 自動更新=陣列A
        // 使用者新增=陣列B
        // 更新結果=A+B

        // 查詢:
        // 可對使用者所新增的陣列B做修改
        public UpdateServerListWindow()
        {
            InitializeComponent();

            Initialize();
        }

        // 有必要?
        ~UpdateServerListWindow()
        {
            officialCollection.Clear();
            customCollection.Clear();
        }

        // 讀取目前 ServerQuery.arkSvCollection 內容
        // 如何拆分custom?
        private void Initialize()
        {

        }


        // ClickUpdate()
        // -> Disable other button 
        // -> UpdateOfficialServerList() 
        // -> Enable other button

        // 更新
        // 讀取原先
        private void UpdateOfficialServerList()
        {
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
            Thread.Sleep(1000); // FIX?
            using (StreamWriter sw = new StreamWriter("ServerList.txt")) svList.ForEach(_sv => sw.Write(_sv));
            sr.Close();
        }

        private void SearchServerInfo(string onlyIP, int p)
        {
            GameServer sv = new GameServer();
            try
            {
                sv = new GameServer(new IPEndPoint(IPAddress.Parse(onlyIP), p));
                // 紀錄ip port name
                string name = sv.name;

                // 篩選不需要的伺服器種類後新增
                if (!(name.Contains("PVE")
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

        private void GenerateFullServerList()
        {

        }

        // 新增
        // 輸入IP, Port
        // 判斷是否與兩集合重複
        // 加入至custom
        private void AddServerToCustomCollection()
        {

        }

        // 修改
        private void EditServerCustomCollection()
        {

        }

        // 刪除
        private void DeleteServerFromCustomCollection()
        {

        }



        private static List<string> svList = new List<string>();

        // 方舟官方 officialserver.ini
        private ArkServerCollection officialCollection = new ArkServerCollection();

        // 玩家新增
        private ArkServerCollection customCollection = new ArkServerCollection();

    }
}
