using System;
using System.Collections.Generic;

using System.IO;

using NBT;
using NBT.Tag;

namespace NBTR_TEST
{
    public class Program
    {
        public static void Main(string[] args)
        {
            NBTFile     file;
            DateTime    times = DateTime.Now;

            file = NBTFile.OpenFile(File.OpenRead(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft/saves/new world/level.dat")), 1);

            file.SaveTag(File.OpenWrite(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "gzip.nbt")), 1);
            file.SaveTag(File.OpenWrite(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "zlib.nbt")), 2);

            file = NBTFile.OpenFile(File.OpenRead(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "gzip.nbt")), 1);
            file = NBTFile.OpenFile(File.OpenRead(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "zlib.nbt")), 2);

            Console.WriteLine(string.Format("complete! took {0} ms", (DateTime.Now - times).TotalMilliseconds));
        }
    }
}
