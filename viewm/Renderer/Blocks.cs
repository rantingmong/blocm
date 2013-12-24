using System;
using System.Collections.Generic;

using SharpDX;

namespace viewm.Renderer
{
    public class Blocks : IDisposable
    {
        public Dictionary<byte, byte[]> BlockList = new Dictionary<byte, byte[]>();

        public Blocks()
        {
            // TODO: DON'T USE DIRECT2D COLORS

            var hehe = Color.ForestGreen.ToArray();

            BlockList.Add(0,    new byte[] { 0, 0, 0, 0 });
            BlockList.Add(1,    Color.DimGray.ToArray());
            BlockList.Add(2,    Color.ForestGreen.ToArray());
            BlockList.Add(3,    new byte[] { 185, 122, 87, 255});
            BlockList.Add(4,    Color.Gray.ToArray());
            BlockList.Add(5,    Color.SandyBrown.ToArray());
            BlockList.Add(7,    Color.Black.ToArray());
            BlockList.Add(8,    new byte[] { 0, 162, 232, 90 });
            BlockList.Add(9,    new byte[] { 0, 162, 232, 90 });
            BlockList.Add(10,   new byte[] { Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 90 });
            BlockList.Add(11,   new byte[] { Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 90 });
            BlockList.Add(12,   Color.LightYellow.ToArray());
            BlockList.Add(13,   Color.PaleVioletRed.ToArray());
            BlockList.Add(14,   Color.Gold.ToArray());
            BlockList.Add(15,   Color.Silver.ToArray());
            BlockList.Add(16,   Color.DimGray.ToArray());
            BlockList.Add(17,   Color.SaddleBrown.ToArray());
            BlockList.Add(18,   new byte[] { Color.DarkGreen.R, Color.DarkGreen.G, Color.DarkGreen.B, 64 });
        }

        public void Dispose()
        {

        }
    }
}
