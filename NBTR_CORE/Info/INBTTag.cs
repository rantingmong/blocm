using System;

namespace NBT.Info
{
    /// <summary>
    /// Represents a node in a NBT file.
    /// </summary>
    public interface INBTTag
    {
        /// <summary>
        /// The name of the node.
        /// </summary>
        string      Name        { get; set; }

        /// <summary>
        /// The payload (value) of the node.
        /// </summary>
        dynamic     Payload     { get; set; }

        /// <summary>
        /// The tag type of the node.
        /// </summary>
        TagNodeType Type        { get; set; }
    }
}
