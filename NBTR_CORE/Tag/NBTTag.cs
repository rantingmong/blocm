using System;
using System.Collections.Generic;

namespace NBT.Tag
{
    /// <summary>
    /// A NBT tag, the building blocks of a NBT file.
    /// </summary>
    public struct NBTTag
    {
        dynamic                 payload;
        /// <summary>
        /// The payload of this tag.
        /// </summary>
        public dynamic          Payload
        {
            get { return this.payload; }
        }

        byte                    type;
        string                  name;
        /// <summary>
        /// The tag type of this tag.
        /// </summary>
        public byte             Type
        {
            get { return this.type; }
        }
        /// <summary>
        /// The name of this tag.
        /// </summary>
        public string           Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Creates a new NBT tag.
        /// </summary>
        /// <param name="name">The name of the tag.</param>
        /// <param name="type">The type of the tag.</param>
        /// <param name="payload">The payload of the tag.</param>
        public                  NBTTag      (string name, byte type, dynamic payload)
        {
            this.payload = payload;

            this.type = type;
            this.name = name;
        }

        /// <summary>
        /// Converts this tag to a human readable string.
        /// </summary>
        /// <returns></returns>
        public override string  ToString    ()
        {
            string payloadValue = payload.ToString();

            if (payload is List<NBTTag> || payload is Dictionary<string, NBTTag>)
            {
                if (payload is List<NBTTag>)
                {
                    payloadValue = "list ";
                }
                else
                {
                    payloadValue = "cmpd ";
                }

                payloadValue += "items: " + payload.Count;
            }

            return string.Format("name: {0}, value: {1}", name, payloadValue);
        }
    }
}
