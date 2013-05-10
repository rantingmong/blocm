using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NBT;
using NBT.Formats;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct2D1;
using SharpDX.WIC;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using BitmapInterpolationMode = SharpDX.Direct2D1.BitmapInterpolationMode;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

namespace viewm.Renderer
{
    public struct ChunkEntry
    {
        public int XPos { get; set; }
        public int ZPos { get; set; }

        public Bitmap RenderedChunk { get; set; }
    }

    public struct RegionEntry
    {
        public int XPos { get; set; }
        public int ZPos { get; set; }

        public Bitmap RenderedRegion { get; set; }
    }

    public class WorldProcessor
    {
        public static int BLOCK_SIZE = 1;

        private readonly string worldLocation = "";

        private SolidColorBrush heightmapBrush;
        private Blocks theBlocks;

        public WorldProcessor(string worldLocation)
        {
            this.worldLocation = worldLocation;
        }

        public Bitmap ResultingBitmap { get; private set; }

        public event Action<string> ProcessFailed;
        public event Action ProcessStarted;
        public event Action ProcessComplete;

        public event Action<RegionFile> RegionLoaded;
        public event Action RegionRendered;

        public event Action<float> ProgressChanged;

        public void Start()
        {
            if (ProcessStarted != null)
                ProcessStarted();

            int index = 0;
            var rendererUtil = new RendererUtil();

            theBlocks = new Blocks(rendererUtil.D2DDeviceContext);

            // obtain file list
            List<string> regionList = Directory.GetFiles(Path.Combine(worldLocation, "region"), "*.mca").ToList();
            var regionEntries = new List<RegionEntry>();

            foreach (string region in regionList)
            {
                try
                {
                    // load the region
                    using (RegionFile regionFile = RegionFile.OpenRegion(File.OpenRead(region)))
                    {
                        var renderedChunks = new List<ChunkEntry>();

                        if (RegionLoaded != null)
                            RegionLoaded(regionFile);

                        Debug.WriteLine("Rendering region");

                        DateTime sTime = DateTime.Now;

                        #region Chunk render

                        foreach (NbtFile chunk in regionFile.Content)
                        {
                            ChunkEntry entry;
                            RenderSegment(new Anvil(chunk), rendererUtil.D2DDeviceContext, out entry);

                            renderedChunks.Add(entry);
                        }

                        #endregion

                        #region Region compositor

                        using (var renderTarget = new BitmapRenderTarget(rendererUtil.D2DDeviceContext, CompatibleRenderTargetOptions.None, new DrawingSizeF(512, 512), new DrawingSize(512, 512), new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied)))
                        {
                            renderTarget.BeginDraw();

                            renderTarget.Clear(Color.Transparent);

                            // compose the images
                            foreach (ChunkEntry chunk in renderedChunks)
                            {
                                int cxPos = chunk.XPos % 32;
                                int czPos = chunk.ZPos % 32;

                                if (cxPos < 0)
                                    cxPos = 32 + cxPos;

                                if (czPos < 0)
                                    czPos = 32 + czPos;

                                int xPos = cxPos * 16;
                                int zPos = czPos * 16;

                                renderTarget.Transform = Matrix3x2.Translation(xPos, zPos);
                                renderTarget.DrawBitmap(chunk.RenderedChunk, 1, BitmapInterpolationMode.Linear);
                            }

                            // ReSharper disable PossibleNullReferenceException
                            string[] info = Path.GetFileNameWithoutExtension(region).Split('.');
                            // ReSharper restore PossibleNullReferenceException

                            regionEntries.Add(new RegionEntry
                                {
                                    RenderedRegion = renderTarget.Bitmap,
                                    XPos = Convert.ToInt32(info[1]),
                                    ZPos = Convert.ToInt32(info[2])
                                });

                            renderTarget.EndDraw();
                        }

                        #endregion

                        Debug.WriteLine("Render time is: " + (DateTime.Now - sTime).Seconds + " seconds.");

                        if (RegionRendered != null)
                            RegionRendered();

                        #region Cleanup

                        foreach (ChunkEntry chunk in renderedChunks)
                            chunk.RenderedChunk.Dispose();

                        renderedChunks.Clear();

                        #endregion
                    }
                }
                catch (Exception exception)
                {
                    if (ProcessFailed != null)
                        ProcessFailed(exception.Message + "\nAt:\n" + exception);
                }

                if (ProgressChanged != null)
                    ProgressChanged(++index / (float)regionList.Count);
            }

            #region Extrema processor

            int xMin = 0;
            int zMin = 0;

            int xMax = 0;
            int zMax = 0;

            foreach (RegionEntry entry in regionEntries)
            {
                if (xMin > entry.XPos)
                    xMin = entry.XPos;

                if (xMax < entry.XPos)
                    xMax = entry.XPos;

                if (zMin > entry.ZPos)
                    zMin = entry.ZPos;

                if (zMax < entry.ZPos)
                    zMax = entry.ZPos;
            }

            int wSizeX = (xMax - xMin) * 512 + 512;
            int wSizeZ = (zMax - zMin) * 512 + 512;

            xMin = Math.Abs(xMin);
            zMin = Math.Abs(zMin);

            #endregion

            #region World compositor

            ResultingBitmap = new Bitmap1(rendererUtil.D2DDeviceContext,
                                          new DrawingSize(wSizeX, wSizeZ),
                                          new BitmapProperties1
                                              {
                                                  BitmapOptions = BitmapOptions.Target,
                                                  PixelFormat = new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied)
                                              });

            rendererUtil.D2DDeviceContext.Target = ResultingBitmap;

            rendererUtil.D2DDeviceContext.BeginDraw();

            rendererUtil.D2DDeviceContext.Clear(Color.Transparent);

            foreach (RegionEntry entry in regionEntries)
            {
                int xPos = ((xMin + entry.XPos) * 512);
                int zPos = ((zMin + entry.ZPos) * 512);

                rendererUtil.D2DDeviceContext.Transform = Matrix3x2.Translation(xPos, zPos);
                rendererUtil.D2DDeviceContext.DrawBitmap(entry.RenderedRegion, 1, BitmapInterpolationMode.Linear);
            }

            rendererUtil.D2DDeviceContext.EndDraw();

            FileStream file = File.OpenWrite(Path.GetFileName(worldLocation) + ".png");

            var encoder = new PngBitmapEncoder(rendererUtil.ImagingFactory);
            encoder.Initialize(file);

            var frameEncode = new BitmapFrameEncode(encoder);
            frameEncode.Initialize();

            var imageEncoder = new ImageEncoder(rendererUtil.ImagingFactory, rendererUtil.D2DDevice);
            imageEncoder.WriteFrame(ResultingBitmap,
                                    frameEncode,
                                    new ImageParameters(
                                        new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied),
                                        96,
                                        96,
                                        0,
                                        0,
                                        wSizeX,
                                        wSizeZ));

            frameEncode.Commit();
            encoder.Commit();

            file.Close();
            file.Dispose();

            #endregion

            #region Cleanup

            foreach (RegionEntry bitmap in regionEntries)
            {
                bitmap.RenderedRegion.Dispose();
            }

            regionEntries.Clear();

            rendererUtil.Dispose();
            theBlocks.Dispose();

            #endregion

            if (ProcessComplete != null)
                ProcessComplete();
        }

        public void RenderSegment(Anvil anvil, RenderTarget renderTarget, out ChunkEntry output)
        {
            if (heightmapBrush == null)
                heightmapBrush = new SolidColorBrush(renderTarget, Color.Black);

            var theRectangle = new RectangleF();

            using (var context = new BitmapRenderTarget(renderTarget, CompatibleRenderTargetOptions.None, new DrawingSizeF(16, 16), new DrawingSize(16, 16), new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied)))
            {
                // render code here
                context.BeginDraw();

                foreach (AnvilSection section in anvil.Sections)
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

                                context.FillRectangle(theRectangle, theBlocks.BlockList[blockId]);
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

                        heightmapBrush.Opacity = MathUtil.Clamp((((float)anvil.HeightMap[z * 16 + x] - 64) / 256) * 2, 0, 1);

                        context.FillRectangle(theRectangle, heightmapBrush);
                    }
                }

                context.EndDraw();

                output = new ChunkEntry { XPos = anvil.XPos, ZPos = anvil.ZPos, RenderedChunk = context.Bitmap };
            }
        }
    }
}
