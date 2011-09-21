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

    public class NBTFile
    {
        bool                        named       = false;

        List<NBT_Tag>               list;
        Dictionary<string, NBT_Tag> dict;

        public                      NBTFile     (bool named)
        {
            this.named = named;

            if (!named)
            {
                this.list = new List<NBT_Tag>();
            }
            else
            {
                this.dict = new Dictionary<string, NBT_Tag>();
            }
        }

        /// <summary>
        /// Writes the data currently this data has to a specified stream.
        /// </summary>
        /// <param name="stream">The stream to flush data on.</param>
        /// <returns>Returns true if the operation is successful, otherwise false.</returns>
        public bool                 Save        (Stream stream)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {

                }
            }
            catch (IOException ex)
            {

            }

            return true;
        }
        /// <summary>
        /// Reads the data from a specified stream.
        /// </summary>
        /// <param name="stream">The stream to read the data from.</param>
        /// <returns>Returns true if the operation is successful, otherwise false.</returns>
        public bool                 Open        (Stream stream)
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {

                }
            }
            catch (IOException ex)
            {

            }

            return true;
        }

        /// <summary>
        /// Finds a NBT tag in an unnamed list.
        /// </summary>
        /// <param name="index">The index of the tag.</param>
        /// <returns>Returns the tag given the index specified, otherwise it will return null.</returns>
        public NBT_Tag              FindTag     (int index)
        {
            if (!this.named && this.list.Count >= (index + 1))
            {
                return this.list[index];
            }
            else
            {
                throw new InvalidProgramException();
            }
        }
        /// <summary>
        /// Finds a NBT tag in a named list.
        /// </summary>
        /// <param name="name">The name of the tag.</param>
        /// <returns>Returns the tag given the name specified, otherwise it will return null.</returns>
        public NBT_Tag              FindTag     (string name)
        {
            if (this.named && this.dict.ContainsKey(name))
            {
                return this.dict[name];
            }
            else
            {
                throw new InvalidProgramException();
            }
        }

        /// <summary>
        /// Inserts a NBT tag after the last item of an unnamed or named list.
        /// </summary>
        /// <param name="info">The NBT tag to be added.</param>
        public void                 InsertTag   (NBT_Tag info)
        {
            if (this.named)
            {
                this.dict.Add(info.TagName, info);
            }
            else
            {
                this.list.Add(info);
            }
        }

        /// <summary>
        /// Removes the NBT tag at a specified index.
        /// </summary>
        /// <param name="index">The index of the tag to be removed.</param>
        public void                 RemoveTag   (int index)
        {
            if (!this.named)
            {
                this.list.RemoveAt(index);
            }
        }
        /// <summary>
        /// Removes the NBT tag given a specified key.
        /// </summary>
        /// <param name="name">The key of the tag to be removed.</param>
        public void                 RemoveTag   (string name)
        {
            if (this.named)
            {
                this.dict.Remove(name);
            }
        }

        /// <summary>
        /// Creates and opens a NBT file.
        /// </summary>
        /// <param name="stream">A stream where the NBT file is stored.</param>
        /// <returns>A NBT file that can be read.</returns>
        public static NBTFile       OpenNBT     (Stream stream)
        {
            return null;
        }
    }
}
