using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBDumpWV
{
    public class TPIStream
    {
        public enum TPIVersion
        {
            V40 = 19950410, //0x01306B4A
            V41 = 19951122, //0x01306E12
            V50 = 19961031, //0x013094C7
            V70 = 19990903, //0x01310977
            V80 = 20040203, //0x0131CA0B
        }
        public TPIVersion Version;
        public uint HeaderSize;
        public uint TypeIndexBegin;
        public uint TypeIndexEnd;
        public uint TypeRecordBytes;
        public ushort HashStreamIndex;
        public ushort HashAuxStreamIndex;
        public uint HashKeySize;
        public uint NumHashBuckets;
        public uint HashValueBufferOffset;
        public uint HashValueBufferLength;
        public uint IndexOffsetBufferOffset;
        public uint IndexOffsetBufferLength;
        public uint HashAdjBufferOffset;
        public uint HashAdjBufferLength;
        public List<TypeRecord> records = new List<TypeRecord>();
        public TPIStream(Stream s)
        {
            Version = (TPIVersion)StreamHelper.ReadU32(s);
            HeaderSize = StreamHelper.ReadU32(s);
            TypeIndexBegin = StreamHelper.ReadU32(s);
            TypeIndexEnd = StreamHelper.ReadU32(s);
            TypeRecordBytes = StreamHelper.ReadU32(s);
            HashStreamIndex = StreamHelper.ReadU16(s);
            HashAuxStreamIndex = StreamHelper.ReadU16(s);
            HashKeySize = StreamHelper.ReadU32(s);
            NumHashBuckets = StreamHelper.ReadU32(s);
            HashValueBufferOffset = StreamHelper.ReadU32(s);
            HashValueBufferLength = StreamHelper.ReadU32(s);
            IndexOffsetBufferOffset = StreamHelper.ReadU32(s);
            IndexOffsetBufferLength = StreamHelper.ReadU32(s);
            HashAdjBufferOffset = StreamHelper.ReadU32(s);
            HashAdjBufferLength = StreamHelper.ReadU32(s);
            long pos = s.Position;
            int count = 0;
            while (s.Position - pos < TypeRecordBytes)
            {
                records.Add(new TypeRecord(s));
                if((count++ % 10000) == 0)
                {
                    float f = (s.Position - pos) / (float)TypeRecordBytes;
                    f *= 100f;
                    Console.Write((int)f + "%\r");
                }
            }
        }
    }
}
