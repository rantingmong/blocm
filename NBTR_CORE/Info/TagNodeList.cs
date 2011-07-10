using System;
using System.Collections.Generic;

namespace NBT.Info
{
    /// <summary>
    /// Collection of unamed <c>TagNode</c> that is integer indexed.
    /// </summary>
    public class TagNodeList : List<INBTTag>, INBTTag
    {
        /// <summary>
        /// Gets or sets the name of the List.
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        

        private             TagNodeType     _cType;
        
        /// <summary>
        /// Gets the TAG_TYPE of the items that the list currently holds.
        /// </summary>
        public              TagNodeType     ChildType
        {
            get { return this._cType; }
        }
        

        /// <summary>
        /// Simply returns a TAG_LIST when called.
        /// </summary>
        public              TagNodeType     Type
        {
            get { return TagNodeType.TAG_LIST; }
            set { throw new NotImplementedException(); }
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
        /// Creates a new TagNodeList.
        /// </summary>
        /// <param name="name">The name of the list.</param>
        /// <param name="contents">The TAG_TYPE of the items currently hold.</param>
        public                              TagNodeList         (string name, TagNodeType contents)
            : base()
        {
            this.Name   = name;
            this._cType = contents;
        }


        public override     string          ToString            ()
        {
            string returnFormat = string.Format("List has a name of {0} with {1} child nodes.\n", this.Name, this.Count);

            foreach (INBTTag node in this)
            {
                returnFormat += "\t" + node.ToString() + "\n";
            }

            return returnFormat;
        }
    }
}
