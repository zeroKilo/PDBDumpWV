using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PDBDumpWV
{
    public class PDBFile
    {
        public class RootStream
        {
            public uint size;
            public uint[] pages;
            public string name1 = "";
            public string name2 = "";
        }

        public uint dPageBytes;
        public uint dFlagPage;
        public uint dFilePages;
        public uint dRootBytes;
        public uint dReserved;
        public uint pAdIndexPages;
        public uint[] adIndexPages;
        public RootStream[] rootStreams;
        public List<string> names;
        public TOCStream toc;
        public TPIStream tpi;
        public DebugInfoStream dbi;
        public List<SymbolRecord> symbols = new List<SymbolRecord>();
        MemoryStream raw;

        public PDBFile(string filename)
        {
            raw = new MemoryStream(File.ReadAllBytes(filename));
            raw.Seek(0, 0);
            string magic = StreamHelper.ReadFixedString(raw, 24);
            if (magic != "Microsoft C/C++ MSF 7.00")
                return;
            raw.Seek(0x20, 0);
            dPageBytes = StreamHelper.ReadU32(raw);
            dFlagPage = StreamHelper.ReadU32(raw);
            dFilePages = StreamHelper.ReadU32(raw);
            dRootBytes = StreamHelper.ReadU32(raw);
            dReserved = StreamHelper.ReadU32(raw);
            pAdIndexPages = StreamHelper.ReadU32(raw);
            raw.Seek(pAdIndexPages * dPageBytes, 0);
            List<uint> pages = new List<uint>();
            while (raw.Position < pAdIndexPages * dPageBytes + dRootBytes)
            {
                uint u = StreamHelper.ReadU32(raw);
                if (u != 0)
                    pages.Add(u);
                else
                    break;
            }
            adIndexPages = pages.ToArray();
            ReadRootStreams();
            ReadGlobalNameTable();
            ReadTPIStream();
            ReadDebugInfoStream();
            ReadSymbolRecordStream();
        }

        public void ReadRootStreams()
        {
            MemoryStream m = new MemoryStream();
            foreach (uint page in adIndexPages)
                CopyPage(page, raw, m);
            m.Seek(0, 0);
            uint count = StreamHelper.ReadU32(m);
            rootStreams = new RootStream[count];
            for (int i = 0; i < count; i++)
            {
                rootStreams[i] = new RootStream();
                rootStreams[i].size = StreamHelper.ReadU32(m);
                if (rootStreams[i].size == 0xFFFFFFFF)
                    rootStreams[i].size = 0;
            }
            for (int i = 0; i < count; i++)
            {

                uint subcount = rootStreams[i].size / dPageBytes;
                if ((rootStreams[i].size % dPageBytes) != 0)
                    subcount++;
                rootStreams[i].pages = new uint[subcount];
                for (int j = 0; j < subcount; j++)
                    rootStreams[i].pages[j] = StreamHelper.ReadU32(m);
            }
            rootStreams[0].name1 = "Old Directory";
            rootStreams[1].name1 = "TOC Stream";
            rootStreams[2].name1 = "TPI Stream";
            rootStreams[3].name1 = "DBI Stream";
        }

        public void ReadGlobalNameTable()
        {
            toc = new TOCStream(new MemoryStream(GetStreamData(rootStreams[1])));
            names = new List<string>();
            foreach (KeyValuePair<string, uint> pair in toc.files)
            {
                if (pair.Key == "/names")
                {
                    MemoryStream m = new MemoryStream(GetStreamData(rootStreams[pair.Value]));
                    m.Seek(0x8, 0);
                    uint size = StreamHelper.ReadU32(m) + 0xC;
                    while (m.Position < size)
                        names.Add(StreamHelper.ReadCString(m));
                }
                rootStreams[pair.Value].name1 = pair.Key;
            }            
        }

        public void ReadTPIStream()
        {
            tpi = new TPIStream(new MemoryStream(GetStreamData(rootStreams[2])));
        }

        public void ReadDebugInfoStream()
        {
            dbi = new DebugInfoStream(new MemoryStream(GetStreamData(rootStreams[3])));
            foreach (DebugInfoStream.ModInfo mod in dbi.modInfo)
                if(mod.ModuleSymStream != 0xFFFF)
                {
                    rootStreams[mod.ModuleSymStream].name1 = mod.ObjFileName;
                    rootStreams[mod.ModuleSymStream].name2 = mod.ModuleName;
                }
            rootStreams[dbi.GlobalStreamIndex].name1 = "Global Stream";
            rootStreams[dbi.PublicStreamIndex].name1 = "Public Stream";
            rootStreams[dbi.SymRecordStream].name1 = "Symbol Record Stream";
        }

        public void ReadSymbolRecordStream()
        {
            MemoryStream m = new MemoryStream(GetStreamData(rootStreams[dbi.SymRecordStream]));
            long len = m.Length;
            while (m.Position < len)
                symbols.Add(new SymbolRecord(m));
        }

        public byte[] GetStreamData(RootStream rs)
        {
            MemoryStream m = new MemoryStream();
            foreach (uint page in rs.pages)
                CopyPage(page, raw, m);
            byte[] data = new byte[rs.size];
            m.Seek(0, 0);
            m.Read(data, 0, (int)rs.size);
            return data;
        }

        public void CopyPage(uint page, Stream src, Stream dst)
        {
            byte[] buff = new byte[dPageBytes];
            src.Seek(page * dPageBytes, 0);
            src.Read(buff, 0, (int)dPageBytes);
            dst.Write(buff, 0, (int)dPageBytes);
        }
    }
}
