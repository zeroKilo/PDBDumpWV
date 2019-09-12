using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBDumpWV
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1 || !File.Exists(args[0]))
                return;
            if (!Directory.Exists("output\\Streams")) 
                Directory.CreateDirectory("output\\Streams");
            PDBFile pdb = new PDBFile(args[0]);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < pdb.rootStreams.Length; i++)
            {
                sb.AppendLine("Stream " + i.ToString("D4") + " = " + pdb.rootStreams[i].name1);
                sb.AppendLine("            = " + pdb.rootStreams[i].name2);
                File.WriteAllBytes("output\\Streams\\Stream" + i.ToString("D4") + ".bin", pdb.GetStreamData(pdb.rootStreams[i]));
            }
            File.WriteAllText("output\\StreamNameMap.txt", sb.ToString());
            sb = new StringBuilder();
            foreach (TypeRecord t in pdb.tpi.records)
                sb.Append(t);
            File.WriteAllText("output\\TypeInfo.txt", sb.ToString());
            sb = new StringBuilder();
            foreach (SymbolRecord sym in pdb.symbols)
                sb.AppendLine(sym.Dump());
            File.WriteAllText("output\\SymbolRecords.txt", sb.ToString());
            File.WriteAllLines("output\\GlobalNameTable.txt", pdb.names.ToArray());
            File.WriteAllText("output\\DBIFileInfoNameTable.txt", pdb.dbi.GetNameTable());
            Console.WriteLine();
        }
    }
}
