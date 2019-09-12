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
        public uint Version;
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
            Version = StreamHelper.ReadU32(s);
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
            while (s.Position - pos < TypeRecordBytes)
                records.Add(new TypeRecord(s));
        }
    }
}
