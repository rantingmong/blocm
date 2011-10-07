using System;

namespace NBT.Utils
{
    public struct MCRTStamp
    {
        int         timestamp;
        public int  Timestamp
        {
            get { return this.timestamp; }
            set { this.timestamp = value; }
        }
    }
}
