using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBDumpWV
{
    public class TypeRecord
    {
        public class FieldListEntry
        {
            public uint pos;
            public ushort u1;
            public ushort u2;
            public ushort u3;
            public uint u4;
            public uint u5;
            public string name;
            public FieldListEntry(Stream s)
            {
                pos = (uint)s.Position;
                u1 = StreamHelper.ReadU16(s);
                switch (u1)
                {
                    case 0x1502:
                        u2 = StreamHelper.ReadU16(s);
                        u3 = StreamHelper.ReadU16(s);
                        if ((u3 & 0x8000) != 0)
                            u4 = StreamHelper.ReadU32(s);
                        name = StreamHelper.ReadCString(s);
                        break;
                    case 0x150D:
                        u2 = StreamHelper.ReadU16(s);
                        u3 = StreamHelper.ReadU16(s);
                        u4 = StreamHelper.ReadU32(s);
                        if ((u4 & 0x80000000) != 0)
                            u5 = StreamHelper.ReadU32(s);
                        name = StreamHelper.ReadCString(s);
                        break;
                    case 0x150E:
                    case 0x150F:
                    case 0x1510:
                    case 0x1511:
                        u2 = StreamHelper.ReadU16(s);
                        u3 = StreamHelper.ReadU16(s);
                        u4 = StreamHelper.ReadU16(s);
                        if((u2 & 0x10) != 0)
                            u5 = StreamHelper.ReadU32(s);
                        name = StreamHelper.ReadCString(s);
                        break;
                    case 0x1400:
                        u2 = StreamHelper.ReadU16(s);
                        u3 = StreamHelper.ReadU16(s);
                        u4 = StreamHelper.ReadU32(s);
                        name = "";
                        break;
                    case 0x1401:
                        u2 = StreamHelper.ReadU16(s);
                        u3 = StreamHelper.ReadU16(s);
                        u4 = StreamHelper.ReadU32(s);
                        StreamHelper.ReadU32(s);
                        StreamHelper.ReadU16(s);
                        name = "";
                        break;
                    case 0x1402:
                        u2 = StreamHelper.ReadU16(s);
                        u3 = StreamHelper.ReadU16(s);
                        u4 = StreamHelper.ReadU32(s);
                        u5 = StreamHelper.ReadU32(s);
                        StreamHelper.ReadU16(s);
                        name = "";
                        break;
                    case 0x1404:
                        u2 = StreamHelper.ReadU16(s);
                        u3 = StreamHelper.ReadU16(s);
                        u4 = StreamHelper.ReadU16(s);
                        u5 = StreamHelper.ReadU16(s);
                        StreamHelper.ReadU16(s);
                        name = "";
                        break;
                    case 0x1409:
                        u2 = StreamHelper.ReadU16(s);
                        u3 = StreamHelper.ReadU16(s);
                        u4 = StreamHelper.ReadU16(s);
                        name = "";
                        break;
                    default:
                        throw new Exception();
                }
                StreamHelper.Align(s);
            }

            public string ToString(int idx)
            {
                return " Field #" + idx.ToString("D4") + " @0x" + pos.ToString("X8") + " = "
                       + u1.ToString("X4") + " "
                       + u2.ToString("X4") + " "
                       + u3.ToString("X4") + " " + name;
            }
        }

        private uint pos;
        public ushort size;
        public ushort type;
        public List<object> values = new List<object>();
        public TypeRecord(Stream s)
        {
            pos = (uint)(s.Position - 4);
            size = (ushort)(StreamHelper.ReadU16(s) - 2);
            type = StreamHelper.ReadU16(s);
            switch (type)
            {
                case 0x1203:
                    ReadFieldList(s);
                    break;
                default:                    
                    byte[] raw = new byte[size];
                    s.Read(raw, 0, size);
                    break;
            }
        }

        private void ReadFieldList(Stream s)
        {
            long pos = s.Position;
            while (s.Position - pos < size)
                values.Add(new FieldListEntry(s));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Type @0x" + pos.ToString("X8") + " = 0x" + type.ToString("X4"));
            switch (type)
            {
                case 0x1203:
                    for (int i = 0; i < values.Count; i++)
                        sb.AppendLine(((FieldListEntry)values[i]).ToString(i));
                    break;
            }
            return sb.ToString();
        }
    }
}
