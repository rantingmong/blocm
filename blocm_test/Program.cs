using System;
using System.IO;
using System.Threading;
using NBT;
using NBT.Formats;

namespace MCNBTtest
{
    internal class Program
    {
        private static void zeMain(string[] args)
        {
            string[] regions = Directory.GetFiles(@"D:\Minecraft\Minecraft SMP server\world_1 - Copy\region");

            DateTime tStart = DateTime.Now;

            foreach (string path in regions)
            {
                Console.WriteLine("Reading region " + Path.GetFileName(path) + "\n");

                RegionFile.OpenRegion(File.OpenRead(path));
                Thread.Sleep(100);

                Console.WriteLine();
            }

            Console.WriteLine("Read complete! Took " + Math.Round((DateTime.Now - tStart).TotalSeconds, 2) + "sec. to read the world.");
            Console.ReadLine();
        }

        private static void Main(string[] args)
        {
            NbtFile nbtFile;

            RegionFile regionFile = null;
            RegionFile anvilFile = null;

            int choice = 3;

            foo:
            Console.WriteLine("Hiya! This is a test application for my NBT reader, MC NBT reader.");
            Console.WriteLine("Be sure to have a .nbt and/or .mcr on your desktop or else this won't work...");

            Console.WriteLine("\n");
            Console.WriteLine("(1) open a NBT file");
            Console.WriteLine("(2) open a MCR file");
            Console.WriteLine("(2) open a MCA file");
            Console.WriteLine("(3) say bye bye to this application");

            Console.Write("\nEnter choice: ");

            string parse = Console.ReadLine();

            if (!int.TryParse(parse, out choice) || (choice > 3 || choice < 0))
            {
                Console.Beep();
                Console.Clear();

                goto foo;
            }
            else
            {
                Console.Clear();

                try
                {
                    switch (choice)
                    {
                    case 1:
                        nbtFile = NbtFile.OpenFile(File.OpenRead(Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "*.nbt")[0]), 1);
                        break;
                    case 2:
                        regionFile = RegionFile.OpenRegion(File.OpenRead(Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "*.mcr")[0]));
                        break;
                    case 3:
                        anvilFile = RegionFile.OpenRegion(File.OpenRead(Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "*.mca")[0]));
                        break;
                    case 4:
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            Console.WriteLine("parse complete!");

            foreach (NbtFile file in anvilFile.Content)
            {
                if (file != null)
                {
                    var anvil = new Anvil(file);

                    Console.WriteLine(anvil.XPos + " " + anvil.ZPos);
                }
            }

            Console.WriteLine(anvilFile.Content.Length);

            Console.ReadLine();
        }
    }
}
