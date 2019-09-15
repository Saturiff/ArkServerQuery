using System;

namespace SourceQuery
{
    [Flags]
    [Serializable]
    public enum ExtraDataFlags : byte
    {
        GamePort = 0x80,
        SpectatorInfo = 0x40,
        gameTagData = 0x20,
        SteamID = 0x10,
        GameID = 0x01,
    }
}
