using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ARKServerQuery
{
    static class LocalServer
    {
        // 初始化Tcp執行緒
        public static void InitLocalServer() => serverTread = new Thread(LocalHost);
        // 處理查詢介面與監控介面的資料傳遞
        // 使用本機的Port: 18500進行資料傳遞
        private static void LocalHost()
        {
            var myListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 18500);
            myListener.Start();
            newClient = myListener.AcceptTcpClient();
            // 未使用的功能：接收資料
            /*
            while (true)
            {
                try
                {
                    clientStream = newClient.GetStream();
                    var br = new BinaryReader(clientStream);
                    string receive = null;
                    receive = br.ReadString();
                }
                catch { }
            }
            */
        }

        // 傳遞字串資料到監控介面
        public static void Send(string data)
        {
            clientStream = newClient.GetStream();
            var bw = new BinaryWriter(clientStream);
            bw.Write(data);
        }

        private static TcpClient newClient;
        private static NetworkStream clientStream;
        public static Thread serverTread;
    }
}
