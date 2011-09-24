/*  Minecraft NBT reader
 * 
 *  Copyright 2011 Michael Ong, all rights reserved.
 *  
 *  Any part of this code is governed by the GNU General Public License version 2.
 */
namespace NBT
{
    using System;
    using System.Collections.Generic;

    using System.IO;
    using System.IO.Compression;

    using System.Text;

    using System.Threading;

    using NBT.Tag;
    using NBT.Utils;

    public class NBTFile
    {
        #region FIELDS

        bool                        named       = false;

        List<NBT_Tag>               list        = null;
        Dictionary<string, NBT_Tag> dict        = null;

        string                      nbtName     = "null";
        /// <summary>
        /// Obtains the name of this NBT tag.
        /// </summary>
        public string               NBTName
        {
            get { return this.nbtName; }
        }

        /// <summary>
        /// Obtains the total tag count of this NBT file.
        /// </summary>
        public int                  Count
        {
            get
            {
                if (named)
                    return this.dict.Count;
                else
                    return this.list.Count;
            }
        }

        #endregion

        #region CTOR

        private                     NBTFile     ()
        {

        }

        /// <summary>
        /// Creates a NBT container.
        /// </summary>
        /// <param name="named">True for TAG_LIST, False for TAG_COMPOUND.</param>
        public                      NBTFile     (bool named, string name)
        {
            this.named = named;
            this.nbtName = name;

            if (!named)
            {
                this.list = new List<NBT_Tag>();
            }
            else
            {
                this.dict = new Dictionary<string, NBT_Tag>();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Writes the data currently this data has to a specified stream.
        /// </summary>
        /// <param name="stream">The stream to flush data on.</param>
        /// <returns>Returns true if the operation is successful, otherwise false.</returns>
        public  bool            Save        (Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(this.named ? 10 : 9);
                writer.Write(this.nbtName);
            }

            return true;
        }

        /// <summary>
        /// Finds a NBT tag in an unnamed list.
        /// </summary>
        /// <param name="index">The index of the tag.</param>
        /// <returns>Returns the tag given the index specified, otherwise it will return null.</returns>
        public  NBT_Tag         FindTag     (int index)
        {
            if (!this.named && this.list.Count >= (index + 1))
                return this.list[index];
            else
                return null;
        }
        /// <summary>
        /// Finds a NBT tag in a named list.
        /// </summary>
        /// <param name="name">The name of the tag.</param>
        /// <returns>Returns the tag given the name specified, otherwise it will return null.</returns>
        public  NBT_Tag         FindTag     (string name)
        {
            if (this.named && this.dict.ContainsKey(name))
                return this.dict[name];
            else
                return null;
        }

        /// <summary>
        /// Inserts a NBT tag after the last item of an unnamed or named list.
        /// </summary>
        /// <param name="info">The NBT tag to be added.</param>
        public  void            InsertTag   (NBT_Tag info)
        {
            if (this.named)
                this.dict.Add(info.TagName, info);
            else
                this.list.Add(info);
        }

        /// <summary>
        /// Removes the NBT tag at a specified index.
        /// </summary>
        /// <param name="index">The index of the tag to be removed.</param>
        public  void            RemoveTag   (int index)
        {
            if (!this.named)
                this.list.RemoveAt(index);
        }
        /// <summary>
        /// Removes the NBT tag given a specified key.
        /// </summary>
        /// <param name="name">The key of the tag to be removed.</param>
        public  void            RemoveTag   (string name)
        {
            if (this.named)
                this.dict.Remove(name);
        }

        #endregion

        #region STATIC METHODS

        /// <summary>
        /// Creates and opens a NBT file.
        /// </summary>
        /// <param name="stream">A stream where the NBT file is stored.</param>
        /// <param name="version">Specify '1' for the original GZip compression, 2 for zlib compression.</param>
        /// <returns>A NBT file that can be read.</returns>
        public  static NBTFile  OpenNBT     (Stream stream, int version)
        {
            NBTFile file = new NBTFile();

            Stream compressStream = null;

            try
            {
                // check the compression version of this NBT file 2 for zlib, 1 for gzip
                if (version == 2)
                {
                    stream.ReadByte();
                    stream.ReadByte();

                    compressStream = new DeflateStream(stream, CompressionMode.Decompress);
                }
                // this part is 'else'ed because i don't wany any a-hole to stupidly specify a value other than 1 or 2
                else
                {
                    compressStream = new GZipStream(stream, CompressionMode.Decompress);
                }

                BinaryReader reader = new BinaryReader(compressStream);
                {
                    Encoding textEncoding = Encoding.UTF8;

                    // read the initial type and name of the file
                    file.named      = reader.ReadByte() == 10;
                    file.nbtName    = textEncoding.GetString(reader.ReadBytes(EndianessConverter.ToInt16(reader.ReadInt16())));

                    // if the file is TAG_COMPOUND
                    if (file.named)
                    {
                        file.dict = new Dictionary<string, NBT_Tag>();

                        byte    tagType;
                        string  tagName;

                        // read the children tags of this file
                        while ((tagType = reader.ReadByte()) != 0)
                        {
                            tagName = textEncoding.GetString(reader.ReadBytes(EndianessConverter.ToInt16(reader.ReadInt16())));

                            file.InsertTag(new NBT_Tag(tagName, (TagType)tagType, file.ReadPayload(ref reader, tagType)));
                        }
                    }
                    // if the file is TAG_LIST
                    else
                    {
                        file.list = new List<NBT_Tag>();

                        byte    tagType = reader.ReadByte();
                        int     tagSize = EndianessConverter.ToInt32(reader.ReadInt32());

                        // read the children tags of this file
                        for (int i = 0; i < tagSize; i++)
                        {
                            file.InsertTag(new NBT_Tag("", (TagType)tagType, file.ReadPayload(ref reader, tagType)));
                        }
                    }
                }
                reader.Close();
                reader.Dispose();

                return file;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                compressStream.Close();
                compressStream.Dispose();
            }
        }

        #endregion

        private dynamic         ReadPayload (ref BinaryReader reader, byte type)
        {
            switch (type)
            {
                case 0:     //TAG_END
                    return null;
                case 1:     //TAG_BYTE
                    return reader.ReadByte();
                case 2:     //TAG_SHORT
                    return EndianessConverter.ToInt16(reader.ReadInt16());
                case 3:     //TAG_INT
                    return EndianessConverter.ToInt32(reader.ReadInt32());
                case 4:     //TAG_LONG
                    return EndianessConverter.ToInt64(reader.ReadInt64());
                case 5:     //TAG_FLOAT
                    return EndianessConverter.ToSingle(reader.ReadSingle());
                case 6:     //TAG_DOUBLE
                    return EndianessConverter.ToDouble(reader.ReadDouble());
                case 7:     //TAG_BYTE_ARRAY
                    return reader.ReadBytes(EndianessConverter.ToInt32(reader.ReadInt32()));
                case 8:     //TAG_STRING
                    return Encoding.UTF8.GetString(reader.ReadBytes(EndianessConverter.ToInt16(reader.ReadInt16())));
                case 9:     //TAG_LIST
                    {
                        NBT_List list = new NBT_List();

                        byte    tagType = reader.ReadByte();
                        int     tagSize = EndianessConverter.ToInt32(reader.ReadInt32());

                        for (int i = 0; i < tagSize; i++)
                        {
                            list.Add(new NBT_Tag("", (TagType)tagType, ReadPayload(ref reader, tagType)));
                        }

                        return list;
                    }
                case 10:    //TAG_COMPOUND
                    {
                        NBT_Compound dict = new NBT_Compound();

                        byte    tagType;
                        string  tagName;

                        while ((tagType = reader.ReadByte()) != 0)
                        {
                            tagName = Encoding.UTF8.GetString(reader.ReadBytes(EndianessConverter.ToInt16(reader.ReadInt16())));

                            dict.Add(tagName, new NBT_Tag(tagName, (TagType)tagType, ReadPayload(ref reader, tagType)));
                        }

                        return dict;
                    }
                default:
                    throw new NotSupportedException("Tag type not supported!");
            }
        }
    }
}
