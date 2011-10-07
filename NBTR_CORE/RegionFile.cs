﻿/*  Minecraft NBT reader
 * 
 *  Copyright 2010-2011 Michael Ong, all rights reserved.
 *  
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public License
 *  as published by the Free Software Foundation; either version 2
 *  of the License, or (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
using System;
using System.Collections.Generic;

using System.IO;
using System.IO.Compression;

using System.Threading;

using NBT.Tag;
using NBT.Utils;

namespace NBT
{
    public class RegionFile
    {
        NBTFile[]                   chunks;
        public  NBTFile[]           Content
        {
            get { return this.chunks; }
        }

        MCROffset[]                 offsets;
        public  MCROffset[]         Offsets
        {
            get { return this.offsets; }
        }

        MCRTStamp[]                 tstamps;
        public  MCRTStamp[]         TStamps
        {
            get { return this.tstamps; }
        }

        int                         count;
        public  int                 Count
        {
            get { return this.count; }
        }

        private                     RegionFile  ()
        {
            this.chunks     = new NBTFile[1024];
            
            this.offsets    = new MCROffset[1024];
            this.tstamps    = new MCRTStamp[1024];

            this.count  = 0;
        }

        public  void                InsertChunk (MCPoint location, NBTTag chunk)
        {
            
        }
        public  void                ModifyChunk (MCPoint location, NBTTag oldChunk, NBTTag newChunk)
        {

        }
        public  void                RemoveChunk (MCPoint location)
        {

        }

        public  void                SaveRegion  (Stream stream)
        {

        }

        public  static RegionFile   OpenRegion  (Stream stream)
        {
            RegionFile region = new RegionFile();

            bool c1 = false;
            bool c2 = false;
            bool c3 = false;
            bool c4 = false;

            using (BinaryReader reader = new BinaryReader(stream))
            {
                bool sc = false;
                bool tc = false;

                int[] sectors = new int[1024];
                int[] tstamps = new int[1024];

                for (int i = 0; i < 1024; i++)
                    sectors[i] = reader.ReadInt32();

                for (int i = 0; i < 1024; i++)
                    tstamps[i] = reader.ReadInt32();

                Thread offsetThread = new Thread(new ThreadStart(() =>
                {
                    for (int i = 0; i < 1024; i++)
                    {
                        sectors[i] = EndianessConverter.ToInt32(sectors[i]);

                        region.offsets[i] = new MCROffset()
                        {
                            SectorOffset    = sectors[i] >> 8,
                            SectorSize      = (byte)(sectors[i] & 0xFF),
                        };
                    }

                    sectors = null;

                    sc = true;
                }));
                offsetThread.Name = "offset calculator thread";
                offsetThread.Start();
               
                Thread tstampThread = new Thread(new ThreadStart(() =>
                {
                    for (int i = 0; i < 1024; i++)
                    {
                        tstamps[i] = EndianessConverter.ToInt32(tstamps[i]);

                        region.tstamps[i] = new MCRTStamp()
                        {
                            Timestamp = tstamps[i],
                        };
                    }

                    tstamps = null;

                    tc = true;
                }));
                tstampThread.Name = "timestamp calculator thread";
                tstampThread.Start();

                while (true)
                {
                    if (tc && sc)
                    {
                        byte[][] part1 = new byte[256][];
                        byte[][] part2 = new byte[256][];
                        byte[][] part3 = new byte[256][];
                        byte[][] part4 = new byte[256][];

                        for (int i = 0; i < 1024; i++)
                        {
                            MCROffset offset = region.offsets[i];

                            if (offset.SectorOffset == 0)
                                continue;

                            stream.Seek(offset.SectorOffset * 4096, SeekOrigin.Begin);

                            int len = EndianessConverter.ToInt32(reader.ReadInt32());
                            reader.ReadByte();

                            if (i < 256)
                            {
                                part1[i] = reader.ReadBytes(len - 1);
                            }
                            else if (i < 512)
                            {
                                part2[i - 256] = reader.ReadBytes(len - 1);
                            }
                            else if (i < 768)
                            {
                                part3[i - 512] = reader.ReadBytes(len - 1);
                            }
                            else
                            {
                                part4[i - 768] = reader.ReadBytes(len - 1);
                            }
                        }

                        Thread t1 = new Thread(new ThreadStart(() =>
                        {
                            for (int i = 0; i < 256; i++)
                            {
                                if (part1[i] == null)
                                    continue;

                                MemoryStream mstream = new MemoryStream(part1[i]);

                                region.chunks[i] = NBTFile.OpenFile(mstream, 2);

                                mstream.Close();
                                mstream.Dispose();

                                mstream = null;

                                part1[i] = null;
                            }

                            c1 = true;
                        }));
                        t1.Name = "chunk parser worker 1";
                        t1.Start();
                        
                        Thread t2 = new Thread(new ThreadStart(() =>
                        {
                            for (int i = 0; i < 256; i++)
                            {
                                if (part2[i] == null)
                                    continue;

                                MemoryStream mstream = new MemoryStream(part2[i]);

                                region.chunks[i + 256] = NBTFile.OpenFile(mstream, 2);

                                mstream.Close();
                                mstream.Dispose();

                                mstream = null;

                                part2[i] = null;
                            }

                            c2 = true;
                        }));
                        t2.Name = "chunk parser worker 2";
                        t2.Start();
                        
                        Thread t3 = new Thread(new ThreadStart(() =>
                        {
                            for (int i = 0; i < 256; i++)
                            {
                                if (part3[i] == null)
                                    continue;

                                MemoryStream mstream = new MemoryStream(part3[i]);

                                region.chunks[i + 512] = NBTFile.OpenFile(mstream, 2);

                                mstream.Close();
                                mstream.Dispose();

                                mstream = null;

                                part3[i] = null;
                            }

                            c3 = true;
                        }));
                        t3.Name = "chunk parser worker 3";
                        t3.Start();

                        Thread t4 = new Thread(new ThreadStart(() =>
                        {
                            for (int i = 0; i < 256; i++)
                            {
                                if (part4[i] == null)
                                    continue;

                                MemoryStream mstream = new MemoryStream(part4[i]);

                                region.chunks[i + 768] = NBTFile.OpenFile(mstream, 2);

                                mstream.Close();
                                mstream.Dispose();

                                mstream = null;

                                part4[i] = null;
                            }

                            c4 = true;
                        }));
                        t4.Name = "chunk parser worker 4";
                        t4.Start();

                        while (!c1 && !c2 && !c3 && !c4) ;

                        break;
                    }
                }
            }

            GC.Collect();

            return region;
        }
    }
}
