namespace ARKServerQuery
{
    public class ServerInfo
    {
        public ServerInfo(string ip, int port, string name)
        {
            this.ip = ip;
            this.port = port;
            this.name = name;
        }
        public ServerInfo(string ip, int port, string name, int currentPlayer, string maxPlayer)
        {
            this.ip = ip;
            this.port = port;
            this.name = name;
            this.currentPlayer = currentPlayer;
            this.maxPlayer = maxPlayer;
            dgmaxPlayer = " / " + maxPlayer;
            watchdogString = ip + ":" + port + ',' + name;
        }
        // 伺服器IP
        public string ip { get; set; }
        // 伺服器Port
        public int port { get; set; }
        // 伺服器名稱
        public string name { get; set; }
        // 傳遞到監控介面的字串
        public string watchdogString { get; set; }
        // 目前玩家數
        public int currentPlayer { get; set; }
        // 查詢介面所顯示的最大玩家數
        public string dgmaxPlayer { get; set; }
        // 最大玩家數
        public string maxPlayer { get; set; }
    };
}
