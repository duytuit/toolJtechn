using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdamLibrary
{
    public static class StringReaderBuilderExtensions
    {
        public static string ReadString(this StringReader reader, int length)
        {
            char[] block = new char[length];
            int charsRead = reader.ReadBlock(block, 0, length);
            return new string(block, 0, charsRead);
        }

        public static double ReadDouble(this StringReader reader, int length)
        {
            string s = reader.ReadString(length);
            return double.Parse(s);
        }

        public static bool TryReadDouble(this StringReader reader, int length, out double value)
        {
            string s = reader.ReadString(length);
            return double.TryParse(s, out value);
        }

        public static int ReadInt(this StringReader reader, int length, NumberStyles style = NumberStyles.Any)
        {
            string s = reader.ReadString(length);
            return int.Parse(s, style);
        }

        public static bool TryReadInt(this StringReader reader, int length, out int value, NumberStyles style = NumberStyles.Any)
        {
            string s = reader.ReadString(length);
            return int.TryParse(s, style, CultureInfo.InvariantCulture.NumberFormat, out value);
        }

        public static void AppendDouble(this StringBuilder builder, double value)
        {
            string s = value.ToString("00.000");
            if (value >= 0)
                builder.Append("+");
            builder.Append(s);
        }
    }
}
