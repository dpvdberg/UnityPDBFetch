using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;

namespace UnityPDBFetch
{
    class Program
    {
        public class Options
        {
            [Option('l', "dlls", Required = true, HelpText = "Input dlls to be processed.")]
            public IEnumerable<string> InputFiles { get; set; }
            
            [Option('o', "out", Required = false, HelpText = "Output dir to put pdbs.")]
            public string outdir { get; set; }
        }
        
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions);
        }
        
        static void RunOptions(Options opts)
        {
            foreach (var input in opts.InputFiles)
            {
                if (PDBFetcher.fetch(input, opts.outdir))
                {
                    Console.WriteLine($"Downloaded PDB for '{input}'");
                }
                else
                {
                    Console.WriteLine($"Could not download PDB for '{input}'");
                }
            }
        }
    }
}