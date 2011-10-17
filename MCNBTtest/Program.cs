using System;
using System.Collections.Generic;

using System.IO;

using NBT;
using NBT.Tag;

namespace MCNBTtest
{
    class Program
    {
        static void Main(string[] args)
        {
            RegionFile regionFile = RegionFile.OpenRegion(File.OpenRead(Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "*.mcr")[0]));

            Console.ReadLine();
        }
    }
}
