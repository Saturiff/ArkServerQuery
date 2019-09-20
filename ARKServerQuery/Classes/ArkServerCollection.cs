using System;
using System.Collections.ObjectModel;
using System.Windows;
using SourceQuery;

namespace ARKServerQuery
{
    public class ArkServerCollection
    {
        // 新增伺服器至Collection
        public void Add(ServerInfo sv, GameServer arkServer)
        {
            if (arkServer != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    _collection.Add(new ServerInfo(sv.ip, sv.port, sv.name, arkServer.currentPlayer, Convert.ToString(arkServer.maxPlayer)));
                });
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    _collection.Add(new ServerInfo(sv.ip, sv.port, sv.name, 0, "離線"));
                });
            }
        }
        // 清空伺服器Collection
        public void Clear()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
            {
                _collection.Clear();
            });
        }

        // 儲存伺服器的主Collection
        protected static ObservableCollection<ServerInfo> _collection = new ObservableCollection<ServerInfo>();
        public static ObservableCollection<ServerInfo> collection { get { return _collection; } }
    }
}
