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

using Bitmap = SharpDX.WIC.Bitmap;
using PixelFormat = SharpDX.WIC.PixelFormat;
using System.Threading;
using System.Threading.Tasks;

namespace viewm.Chunk
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

	public class ChunkProcessor
	{
		// processor pipeline stages
		private readonly RenderTargetProperties rtProp = new RenderTargetProperties
			{
				PixelFormat = new SharpDX.Direct2D1.PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied)
			};

		private readonly string worldLocation = "";

		public Bitmap ResultingBitmap { get; private set; }

		/// <summary>
		///     Creates an instance of the chunk processor.
		/// </summary>
		/// <param name="worldLocation">The location of the level.dat of the world.</param>
		public ChunkProcessor(string worldLocation)
		{
			this.worldLocation = worldLocation;
		}

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

			var index = 0;

			// obtain file list
			var regionList = Directory.GetFiles(Path.Combine(worldLocation, "region"), "*.mca").ToList();
			var regionEntries = new List<RegionEntry>();

			var bitmapProperties = new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(
				                                            Format.R8G8B8A8_UNorm,
				                                            AlphaMode.Unknown));

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

                        foreach (var chunk in regionFile.Content)
                        {
                            ChunkEntry entry;
                            ChunkRenderer.RenderSegment(new Anvil(chunk), out entry);

                            renderedChunks.Add(entry);
                        }

						#endregion

						#region Region compositor

						// initialize composed image
                        var wicBitmap = new Bitmap(ChunkRenderer.GetWicFactory(), 512, 512, PixelFormat.Format32bppPRGBA, BitmapCreateCacheOption.CacheOnDemand);

						using (var renderTarget = new WicRenderTarget(ChunkRenderer.GetD2DFactory(), wicBitmap, rtProp))
						{
							renderTarget.BeginDraw();

							renderTarget.Clear(Color.Transparent);

							// compose the images
							foreach (ChunkEntry chunk in renderedChunks)
                            {
                                SharpDX.Direct2D1.Bitmap bitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(renderTarget, chunk.RenderedChunk, bitmapProperties);

								int cxPos = chunk.XPos % 32;
								int czPos = chunk.ZPos % 32;

								if (cxPos < 0)
									cxPos = 32 + cxPos;

								if (czPos < 0)
									czPos = 32 + czPos;

								int xPos = cxPos * 16;
								int zPos = czPos * 16;

								renderTarget.Transform = Matrix3x2.Translation(xPos, zPos);

								renderTarget.DrawBitmap(bitmap, 1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);

                                bitmap.Dispose();
							}

							renderTarget.EndDraw();
						}

						#endregion

                        Debug.WriteLine("Render time is: " + (DateTime.Now - sTime).Seconds + " seconds.");

						#region Add rendered region to list along with its position

						// ReSharper disable PossibleNullReferenceException
						string[] info = Path.GetFileNameWithoutExtension(region).Split('.');
						// ReSharper restore PossibleNullReferenceException

						regionEntries.Add(new RegionEntry
							{
								RenderedRegion	= wicBitmap,
								XPos			= Convert.ToInt32(info[1]),
								ZPos			= Convert.ToInt32(info[2])
							});

						#endregion

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
					ProgressChanged(++index / (float) regionList.Count);
			}

			#region Extrema processor

			int xMin = 0;
			int zMin = 0;

			int xMax = 0;
			int zMax = 0;

			foreach (var entry in regionEntries)
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

            ResultingBitmap = new Bitmap(ChunkRenderer.GetWicFactory(), wSizeX, wSizeZ, PixelFormat.Format32bppPRGBA, BitmapCreateCacheOption.CacheOnDemand);

            using (var renderTarget = new WicRenderTarget(ChunkRenderer.GetD2DFactory(), ResultingBitmap, rtProp))
            {
                renderTarget.BeginDraw();

                renderTarget.Clear(Color.Transparent);

                foreach (var entry in regionEntries)
                {
                    SharpDX.Direct2D1.Bitmap bitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(renderTarget, entry.RenderedRegion, bitmapProperties);

                    int xPos = ((xMin + entry.XPos) * 512);
                    int zPos = ((zMin + entry.ZPos) * 512);

                    renderTarget.Transform = Matrix3x2.Translation(xPos, zPos);

                    renderTarget.DrawBitmap(bitmap, 1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);

                    bitmap.Dispose();
                }

                renderTarget.EndDraw();
            }

			var file = File.OpenWrite(Path.GetFileName(worldLocation) + ".png");

			PngBitmapEncoder encoder = new PngBitmapEncoder(ChunkRenderer.GetWicFactory());
			encoder.Initialize(file);

			BitmapFrameEncode frame = new BitmapFrameEncode(encoder);
			frame.Initialize();

			frame.SetSize(ResultingBitmap.Size.Width, ResultingBitmap.Size.Height);
			frame.WriteSource(ResultingBitmap);

			frame.Commit();
			encoder.Commit();

			frame.Dispose();
			encoder.Dispose();

			file.Close();
			file.Dispose();

			#endregion

			#region Cleanup

			foreach (var bitmap in regionEntries)
			{
				bitmap.RenderedRegion.Dispose();
			}

			regionEntries.Clear();

			#endregion

			if (ProcessComplete != null)
				ProcessComplete();
		}
	}
}
