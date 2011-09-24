/*  Minecraft NBT reader
 * 
 *  Copyright 2011 Michael Ong, all rights reserved.
 *  
 *  Any part of this code is governed by the GNU General Public License version 2.
 */
namespace NBT.Utils
{
    using System;

    /// <summary>
    /// Provides helper methods to convert between byte endianess.
    /// </summary>
    public class EndianessConverter
    {
        public static short     ToInt16     (short value)
        {
            byte[] reverse = BitConverter.GetBytes(value);
            Array.Reverse(reverse);

            return BitConverter.ToInt16(reverse, 0);
        }
        public static int       ToInt32     (int value)
        {
            byte[] reverse = BitConverter.GetBytes(value);
            Array.Reverse(reverse);

            return BitConverter.ToInt32(reverse, 0);
        }
        public static long      ToInt64     (long value)
        {
            byte[] reverse = BitConverter.GetBytes(value);
            Array.Reverse(reverse);

            return BitConverter.ToInt64(reverse, 0);
        }

        public static float     ToSingle    (float value)
        {
            byte[] reverse = BitConverter.GetBytes(value);
            Array.Reverse(reverse);

            return BitConverter.ToSingle(reverse, 0);
        }
        public static double    ToDouble    (double value)
        {
            byte[] reverse = BitConverter.GetBytes(value);
            Array.Reverse(reverse);

            return BitConverter.ToDouble(reverse, 0);
        }
    }
}
