using System;
using System.Collections.Generic;

using System.Text;

namespace NBT.Util
{
    /// <summary>
    /// Utility class for converting Endian bit order.
    /// </summary>
    public static class EndianConverter
    {
        /// <summary>
        /// Swaps a short integer value
        /// </summary>
        /// <param name="value">The value to be swapped</param>
        /// <returns>The swapped value</returns>
        public static   Int16   SwapInt16   (Int16 value)
        {
            byte[] cVal = BitConverter.GetBytes(value);

            Array.Reverse(cVal);

            return BitConverter.ToInt16(cVal, 0);
        }

        /// <summary>
        /// Swaps an integer value
        /// </summary>
        /// <param name="value">The value to be swapped</param>
        /// <returns>The swapped value</returns>
        public static   Int32   SwapInt32   (Int32 value)
        {
            byte[] cVal = BitConverter.GetBytes(value);

            Array.Reverse(cVal);

            return BitConverter.ToInt32(cVal, 0);            
        }
        
        /// <summary>
        /// Swaps a long integer value
        /// </summary>
        /// <param name="value">The value to be swapped</param>
        /// <returns>The swapped value</returns>
        public static   Int64   SwapInt64   (Int64 value)
        {
            byte[] cVal = BitConverter.GetBytes(value);

            Array.Reverse(cVal);

            return BitConverter.ToInt64(cVal, 0);
        }
        
        /// <summary>
        /// Swaps a single precision floating-point value
        /// </summary>
        /// <param name="value">The value to be swapped</param>
        /// <returns>The swapped value</returns>
        public static   Single  SwapSingle  (Single value)
        {
            byte[] cVal = BitConverter.GetBytes(value);

            Array.Reverse(cVal);

            return BitConverter.ToSingle(cVal, 0);
        }
        
        /// <summary>
        /// Swaps a double precision floating-point value
        /// </summary>
        /// <param name="value">The value to be swapped</param>
        /// <returns>The swapped value</returns>
        public static   Double  SwapDouble  (Double value)
        {
            byte[] cVal = BitConverter.GetBytes(value);

            Array.Reverse(cVal);

            return BitConverter.ToDouble(cVal, 0);
        }
    }
}
