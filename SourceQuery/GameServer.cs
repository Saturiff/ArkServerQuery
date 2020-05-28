using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Checksums;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace SourceQuery
{
    [Serializable]
    public class GameServer
    {
        [NonSerialized]
        private IPEndPoint _endPoint;
        [NonSerialized]
        private UdpClient _client;

        // TSource Engine Query
        [NonSerialized]
        private static readonly byte[] A2S_INFO = { 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00 };
        private string _name;
        private byte _maxPlayer;
        private string _gameTagData;

        public string name => _name.Split(' ')[0];
        public int currentPlayer => maxPlayer - Convert.ToInt16(_gameTagData.Split(',')[3].Split(':')[1]);
        public int maxPlayer => Convert.ToInt16(_maxPlayer);
        public bool connectStatus;

        public GameServer() => connectStatus = false;

        public GameServer(IPEndPoint endpoint)
            : this()
        {
            _endPoint = endpoint;

            using (_client = new UdpClient())
            {
                _client.Client.SendTimeout = 500;
                _client.Client.ReceiveTimeout = 500;
                _client.Connect(endpoint);

                RefreshMainInfo();
            }
            _client = null;

            if (_gameTagData?.Length > 0) connectStatus = true;
        }

        public void RefreshMainInfo()
        {
            Send(A2S_INFO);
            var infoData = Receive();
            using (var br = new BinaryReader(new MemoryStream(infoData)))
            {
                br.ReadBytes(2);                                                                    // - type byte, Protocol Version
                _name = br.ReadAnsiString();                                                        // + Server Name
                br.PassAnsiStrings(3);                                                              // - Map, Folder, Game
                br.ReadInt16();                                                                     // - AppID
                br.ReadByte();                                                                      // - Player Count
                _maxPlayer = br.ReadByte();                                                         // + Max Players
                br.ReadBytes(5);                                                                    // - Bot Count, Server Type, Platform, Is Private, Have VAC
                br.ReadAnsiString();                                                                // - Version
                var edf = (ExtraDataFlags)br.ReadByte();                                            // - Extra Data Flag
                if (edf.HasFlag(ExtraDataFlags.GamePort)) br.ReadInt16();                           // - Port
                if (edf.HasFlag(ExtraDataFlags.SteamID)) br.ReadUInt64();                           // - SteamID
                if (edf.HasFlag(ExtraDataFlags.SpectatorInfo))                                      // - Spectator Info
                {
                    br.ReadInt16();
                    br.ReadAnsiString();
                }
                if (edf.HasFlag(ExtraDataFlags.gameTagData)) _gameTagData = br.ReadAnsiString();    // + gameTagData
                if (edf.HasFlag(ExtraDataFlags.GameID)) br.ReadUInt64();                            // - GameID
            }
        }

        // 將訊息更換為A2S指定的格式再送出
        private void Send(byte[] message)
        {
            var fullmessage = new byte[4 + message.Length];
            fullmessage[0] = fullmessage[1] = fullmessage[2] = fullmessage[3] = 0xFF;

            Buffer.BlockCopy(message, 0, fullmessage, 4, message.Length);
            _client.Send(fullmessage, fullmessage.Length);
        }

        private byte[] Receive()
        {
            byte[][] packets = null;
            byte packetNumber = 0, packetCount = 1;
            bool usesBzip2 = false;
            int crc = 0;

            try
            {
                do
                {
                    var result = _client.Receive(ref _endPoint);
                    using (var br = new BinaryReader(new MemoryStream(result)))
                    {
                        if (br.ReadInt32() == -2)
                        {
                            int requestId = br.ReadInt32();
                            usesBzip2 = (requestId & 0x80000000) == 0x80000000;
                            packetNumber = br.ReadByte();
                            packetCount = br.ReadByte();
                            int splitSize = br.ReadInt32();

                            if (usesBzip2 && packetNumber == 0)
                            {
                                int decompressedSize = br.ReadInt32();
                                crc = br.ReadInt32();
                            }
                        }

                        if (packets == null) packets = new byte[packetCount][];

                        var data = new byte[result.Length - br.BaseStream.Position];
                        Buffer.BlockCopy(result, (int)br.BaseStream.Position, data, 0, data.Length);
                        packets[packetNumber] = data;
                    }
                } while (packets.Any(p => p == null));


                var combinedData = Combine(packets);
                if (usesBzip2)
                {
                    combinedData = Decompress(combinedData);
                    Crc32 crc32 = new Crc32();
                    crc32.Update(combinedData);
                    if (crc32.Value != crc) throw new Exception("Invalid CRC for compressed packet data");
                    return combinedData;
                }
                return combinedData;
            }
            catch
            {
                return null;
            }
        }

        private byte[] Decompress(byte[] combinedData)
        {
            using (var compressedData = new MemoryStream(combinedData))
            using (var uncompressedData = new MemoryStream())
            {
                BZip2.Decompress(compressedData, uncompressedData, true);
                return uncompressedData.ToArray();
            }
        }

        private byte[] Combine(byte[][] arrays)
        {
            var rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }    
}
