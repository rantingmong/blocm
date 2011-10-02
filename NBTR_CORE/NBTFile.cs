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
using System.Text;

using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.IO.Compression;

using NBT.Tag;
using NBT.Utils;

namespace NBT
{
    /// <summary>
    /// A Minecraft NBT file.
    /// </summary>
    public class NBTFile : IDisposable
    {
        List<NBTTag>                list;
        Dictionary<string, NBTTag>  dict;
        /// <summary>
        /// Gets the contents of this NBT file.
        /// </summary>
        public  ICollection         Contents
        {
            get
            {
                if (named)
                    return dict;
                else
                    return list;
            }
        }

        bool                        named;
        /// <summary>
        /// Gets the list type of this NBT file. 'true' if the file is TAG_COMPOUND, 'false' for TAG_LIST.
        /// </summary>
        public  bool                NamedNBT
        {
            get { return named; }
        }

        string                      rootname;
        /// <summary>
        /// Gets the root name of this NBT file.
        /// </summary>
        public  string              RootName
        {
            get { return this.rootname; }
        }

        int                         count;
        /// <summary>
        /// Returns the total number of elements the root list have.
        /// </summary>
        public  int                 CollectionCount
        {
            get { return this.count; }
        }


        private                     NBTFile     ()
        {
            list        = new List<NBTTag>();
            dict        = new Dictionary<string, NBTTag>();

            named       = false;
            rootname    = "";

            count       = 0;
        }
        /// <summary>
        /// Creates a new NBT file.
        /// </summary>
        /// <param name="named">The list type of this NBT file, 'true' for TAG_COMPOUND, 'false' for TAG_LIST.</param>
        /// <param name="rootname">The root name of this NBT file.</param>
        public                      NBTFile     (bool named, string rootname)
        {
            this.named      = named;
            this.rootname   = rootname;
        }

        /// <summary>
        /// Inserts a new NBT tag in the list.
        /// </summary>
        /// <param name="tag">The tag to be inserted.</param>
        public  void                InsertTag   (NBTTag tag)
        {
            if (named)
                dict.Add(tag.Name, tag);
            else
                list.Add(tag);
        }
        /// <summary>
        /// Removes a exsisting NBT tag in the list.
        /// </summary>
        /// <param name="tag">The tag to be removed.</param>
        public  void                RemoveTag   (NBTTag tag)
        {
            if (named)
                dict.Remove(tag.Name);
            else
                list.Remove(tag);
        }
        /// <summary>
        /// Modifies the a NBT tag in the list.
        /// </summary>
        /// <param name="tag">The tag to be modified.</param>
        /// <param name="info">The new tag to be replaced.</param>
        public  void                ModifyTag   (NBTTag tag, NBTTag info)
        {
            if (named)
                dict[tag.Name] = info;
            else
                list[list.IndexOf(tag)] = info;
        }

        /// <summary>
        /// Saves this NBT file in a stream.
        /// </summary>
        /// <param name="stream">The output stream this NBT file will write onto.</param>
        /// <param name="version">The compression version of the NBT, specify '1' for the original gzip compression, '2' for the mcregion zlib compression.</param>
        public  void                SaveTag     (Stream stream, int version)
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

            BinaryWriter writer = new BinaryWriter(compressStream);
            {
                writer.Write((byte)(this.named ? 10 : 9)); 
                writer.Write(EndianessConverter.ToInt16((short)this.rootname.Length));

                byte[] oString = Encoding.UTF8.GetBytes(this.rootname);

                for (int i = 0; i < oString.Length; i++)
                {
                    writer.Write(oString[i]);
                }

                if (this.named)
                {
                    foreach (KeyValuePair<string, NBTTag> tag in this.dict)
                    {
                        writer.Write(tag.Value.Type);
                        writer.Write(EndianessConverter.ToInt16((short)tag.Value.Name.Length));

                        oString = Encoding.UTF8.GetBytes(tag.Value.Name);

                        for (int i = 0; i < oString.Length; i++)
                        {
                            writer.Write(oString[i]);
                        }

                        SavePayload(ref writer, tag.Value.Type, tag.Value.Payload);
                    }
                    writer.Write((byte)0);
                }
                else
                {
                    writer.Write(this.list[0].Type);
                    writer.Write(EndianessConverter.ToInt32(this.list.Count));

                    for (int i = 0; i < this.list.Count; i++)
                    {
                        SavePayload(ref writer, this.list[0].Type, this.list[i].Payload);
                    }
                }
            }
            writer.Close();
            writer.Dispose();

            writer = null;

            compressStream.Close();
            compressStream.Dispose();

            compressStream = null;
        }

        /// <summary>
        /// Opens an exsisting NBT file from a stream.
        /// </summary>
        /// <param name="stream">The stream to get the NBT file from.</param>
        /// <param name="version">The compression version of the NBT, specify '1' for the original gzip compression, '2' for the mcregion zlib compression.</param>
        /// <returns>An opened NBT file.</returns>
        public  static NBTFile      OpenFile    (Stream stream, int version)
        {
            NBTFile file = new NBTFile();

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

            BinaryReader reader = new BinaryReader(compressStream);
            {
                Encoding textEncoding = Encoding.UTF8;

                file.named      = reader.ReadByte() == 10;
                file.rootname   = textEncoding.GetString(reader.ReadBytes(EndianessConverter.ToInt16(reader.ReadInt16())));

                if (file.named)
                {
                    byte    type;
                    string  name;

                    while ((type = reader.ReadByte()) != 0)
                    {
                        name = textEncoding.GetString(reader.ReadBytes(EndianessConverter.ToInt16(reader.ReadInt16())));

                        file.InsertTag(new NBTTag(name, type, file.ReadPayload(ref reader, type)));
                    }
                }
                else
                {
                    byte    type = reader.ReadByte();
                    int     size = EndianessConverter.ToInt32(reader.ReadInt32());

                    for (int i = 0; i < size; i++)
                    {
                        file.InsertTag(new NBTTag("", type, file.ReadPayload(ref reader, type)));
                    }
                }
            }
            reader.Close();
            reader.Dispose();

            reader = null;

            compressStream.Close();
            compressStream.Dispose();

            compressStream = null;

            return file;
        }

        private dynamic             ReadPayload (ref BinaryReader reader, byte type)
        {
            switch (type)
            {
                case 0:
                    return 0;
                case 1:
                    return reader.ReadByte();
                case 2:
                    return EndianessConverter.ToInt16(reader.ReadInt16());
                case 3:
                    return EndianessConverter.ToInt32(reader.ReadInt32());
                case 4:
                    return EndianessConverter.ToInt64(reader.ReadInt64());
                case 5:
                    return EndianessConverter.ToSingle(reader.ReadSingle());
                case 6:
                    return EndianessConverter.ToDouble(reader.ReadDouble());
                case 7:
                    return reader.ReadBytes(EndianessConverter.ToInt32(reader.ReadInt32()));
                case 8:
                    return Encoding.UTF8.GetString(reader.ReadBytes(EndianessConverter.ToInt16(reader.ReadInt16())));
                case 9:
                    {
                        List<NBTTag> ret = new List<NBTTag>();
                        {
                            byte    containerType = reader.ReadByte();
                            int     containerSize = EndianessConverter.ToInt32(reader.ReadInt32());

                            for (int i = 0; i < containerSize; i++)
                                ret.Add(new NBTTag("", containerType, ReadPayload(ref reader, containerType)));
                        }
                        return ret;
                    }
                case 10:
                    {
                        Dictionary<string, NBTTag> dic = new Dictionary<string, NBTTag>();
                        {
                            byte    containerType;
                            string  containerName;

                            while ((containerType = reader.ReadByte()) != 0)
                            {
                                containerName = Encoding.UTF8.GetString(reader.ReadBytes(EndianessConverter.ToInt16(reader.ReadInt16())));

                                dic.Add(containerName, new NBTTag(containerName, containerType, ReadPayload(ref reader, containerType)));
                            }
                        }
                        return dic;
                    }
                default:
                    throw new NotSupportedException("Tag type is invalid!");
            }
        }
        private void                SavePayload (ref BinaryWriter writer, byte type, dynamic payload)
        {
            switch (type)
            {
                case 0:
                    writer.Write((byte)0);
                    break;
                case 1:
                    writer.Write((byte)payload);
                    break;
                case 2:
                    writer.Write(EndianessConverter.ToInt16(payload));
                    break;
                case 3:
                    writer.Write(EndianessConverter.ToInt32(payload));
                    break;
                case 4:
                    writer.Write(EndianessConverter.ToInt64(payload));
                    break;
                case 5:
                    writer.Write(EndianessConverter.ToSingle(payload));
                    break;
                case 6:
                    writer.Write(EndianessConverter.ToDouble(payload));
                    break;
                case 7:
                    writer.Write(EndianessConverter.ToInt32(payload.Length));

                    for (int i = 0; i < payload.Length; i++)
                    {
                        writer.Write(payload[i]);
                    }
                    break;
                case 8:
                    writer.Write(EndianessConverter.ToInt16((short)payload.Length));

                    byte[] oString = Encoding.UTF8.GetBytes(payload);

                    for (int i = 0; i < oString.Length; i++)
                    {
                        writer.Write(oString[i]);
                    }
                    break;
                case 9:

                    writer.Write(payload[0].Type);
                    writer.Write(EndianessConverter.ToInt32(payload.Count));

                    foreach (NBTTag tag in payload)
                    {
                        SavePayload(ref writer, tag.Type, tag.Payload);
                    }

                    break;
                case 10:

                    foreach (KeyValuePair<string, NBTTag> tag in payload)
                    {
                        writer.Write(tag.Value.Type);
                        writer.Write(EndianessConverter.ToInt16((short)tag.Key.Length));

                        byte[] cString = Encoding.UTF8.GetBytes(tag.Key);

                        for (int i = 0; i < cString.Length; i++)
                        {
                            writer.Write(cString[i]);
                        }

                        SavePayload(ref writer, tag.Value.Type, tag.Value.Payload);
                    }
                    writer.Write((byte)0);

                    break;
            }
        }

        /// <summary>
        /// Cleans up any resources this file used.
        /// </summary>
        public  void                Dispose     ()
        {
            this.dict.Clear();
            this.list.Clear();

            this.dict = null;
            this.list = null;
        }
    }
}
