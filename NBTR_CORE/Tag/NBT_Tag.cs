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

    /// <summary>
    /// Available NBT tag types
    /// </summary>
    public enum TagType
    {
        /// <summary>
        /// Denotes a null (end of structure) tag.
        /// </summary>
        TAG_END,
        /// <summary>
        /// Denotes a 8-bit value tag.
        /// </summary>
        TAG_BYTE,
        /// <summary>
        /// Denotes a 16-bit value tag.
        /// </summary>
        TAG_SHORT,
        /// <summary>
        /// Denotes a 32-bit value tag.
        /// </summary>
        TAG_INT,
        /// <summary>
        /// Denotes a 64-bit value tag.
        /// </summary>
        TAG_LONG,
        /// <summary>
        /// Denotes a single-precision value tag.
        /// </summary>
        TAG_FLOAT,
        /// <summary>
        /// Denotes a double-precision value tag.
        /// </summary>
        TAG_DOUBLE,
        /// <summary>
        /// Denotes a sequential array of bytes.
        /// </summary>
        TAG_BYTE_ARRAY,
        /// <summary>
        /// Denotes a UTF-8 compatible string.
        /// </summary>
        TAG_STRING,
        /// <summary>
        /// Denotes an ordered, unnamed list of NBT tags.
        /// </summary>
        TAG_LIST,
        /// <summary>
        /// Denotes an ordered, named list of NBT tags.
        /// </summary>
        TAG_COMPOUND,
    }

    /// <summary>
    /// Represents the basic structure of a Named Binary Tag file.
    /// </summary>
    public class NBT_Tag
    {
        #region FIELDS

        string name;
        /// <summary>
        /// The name of the tag.
        /// </summary>
        public string   TagName
        {
            get { return this.name; }
        }

        TagType type;
        /// <summary>
        /// The payload type of the tag.
        /// </summary>
        public TagType  TagType
        {
            get { return this.type; }
        }

        dynamic payload;
        /// <summary>
        /// The value of the tag.
        /// </summary>
        public dynamic  TagPayload
        {
            get { return this.payload; }
        }

        #endregion

        #region CTOR

        /// <summary>
        /// Creates a new NBT tag.
        /// </summary>
        /// <param name="name">The name (identifier) of the tag.</param>
        /// <param name="type">The payload type of the tag.</param>
        /// <param name="payload">The value of the tag. </param>
        public NBT_Tag(string name, TagType type, dynamic payload)
        {
            this.name       = name;
            this.type       = type;
            this.payload    = payload;
        }

        #endregion

        #region OVERRIDES

        public override string ToString()
        {
            return string.Format("Tag name: {0} (value = {1})", name, payload);
        }

        #endregion
    }
}
