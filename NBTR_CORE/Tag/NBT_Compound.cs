/*  Minecraft NBT reader
 * 
 *  Copyright 2011 Michael Ong, all rights reserved.
 *  
 *  Any part of this code is governed by the GNU General Public License version 2.
 */
namespace NBT.Tag
{
    using System;
    using System.Collections.Generic;

    public class NBT_Compound : Dictionary<string, NBT_Tag>
    {
        string listName;
        public string ListName
        {
            get { return this.listName; }
            set { this.listName = value; }
        }
    }
}
