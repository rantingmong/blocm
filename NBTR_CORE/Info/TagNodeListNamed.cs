using System;
using System.Collections.Generic;

namespace NBT.Info
{
    /// <summary>
    /// Collection of named <c>TagNode</c> that is indexed by the node's name.
    /// </summary>
    public class TagNodeListNamed : Dictionary<string, INBTTag>, INBTTag
    {
        /// <summary>
        /// Gets the name of the list.
        /// </summary>
        public              string          Name
        {
            get;
            set;
        }
        
        
        /// <summary>
        /// Gets the value (payload) of the list.
        /// </summary>
        public              dynamic         Payload
        {
            get { return this; }
            set { throw new NotImplementedException(); }
        }


        /// <summary>
        /// Simply returns a TAG_COMPOUND when called.
        /// </summary>
        public              TagNodeType     Type
        {
            get { return TagNodeType.TAG_COMPOUND; }
            set { throw new NotImplementedException(); }
        } 
  

        /// <summary>
        /// Creates a new TagNodeListNamed.
        /// </summary>
        /// <param name="name">The name of the list.</param>
        public                              TagNodeListNamed    (string name)
            : base()
        {
            this.Name = name;
        }


        public override     string          ToString            ()
        {
            string returnFormat = string.Format("List has a name of {0} with {1} child nodes.\n", this.Name, this.Count);

            foreach (INBTTag node in this.Values)
            {
                returnFormat += "\t" + node.ToString() + "\n";
            }

            return returnFormat;
        }
    }
}
