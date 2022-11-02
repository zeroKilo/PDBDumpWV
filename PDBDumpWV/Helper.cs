using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBDumpWV
{
    public static class Helper
    {
        public static string MakeTabs(int count)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
                sb.Append("\t");
            return sb.ToString();
        }

        private static readonly uint[] _lookup32 = CreateLookup32();

        private static uint[] CreateLookup32()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }

        public static string MakeHexString(byte[] bytes)
        {
            var lookup32 = _lookup32;
            var result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
            return new string(result);
        }

        public static byte[] ReverseArray(byte[] arr)
        {
            byte[] result = new byte[arr.Length];
            for (int i = 0; i < arr.Length; i++)
                result[i] = arr[arr.Length - i - 1];
            return result;
        }

        public static uint GetBits(uint buff, int start, int count)
        {
            uint result = buff >> start;
            result &= 0xFFFFFFFF >> 32 - count;
            return result;
        }
    }
}
