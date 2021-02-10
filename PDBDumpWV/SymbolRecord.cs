using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBDumpWV
{
    public class SymbolRecord
    {
        public ushort reclen;
        public ushort rectyp;
        public uint pubsymflags;
        public uint off;
        public ushort seg;
        public string name;
        public SymbolRecord(Stream s)
        {            
            reclen = StreamHelper.ReadU16(s);
            long pos = s.Position;
            rectyp = StreamHelper.ReadU16(s);
            pubsymflags = StreamHelper.ReadU32(s);
            off = StreamHelper.ReadU32(s);
            seg = StreamHelper.ReadU16(s);
            name = StreamHelper.ReadCString(s);
            while (s.Position - pos < reclen && s.Position < s.Length)
                s.ReadByte();
        }

        public override string ToString()
        {
            return name;
        }

        public string Dump()
        {
            return rectyp.ToString("X4") + " " + pubsymflags.ToString("X8") + " " + seg.ToString("X4") + ":" + off.ToString("X8") + "\t" + name;
        }
    }
}
