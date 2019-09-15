using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SourceQuery
{
    public static class BinaryReaderExtensions
    {
        public static string ReadAnsiString(this BinaryReader br)
        {
            var stringBytes = new List<byte>();
            byte charByte;
            while ((charByte = br.ReadByte()) != 0)
            {
                stringBytes.Add(charByte);
            }
            return Encoding.ASCII.GetString(stringBytes.ToArray());
        }

        public static void PassAnsiStrings(this BinaryReader br, int count)
        {
            for (int i = 0; i < count; i++) while ((br.ReadByte()) != 0) ;
        }
    }
}
