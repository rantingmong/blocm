using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct2D1;

namespace viewm.Renderer
{
    public class Blocks : IDisposable
    {
        public Dictionary<byte, SolidColorBrush> BlockList = new Dictionary<byte, SolidColorBrush>();

        public Blocks(RenderTarget renderTarget)
        {
            BlockList.Add(0, new SolidColorBrush(renderTarget, Color.Transparent));
            BlockList.Add(1, new SolidColorBrush(renderTarget, Color.DimGray));
            BlockList.Add(2, new SolidColorBrush(renderTarget, Color.ForestGreen));
            BlockList.Add(3, new SolidColorBrush(renderTarget, new Color(185, 122, 87, 255)));
            BlockList.Add(4, new SolidColorBrush(renderTarget, Color.Gray));
            BlockList.Add(5, new SolidColorBrush(renderTarget, Color.SandyBrown));
            BlockList.Add(7, new SolidColorBrush(renderTarget, Color.Black));
            BlockList.Add(8, new SolidColorBrush(renderTarget, new Color(0, 162, 232, 255)) { Opacity = 0.35f });
            BlockList.Add(9, new SolidColorBrush(renderTarget, new Color(0, 162, 232, 255)) { Opacity = 0.35f });
            BlockList.Add(10, new SolidColorBrush(renderTarget, Color.DarkOrange) { Opacity = 0.35f });
            BlockList.Add(11, new SolidColorBrush(renderTarget, Color.DarkOrange) { Opacity = 0.35f });
            BlockList.Add(12, new SolidColorBrush(renderTarget, Color.LightYellow));
            BlockList.Add(13, new SolidColorBrush(renderTarget, Color.PaleVioletRed));
            BlockList.Add(14, new SolidColorBrush(renderTarget, Color.Gold));
            BlockList.Add(15, new SolidColorBrush(renderTarget, Color.Silver));
            BlockList.Add(16, new SolidColorBrush(renderTarget, Color.DimGray));
            BlockList.Add(17, new SolidColorBrush(renderTarget, Color.SaddleBrown));
            BlockList.Add(18, new SolidColorBrush(renderTarget, Color.DarkGreen) { Opacity = 0.25f });
        }

        public void Dispose()
        {
            foreach (var solidColorBrush in BlockList)
            {
                solidColorBrush.Value.Dispose();
            }
        }
    }
}
