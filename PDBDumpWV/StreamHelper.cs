using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBDumpWV
{
    public static class StreamHelper
    {
        public static bool littleEndian = true;

        public static void WriteU8(Stream s, byte v)
        {
            s.WriteByte(v);
        }

        public static void WriteS8(Stream s, sbyte v)
        {
            WriteU8(s, (byte)v);
        }

        public static byte ReadU8(Stream s)
        {
            return (byte)s.ReadByte();
        }

        public static sbyte ReadS8(Stream s)
        {
            return (sbyte)ReadU8(s);
        }

        public static void WriteU16(Stream s, ushort v)
        {
            if (littleEndian)
            {
                s.WriteByte((byte)(v & 0xFF));
                s.WriteByte((byte)((v >> 8) & 0xFF));
            }
            else
            {
                s.WriteByte((byte)((v >> 8) & 0xFF));
                s.WriteByte((byte)(v & 0xFF));
            }
        }

        public static void WriteS16(Stream s, short v)
        {
            WriteU16(s, (ushort)v);
        }

        public static ushort ReadU16(Stream s)
        {
            ushort result = (byte)s.ReadByte();
            if (littleEndian)
            {
                result |= (ushort)((byte)s.ReadByte() << 8);
            }
            else
            {
                result = (ushort)((result << 8) | (byte)s.ReadByte());
            }
            return result;
        }

        public static short ReadS16(Stream s)
        {
            return (short)ReadU16(s);
        }

        public static void WriteU32(Stream s, uint v)
        {
            if (littleEndian)
            {
                s.WriteByte((byte)(v & 0xFF));
                s.WriteByte((byte)((v >> 8) & 0xFF));
                s.WriteByte((byte)((v >> 16) & 0xFF));
                s.WriteByte((byte)((v >> 24) & 0xFF));
            }
            else
            {
                s.WriteByte((byte)((v >> 24) & 0xFF));
                s.WriteByte((byte)((v >> 16) & 0xFF));
                s.WriteByte((byte)((v >> 8) & 0xFF));
                s.WriteByte((byte)(v & 0xFF));
            }
        }

        public static void WriteS32(Stream s, int v)
        {
            WriteU32(s, (uint)v);
        }

        public static uint ReadU32(Stream s)
        {
            uint result = (byte)s.ReadByte();
            if (littleEndian)
            {
                result |= (uint)((byte)s.ReadByte() << 8);
                result |= (uint)((byte)s.ReadByte() << 16);
                result |= (uint)((byte)s.ReadByte() << 24);
            }
            else
            {
                result = (uint)((result << 8) | (byte)s.ReadByte());
                result = (uint)((result << 8) | (byte)s.ReadByte());
                result = (uint)((result << 8) | (byte)s.ReadByte());
            }
            return result;
        }

        public static int ReadS32(Stream s)
        {
            return (int)ReadU32(s);
        }

        public static string ReadFixedString(Stream s, int len)
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < len; i++)
                sb.Append((char)s.ReadByte());
            return sb.ToString();
        }

        public static byte[] ReadFixedBuffer(Stream s, int len)
        {
            byte[] result = new byte[len];
            s.Read(result, 0, len);
            return result;
        }

        public static void WriteFixedString(Stream s, string v)
        {
            foreach (char c in v)
                s.WriteByte((byte)c);
        }

        public static string ReadCString(Stream s)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                byte b = (byte)s.ReadByte();
                if (b == 0)
                    break;
                else
                    sb.Append((char)b);
            }
            return sb.ToString();
        }

        public static void WriteCString(Stream s, string v)
        {
            foreach (char c in v)
                s.WriteByte((byte)c);
            s.WriteByte(0);
        }

        public static void Align(Stream s)
        {
            while ((s.Position % 4) != 0)
                s.Seek(1, SeekOrigin.Current);
        }
    }
}
