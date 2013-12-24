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
        public int      XPos            { get; set; }
        public int      ZPos            { get; set; }

        public Bitmap   RenderedChunk   { get; set; }
    }

    public struct RegionEntry
    {
        public int      XPos            { get; set; }
        public int      ZPos            { get; set; }

        public Bitmap   RenderedRegion  { get; set; }
    }

    public class WorldProcessor
    {
        public event Action<string>     ProcessFailed;
        public event Action             ProcessStarted;
        public event Action             ProcessComplete;

        public event Action<RegionFile> RegionLoaded;
        public event Action             RegionRendered;

        public event Action<float>      ProgressChanged;

        // -----------------------------------------------------------------------------------------------------------------------

        private readonly string         worldLocation   = "";

        private Blocks                  theBlocks;

        // -----------------------------------------------------------------------------------------------------------------------

        public bool                     DrawHeightmap
        {
            get;
            set;
        }

        public bool                     DrawBiomes
        {
            get;
            set;
        }

        // -----------------------------------------------------------------------------------------------------------------------

        public                          WorldProcessor  (string worldLocation)
        {
            this.theBlocks      = new Blocks();
            this.worldLocation  = worldLocation;
        }

        public void                     Start           ()
        {
            if (ProcessStarted != null)
                ProcessStarted();

            int index           = 0;
            var rendererUtil    = new RendererUtil();

            // obtain file list
            var regionList      = Directory.GetFiles(Path.Combine(worldLocation, "region"), "*.mca").ToList();
            var regionEntries   = new List<RegionEntry>();

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

                        using (var renderTarget = new BitmapRenderTarget(rendererUtil.D2DDeviceContext, CompatibleRenderTargetOptions.None, new DrawingSizeF(16, 16), new DrawingSize(16, 16), new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied)))
                        {
                            foreach (var anvilChunk in regionFile.Content.Select(chunk => new Anvil(chunk)))
                            {
                                ChunkEntry entry;
                                RenderSegment(anvilChunk, renderTarget, out entry);

                                renderedChunks.Add(entry);
                            }
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
                                RenderedRegion  = renderTarget.Bitmap,
                                XPos            = Convert.ToInt32(info[1]),
                                ZPos            = Convert.ToInt32(info[2])
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

            var ResultingBitmap = new Bitmap1(rendererUtil.D2DDeviceContext,
                                              new DrawingSize(wSizeX, wSizeZ),
                                              new BitmapProperties1
                                              {
                                                  BitmapOptions = BitmapOptions.Target,
                                                  PixelFormat   = new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied)
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

            #endregion

            #region File save

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

            #endregion

            #region Cleanup

            file.Close();
            file.Dispose();

            foreach (RegionEntry bitmap in regionEntries)
            {
                bitmap.RenderedRegion.Dispose();
            }

            regionEntries.Clear();

            rendererUtil.Dispose();
            theBlocks.Dispose();

            ResultingBitmap.Dispose();

            #endregion

            if (ProcessComplete != null)
                ProcessComplete();
        }

        public void                     RenderSegment   (Anvil anvil, RenderTarget renderTarget, out ChunkEntry output)
        {
            // create char array to hold rendered blocks
            int[] drawnChunk = new int[16 * 16]; // x * z * 4 where 4 is colors

            // render code here
            foreach (AnvilSection section in anvil.Sections)
            {
                // render blocks
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        for (int x = 0; x < 16; x++)
                        {
                            int     index       = y * 256 + z * 16 + x;
                            int     dindex      = z *  16 + x;

                            byte    blockId     = section.Blocks[index];

                            if (blockId == 0 || !theBlocks.BlockList.ContainsKey(blockId))
                                continue;

                            var     blockCol    = theBlocks.BlockList[blockId];
                            var     finalCol    = new byte[4];
                            var     prevCol     = BitConverter.GetBytes(drawnChunk[dindex]);

                            finalCol[0] = processAlpha(blockCol[0], prevCol[0], blockCol[3] / 255.0f);
                            finalCol[1] = processAlpha(blockCol[1], prevCol[1], blockCol[3] / 255.0f);
                            finalCol[2] = processAlpha(blockCol[2], prevCol[2], blockCol[3] / 255.0f);
                            finalCol[3] = 255;

                            drawnChunk[dindex] = BitConverter.ToInt32(finalCol, 0);
                        }
                    }
                }
            }

            // render heightmap
            for (int z = 0; z < 16; z++)
            {
                for (int x = 0; x < 16; x++)
                {
                    int dindex      = z * 16 + x;

                    var value       = (byte)MathUtil.Clamp((anvil.HeightMap[z * 16 + x] - 64) * 2, 0, 255);
                    var finalCol    = new byte[4];
                    var prevCol     = BitConverter.GetBytes(drawnChunk[dindex]);

                    finalCol[0] = processAlpha(255, prevCol[0], value / 255.0f);
                    finalCol[1] = processAlpha(255, prevCol[1], value / 255.0f);
                    finalCol[2] = processAlpha(255, prevCol[2], value / 255.0f);
                    finalCol[3] = 255;

                    drawnChunk[dindex] = BitConverter.ToInt32(finalCol, 0);
                }
            }

            Bitmap newBitmap = new Bitmap(renderTarget, new DrawingSize(16, 16), new BitmapProperties()
                {
                    PixelFormat = new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied)
                });
                   newBitmap.CopyFromMemory(drawnChunk, 16 * 4);

            output = new ChunkEntry
            {
                XPos            = anvil.XPos,
                ZPos            = anvil.ZPos,
                RenderedChunk   = newBitmap
            };
        }

        private byte                    processAlpha    (byte newInput, byte previousInput, float alpha)
        {
            return (byte)(newInput * alpha + previousInput * (1 - alpha));
        }
    }
}
