using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using SharpDX;
using SharpDX.DXGI;
using D2D = SharpDX.Direct2D1;
using WIC = SharpDX.WIC;

namespace viewm
{
	public partial class MapViewer : Panel
	{
		private							D2D.Factory				factory;

		private							D2D.WindowRenderTarget	windowRenderTarget;

		private							D2D.Bitmap				renderBitmap;
		private							WIC.Bitmap				inputBitmap;

		public							WIC.Bitmap				InputBitmap
		{
			get { return inputBitmap; }
			set
			{
				inputBitmap = value;

				if (inputBitmap == null)
					return;

				renderBitmap = D2D.Bitmap.FromWicBitmap(windowRenderTarget, inputBitmap,
				                                        new D2D.BitmapProperties(
					                                        new SharpDX.Direct2D1.PixelFormat(Format.R8G8B8A8_UNorm,
					                                                                          D2D.AlphaMode.Unknown)));

				Invalidate();
			}
		}

		public							MapViewer				()
		{
			InitializeComponent();

			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);

			if (!DesignMode)
				InitD2D();
		}

		public							MapViewer				(IContainer container)
		{
			container.Add(this);

			InitializeComponent();

			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);

			if (!DesignMode)
				InitD2D();
		}

		protected override	void		OnPaint					(PaintEventArgs e)
		{
			if (DesignMode)
			{
				base.OnPaint(e);
			}
			else
			{
				windowRenderTarget.BeginDraw();

				windowRenderTarget.Clear(Color.DimGray);

				if (renderBitmap != null)
				{
					windowRenderTarget.DrawBitmap(renderBitmap, 1, D2D.BitmapInterpolationMode.Linear);
				}

				windowRenderTarget.EndDraw();
			}
		}

		protected override	void		OnSizeChanged			(EventArgs e)
		{
			base.OnSizeChanged(e);

			if (windowRenderTarget != null)
			{
				windowRenderTarget.Resize(new DrawingSize(ClientSize.Width, ClientSize.Height));
				Invalidate();
			}
		}

		private				void		InitD2D					()
		{
			factory = new D2D.Factory();
			
			windowRenderTarget = new D2D.WindowRenderTarget(factory,
			                                                new D2D.RenderTargetProperties(
				                                                new D2D.PixelFormat(Format.R8G8B8A8_UNorm,
				                                                                    D2D.AlphaMode.Premultiplied)),
			                                                new D2D.HwndRenderTargetProperties
				                                                {
					                                                Hwnd = Handle,
					                                                PixelSize = new DrawingSize(ClientSize.Width, ClientSize.Height),
					                                                PresentOptions = D2D.PresentOptions.Immediately
				                                                });
		}
	}
}
