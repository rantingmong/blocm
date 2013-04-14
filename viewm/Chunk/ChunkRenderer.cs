using NBT.Formats;
using SharpDX;
using SharpDX.DXGI;
using System.Collections.Generic;
using viewm.Chunk;
using D2D = SharpDX.Direct2D1;
using WIC = SharpDX.WIC;

namespace viewm
{
    public class Blocks
    {
        public Dictionary<byte, D2D.SolidColorBrush> BlockList = new Dictionary<byte, D2D.SolidColorBrush>();

        public Blocks(D2D.RenderTarget renderTarget)
        {
            BlockList.Add(0, new D2D.SolidColorBrush(renderTarget, Color.Transparent));
            BlockList.Add(1, new D2D.SolidColorBrush(renderTarget, Color.DimGray));
            BlockList.Add(2, new D2D.SolidColorBrush(renderTarget, Color.ForestGreen));
            BlockList.Add(3, new D2D.SolidColorBrush(renderTarget, new Color(185, 122, 87, 255)));
            BlockList.Add(4, new D2D.SolidColorBrush(renderTarget, Color.Gray));
            BlockList.Add(5, new D2D.SolidColorBrush(renderTarget, Color.SandyBrown));
            BlockList.Add(7, new D2D.SolidColorBrush(renderTarget, Color.Black));
            BlockList.Add(8, new D2D.SolidColorBrush(renderTarget, new Color(0, 162, 232, 255)) { Opacity = 0.35f });
            BlockList.Add(9, new D2D.SolidColorBrush(renderTarget, new Color(0, 162, 232, 255)) { Opacity = 0.35f });
            BlockList.Add(10, new D2D.SolidColorBrush(renderTarget, Color.DarkOrange) { Opacity = 0.35f });
            BlockList.Add(11, new D2D.SolidColorBrush(renderTarget, Color.DarkOrange) { Opacity = 0.35f });
            BlockList.Add(12, new D2D.SolidColorBrush(renderTarget, Color.LightYellow));
            BlockList.Add(13, new D2D.SolidColorBrush(renderTarget, Color.PaleVioletRed));
            BlockList.Add(14, new D2D.SolidColorBrush(renderTarget, Color.Gold));
            BlockList.Add(15, new D2D.SolidColorBrush(renderTarget, Color.Silver));
            BlockList.Add(16, new D2D.SolidColorBrush(renderTarget, Color.DimGray));
            BlockList.Add(17, new D2D.SolidColorBrush(renderTarget, Color.SaddleBrown));
            BlockList.Add(18, new D2D.SolidColorBrush(renderTarget, Color.DarkGreen) { Opacity = 0.25f });
        }
    }

    public class ChunkRenderer
    {
        public static int BLOCK_SIZE = 1;

        private static Blocks theBlocks;

        private static D2D.RenderTargetProperties rtProp = new D2D.RenderTargetProperties
                {
                    PixelFormat = new D2D.PixelFormat(Format.R8G8B8A8_UNorm, D2D.AlphaMode.Premultiplied)
                };

        private static D2D.SolidColorBrush hmapBrush;

        private static D2D.Factory factory;
        private static WIC.ImagingFactory imagingFactory;

        public static D2D.Factory GetD2DFactory()
        {
            if (factory == null)
                factory = new D2D.Factory(D2D.FactoryType.MultiThreaded);

            lock (factory)
            {
                return factory;
            }
        }

        public static WIC.ImagingFactory GetWicFactory()
        {
            if (imagingFactory == null)
                imagingFactory = new WIC.ImagingFactory();

            lock (imagingFactory)
            {
                return imagingFactory;
            }
        }

        public static void RenderSegment(Anvil anvil, out ChunkEntry output)
        {
            // i feel bad about doing this ON EVERY FUCKING CHUNK!
            var wicBitmap = new WIC.Bitmap(GetWicFactory(),
                                           16,
                                           16,
                                           WIC.PixelFormat.Format32bppPRGBA,
                                           WIC.BitmapCreateCacheOption.CacheOnLoad);

            var theRectangle = new RectangleF();

            using (var renderTarget = new D2D.WicRenderTarget(GetD2DFactory(), wicBitmap, rtProp))
            {
                renderTarget.AntialiasMode = D2D.AntialiasMode.Aliased;

                if (hmapBrush == null)
                    hmapBrush = new D2D.SolidColorBrush(renderTarget, Color.Black);

                if (theBlocks == null)
                    theBlocks = new Blocks(renderTarget);

                // render code here
                renderTarget.BeginDraw();

                foreach (var section in anvil.Sections)
                {
                    // render blocks
                    for (int y = 0; y < 16; y++)
                    {
                        for (int z = 0; z < 16; z++)
                        {
                            for (int x = 0; x < 16; x++)
                            {
                                byte blockId = section.Blocks[y * 256 + z * 16 + x];

                                if (blockId == 0 || !theBlocks.BlockList.ContainsKey(blockId))
                                    continue;

                                // render the fucking thing
                                theRectangle.Left = x;
                                theRectangle.Top = z;

                                theRectangle.Right = x + BLOCK_SIZE;
                                theRectangle.Bottom = z + BLOCK_SIZE;

                                renderTarget.FillRectangle(theRectangle, theBlocks.BlockList[blockId]);
                            }
                        }
                    }
                }

                // render heightmap
                for (int z = 0; z < 16; z++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        theRectangle.Left = x;
                        theRectangle.Top = z;

                        theRectangle.Right = x + BLOCK_SIZE;
                        theRectangle.Bottom = z + BLOCK_SIZE;

                        hmapBrush.Opacity = MathUtil.Clamp((((float)anvil.HeightMap[z * 16 + x] - 64) / 256) * 2, 0, 1);

                        renderTarget.FillRectangle(theRectangle, hmapBrush);
                    }
                }

                renderTarget.EndDraw();
            }

            output = new ChunkEntry { XPos = anvil.XPos, ZPos = anvil.ZPos, RenderedChunk = wicBitmap };
        }
    }
}
