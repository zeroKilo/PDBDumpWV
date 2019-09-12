using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBDumpWV
{
    public class TOCStream
    {
        public Dictionary<string, uint> files = new Dictionary<string, uint>();

        public TOCStream(Stream s)
        {
            s.Seek(0x1C, 0);
            uint stringTableSize = StreamHelper.ReadU32(s);
            byte[] sTable = new byte[stringTableSize];
            s.Read(sTable, 0, (int)stringTableSize);
            MemoryStream st = new MemoryStream(sTable);
            uint count = StreamHelper.ReadU32(s);
            s.Seek(0x10, SeekOrigin.Current);
            int pos = (int)s.Position;
            for (int i = 0; i < count; i++)
            {
                s.Seek(pos + i * 8, 0);
                uint start = StreamHelper.ReadU32(s);
                uint streamIdx = StreamHelper.ReadU32(s);
                st.Seek(start, 0);
                string name = StreamHelper.ReadCString(st);
                files.Add(name, streamIdx);
            }
        }
    }
}
