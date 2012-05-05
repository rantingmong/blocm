/*  Minecraft NBT reader
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
    /// <summary>
    /// A Minecraft region file.
    /// </summary>
    public class RegionFile
    {
        /// <summary>
        /// The maximum threads the region reader will allocate (use 2^n values).
        /// </summary>
        public static int           MaxTHREADS      = 4;

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
        public  NBTFile             this            [MCPoint point]
        {
            get
            {
                return this.chunks[point.X + point.Y * 32];
            }
            set
            {
                InsertChunk(point, value);
            }
        }

        private                     RegionFile      ()
        {
            this.chunks         = new NBTFile   [1024];
            
            this.offsets        = new MCROffset [1024];
            this.tstamps        = new MCRTStamp [1024];
        }

        /// <summary>
        /// Inserts/replaces a new chunk on a specified location.
        /// </summary>
        /// <param name="location">The region location of the chunk.</param>
        /// <param name="chunk">The chunk to be added.</param>
        public  void                InsertChunk     (MCPoint location, NBTFile chunk)
        {
            int offset = location.X + (location.Y * 32);

            this.chunks[offset] = chunk;
        }
        /// <summary>
        /// Removes a chunk on a specified location.
        /// </summary>
        /// <param name="location">The region location of the chunk to be removed.</param>
        public  void                RemoveChunk     (MCPoint location)
        {
            int offset = location.X + (location.Y * 32);

            this.chunks[offset] = null;
        }

        /// <summary>
        /// Saves the region file to a stream.
        /// </summary>
        /// <param name="stream">The stream the region file will write to.</param>
        public  void                SaveRegion      (Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {

            }
        }

        /// <summary>
        /// Opens the region file from a stream.
        /// </summary>
        /// <param name="stream">The stream the region file will read from.</param>
        /// <returns>The parsed region file.</returns>
        public  static RegionFile   OpenRegion      (Stream stream)
        {
            RegionFile  region = new RegionFile();

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // initialize values
                #region Init

                int[] sectors = new int[1024];
                int[] tstamps = new int[1024];

                #endregion

                // read header information
                #region Header IO read

                for (int i = 0; i < 1024; i++)
                    sectors[i] = reader.ReadInt32();

                for (int i = 0; i < 1024; i++)
                    tstamps[i] = reader.ReadInt32();

                #endregion

                // parse header information
                #region Offset parse

                Thread offsetCalcThread = new Thread(new ThreadStart(() =>
                {
                    int sector = 0;

                    lock (sectors)
                        for (int i = 0; i < 1024; i++)
                        {
                            sector = EndiannessConverter.ToInt32(sectors[i]);

                            region.offsets[i] = new MCROffset()
                            {
                                SectorSize = (byte)(sector & 0xFF), // get the sector size of the chunk
                                SectorOffset = sector >> 8,         // get the sector offset of the chunk
                            };
                        }
                }));
                offsetCalcThread.Name = "offset calculator thread";
                offsetCalcThread.Start();

                #endregion

                #region Timestamp parse
                Thread tstampCalcThread = new Thread(new ThreadStart(() =>
                {
                    int tstamp = 0;

                    lock (tstamps)
                        for (int i = 0; i < 1024; i++)
                        {
                            tstamp = EndiannessConverter.ToInt32(tstamps[i]);

                            region.tstamps[i] = new MCRTStamp()
                            {
                                Timestamp = tstamp
                            };
                        }
                }));
                tstampCalcThread.Name = "timestamp calculator thread";
                tstampCalcThread.Start();

                #endregion

                tstampCalcThread.Join();
                offsetCalcThread.Join();

                // read chunks from disk
                #region Chunk IO read

                byte[][] chunkBuffer = new byte[sectors.Length][];
                {
                    int         length;
                    MCROffset   offset;

                    for (int i = 0; i < 1024; i++)
                    {
                        offset = region.offsets[i];

                        if (offset.SectorOffset > 0)
                        {
                            stream.Seek(offset.SectorOffset * 4096, SeekOrigin.Begin);

                            length = EndiannessConverter.ToInt32(reader.ReadInt32());
                            reader.ReadByte();

                            chunkBuffer[i] = reader.ReadBytes(length - 1);
                        }
                    }
                }

                #endregion

                // parse chunk information
                #region Parse chunks

                int chunkSlice = 1024 / MaxTHREADS;
                Thread[] workerThreads = new Thread[MaxTHREADS];
                {
                    for (int i = 0; i < MaxTHREADS; i++)
                    {
                        int index = i;

                        workerThreads[i] = new Thread(new ThreadStart(() =>
                        {
                            int offset = index * (1024 / MaxTHREADS);

                            MemoryStream mmStream = null;

                            for (int n = offset; n < (chunkSlice + offset); n++)
                            {
                                byte[] chunk = chunkBuffer[n];

                                if (chunk == null)
                                    continue;

                                using (mmStream = new MemoryStream(chunk))
                                    region.chunks[n - offset] = NBTFile.OpenFile(mmStream, 2);
                            }
                        }));
                        workerThreads[i].Start();
                    }

                    for (int i = 0; i < workerThreads.Length; i++)
                        workerThreads[i].Join();
                }

                #endregion
            }
            
            return region;
        }
    }
}
