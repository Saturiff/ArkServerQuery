using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Checksums;
using System;
using System.Collections.Generic;
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
        private IPEndPoint _endpoint;
        [NonSerialized]
        private UdpClient _client;

        // TSource Engine Query
        [NonSerialized]
        private static readonly byte[] A2S_INFO = { 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00 };

        public string Name;
        public byte MaximumPlayerCount;
        public bool _bool;
        public string GameTagData;
        public bool ConnectStatus = true;

        public List<PlayerInfo> Players { get; set; }
        public Dictionary<string, string> Rules { get; set; }
        public string Endpoint { get; set; }

        public GameServer()
        {
            Players = new List<PlayerInfo>();
            Rules = new Dictionary<string, string>();
        }

        public GameServer(IPAddress address)
            : this(new IPEndPoint(address, 27015))
        {            
        }

        public GameServer(IPEndPoint endpoint)
            : this()
        {
            _endpoint = endpoint;
            Endpoint = endpoint.ToString();

            using (_client = new UdpClient())
            {
                _client.Client.SendTimeout = (int)500;
                _client.Client.ReceiveTimeout = (int)500;
                _client.Connect(endpoint);

                RefreshMainInfo();
            }
            _client = null;
        }

        public int GetCurrentPlayer() => Convert.ToInt16(MaximumPlayerCount) - Convert.ToInt16(GameTagData.Split(',')[3].Split(':')[1]);

        public string GetFixedServerName() { return Name.Split(' ')[0]; }

        public void RefreshMainInfo()
        {
            Send(A2S_INFO);
            var infoData = Receive();
            using (var br = new BinaryReader(new MemoryStream(infoData)))
            {
                br.ReadByte();                                                                  // - type byte, not needed
                
                br.ReadByte();                                                                  // - Protocol Version
                Name = br.ReadAnsiString();                                                     // + Server Name
                br.ReadAnsiString();                                                            // - Map
                br.ReadAnsiString();                                                            // - Folder
                br.ReadAnsiString();                                                            // - Game
                br.ReadInt16();                                                                 // - AppID
                br.ReadByte();                                                                  // - Player Count
                MaximumPlayerCount = br.ReadByte();                                             // + Max Players
                br.ReadByte();                                                                  // - Bot Count
                br.ReadByte();                                                                  // - Server Type
                br.ReadByte();                                                                  // - Platform
                _bool = br.ReadByte() == 0x01;                                                  // - Is Private
                _bool = br.ReadByte() == 0x01;                                                  // - Have VAC
                br.ReadAnsiString();                                                            // - Version
                var edf = (ExtraDataFlags)br.ReadByte();                                        // - Extra Data Flag

                if (edf.HasFlag(ExtraDataFlags.GamePort)) br.ReadInt16();                       // - Port
                if (edf.HasFlag(ExtraDataFlags.SteamID)) br.ReadUInt64();                       // - SteamID
                if (edf.HasFlag(ExtraDataFlags.SpectatorInfo))                                  // - Spectator Info
                {
                    br.ReadInt16();
                    br.ReadAnsiString();
                }
                if (edf.HasFlag(ExtraDataFlags.GameTagData)) GameTagData = br.ReadAnsiString(); // + GameTagData
                if (edf.HasFlag(ExtraDataFlags.GameID)) br.ReadUInt64();                        // - GameID
            }
        }

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
                    var result = _client.Receive(ref _endpoint);
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
            catch (Exception e)
            {
                Console.WriteLine("連線錯誤: " + e.ToString());
                ConnectStatus = false;
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
