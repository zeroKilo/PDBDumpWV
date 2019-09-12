using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBDumpWV
{
    public class DebugInfoStream
    {
        public class ModInfo
        {
            public SectionContribEntry sce;
            public ushort Flags;
            public ushort ModuleSymStream;
            public uint SymByteSize;
            public uint C11ByteSize;
            public uint C13ByteSize;
            public ushort SourceFileCount;
            public uint SourceFileNameIndex;
            public uint PdbFilePathNameIndex;
            public string ModuleName;
            public string ObjFileName;
            public ModInfo(Stream s)
            {
                StreamHelper.ReadU32(s);
                sce = new SectionContribEntry(s);
                Flags = StreamHelper.ReadU16(s);
                ModuleSymStream = StreamHelper.ReadU16(s);
                SymByteSize = StreamHelper.ReadU32(s);
                C11ByteSize = StreamHelper.ReadU32(s);
                C13ByteSize = StreamHelper.ReadU32(s);
                SourceFileCount = StreamHelper.ReadU16(s);
                StreamHelper.ReadU16(s);
                StreamHelper.ReadU32(s);
                SourceFileNameIndex = StreamHelper.ReadU32(s);
                PdbFilePathNameIndex = StreamHelper.ReadU32(s);
                ModuleName = StreamHelper.ReadCString(s);
                ObjFileName = StreamHelper.ReadCString(s);
                StreamHelper.Align(s);
            }
        }

        public class SectionContribEntry
        {
            public ushort Section;
            public uint Offset;
            public uint Size;
            public uint Characteristics;
            public ushort ModuleIndex;            
            public uint DataCrc;
            public uint RelocCrc;
            public SectionContribEntry(Stream s)
            {
                Section = StreamHelper.ReadU16(s);
                StreamHelper.ReadU16(s);
                Offset = StreamHelper.ReadU32(s);
                Size = StreamHelper.ReadU32(s);
                Characteristics = StreamHelper.ReadU32(s);
                DataCrc = StreamHelper.ReadU32(s);
                RelocCrc = StreamHelper.ReadU32(s);
                ModuleIndex = StreamHelper.ReadU16(s);
                StreamHelper.ReadU16(s);
            }
        }

        public class SectionMapEntry 
        {
            public ushort Flags;         
            public ushort Ovl;           
            public ushort Group;         
            public ushort Frame;
            public ushort SectionName;   
            public ushort ClassName;     
            public uint Offset;        
            public uint SectionLength; 
            public SectionMapEntry(Stream s)
            {
                Flags = StreamHelper.ReadU16(s);
                Ovl = StreamHelper.ReadU16(s);
                Group = StreamHelper.ReadU16(s);
                Frame = StreamHelper.ReadU16(s);
                SectionName = StreamHelper.ReadU16(s);
                ClassName = StreamHelper.ReadU16(s);
                Offset = StreamHelper.ReadU32(s);
                SectionLength = StreamHelper.ReadU32(s);
            }
        }

        public class FileInfoSubstream 
        {
            public ushort NumModules;
            public uint NumSourceFiles;
            public List<ushort> ModIndices = new List<ushort>();
            public List<ushort> ModFileCounts = new List<ushort>();
            public List<uint> FileNameOffsets = new List<uint>();
            public Dictionary<uint, string> NamesBuffer = new Dictionary<uint, string>();
            public FileInfoSubstream(Stream s)
            {
                NumModules = StreamHelper.ReadU16(s);
                NumSourceFiles = StreamHelper.ReadU16(s);
                for (int i = 0; i < NumModules; i++)
                    ModIndices.Add(StreamHelper.ReadU16(s));                
                uint totalCount = 0;
                for (int i = 0; i < NumModules; i++)
                {
                    ModFileCounts.Add(StreamHelper.ReadU16(s));
                    totalCount += ModFileCounts[i];
                }
                for (int i = 0; i < totalCount; i++)
                    FileNameOffsets.Add(StreamHelper.ReadU32(s));
                FileNameOffsets = FileNameOffsets.Distinct().ToList();
                long pos = s.Position;
                foreach (uint offset in FileNameOffsets)
                {
                    s.Seek(pos + offset, 0);
                    NamesBuffer.Add(offset, StreamHelper.ReadCString(s));
                }
            }
        }

        public uint VersionSignature;
        public uint VersionHeader;
        public uint Age;
        public ushort GlobalStreamIndex;
        public ushort BuildNumber;
        public ushort PublicStreamIndex;
        public ushort PdbDllVersion;
        public ushort SymRecordStream;
        public ushort PdbDllRbld;
        public uint ModInfoSize;
        public uint SectionContributionSize;
        public uint SectionMapSize;
        public uint SourceInfoSize;
        public uint TypeServerMapSize;
        public uint MFCTypeServerIndex;
        public uint OptionalDbgHeaderSize;
        public uint ECSubstreamSize;
        public ushort Flags;
        public ushort Machine;
        public List<ModInfo> modInfo = new List<ModInfo>();
        public List<SectionContribEntry> contribs = new List<SectionContribEntry>();
        public List<SectionMapEntry> sectionMap = new List<SectionMapEntry>();
        
        public FileInfoSubstream fileInfo;

        public DebugInfoStream(Stream s)
        {
            VersionSignature = StreamHelper.ReadU32(s);
            VersionHeader = StreamHelper.ReadU32(s);
            Age = StreamHelper.ReadU32(s);
            GlobalStreamIndex = StreamHelper.ReadU16(s);
            BuildNumber = StreamHelper.ReadU16(s);
            PublicStreamIndex = StreamHelper.ReadU16(s);
            PdbDllVersion = StreamHelper.ReadU16(s);
            SymRecordStream = StreamHelper.ReadU16(s);
            PdbDllRbld = StreamHelper.ReadU16(s);
            ModInfoSize = StreamHelper.ReadU32(s);
            SectionContributionSize = StreamHelper.ReadU32(s);
            SectionMapSize = StreamHelper.ReadU32(s);
            SourceInfoSize = StreamHelper.ReadU32(s);
            TypeServerMapSize = StreamHelper.ReadU32(s);
            MFCTypeServerIndex = StreamHelper.ReadU32(s);
            OptionalDbgHeaderSize = StreamHelper.ReadU32(s);
            ECSubstreamSize = StreamHelper.ReadU32(s);
            Flags = StreamHelper.ReadU16(s);
            Machine = StreamHelper.ReadU16(s);
            StreamHelper.ReadU32(s);
            long pos = s.Position;
            while (s.Position - pos < ModInfoSize)
                modInfo.Add(new ModInfo(s));
            pos = s.Position;
            uint magic = StreamHelper.ReadU32(s);
            while (s.Position - pos < SectionContributionSize)
            {
                contribs.Add(new SectionContribEntry(s));
                if(magic == 0xF13151E4)
                    StreamHelper.ReadU32(s);
            }
            pos = s.Position;
            StreamHelper.ReadU32(s);
            while (s.Position - pos < SectionMapSize)
                sectionMap.Add(new SectionMapEntry(s));
            fileInfo = new FileInfoSubstream(s);
        }

        public string GetNameTable()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<uint, string> pair in fileInfo.NamesBuffer)
                sb.AppendLine(pair.Value);
            return sb.ToString();
        }
    }
}
