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
using System.Collections.Generic;
using System.Linq;

namespace NBT
{
    /// <summary>
    /// NBT tag types.
    /// </summary>
    public enum  NbtTagType
    {
        Invalid = -1,
        End = 0,
        Byte = 1,
        Short = 2,
        Int = 3,
        Long = 4,
        Float = 5,
        Double = 6,
        ByteArray = 7,
        String = 8,
        List = 9,
        Compound = 10,
        IntArray = 11
    }

    /// <summary>
    ///     A NBT tag, the building blocks of a NBT file.
    /// </summary>
    public struct NbtTag
    {
        /// <summary>
        ///     Creates a new NBT tag.
        /// </summary>
        /// <param name="name">The name of the tag.</param>
        /// <param name="type">The type of the tag.</param>
        /// <param name="payload">The payload of the tag.</param>
        public NbtTag(string name, NbtTagType type, object payload) : this()
        {
            Payload = payload;
            Name = name;
            Type = type;

            var error = false;

            switch (type)
            {
            case NbtTagType.End:
                if (payload.GetType() != typeof (byte) && (byte)payload != 0)
                    error = true;
                break;
            case NbtTagType.Byte:
                if (payload.GetType() != typeof (byte))
                    error = true;
                break;
            case NbtTagType.Short:
                if (payload.GetType() != typeof (short))
                    error = true;
                break;
            case NbtTagType.Int:
                if (payload.GetType() != typeof (int))
                    error = true;
                break;
            case NbtTagType.Long:
                if (payload.GetType() != typeof (long))
                    error = true;
                break;
            case NbtTagType.Float:
                if (payload.GetType() != typeof (float))
                    error = true;
                break;
            case NbtTagType.Double:
                if (payload.GetType() != typeof (double))
                    error = true;
                break;
            case NbtTagType.ByteArray:
                if (payload.GetType() != typeof (byte[]))
                    error = true;
                break;
            case NbtTagType.String:
                if (payload.GetType() != typeof (string))
                    error = true;
                break;
            case NbtTagType.List:
                if (payload.GetType() != typeof (List<NbtTag>))
                    error = true;
                break;
            case NbtTagType.Compound:
                if (payload.GetType() != typeof (Dictionary<string, NbtTag>))
                    error = true;
                break;
            case NbtTagType.IntArray:
                if (payload.GetType() != typeof (int[]))
                    error = true;
                break;
            }

            if (error)
                throw new InvalidOperationException("NBT type differs from NBT payload.");
        }

        /// <summary>
        ///     The name of this tag.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     The tag type of this tag.
        /// </summary>
        public NbtTagType Type { get; private set; }

        /// <summary>
        ///     The payload of this tag.
        /// </summary>
        public object Payload { get; private set; }

        /// <summary>
        ///     Converts this tag to a human readable string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets the size of this tag.
        /// </summary>
        /// <returns>The tag size in bytes.</returns>
        public int Size()
        {
            switch (Type)
            {
            case NbtTagType.End:
                return 1;
            case NbtTagType.Byte:
                return 1;
            case NbtTagType.Short:
                return 2;
            case NbtTagType.Int:
                return 4;
            case NbtTagType.Long:
                return 8;
            case NbtTagType.Float:
                return 4;
            case NbtTagType.Double:
                return 8;
            case NbtTagType.ByteArray:
                return ((byte[])Payload).Length;
            case NbtTagType.String:
                return ((string)Payload).Length * 2;
            case NbtTagType.List:
                return ((List<NbtTag>)Payload).Sum(tag => tag.Size());
            case NbtTagType.Compound:
                return ((Dictionary<string, NbtTag>)Payload).Sum(tag => tag.Value.Size());
            case NbtTagType.IntArray:
                return ((int[])Payload).Length * 4;
            }

            throw new InvalidOperationException("Cannot get size of invalid tag type.");
        }
    }
}
