﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Updater
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("- Extracting new files...");
            if (args.Length == 0 || !args[0].ToLower().Contains(".zip"))
            {
                args = new string[] { "PoEPartyGear.zip" };
            }
            using (ZipArchive archive = ZipFile.OpenRead(args[0]))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if(entry.FullName.EndsWith("/"))
                        Directory.CreateDirectory(entry.FullName);
                    else
                    {
                        if (!entry.FullName.Contains("Updater.exe"))
                        {
                            Console.WriteLine("Extracting: " + entry.FullName);
                            entry.ExtractToFile(entry.FullName, true);
                        }
                    }
                }
            }
            Console.WriteLine("Done");
            Console.WriteLine();
            Console.WriteLine("- Deleting installation files...");
            Thread.Sleep(250);
            File.Delete(args[0]);
            Console.WriteLine("Done");
            Console.WriteLine();
            Console.WriteLine("- Lauching PoEPartyGear...");
            Process.Start("PoEPartyGear.exe");
            Console.WriteLine("Done");
        }
    }
}
