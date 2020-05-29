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
            self = this;
        }

        // 伺服器IP
        private string _ip;
        public string ip
        {
            get => _ip;
            set => _ip = value;
        }

        // 伺服器Port
        private int _port;
        public int port
        {
            get => _port;
            set => _port = value;
        }

        // 伺服器名稱
        private string _name;
        public string name
        {
            get => _name;
            set => _name = value;
        }

        // 目前玩家數
        private int _currentPlayer;
        public int currentPlayer
        {
            get => _currentPlayer;
            set => _currentPlayer = value;
        }

        // 查詢介面所顯示的最大玩家數
        private string _dgmaxPlayer;
        public string dgmaxPlayer
        {
            get => _dgmaxPlayer;
            set => _dgmaxPlayer = value;
        }

        // 最大玩家數
        private string _maxPlayer;
        public string maxPlayer
        {
            get => _maxPlayer;
            set => _maxPlayer = value;
        }

        // 傳遞到監控介面的物件，一定是this
        private ServerInfo _self;
        public ServerInfo self
        {
            get => _self;
            set => _self = value;
        }
    };
}
