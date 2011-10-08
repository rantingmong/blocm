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
        /// <summary>
        /// The maximum threads the region reader will allocate (use 2^n values).
        /// </summary>
        public static readonly int  MaxTHREADS = 4;

        NBTFile[]                   chunks;
        /// <summary>
        /// The chunks of the region file.
        /// </summary>
        public  NBTFile[]           Content
        {
            get { return this.chunks; }
        }

        MCROffset[]                 offsets;
        MCRTStamp[]                 tstamps;

        /// <summary>
        /// Gets a chunk from this region.
        /// </summary>
        /// <param name="point">The location of the chunk.</param>
        /// <returns>An NBT file that has the </returns>
        public NBTFile              this            [MCPoint point]
        {
            get
            {
                return this.chunks[point.X + point.Y * 32];
            }
        }

        private                     RegionFile      ()
        {
            this.chunks     = new NBTFile[1024];
            
            this.offsets    = new MCROffset[1024];
            this.tstamps    = new MCRTStamp[1024];
        }

        public  void                InsertChunk     (MCPoint location, NBTFile chunk)
        {
            
        }
        public  void                ModifyChunk     (MCPoint location, NBTFile oldChunk, NBTFile newChunk)
        {

        }
        public  void                RemoveChunk     (MCPoint location)
        {

        }

        public  void                SaveRegion      (Stream stream)
        {

        }

        public  static RegionFile   OpenRegion      (Stream stream)
        {
            RegionFile region = new RegionFile();

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
                        byte[][] chunkBuffer = new byte[1024][];

                        for (int i = 0; i < 1024; i++)
                        {
                            MCROffset offset = region.offsets[i];

                            if (offset.SectorOffset == 0)
                                continue;

                            stream.Seek(offset.SectorOffset * 4096, SeekOrigin.Begin);

                            int     length = EndianessConverter.ToInt32(reader.ReadInt32());
                            byte    verson = reader.ReadByte();

                            chunkBuffer[i] = reader.ReadBytes(length - 1);
                        }

                        int chunkSlice = 1024 / MaxTHREADS;
                        bool[] allOK = new bool[MaxTHREADS];

                        for (int i = 0; i < MaxTHREADS; i++)
                        {
                            byte[][] chunkWorkerBuffer = new byte[chunkSlice][];
                            Array.Copy(chunkBuffer, i * chunkSlice, chunkWorkerBuffer, 0, chunkSlice);

                            int index = i;

                            Thread workerThread = new Thread(new ThreadStart(() =>
                            {
                                int offset = index * (1024 / MaxTHREADS);

                                for (int n = 0; n < chunkWorkerBuffer.Length; n++)
                                {
                                    byte[] chunk = chunkWorkerBuffer[n];

                                    if (chunk == null)
                                        continue;

                                    using (MemoryStream mStream = new MemoryStream(chunk))
                                        region.chunks[n + offset] = NBTFile.OpenFile(mStream, 2);
                                }

                                allOK[index] = true;
                                chunkWorkerBuffer = null;

                                Console.WriteLine("\tThread worker " + (index + 1) + " is complete!");
                            }));
                            workerThread.Name = "chunk worker thread " + (index + 1);
                            workerThread.Start();
                        }

                        chunkBuffer = null;

                        while (true)
                            if (CheckIfAllOK(allOK))
                                break;
                        
                        break;
                    }
                }
            }

            GC.Collect();

            return region;
        }

        private static bool         CheckIfAllOK    (bool[] collection)
        {
            for (int i = 0; i < collection.Length; i++)
            {
                if (!collection[i])
                    return false;
            }

            return true;
        }
    }
}
