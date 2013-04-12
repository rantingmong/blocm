/*  Minecraft NBT reader
 * 
 *  Copyright 2010-2013 Michael Ong, all rights reserved.
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
using System.IO;
using System.Threading;

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
	    public const				int			MaxThreads	= 8;

	    /// <summary>
	    /// The content of the region file.
	    /// </summary>
	    public						NbtFile[]	Content		{ get; private set; }

	    private readonly			Offset[]    offsets;
		private readonly			TimeStamp[]	timeStamps;

        /// <summary>
        /// Gets a chunk from this region.
        /// </summary>
        /// <param name="point">The location of the chunk.</param>
        /// <returns>An NBT file that has the </returns>
        public  NbtFile             this        [Point point]
        {
            get
            {
                return this.Content[point.X + point.Y * 32];
            }
            set
            {
                InsertChunk(point, value);
            }
        }

        private                     RegionFile  ()
        {
            Content        = new NbtFile   [1024];
            
            offsets        = new Offset [1024];
            timeStamps     = new TimeStamp [1024];
        }

        /// <summary>
        /// Inserts/replaces a new chunk on a specified location.
        /// </summary>
        /// <param name="location">The region location of the chunk.</param>
        /// <param name="chunk">The chunk to be added.</param>
        public  void                InsertChunk (Point location, NbtFile chunk)
        {
            int offset = location.X + (location.Y * 32);

            Content[offset] = chunk;
        }
        /// <summary>
        /// Removes a chunk on a specified location.
        /// </summary>
        /// <param name="location">The region location of the chunk to be removed.</param>
        public  void                RemoveChunk (Point location)
        {
            int offset = location.X + (location.Y * 32);

            Content[offset] = null;
        }

        /// <summary>
        /// Saves the region file to a stream.
        /// </summary>
        /// <param name="stream">The stream the region file will write to.</param>
        public  void                SaveRegion  (Stream stream)
        {
			//using (BinaryWriter writer = new BinaryWriter(stream))
			//{
			//	// write header information
			//	foreach (var offset in offsets)
			//	{
			//		writer.Write(EndiannessConverter.ToInt16((short) offset.SectorOffset));
			//		writer.Write(EndiannessConverter.ToInt16(offset.SectorSize));
			//	}

			//	foreach (var timeStamp in timeStamps)
			//	{
			//		writer.Write(EndiannessConverter.ToInt32((int) timeStamp.Timestamp));
			//	}

			//	// write chunk information
			//	foreach (var content in Content)
			//	{
			//		content.SaveTag(stream, 2);
			//	}
			//}
        }

        /// <summary>
        /// Opens the region file from a stream.
        /// </summary>
        /// <param name="stream">The stream the region file will read from.</param>
        /// <returns>The parsed region file.</returns>
        public  static RegionFile   OpenRegion  (Stream stream, bool anvil = false)
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

				Thread offsetCalcThread = new Thread(() =>
					{
						lock (sectors)
							for (int i = 0; i < 1024; i++)
							{
								int sector = EndiannessConverter.ToInt32(sectors[i]);

								region.offsets[i] = new Offset
									{
										// get the sector size of the chunk
										SectorSize = (byte)(sector & 0xFF),
										// get the sector offset of the chunk
										SectorOffset = sector >> 8,
									};
							}
					}) { Name = "offset calculator thread" };

				offsetCalcThread.Start();

				#endregion

				#region Timestamp parse

				Thread tstampCalcThread = new Thread(() =>
					{
						lock (tstamps)
							for (int i = 0; i < 1024; i++)
							{
								int tstamp = EndiannessConverter.ToInt32(tstamps[i]);

								region.timeStamps[i] = new TimeStamp
									{
										Timestamp = tstamp
									};
							}
					}) { Name = "timestamp calculator thread" };

				tstampCalcThread.Start();

				#endregion

				tstampCalcThread.Join();
				offsetCalcThread.Join();

				// read content from disk
				#region Chunk IO read

				byte[][] chunkBuffer = new byte[sectors.Length][];
				{
					for (int i = 0; i < 1024; i++)
					{
						Offset offset = region.offsets[i];

						if (offset.SectorOffset <= 0)
							continue;

						stream.Seek(offset.SectorOffset * 4096, SeekOrigin.Begin);
						reader.ReadByte();

						chunkBuffer[i] = reader.ReadBytes(EndiannessConverter.ToInt32(reader.ReadInt32()) - 1);
					}
				}

				#endregion

				// parse chunk information
				#region Parse content

				const int chunkSlice = 1024 / MaxThreads;
				Thread[] workerThreads = new Thread[MaxThreads];
				{
					for (int i = 0; i < MaxThreads; i++)
					{
						int index = i;

						workerThreads[i] = new Thread(() =>
							{
								int offset = index * (1024 / MaxThreads);

								for (int n = offset; n < (chunkSlice + offset); n++)
								{
									byte[] chunk = chunkBuffer[n];

									if (chunk == null)
										continue;

									using (MemoryStream mmStream = new MemoryStream(chunk))
									{
										region.Content[n - offset] = NbtFile.OpenFile(mmStream, 2);
									}
								}
							});

						workerThreads[i].Start();
					}

					foreach (Thread t in workerThreads)
						t.Join();
				}

				#endregion
			}
            
            return region;
        }
    }
}
