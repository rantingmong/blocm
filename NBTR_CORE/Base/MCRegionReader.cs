using System;

using System.Text;

using System.IO;
using System.IO.Compression;

using System.Collections.Generic;

using NBT.Info;
using NBT.Util;

namespace NBT.Base
{
    /// <summary>
    /// Reader for Minecraft region (MCRegion) files.
    /// </summary>
    public class MCRegionReader
    {
        /// <summary>
        /// The (maximum) size of chunks contained in a MCRegion file.
        /// </summary>
        public static int                   REGION_SIZE         = 1024;

        /// <summary>
        /// The starting position where chunk data are written.
        /// </summary>
        public static int                   REGION_SPOS         = MCRegionReader.REGION_SIZE * 4;


        private     BinaryReader            _bread              = null;

        public      BinaryReader            BaseReader
        {
            get { return this._bread; }
        }


        private     int[]                   _offsets            = new int[MCRegionReader.REGION_SIZE];

        private     int[]                   _tStamps            = new int[MCRegionReader.REGION_SIZE];


        /// <summary>
        /// Creates a new Region loader with the specified file location.
        /// </summary>
        /// <param name="streamIn">A stream where the region is located.</param>
        public                              MCRegionReader      (Stream streamIn)
        {
            // if the stream specified is not a FileStream or a MemoryStream
            if (!(streamIn is FileStream || streamIn is MemoryStream))
                throw new InvalidCastException("Stream specified is not a FileStream or a MemoryStream!");

            _bread = new BinaryReader(streamIn);

            _bread.BaseStream.Seek(0, SeekOrigin.Begin);

            for (int i = 0; i < MCRegionReader.REGION_SIZE; i++)
            {
                _offsets[i] = EndianConverter.SwapInt32(_bread.ReadInt32());
            }

            for (int i = 0; i < MCRegionReader.REGION_SIZE; i++)
            {
                _tStamps[i] = EndianConverter.SwapInt32(_bread.ReadInt32());
            }
        }


        /// <summary>
        /// Gets the chunk inside the region with the specified coordinates.
        /// </summary>
        /// <param name="x">The abscissa of the region.</param>
        /// <param name="z">The ordinate of the region.</param>
        /// <returns>Returns a TagNodeListNamed that contains the chunk data, returns null if coordinates does not contain chunk data.</returns>
        public      INBTTag                 GetChunkData        (int x, int z)
        {
            lock(_bread)
                try
                {
                    this._bread.BaseStream.Seek(0, SeekOrigin.Begin);

                    if (IsOutOfBounds(x, z))
                        throw new ArgumentOutOfRangeException();

                    int offset = GetChunkOffset(x, z);

                    if (!IsOffsetHasChunk(x, z))
                        return null;

                    int sectornumber = offset >> 8;
                    int nosofsectors = offset & 0xFF;

                    this._bread.BaseStream.Seek(sectornumber * MCRegionReader.REGION_SPOS, SeekOrigin.Begin);

                    int chunklength     = EndianConverter.SwapInt32(_bread.ReadInt32());
                    int chunkVersion    = _bread.ReadByte();

                    MemoryStream    _chunkMem   = new MemoryStream(_bread.ReadBytes(chunklength - 1));
                    NBTReader       _nbtRead    = new NBTReader(_chunkMem, chunkVersion);

                    try
                    {
                        return _nbtRead.BeginRead();
                    }
                    finally
                    {
                        _chunkMem.Dispose();
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Problem occurred while reading chunk from region.");
                    Console.WriteLine("Exception message: " + ex.Message + "\n");
                    Console.WriteLine("Stack trace:\n" + ex.StackTrace);

                    return null;
                }
        }

        /// <summary>
        /// Gets all the chunk stored inside the region.
        /// </summary>
        /// <returns>Returns the chunks stored inside the chunk.</returns>
        public      INBTTag[][]              GetRegionChunks     ()
        {
            // create a 2D array that will store the chunks
            INBTTag[][] _chunks = new INBTTag[32][];

            // get the chunk location of the chunk with the specified coordinates
            for (int x = 0; x < 32; x++)
            {
                _chunks[x] = new INBTTag[32];

                for (int z = 0; z < 32; z++)
                {
                    _chunks[x][z] = this.GetChunkData(x, z);
                }
            }

            return _chunks;
        }
 

        private     int                     GetChunkOffset      (int x, int z)
        {
            return _offsets[x + z * 32];
        }

        private     bool                    IsOutOfBounds       (int x, int z)
        {
            return x < 0 || x >= 32 || z < 0 || z >= 32;
        }

        private     bool                    IsOffsetHasChunk    (int x, int z)
        {
            return GetChunkOffset(x, z) != 0;
        }
    }
}
