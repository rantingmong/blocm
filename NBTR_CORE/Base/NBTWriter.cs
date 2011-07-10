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
    /// Writer for Named Binary Tags.
    /// </summary>
    public class NBTWriter : IDisposable
    {
        private         BinaryWriter    _bWrite;

        public          BinaryWriter    BaseWriter
        {
            get { return this._bWrite; }
        }


        public                          NBTWriter       (Stream streamOut)
        {
            if (!(streamOut is FileStream || streamOut is MemoryStream))
                throw new InvalidCastException("Stream specified is not a FileStream or a MemoryStream!");

            GZipStream _gzStream = new GZipStream(streamOut, CompressionMode.Compress);

            _bWrite = new BinaryWriter(_gzStream);
        }

        public                          NBTWriter       (Stream streamOut, int version)
        {
            if (!(streamOut is FileStream || streamOut is MemoryStream))
                throw new InvalidCastException("Stream specified is not a FileStream or a MemoryStream!");

            if (version == 1)
            {
                GZipStream gzStream = new GZipStream(streamOut, CompressionMode.Compress);

                this._bWrite = new BinaryWriter(gzStream);   
            }
            else if (version == 2)
            {
                streamOut.WriteByte(0);
                streamOut.WriteByte(0);

                DeflateStream dfStream = new DeflateStream(streamOut, CompressionMode.Compress);

                this._bWrite = new BinaryWriter(dfStream);
            }
        }


        public static   void            WritePayload    (BinaryWriter bWrite, dynamic payload, TagNodeType type)
        {
            switch (type)
            {
                case TagNodeType.TAG_END:
                    {
                        bWrite.Write(0);
                    }
                    break;
                case TagNodeType.TAG_BYTE:
                    {
                        bWrite.Write((byte)payload);
                    }
                    break;
                case TagNodeType.TAG_BYTEA:
                    {
                        WritePayload(bWrite, ((byte[])payload).Length, TagNodeType.TAG_INT);

                        for (int i = 0; i < ((byte[])payload).Length; i++)
                        {
                            WritePayload(bWrite, ((byte[])payload)[i], TagNodeType.TAG_BYTE);
                        }
                    }
                    break;
                case TagNodeType.TAG_SHORT:
                    {
                        bWrite.Write(EndianConverter.SwapInt16((short)payload));
                    }
                    break;
                case TagNodeType.TAG_INT:
                    {
                        bWrite.Write(EndianConverter.SwapInt32((int)payload));
                    }
                    break;
                case TagNodeType.TAG_LONG:
                    {
                        bWrite.Write(EndianConverter.SwapInt64((long)payload));
                    }
                    break;
                case TagNodeType.TAG_SINGLE:
                    {
                        bWrite.Write(EndianConverter.SwapSingle((float)payload));
                    }
                    break;
                case TagNodeType.TAG_DOUBLE:
                    {
                        bWrite.Write(EndianConverter.SwapDouble((double)payload));
                    }
                    break;
                case TagNodeType.TAG_STRING:
                    {
                        WritePayload(bWrite, ((string)payload).Length, TagNodeType.TAG_SHORT);

                        byte[] _outString = Encoding.UTF8.GetBytes(payload);

                        for (int i = 0; i < ((string)payload).Length; i++)
                            WritePayload(bWrite, _outString[i], TagNodeType.TAG_BYTE);
                    }
                    break;
                default:
                    {
                        throw new NotSupportedException("Tag type is not supported by this reader!");
                    }
            }
        }

                                                
        public          void            Dispose         ()
        {
            _bWrite.Dispose();
        }

        public          void            BeginWrite      (INBTTag tagIn)
        {
            WritePayload(this._bWrite, tagIn.Type, TagNodeType.TAG_BYTE);


            if (tagIn.Type != TagNodeType.TAG_END)
            {
                WritePayload(this._bWrite, tagIn.Name, TagNodeType.TAG_STRING);

                if (tagIn is TagNode)
                {
                    WritePayload(this._bWrite, tagIn.Payload, tagIn.Type);
                }
                else if (tagIn is TagNodeList)
                {
                    WritePayload(this._bWrite, ((TagNodeList)tagIn).ChildType,  TagNodeType.TAG_BYTE);
                    WritePayload(this._bWrite, ((TagNodeList)tagIn).Count,      TagNodeType.TAG_INT);

                    foreach (INBTTag node in (TagNodeList)tagIn)
                    {
                        WritePayload(this._bWrite, node.Payload, node.Type);
                    }
                }
                else if (tagIn is TagNodeListNamed)
                {
                    foreach (INBTTag node in ((TagNodeListNamed)tagIn).Values)
                    {
                        if (node.Type != TagNodeType.TAG_END)
                            BeginWrite(node);
                    }

                    this._bWrite.Write(0);
                }
            }
        }
    }
}
