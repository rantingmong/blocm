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
    /// Reader for Named Binary Tags.
    /// </summary>
    public class NBTReader : IDisposable
    {
        private             BinaryReader    _bRead          = null;

        /// <summary>
        /// Base BinaryStream of the reader.
        /// </summary>
        public              BinaryReader    BaseReader
        {
            get { return _bRead; }
        }

        
        /// <summary>
        /// Creates a new NBT reader from a file stream.
        /// </summary>
        /// <param name="streamIn">The stream where the NBT is placed</param>
        public                              NBTReader       (Stream streamIn)
        {
            // if the stream specified is not a FileStream or a MemoryStream
            if (!(streamIn is FileStream || streamIn is MemoryStream))
                throw new InvalidCastException("Stream specified is not a FileStream or a MemoryStream!");

            // decompress the stream
            GZipStream gStream = new GZipStream(streamIn, CompressionMode.Decompress);

            // route the stream to a binary reader
            _bRead = new BinaryReader(gStream);
        }

        /// <summary>
        /// Creates a new NBT reader from a memory stream.
        /// </summary>
        /// <param name="streamIn">The stream where the NBT is placed.</param>
        /// <param name="version">The compression sceme of the NBT file. 1 for gzip and 2 for zlib.</param>
        public                              NBTReader       (Stream streamIn, int version)
        {
            /*  Due to a file specification change on how an application reads a NBT file
             *  (Minecraft maps are now compressed via a zlib deflate stream), this method
             *  provides backwards support for the old gzip decompression stream.
             */

            // if the stream specified is not a FileStream or a MemoryStream
            if (!(streamIn is FileStream || streamIn is MemoryStream))
                throw new InvalidCastException("Stream specified is not a FileStream or a MemoryStream!");

            // if the chunk is gzip compressed
            if (version == 1)
            {
                // decompress the stream
                GZipStream gStream = new GZipStream(streamIn, CompressionMode.Decompress);

                // route the stream to a binary reader
                _bRead = new BinaryReader(streamIn);
            }
            // if the chunk is zlib inflated
            else if (version == 2)
            {
                // a known bug when deflating a zlib stream in C#. For more info, refer to this website: http://www.chiramattel.com/george/blog/2007/09/09/deflatestream-block-length-does-not-match.html
                streamIn.ReadByte();
                streamIn.ReadByte();

                // deflate the stream
                DeflateStream dStream = new DeflateStream(streamIn, CompressionMode.Decompress);

                // route the stream to a binary reader
                _bRead = new BinaryReader(dStream);
            }
        }


        /// <summary>
        /// Reads a single tag node in a NBT reader stream.
        /// </summary>
        /// <param name="inRead">The binary reader in which the NBT is found.</param>
        /// <param name="tagType">The type of data to be read.</param>
        /// <returns>Returns an object that corresponds to its TAG_TYPE (ex: int for TAG_INT, short for TAG_SHORT, etc.)</returns>
        public static       dynamic         Read            (BinaryReader inRead, TagNodeType tagType)
        {
            /*  This method will read the payload of a tag node depending on the TAG_TYPE of the node.
             * 
             *  That is why this method returns a "dynamic" object because the final data type of the
             *  node will only be known during run-time.
             */

            // read the NBT stream depending on the tagType of the node
            switch (tagType)
            {
                case TagNodeType.TAG_END:
                    {
                        return 0;
                    }
                case TagNodeType.TAG_BYTE:
                    {
                        return inRead.ReadByte  ();
                    }
                case TagNodeType.TAG_BYTEA:
                    {
                        return inRead.ReadBytes (Read(inRead, TagNodeType.TAG_INT));
                    }
                case TagNodeType.TAG_SHORT:
                    {
                        return EndianConverter.SwapInt16    (inRead.ReadInt16());
                    }
                case TagNodeType.TAG_INT:
                    {
                        return EndianConverter.SwapInt32    (inRead.ReadInt32());
                    }
                case TagNodeType.TAG_LONG:
                    {
                        return EndianConverter.SwapInt64    (inRead.ReadInt64());
                    }
                case TagNodeType.TAG_SINGLE:
                    {
                        return EndianConverter.SwapSingle   (inRead.ReadSingle());
                    }
                case TagNodeType.TAG_DOUBLE:
                    {
                        return EndianConverter.SwapDouble   (inRead.ReadDouble());
                    }
                case TagNodeType.TAG_STRING:
                    {
                        return Encoding.UTF8.GetString      (inRead.ReadBytes(Read(inRead, TagNodeType.TAG_SHORT)));
                    }
                default:
                    {
                        throw new NotSupportedException("Tag type is not supported by this reader!");
                    }
            }
        } 


        /// <summary>
        /// Disposes any resources used by NBTReader.
        /// </summary>
        public              void            Dispose         ()
        {
            // dispose _bRead (ooh bread! :P)

            _bRead.Dispose();        
        }

        /// <summary>
        /// Parse read the NBT file.
        /// </summary>
        /// <returns>Returns a INBTTAG that contains the values of the NBT file (sequential order).</returns>
        public              INBTTag         BeginRead       ()
        {
            return this.readTagHead();
        }


        private             INBTTag         readTagHead     ()
        {
            /*  As Notch says, a NBT file contains a TYPE, NAME, and PAYLOAD values. This method will read the
             *  first two values of a NBT node. The last one will be handled by the readTagPlod() method.
             */
            
            byte    _tagType = 0;
            string  _tagName = null;

            // read the TAG_TYPE of the file
            _tagType = NBTReader.Read(this._bRead, TagNodeType.TAG_BYTE);

            // check whether the TAG_TYPE of the node is NOT a TAG_END type
            if (_tagType != (byte)TagNodeType.TAG_END)
            {
                // if no, then read the name of the node
                _tagName = NBTReader.Read(this._bRead, TagNodeType.TAG_STRING);
            }
            else
            {
                // if yes, then leave the name blank
                _tagName = "";
            }

            // proceed to reading the value (payload) of the node
            return readTagPlod((TagNodeType)_tagType, _tagName);
        }

        private             INBTTag         readTagPlod     (TagNodeType type, string name)
        {
            /*  There are 3 types of nodes in a NBT file, namely TAG_LIST, TAG_COMPOUND and the base TAG_TYPEs.
             * 
             *  The difference between the data types and TAG_LIST and TAG_COMPOUND is both TAG_LIST and TAG_COMPOUND requires
             *  additional reading methodology to effectively read the whole file.
             *  
             *  First, on a TAG_LIST container type, the nodes are sequentially read WITHOUT the name tag because virtually, it is a
             *  custom data type array.
             *  
             *  Unlike TAG_LISTs, a TAG_COMPOUND container type requires the nodes to be read again by readTagHead() for n times
             *  (listing will only stop if it were to see a TAG_END node) because this container type contains heterogeneous
             *  mix of primitive data types.
             *  
             *  Lastly, if it is a base type data node, it will be directly read by the Read(BinaryReader, TagNodeType) method.
             * 
             *  In a nutshell, this method will read the value (payload) of a node depending on the type of the node. 
             */

            // check the tag type of the node
            switch (type)
            {
                // type is a TAG_LIST
                case TagNodeType.TAG_LIST:
                    {
                        // get the TAG_TYPE of the list
                        byte    _tagType = NBTReader.Read(this._bRead, TagNodeType.TAG_BYTE);

                        // get the number of elements in the list
                        int     _tagCout = NBTReader.Read(this._bRead, TagNodeType.TAG_INT);

                        // create a TagNodeList that will hold the succeeding tag values
                        TagNodeList _assetsList = new TagNodeList(name, (TagNodeType)_tagType);

                        // continuously read the list from index 0 to index _tagCount
                        for (int i = 0; i < _tagCout; i++)
                        {
                            _assetsList.Add((INBTTag)readTagPlod((TagNodeType)_tagType, ""));
                        }

                        return _assetsList;
                    }
                // type is a TAG_COMPOUND
                case TagNodeType.TAG_COMPOUND:
                    {
                        // create a TagNodeList that will hold the succeeding tag values
                        TagNodeListNamed _assetsMaps = new TagNodeListNamed(name);

                        // yes, this is an intentional infinite loop >:)
                        do
                        {
                            // read a tag node
                            INBTTag _nodeMap = readTagHead();

                            // if tag node is not TAG_END, meaning there is more to add
                            if (_nodeMap.Type != TagNodeType.TAG_END)
                            {
                                // add the _nodeMap into the list
                                _assetsMaps.Add(_nodeMap.Name, _nodeMap);
                            }
                            // otherwise
                            else
                            {
                                // break the loop *\o/*
                                break;
                            }
                        } while (true);

                        return _assetsMaps;
                    }
                // tag is a primitive data type
                default:
                    {
                        // read the node according to the type of the node
                        return new TagNode(type, name, Read(this._bRead, type));
                    }
            }
        }
    }
}
