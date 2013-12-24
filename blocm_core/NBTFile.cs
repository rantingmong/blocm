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

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using NBT.Utils;

namespace NBT
{
    /// <summary>
    ///     A Minecraft NBT file.
    /// </summary>
    public class NbtFile : IDisposable
    {
        private Dictionary<string, NbtTag> dict;
        private List<NbtTag> list;

        private NbtFile()
        {
            list = new List<NbtTag>();
            dict = new Dictionary<string, NbtTag>();

            NamedNbt = false;
            RootName = "";
        }

        /// <summary>
        ///     Creates a new NBT file.
        /// </summary>
        /// <param name="named">The list type of this NBT file, 'true' for TAG_COMPOUND, 'false' for TAG_LIST.</param>
        /// <param name="rootname">The root name of this NBT file.</param>
        public NbtFile(bool named, string rootname)
        {
            NamedNbt = named;
            RootName = rootname;

            list = new List<NbtTag>();
            dict = new Dictionary<string, NbtTag>();
        }

        /// <summary>
        ///     Gets the contents of this NBT file.
        /// </summary>
        public ICollection Contents
        {
            get
            {
                if (NamedNbt)
                    return dict;

                return list;
            }
        }

        /// <summary>
        ///     Gets the list type of this NBT file. 'true' if the file is TAG_COMPOUND, 'false' for TAG_LIST.
        /// </summary>
        public bool NamedNbt { get; private set; }

        /// <summary>
        ///     Gets the root name of this NBT file.
        /// </summary>
        public string RootName { get; private set; }

        /// <summary>
        ///     Gets a NBT tag on a specified index (TAG_LIST only).
        /// </summary>
        /// <param name="index">The index of the tag.</param>
        /// <returns>The NBTTag of that index.</returns>
        public NbtTag this[int index]
        {
            get { return list[index]; }
        }

        /// <summary>
        ///     Gets a NBT tag on a specified key (TAG_COMPOUND only).
        /// </summary>
        /// <param name="name">The key of the tag.</param>
        /// <returns>The NBTTag of that index.</returns>
        public NbtTag this[string name]
        {
            get { return dict[name]; }
        }

        /// <summary>
        ///     Cleans up any resources this file used.
        /// </summary>
        public void Dispose()
        {
            dict.Clear();
            list.Clear();

            dict = null;
            list = null;
        }

        /// <summary>
        ///     Inserts a new NBT tag in the list.
        /// </summary>
        /// <param name="tag">The tag to be inserted.</param>
        public void InsertTag(NbtTag tag)
        {
            if (NamedNbt)
                dict.Add(tag.Name, tag);
            else
                list.Add(tag);
        }

        /// <summary>
        ///     Removes a existing NBT tag in the list.
        /// </summary>
        /// <param name="tag">The tag to be removed.</param>
        public void RemoveTag(NbtTag tag)
        {
            if (NamedNbt)
                dict.Remove(tag.Name);
            else
                list.Remove(tag);
        }

        /// <summary>
        ///     Modifies the a NBT tag in the list.
        /// </summary>
        /// <param name="tag">The tag to be modified.</param>
        /// <param name="info">The new tag to be replaced.</param>
        public void ModifyTag(NbtTag tag, NbtTag info)
        {
            if (NamedNbt)
                dict[tag.Name] = info;
            else
                list[list.IndexOf(tag)] = info;
        }

        /// <summary>
        /// Gets the size of this NBT file.
        /// </summary>
        /// <returns>The size of the NBT file in bytes.</returns>
        public int Size()
        {
            return NamedNbt ? dict.Sum(tag => tag.Value.Size()) : list.Sum(tag => tag.Size());
        }

        /// <summary>
        ///     Opens an existing NBT file from a stream.
        /// </summary>
        /// <param name="stream">The stream to get the NBT file from.</param>
        /// <param name="version">The compression version of the NBT, specify '1' for the original gzip compression, '2' for the mcregion zlib compression.</param>
        /// <returns>An opened NBT file.</returns>
        public static NbtFile OpenTag(Stream stream, int version)
        {
            var file = new NbtFile();

            Stream compressStream;

            if (version == 1)
            {
                compressStream = new GZipStream(stream, CompressionMode.Decompress);
            }
            else
            {
                stream.ReadByte();
                stream.ReadByte();

                compressStream = new DeflateStream(stream, CompressionMode.Decompress);
            }

            var reader = new BinaryReader(compressStream);
            {
                Encoding textEncoding = Encoding.UTF8;

                file.NamedNbt = reader.ReadByte() == 10;
                file.RootName = textEncoding.GetString(reader.ReadBytes(EndiannessConverter.ToInt16(reader.ReadInt16())));

                if (file.NamedNbt)
                {
                    byte type;

                    while ((type = reader.ReadByte()) != 0)
                    {
                        file.InsertTag(new NbtTag(textEncoding.GetString(reader.ReadBytes(EndiannessConverter.ToInt16(reader.ReadInt16()))), (NbtTagType)type, ReadPayload(ref reader, type)));
                    }
                }
                else
                {
                    var type = reader.ReadByte();
                    var size = EndiannessConverter.ToInt32(reader.ReadInt32());

                    for (int i = 0; i < size; i++)
                    {
                        file.InsertTag(new NbtTag("", (NbtTagType)type, ReadPayload(ref reader, type)));
                    }
                }
            }

            reader.Dispose();
            compressStream.Dispose();

            return file;
        }

        /// <summary>
        ///     Saves this NBT file to a stream.
        /// </summary>
        /// <param name="stream">The output stream this NBT file will write onto.</param>
        /// <param name="version">The compression version of the NBT, specify '1' for the original gzip compression, '2' for the mcregion zlib compression.</param>
        /// <param name="file">The NBT file to save</param>
        public static void SaveTag(Stream stream, int version, NbtFile file)
        {
            Stream compressStream;

            if (version == 1)
            {
                compressStream = new GZipStream(stream, CompressionMode.Compress);
            }
            else
            {
                stream.WriteByte(0);
                stream.WriteByte(0);

                compressStream = new DeflateStream(stream, CompressionMode.Compress);
            }

            var writer = new BinaryWriter(compressStream);
            {
                writer.Write((byte)(file.NamedNbt ? 10 : 9));
                writer.Write(EndiannessConverter.ToInt16((short)file.RootName.Length));

                byte[] oString = Encoding.UTF8.GetBytes(file.RootName);

                foreach (byte t in oString)
                {
                    writer.Write(t);
                }

                if (file.NamedNbt)
                {
                    foreach (var tag in file.dict)
                    {
                        writer.Write((byte)tag.Value.Type);
                        writer.Write(EndiannessConverter.ToInt16((short)tag.Value.Name.Length));

                        oString = Encoding.UTF8.GetBytes(tag.Value.Name);

                        foreach (byte t in oString)
                        {
                            writer.Write(t);
                        }

                        SavePayload(ref writer, tag.Value.Type, tag.Value.Payload);
                    }
                    writer.Write((byte)0);
                }
                else
                {
                    writer.Write((byte)file.list[0].Type);
                    writer.Write(EndiannessConverter.ToInt32(file.list.Count));

                    foreach (NbtTag t in file.list)
                    {
                        SavePayload(ref writer, file.list[0].Type, t.Payload);
                    }
                }
            }
            writer.Dispose();
            compressStream.Dispose();
        }

        private static object ReadPayload(ref BinaryReader reader, byte type)
        {
            switch (type)
            {
            case 0:
                return 0;
            case 1:
                return reader.ReadByte();
            case 2:
                return EndiannessConverter.ToInt16(reader.ReadInt16());
            case 3:
                return EndiannessConverter.ToInt32(reader.ReadInt32());
            case 4:
                return EndiannessConverter.ToInt64(reader.ReadInt64());
            case 5:
                return EndiannessConverter.ToSingle(reader.ReadSingle());
            case 6:
                return EndiannessConverter.ToDouble(reader.ReadDouble());
            case 7:
                return reader.ReadBytes(EndiannessConverter.ToInt32(reader.ReadInt32()));
            case 8:
                return Encoding.UTF8.GetString(reader.ReadBytes(EndiannessConverter.ToInt16(reader.ReadInt16())));
            case 9:
                {
                    var ret = new List<NbtTag>();
                    {
                        byte containerType = reader.ReadByte();
                        int containerSize = EndiannessConverter.ToInt32(reader.ReadInt32());

                        for (int i = 0; i < containerSize; i++)
                            ret.Add(new NbtTag("", (NbtTagType)containerType, ReadPayload(ref reader, containerType)));
                    }
                    return ret;
                }
            case 10:
                {
                    var dic = new Dictionary<string, NbtTag>();
                    {
                        byte containerType;

                        while ((containerType = reader.ReadByte()) != 0)
                        {
                            string containerName = Encoding.UTF8.GetString(reader.ReadBytes(EndiannessConverter.ToInt16(reader.ReadInt16())));
                            dic.Add(containerName, new NbtTag(containerName, (NbtTagType)containerType, ReadPayload(ref reader, containerType)));
                        }
                    }
                    return dic;
                }
            case 11:

                int length = EndiannessConverter.ToInt32(reader.ReadInt32());
                var intList = new int[length];

                for (int i = 0; i < length; i++)
                {
                    intList[i] = EndiannessConverter.ToInt32(reader.ReadInt32());
                }

                return intList;
            default:
                throw new NotSupportedException("Tag type is invalid!");
            }
        }
        
        private static void SavePayload(ref BinaryWriter writer, NbtTagType type, object payload)
        {
            switch (type)
            {
            case NbtTagType.End:
                writer.Write((byte)0);
                break;
            case NbtTagType.Byte:
                writer.Write((byte)payload);
                break;
            case NbtTagType.Short:
                writer.Write(EndiannessConverter.ToInt16((short)payload));
                break;
            case NbtTagType.Int:
                writer.Write(EndiannessConverter.ToInt32((int)payload));
                break;
            case NbtTagType.Long:
                writer.Write(EndiannessConverter.ToInt64((long)payload));
                break;
            case NbtTagType.Float:
                writer.Write(EndiannessConverter.ToSingle((float)payload));
                break;
            case NbtTagType.Double:
                writer.Write(EndiannessConverter.ToDouble((double)payload));
                break;
            case NbtTagType.ByteArray:
                {
                    var pload = (int[])payload;

                    writer.Write(EndiannessConverter.ToInt32(pload.Length));

                    foreach (var t in pload)
                    {
                        writer.Write(t);
                    }
                }
                break;
            case NbtTagType.String:
                {
                    var pload = (string)payload;

                    writer.Write(EndiannessConverter.ToInt16((short)pload.Length));

                    var oString = Encoding.UTF8.GetBytes(pload);
                    foreach (var t in oString)
                    {
                        writer.Write(t);
                    }
                }
                break;
            case NbtTagType.List:
                {
                    var pload = (List<NbtTag>)payload;

                    writer.Write((byte)pload[0].Type);
                    writer.Write(EndiannessConverter.ToInt32(pload.Count));

                    foreach (var tag in pload)
                    {
                        SavePayload(ref writer, tag.Type, tag.Payload);
                    }
                }
                break;
            case NbtTagType.Compound:
                {
                    foreach (var tag in (Dictionary<string, NbtTag>)payload)
                    {
                        writer.Write((byte)tag.Value.Type);

                        SavePayload(ref writer, NbtTagType.String, tag.Key);
                        SavePayload(ref writer, tag.Value.Type, tag.Value.Payload);
                    }
                    
                    SavePayload(ref writer, NbtTagType.End, null);
                }
                break;
            case NbtTagType.IntArray:
                break;
            }
        }
    }
}
