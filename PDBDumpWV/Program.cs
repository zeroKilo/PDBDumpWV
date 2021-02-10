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
            Console.WriteLine("Writing raw streams...");
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < pdb.rootStreams.Length; i++)
            {
                sb.AppendLine("Stream " + i.ToString("D4") + " = " + pdb.rootStreams[i].name1);
                sb.AppendLine("            = " + pdb.rootStreams[i].name2);
                File.WriteAllBytes("output\\Streams\\Stream" + i.ToString("D4") + ".bin", pdb.GetStreamData(pdb.rootStreams[i]));
            }
            Console.WriteLine("Writing stream name map...");
            File.WriteAllText("output\\StreamNameMap.txt", sb.ToString());
            Console.WriteLine("Writing type info...");
            File.WriteAllText("output\\TypeInfo.txt", "");
            sb = new StringBuilder();
            for (int i = 0; i < pdb.tpi.records.Count; i++)
            {
                sb.Append(pdb.tpi.records[i].ToString());
                if ((i % 10000) == 0)
                {
                    File.AppendAllText("output\\TypeInfo.txt", sb.ToString(), Encoding.ASCII);
                    sb = new StringBuilder();
                }
            }
            File.AppendAllText("output\\TypeInfo.txt", sb.ToString(), Encoding.ASCII);
            sb = new StringBuilder();
            Console.WriteLine("Writing symbol records...");
            foreach (SymbolRecord sym in pdb.symbols)
                sb.AppendLine(sym.Dump());
            File.WriteAllText("output\\SymbolRecords.txt", sb.ToString());
            Console.WriteLine("Writing global name table...");
            File.WriteAllLines("output\\GlobalNameTable.txt", pdb.names.ToArray());
            Console.WriteLine("Writing DBI file info table...");
            File.WriteAllText("output\\DBIFileInfoNameTable.txt", pdb.dbi.GetNameTable());
            Console.WriteLine("Done dumping.");
        }
    }
}
