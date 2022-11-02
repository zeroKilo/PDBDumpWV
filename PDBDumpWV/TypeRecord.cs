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
        public enum LeafRecordKind
        {
            LF_VTSHAPE = 0x000a,
            LF_LABEL = 0x000e,
            LF_ENDPRECOMP = 0x0014,
            LF_MODIFIER = 0x1001,
            LF_POINTER = 0x1002,
            LF_PROCEDURE = 0x1008,
            LF_MFUNCTION = 0x1009,
            LF_ARGLIST = 0x1201,
            LF_FIELDLIST = 0x1203,
            LF_BITFIELD = 0x1205,
            LF_METHODLIST = 0x1206,
            LF_ARRAY = 0x1503,
            LF_CLASS = 0x1504,
            LF_STRUCTURE = 0x1505,
            LF_UNION = 0x1506,
            LF_ENUM = 0x1507,
            LF_PRECOMP = 0x1509,
            LF_TYPESERVER2 = 0x1515,
            LF_INTERFACE = 0x1519,
            LF_VFTABLE = 0x151d,
            LF_FUNC_ID = 0x1601,
            LF_MFUNC_ID = 0x1602,
            LF_BUILDINFO = 0x1603,
            LF_SUBSTR_LIST = 0x1604,
            LF_STRING_ID = 0x1605,
            LF_UDT_SRC_LINE = 0x1606,
            LF_UDT_MOD_SRC_LINE = 0x1607,
        }

        public enum MemberRecordKind
        {
            LF_BCLASS = 0x1400,
            LF_VBCLASS = 0x1401,
            LF_IVBCLASS = 0x1402,
            LF_INDEX = 0x1404,
            LF_VFUNCTAB = 0x1409,
            LF_ENUMERATE = 0x1502,
            LF_MEMBER = 0x150d,
            LF_STMEMBER = 0x150e,
            LF_METHOD = 0x150f,
            LF_NESTTYPE = 0x1510,
            LF_ONEMETHOD = 0x1511,
            LF_BINTERFACE = 0x151a,
        }

        public enum ValueType
        {
            LF_CHAR = 0x8000,
            LF_SHORT = 0x8001,
            LF_USHORT = 0x8002,
            LF_LONG = 0x8003,
            LF_ULONG = 0x8004,
            LF_REAL32 = 0x8005,
            LF_REAL64 = 0x8006,
            LF_REAL80 = 0x8007,
            LF_REAL128 = 0x8008,
            LF_QUADWORD = 0x8009,
            LF_UQUADWORD = 0x800a,
            LF_REAL48 = 0x800b,
            LF_COMPLEX32 = 0x800c,
            LF_COMPLEX64 = 0x800d,
            LF_COMPLEX80 = 0x800e,
            LF_COMPLEX128 = 0x800f,
            LF_VARSTRING = 0x8010,
            LF_OCTWORD = 0x8017,
            LF_UOCTWORD = 0x8018,
            LF_DECIMAL = 0x8019,
            LF_DATE = 0x801a,
            LF_UTF8STRING = 0x801b,
            LF_REAL16 = 0x801c
        }

        public abstract class LeafRecord
        {
            public abstract string Dump(int tabs = 0);
        }

        public abstract class MemberRecord
        {
            public uint _offset;
            public abstract string Dump(int tabs = 0);
        }

        public class Value
        {
            public ValueType type;
            public byte[] data;
            public Value(Stream s)
            {
                ushort test = StreamHelper.ReadU16(s);
                if (test < 0x8000)
                {
                    type = ValueType.LF_USHORT;
                    data = Helper.ReverseArray(BitConverter.GetBytes(test));
                }
                else
                {
                    type = (ValueType)test;
                    switch (type)
                    {
                        case ValueType.LF_CHAR:
                            data = StreamHelper.ReadFixedBuffer(s, 1);
                            break;
                        case ValueType.LF_REAL16:
                        case ValueType.LF_SHORT:
                        case ValueType.LF_USHORT:
                            data = StreamHelper.ReadFixedBuffer(s, 2);
                            break;
                        case ValueType.LF_LONG:
                        case ValueType.LF_ULONG:
                        case ValueType.LF_REAL32:
                            data = StreamHelper.ReadFixedBuffer(s, 4);
                            break;
                        case ValueType.LF_REAL48:
                            data = StreamHelper.ReadFixedBuffer(s, 6);
                            break;
                        case ValueType.LF_DATE:
                        case ValueType.LF_COMPLEX32:
                        case ValueType.LF_REAL64:
                        case ValueType.LF_QUADWORD:
                        case ValueType.LF_UQUADWORD:
                            data = StreamHelper.ReadFixedBuffer(s, 8);
                            break;
                        case ValueType.LF_REAL80:
                            data = StreamHelper.ReadFixedBuffer(s, 10);
                            break;
                        case ValueType.LF_DECIMAL:
                        case ValueType.LF_OCTWORD:
                        case ValueType.LF_UOCTWORD:
                        case ValueType.LF_COMPLEX64:
                        case ValueType.LF_REAL128:
                            data = StreamHelper.ReadFixedBuffer(s, 16);
                            break;
                        case ValueType.LF_COMPLEX80:
                            data = StreamHelper.ReadFixedBuffer(s, 20);
                            break;
                        case ValueType.LF_COMPLEX128:
                            data = StreamHelper.ReadFixedBuffer(s, 32);
                            break;
                        case ValueType.LF_UTF8STRING:
                        case ValueType.LF_VARSTRING:
                            ushort count = StreamHelper.ReadU16(s);
                            data = StreamHelper.ReadFixedBuffer(s, count);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }

        public class FieldAttribute
        {
            public enum MProp
            {
                MTvanilla   = 0x00,
                MTvirtual   = 0x01,
                MTstatic    = 0x02,
                MTfriend    = 0x03,
                MTintro     = 0x04,
                MTpurevirt  = 0x05,
                MTpureintro = 0x06,
                error       = 0x07
            }
            public enum Access
            {
                _unknown = 0,
                _private = 1,
                _protected = 2,
                _public = 3,
            }

            public bool noconstruct;
            public bool noinherit;
            public bool pseudo;
            public MProp mprop;
            public Access access;
            public bool compgenx;
            public ushort _raw;

            public FieldAttribute(ushort attr)
            {
                _raw = attr;
                access = (Access)Helper.GetBits(attr, 0, 2);
                mprop = (MProp)Helper.GetBits(attr, 2, 3);
                pseudo = Helper.GetBits(attr, 5, 1) != 0;
                noinherit = Helper.GetBits(attr, 6, 1) != 0;
                noconstruct = Helper.GetBits(attr, 7, 1) != 0;
                compgenx = Helper.GetBits(attr, 8, 1) != 0;
            }

            public override string ToString()
            {
                return _raw.ToString("X4") + " " + //Convert.ToString(_raw, 2).PadLeft(16, '0') + "b " +
                       (noconstruct ? "noconstruct " : "") +
                       (noinherit ? "noinherit " : "") +
                       (pseudo ? "pseudo " : "") +
                       (compgenx ? "compgenx " : "") +
                       mprop + " " + access;
            }
        }

        public class Property
        {
            public bool fwdref;
            public bool opcast;
            public bool opassign;
            public bool cnested;
            public bool isnested;
            public bool ovlops;
            public bool ctor;
            public bool packed;
            public bool reserved;
            public bool scoped;
            public ushort _raw;

            public Property(ushort u)
            {
                _raw = u;
                packed = Helper.GetBits(u, 0, 1) != 0;
                ctor = Helper.GetBits(u, 1, 1) != 0;
                ovlops = Helper.GetBits(u, 2, 1) != 0;
                isnested = Helper.GetBits(u, 3, 1) != 0;
                cnested = Helper.GetBits(u, 4, 1) != 0;
                opassign = Helper.GetBits(u, 5, 1) != 0;
                opcast = Helper.GetBits(u, 6, 1) != 0;
                fwdref = Helper.GetBits(u, 7, 1) != 0;
                scoped = Helper.GetBits(u, 8, 1) != 0;
                reserved = Helper.GetBits(u, 9, 7) != 0;
            }

            public override string ToString()
            {
                return _raw.ToString("X4") + " " + //Convert.ToString(_raw, 2).PadLeft(16, '0') + "b " +
                       (fwdref ? "fwdref " : "") +
                       (opcast ? "opcast " : "") +
                       (opassign ? "opassign " : "") +
                       (cnested ? "cnested " : "") +
                       (isnested ? "isnested " : "") +
                       (ovlops ? "ovlops " : "") +
                       (ctor ? "ctor " : "") +
                       (packed ? "packed " : "") +
                       (reserved ? "reserved " : "") +
                       (scoped ? "scoped " : "");
            }
        }

        public class MR_Enumerate : MemberRecord
        {
            public FieldAttribute attr;
            public Value value;
            public string name;
            public MR_Enumerate(uint pos, Stream s)
            {
                _offset = pos;
                attr = new FieldAttribute(StreamHelper.ReadU16(s));
                value = new Value(s);
                name = StreamHelper.ReadCString(s);
            }

            public override string Dump(int tabs = 0)
            {
                return Helper.MakeTabs(tabs) + "LF_ENUMERATE @0x" + _offset.ToString("X8") + " : "
                       + "attr = [" + attr + "] "
                       + "value = " + Helper.MakeHexString(value.data) + " "
                       + name;
            }
        }

        public class MR_Member : MemberRecord
        {
            public FieldAttribute attr;
            public uint index;
            public Value offset;
            public string name;
            public MR_Member(uint pos, Stream s)
            {
                _offset = pos;
                attr = new FieldAttribute(StreamHelper.ReadU16(s));
                index = StreamHelper.ReadU32(s);
                offset = new Value(s);
                name = StreamHelper.ReadCString(s);
            }

            public override string Dump(int tabs = 0)
            {
                return Helper.MakeTabs(tabs) + "LF_MEMBER @0x" + _offset.ToString("X8") + " : "
                       + "attr = [" + attr + "] "
                       + "index = 0x" + index.ToString("X8") + " "
                       + "offset = " + Helper.MakeHexString(offset.data) + " "
                       + name;
            }
        }

        public class MR_StaticMember : MemberRecord
        {
            public FieldAttribute attr;
            public uint index;
            public string name;
            public MR_StaticMember(uint pos, Stream s)
            {
                _offset = pos;
                attr = new FieldAttribute(StreamHelper.ReadU16(s));
                index = StreamHelper.ReadU32(s);
                name = StreamHelper.ReadCString(s);
            }

            public override string Dump(int tabs = 0)
            {
                return Helper.MakeTabs(tabs) + "LF_STMEMBER @0x" + _offset.ToString("X8") + " : "
                       + "attr = [" + attr + "] "
                       + "index = 0x" + index.ToString("X8") + " "
                       + name;
            }
        }

        public class MR_Method : MemberRecord
        {
            public ushort count;
            public uint mList;
            public string name;
            public MR_Method(uint pos, Stream s)
            {
                _offset = pos;
                count = StreamHelper.ReadU16(s);
                mList = StreamHelper.ReadU32(s);
                name = StreamHelper.ReadCString(s);
            }

            public override string Dump(int tabs = 0)
            {
                return Helper.MakeTabs(tabs) + "LF_METHOD @0x" + _offset.ToString("X8") + " : "
                       + "count = 0x" + count.ToString("X4") + " "
                       + "mList = 0x" + mList.ToString("X8") + " "
                       + name;
            }
        }

        public class MR_NestType : MemberRecord
        {
            public ushort pad0;
            public uint index;
            public string name;
            public MR_NestType(uint pos, Stream s)
            {
                _offset = pos;
                pad0 = StreamHelper.ReadU16(s);
                index = StreamHelper.ReadU32(s);
                name = StreamHelper.ReadCString(s);
            }

            public override string Dump(int tabs = 0)
            {
                return Helper.MakeTabs(tabs) + "LF_NESTTYPE @0x" + _offset.ToString("X8") + " : "
                       + "pad0 = 0x" + pad0.ToString("X4")
                       + " index = 0x" + index.ToString("X8") + " "
                       + name;
            }
        }

        public class MR_OneMethod : MemberRecord
        {
            public FieldAttribute attr;
            public uint index;
            public uint val;
            public string name;
            public MR_OneMethod(uint pos, Stream s)
            {
                _offset = pos;
                attr = new FieldAttribute(StreamHelper.ReadU16(s));
                index = StreamHelper.ReadU32(s);                
                if (attr.mprop == FieldAttribute.MProp.MTintro || attr.mprop == FieldAttribute.MProp.MTpureintro)
                    val = StreamHelper.ReadU32(s);
                name = StreamHelper.ReadCString(s);
            }

            public override string Dump(int tabs = 0)
            {
                return Helper.MakeTabs(tabs) + "LF_ONEMETHOD @0x" + _offset.ToString("X8") + " : "
                       + "attr = [" + attr + "] "
                       + "index = 0x" + index.ToString("X8") + " "
                       + "val = 0x" + val.ToString("X8") + " "
                       + name;
            }
        }

        public class MR_BaseClass : MemberRecord
        {
            public FieldAttribute attr;
            public uint index;
            public Value offset;
            public MR_BaseClass(uint pos, Stream s)
            {
                _offset = pos;
                attr = new FieldAttribute(StreamHelper.ReadU16(s));
                index = StreamHelper.ReadU32(s);
                offset = new Value(s);
            }

            public override string Dump(int tabs = 0)
            {
                return Helper.MakeTabs(tabs) + "LF_BCLASS @0x" + _offset.ToString("X8") + " : "
                       + "attr = [" + attr + "] "
                       + "index = 0x" + index.ToString("X8") + " "
                       + "offset = " + Helper.MakeHexString(offset.data);
            }
        }

        public class MR_VirtualBaseClass : MemberRecord
        {
            public FieldAttribute attr;
            public uint index;
            public uint vbptr;
            public Value vbpoff;
            public MR_VirtualBaseClass(uint pos, Stream s)
            {
                _offset = pos;
                attr = new FieldAttribute(StreamHelper.ReadU16(s));
                index = StreamHelper.ReadU32(s);
                vbptr = StreamHelper.ReadU32(s);
                vbpoff = new Value(s);
            }

            public override string Dump(int tabs = 0)
            {
                return Helper.MakeTabs(tabs) + "LF_VBCLASS @0x" + _offset.ToString("X8") + " : "
                       + "attr = [" + attr + "] "
                       + "index = 0x" + index.ToString("X8") + " "
                       + "vbptr = 0x" + vbptr.ToString("X8") + " "
                       + "vbpoff = 0x" + Helper.MakeHexString(vbpoff.data) + " ";
            }
        }

        public class MR_InterfaceVirtualBaseClass : MemberRecord
        {
            public FieldAttribute attr;
            public uint index;
            public uint vbptr;
            public Value vbpoff;
            public MR_InterfaceVirtualBaseClass(uint pos, Stream s)
            {
                _offset = pos;
                attr = new FieldAttribute(StreamHelper.ReadU16(s));
                index = StreamHelper.ReadU32(s);
                vbptr = StreamHelper.ReadU32(s);
                vbpoff = new Value(s);
            }

            public override string Dump(int tabs = 0)
            {
                return Helper.MakeTabs(tabs) + "LF_VBCLASS @0x" + _offset.ToString("X8") + " : "
                       + "attr = [" + attr + "] "
                       + "index = 0x" + index.ToString("X8") + " "
                       + "vbptr = 0x" + vbptr.ToString("X8") + " "
                       + "vbpoff = 0x" + Helper.MakeHexString(vbpoff.data) + " ";
            }
        }

        public class MR_Index : MemberRecord
        {
            public ushort pad0;
            public uint index;
            public MR_Index(uint pos, Stream s)
            {
                _offset = pos;
                pad0 = StreamHelper.ReadU16(s);
                index = StreamHelper.ReadU32(s);
            }

            public override string Dump(int tabs = 0)
            {
                return Helper.MakeTabs(tabs) + "LF_INDEX @0x" + _offset.ToString("X8") + " : "
                       + "pad0 = 0x" + pad0.ToString("X4") + " "
                       + "index = 0x" + index.ToString("X8");
            }
        }

        public class MR_VirtualFunctionTable : MemberRecord
        {
            public FieldAttribute attr;
            public uint index;
            public MR_VirtualFunctionTable(uint pos, Stream s)
            {
                _offset = pos;
                attr = new FieldAttribute(StreamHelper.ReadU16(s));
                index = StreamHelper.ReadU32(s);
            }

            public override string Dump(int tabs = 0)
            {
                return Helper.MakeTabs(tabs) + "LF_VFUNCTAB @0x" + _offset.ToString("X8") + " : "
                       + "attr = [" + attr + "] "
                       + "index = 0x" + index.ToString("X8");
            }
        }

        public class LR_FieldList : LeafRecord
        {
            public List<MemberRecord> members = new List<MemberRecord>();
            public LR_FieldList(byte[] raw)
            {
                MemoryStream s = new MemoryStream(raw);
                while(s.Position < s.Length)
                {
                    uint pos = (uint)s.Position;
                    ushort type = StreamHelper.ReadU16(s);
                    switch((MemberRecordKind)type)
                    {
                        case MemberRecordKind.LF_ENUMERATE:
                            members.Add(new MR_Enumerate(pos, s));
                            break;
                        case MemberRecordKind.LF_MEMBER:
                            members.Add(new MR_Member(pos, s));
                            break;
                        case MemberRecordKind.LF_STMEMBER:
                            members.Add(new MR_StaticMember(pos, s));
                            break;
                        case MemberRecordKind.LF_METHOD:
                            members.Add(new MR_Method(pos, s));
                            break;
                        case MemberRecordKind.LF_NESTTYPE:
                            members.Add(new MR_NestType(pos, s));
                            break;
                        case MemberRecordKind.LF_ONEMETHOD:
                            members.Add(new MR_OneMethod(pos, s));
                            break;
                        case MemberRecordKind.LF_BCLASS:
                            members.Add(new MR_BaseClass(pos, s));
                            break;
                        case MemberRecordKind.LF_VBCLASS:
                            members.Add(new MR_VirtualBaseClass(pos, s));
                            break;
                        case MemberRecordKind.LF_IVBCLASS:
                            members.Add(new MR_InterfaceVirtualBaseClass(pos, s));
                            break;
                        case MemberRecordKind.LF_VFUNCTAB:
                            members.Add(new MR_VirtualFunctionTable(pos, s));
                            break;
                        case MemberRecordKind.LF_INDEX:
                            members.Add(new MR_Index(pos, s));
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    StreamHelper.Align(s);
                }
            }

            public override string Dump(int tabs = 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Helper.MakeTabs(tabs));
                foreach(MemberRecord member in members)
                    sb.AppendLine(member.Dump(tabs + 1));
                return sb.ToString();
            }
        }

        public class LR_Enum : LeafRecord
        {
            public ushort count;
            public Property property;
            public uint utype;
            public uint field;
            public string name;
            public LR_Enum(byte[] raw)
            {
                MemoryStream s = new MemoryStream(raw);
                count = StreamHelper.ReadU16(s);
                property = new Property(StreamHelper.ReadU16(s));
                utype = StreamHelper.ReadU32(s);
                field = StreamHelper.ReadU32(s);
                name = StreamHelper.ReadCString(s);
            }
            public override string Dump(int tabs = 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Helper.MakeTabs(tabs + 1) + "Count = " + count +
                          " property: [" + property + "]" +
                          " utype: 0x" + utype.ToString("X8") +
                          " field: 0x" + field.ToString("X8") +
                          " " + name);
                return sb.ToString();
            }
        }

        public class LR_Structure : LeafRecord
        {
            public ushort count;
            public Property property;
            public uint field;
            public uint derived;
            public uint vshape;
            public Value data;
            public string name;
            public LR_Structure(byte[] raw)
            {
                MemoryStream s = new MemoryStream(raw);
                count = StreamHelper.ReadU16(s);
                property = new Property(StreamHelper.ReadU16(s));
                field = StreamHelper.ReadU32(s);
                derived = StreamHelper.ReadU32(s);
                vshape = StreamHelper.ReadU32(s);
                data = new Value(s);
                name = StreamHelper.ReadCString(s);
            }
            public override string Dump(int tabs = 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Helper.MakeTabs(tabs + 1) + "Count = " + count +
                          " property: [" + property + "]" +
                          " field: 0x" + field.ToString("X8") +
                          " derived: 0x" + derived.ToString("X8") +
                          " vshape: 0x" + derived.ToString("X8") +
                          " data: " + Helper.MakeHexString(data.data) +
                          " " + name);
                return sb.ToString();
            }
        }

        public class LR_BitField : LeafRecord
        {
            public uint type;
            public byte length;
            public byte position;
            public LR_BitField(byte[] raw)
            {
                MemoryStream s = new MemoryStream(raw);
                type = StreamHelper.ReadU32(s);
                length = (byte)s.ReadByte();
                position = (byte)s.ReadByte();
            }
            public override string Dump(int tabs = 0)
            {
                return Helper.MakeTabs(tabs + 1) +
                          "type = 0x" + type.ToString("X8") +
                          " length = 0x" + length.ToString("X2") +
                          " position = 0x" + position.ToString("X2");
            }
        }

        public class LR_Union : LeafRecord
        {
            public ushort count;
            public Property property;
            public uint field;
            public Value data;
            public string name;
            public LR_Union(byte[] raw)
            {
                MemoryStream s = new MemoryStream(raw);
                count = StreamHelper.ReadU16(s);
                property = new Property(StreamHelper.ReadU16(s));
                field = StreamHelper.ReadU32(s);
                data = new Value(s);
                name = StreamHelper.ReadCString(s);
            }
            public override string Dump(int tabs = 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Helper.MakeTabs(tabs + 1) + "Count = " + count +
                          " property: [" + property + "]" +
                          " field: 0x" + field.ToString("X8") +
                          " data: " + Helper.MakeHexString(data.data) +
                          " " + name);
                return sb.ToString();
            }
        }

        public class LR_ArgList : LeafRecord
        {
            public uint count;
            public uint[] arg;
            public LR_ArgList(byte[] raw)
            {
                MemoryStream s = new MemoryStream(raw);
                count = StreamHelper.ReadU32(s);
                arg = new uint[count];
                for(int i = 0; i < count; i++)
                    arg[i] = StreamHelper.ReadU32(s);
            }
            public override string Dump(int tabs = 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Helper.MakeTabs(tabs + 1) +
                          "count = 0x" + count.ToString("X8") +
                          " arg = [ ");
                foreach (uint u in arg)
                    sb.Append("0x" + u.ToString("X8") + " ");
                sb.Append("]");
                return sb.ToString();
            }
        }

        public class LR_Class : LeafRecord
        {
            public ushort count;
            public Property property;
            public uint field;
            public uint derived;
            public uint vshape;
            public Value data;
            public string name;
            public LR_Class(byte[] raw)
            {
                MemoryStream s = new MemoryStream(raw);
                count = StreamHelper.ReadU16(s);
                property = new Property(StreamHelper.ReadU16(s));
                field = StreamHelper.ReadU32(s);
                derived = StreamHelper.ReadU32(s);
                vshape = StreamHelper.ReadU32(s);
                data = new Value(s);
                name = StreamHelper.ReadCString(s);
            }
            public override string Dump(int tabs = 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Helper.MakeTabs(tabs + 1) + "Count = " + count +
                          " property = [" + property + "]" +
                          " field = 0x" + field.ToString("X8") +
                          " derived = 0x" + derived.ToString("X8") +
                          " vshape = 0x" + derived.ToString("X8") +
                          " data = " + Helper.MakeHexString(data.data) +
                          " " + name);
                return sb.ToString();
            }
        }

        public class LR_MFunction : LeafRecord
        {
            public enum CallType
            {
                NEAR_C = 0x00000000,
                FAR_C = 0x00000001,
                NEAR_PASCAL = 0x00000002,
                FAR_PASCAL = 0x00000003,
                NEAR_FAST = 0x00000004,
                FAR_FAST = 0x00000005,
                SKIPPED = 0x00000006,
                NEAR_STD = 0x00000007,
                FAR_STD = 0x00000008,
                NEAR_SYS = 0x00000009,
                FAR_SYS = 0x0000000A,
                THISCALL = 0x0000000B,
                MIPSCALL = 0x0000000C,
                GENERIC = 0x0000000D,
                ALPHACALL = 0x0000000E,
                PPCCALL = 0x0000000F,
                SHCALL = 0x00000010,
                ARMCALL = 0x00000011,
                AM33CALL = 0x00000012,
                TRICALL = 0x00000013,
                SH5CALL = 0x00000014,
                M32RCALL = 0x00000015,
                RESERVED = 0x00000016,
            }
            public uint rvtype;
            public uint classtype;
            public uint thistype;
            public CallType calltype;
            public byte reserved;
            public ushort parmcount;
            public uint arglist;
            public LR_MFunction(byte[] raw)
            {
                MemoryStream s = new MemoryStream(raw);
                rvtype = StreamHelper.ReadU32(s);
                classtype = StreamHelper.ReadU32(s);
                thistype = StreamHelper.ReadU32(s);
                calltype = (CallType)s.ReadByte();
                reserved = (byte)s.ReadByte();
                parmcount = StreamHelper.ReadU16(s);
                arglist = StreamHelper.ReadU32(s);
            }
            public override string Dump(int tabs = 0)
            {
                return Helper.MakeTabs(tabs + 1) +
                          "rvtype = 0x" + rvtype.ToString("X8") +
                          " classtype = 0x" + classtype.ToString("X8") +
                          " thistype = 0x" + thistype.ToString("X8") +
                          " calltype = " + calltype +
                          " reserved = 0x" + reserved.ToString("X2") +
                          " parmcount = 0x" + parmcount.ToString("X4") +
                          " arglist = 0x" + arglist.ToString("X8");
            }
        }

        public class LR_Modifier : LeafRecord
        {
            public enum ModAttr
            {
                MOD_const = 0x0001,
                MOD_volatile = 0x0002,
                MOD_unaligned = 0x0004,
            }
            public uint type;
            public ModAttr attr;
            public LR_Modifier(byte[] raw)
            {
                MemoryStream s = new MemoryStream(raw);
                type = StreamHelper.ReadU32(s);
                attr = (ModAttr)StreamHelper.ReadU16(s);
            }
            public override string Dump(int tabs = 0)
            {
                return Helper.MakeTabs(tabs + 1) +
                          "type = 0x" + type.ToString("X8") +
                          " attr = " + attr;
            }
        }

        public class LR_Pointer : LeafRecord
        {
            public class LeafPointerAttr
            {
                public enum PointerType
                {
                    PTR_BASE_SEG = 0x03,
                    PTR_BASE_VAL = 0x04,
                    PTR_BASE_SEGVAL = 0x05,
                    PTR_BASE_ADDR = 0x06,
                    PTR_BASE_SEGADDR = 0x07,
                    PTR_BASE_TYPE = 0x08,
                    PTR_BASE_SELF = 0x09,
                    PTR_NEAR32 = 0x0a,
                    PTR_64 = 0x0c,
                    PTR_UNUSEDPTR = 0x0d
                };
                public enum PointerMode
                {
                    PTR_MODE_PTR = 0x00,
                    PTR_MODE_REF = 0x01,
                    PTR_MODE_PMEM = 0x02,
                    PTR_MODE_PMFUNC = 0x03,
                    PTR_MODE_RESERVED = 0x04
                };

                public PointerType ptrtype;
                public PointerMode ptrmode;
                public bool isflat32;
                public bool isvolatile;
                public bool isconst;
                public bool isunaligned;
                public bool isrestrict;
                public uint _raw;

                public LeafPointerAttr(uint attr)
                {
                    _raw = attr;
                    ptrtype = (PointerType)Helper.GetBits(attr, 0, 5);
                    ptrmode = (PointerMode)Helper.GetBits(attr, 5, 3);
                    isflat32 = Helper.GetBits(attr, 8, 1) != 0;
                    isvolatile = Helper.GetBits(attr, 9, 1) != 0;
                    isconst = Helper.GetBits(attr, 10, 1) != 0;
                    isunaligned = Helper.GetBits(attr, 11, 1) != 0;
                    isrestrict = Helper.GetBits(attr, 12, 1) != 0;
                }

                public override string ToString()
                {
                    return _raw.ToString("X8") + " " + ptrtype + " " + ptrmode + " " +
                           (isflat32 ? "isflat32 " : "") +
                           (isvolatile ? "isvolatile " : "") +
                           (isconst ? "isconst " : "") +
                           (isunaligned ? "isunaligned " : "") +
                           (isrestrict ? "isrestrict " : "");
                }
            }
            public uint type;
            public LeafPointerAttr attr;
            public LR_Pointer(byte[] raw)
            {
                MemoryStream s = new MemoryStream(raw);
                type = StreamHelper.ReadU32(s);
                attr = new LeafPointerAttr(StreamHelper.ReadU32(s));
            }
            public override string Dump(int tabs = 0)
            {
                return Helper.MakeTabs(tabs + 1) +
                          "type = 0x" + type.ToString("X8") +
                          " attr = [" + attr + "]";
            }
        }

        public class LR_Procedure : LeafRecord
        {
            public uint rvtype;
            public byte calltype;
            public byte reserved;
            public ushort parmcount;
            public uint arglist;
            public LR_Procedure(byte[] raw)
            {
                MemoryStream s = new MemoryStream(raw);
                rvtype = StreamHelper.ReadU32(s);
                calltype = (byte)s.ReadByte();
                reserved = (byte)s.ReadByte();
                parmcount = StreamHelper.ReadU16(s);
                arglist = StreamHelper.ReadU32(s);
            }
            public override string Dump(int tabs = 0)
            {
                return Helper.MakeTabs(tabs + 1) +
                          "rvtype = 0x" + rvtype.ToString("X8") +
                          " calltype = 0x" + calltype.ToString("X2") +
                          " reserved = 0x" + reserved.ToString("X2") +
                          " parmcount = 0x" + parmcount.ToString("X4") +
                          " arglist = 0x" + arglist.ToString("X8");
            }
        }

        public class LR_Array : LeafRecord
        {
            public uint elemtype;
            public uint idxtype;
            public Value data;
            public string name;
            public LR_Array(byte[] raw)
            {
                MemoryStream s = new MemoryStream(raw);
                elemtype = StreamHelper.ReadU32(s);
                idxtype = StreamHelper.ReadU32(s);
                data = new Value(s);
                name = StreamHelper.ReadCString(s);
            }
            public override string Dump(int tabs = 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Helper.MakeTabs(tabs + 1) +
                          "elemtype = 0x" + elemtype.ToString("X8") +
                          " idxtype = 0x" + idxtype.ToString("X8") +
                          " data: " + Helper.MakeHexString(data.data) +
                          " " + name);
                return sb.ToString();
            }
        }

        public class LR_MethodList : LeafRecord
        {
            public class Method
            {
                public FieldAttribute attr;
                public uint index;
                public uint vbaseoff;
                public Method(Stream s)
                {
                    attr = new FieldAttribute((ushort)StreamHelper.ReadU32(s));
                    index = StreamHelper.ReadU32(s);
                    if(attr.mprop == FieldAttribute.MProp.MTstatic || attr.mprop == FieldAttribute.MProp.MTfriend) 
                        vbaseoff = StreamHelper.ReadU32(s);
                }
            }
            public List<Method> list;
            public LR_MethodList(byte[] raw)
            {
                MemoryStream s = new MemoryStream(raw);
                list = new List<Method>();
                while(s.Position < s.Length)
                    list.Add(new Method(s));
            }
            public override string Dump(int tabs = 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Method m in list)
                    sb.AppendLine(Helper.MakeTabs(tabs + 1) +
                                  "attr = [" + m.attr + "]" +
                                  " index = 0x" + m.index.ToString("X8") +
                                  " vbaseoff = 0x" + m.vbaseoff.ToString("X8"));
                return sb.ToString();
            }
        }

        public class LR_VTShape : LeafRecord
        {
            public enum Shape
            {
                near = 0x00,
                far = 0x01,
                thin = 0x02,
                outer = 0x03,
                meta = 0x04,
                near32 = 0x05,
                far32 = 0x06,
                unused = 0x07
            }
            public List<Shape> list;
            public LR_VTShape(byte[] raw)
            {
                MemoryStream s = new MemoryStream(raw);
                ushort count = StreamHelper.ReadU16(s);
                ushort bcount = (ushort)(count / 2);
                if ((count % 2) != 0)
                    bcount++;
                byte[] buff = new byte[bcount];
                s.Read(buff, 0, bcount);
                list = new List<Shape>();
                foreach(byte b in buff)
                {
                    list.Add((Shape)(b >> 4));
                    if (list.Count == count)
                        break;
                    list.Add((Shape)(b & 0xf));
                    if (list.Count == count)
                        break;
                }
            }
            public override string Dump(int tabs = 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Helper.MakeTabs(tabs + 1));
                foreach (Shape s in list)
                    sb.Append(s + " ");
                return sb.ToString();
            }
        }

        private uint pos;
        public ushort size;
        public ushort kind;
        public LeafRecord record;
        public TypeRecord(Stream s)
        {
            pos = (uint)s.Position;
            size = (ushort)(StreamHelper.ReadU16(s) - 2);
            kind = StreamHelper.ReadU16(s);
            byte[] raw = new byte[size];
            s.Read(raw, 0, size);
            switch ((LeafRecordKind)kind)
            {
                case LeafRecordKind.LF_FIELDLIST:
                    record = new LR_FieldList(raw);
                    break;
                case LeafRecordKind.LF_STRUCTURE:
                    record = new LR_Structure(raw);
                    break;
                case LeafRecordKind.LF_ENUM:
                    record = new LR_Enum(raw);
                    break;
                case LeafRecordKind.LF_BITFIELD:
                    record = new LR_BitField(raw);
                    break;
                case LeafRecordKind.LF_UNION:
                    record = new LR_Union(raw);
                    break;
                case LeafRecordKind.LF_ARGLIST:
                    record = new LR_ArgList(raw);
                    break;
                case LeafRecordKind.LF_CLASS:
                    record = new LR_Class(raw);
                    break;
                case LeafRecordKind.LF_MFUNCTION:
                    record = new LR_MFunction(raw);
                    break;
                case LeafRecordKind.LF_MODIFIER:
                    record = new LR_Modifier(raw);
                    break;
                case LeafRecordKind.LF_POINTER:
                    record = new LR_Pointer(raw);
                    break;
                case LeafRecordKind.LF_PROCEDURE:
                    record = new LR_Procedure(raw);
                    break;
                case LeafRecordKind.LF_ARRAY:
                    record = new LR_Array(raw);
                    break;
                case LeafRecordKind.LF_METHODLIST:
                    record = new LR_MethodList(raw);
                    break;
                case LeafRecordKind.LF_VTSHAPE:
                    record = new LR_VTShape(raw);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public string MakeString(int index)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Type IDX=0x" + index.ToString("X4") + " @0x" + pos.ToString("X8") + " = " + (LeafRecordKind)kind);
            sb.AppendLine(record.Dump());
            return sb.ToString();
        }
    }
}
