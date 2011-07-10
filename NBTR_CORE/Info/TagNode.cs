using System;

namespace NBT.Info
{
    /// <summary>
    /// Basic unit of a node in a NBT file.
    /// </summary>
    public class TagNode : INBTTag
    {
        private         string          _name;
        
        /// <summary>
        /// Gets or sets the name for this node.
        /// </summary>
        public          string          Name
        {
            get { return this._name; }
            set { this._name = value; }
        }
        
        
        private         dynamic         _payload;
        
        /// <summary>
        /// Gets or sets the value (payload) of this node.
        /// </summary>
        public          dynamic         Payload
        {
            get { return this._payload; }
            set { this._payload = value; }
        }


        private         TagNodeType     _type;
        
        /// <summary>
        /// Gets or sets the TAG_TYPE for this node.
        /// </summary>
        public          TagNodeType     Type
        {
            get { return this._type; }
            set { this._type = value; }
        }
                         
   
        /// <summary>
        /// Creates a new tag node.
        /// </summary>
        /// <param name="type">The TAG_TYPE of the node.</param>
        /// <param name="name">The name of the node.</param>
        /// <param name="payload">The value (payload) of the node.</param>
        public                          TagNode     (TagNodeType type, string name, dynamic payload)
        {
            this._type      = type;
            this._name      = name;
            this._payload   = payload;
        }


        /// <summary>
        /// Returns a System.String that represents this TagNode.
        /// </summary>
        /// <returns>The System.String that represents the current TagNode.</returns>
        public override string          ToString    ()
        {
            return string.Format("Node has a name of {0} with value {2} (type {1}).", this._name, this._type, this._payload);
        }
    }
}
