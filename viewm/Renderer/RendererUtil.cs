using System;
using SharpDX.DXGI;
using SharpDX.WIC;
using D2D = SharpDX.Direct2D1;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

namespace viewm.Renderer
{
    public class RendererUtil : IDisposable
    {
        private readonly D2D.Device d2dDevice;
        private readonly D2D.DeviceContext d2dDeviceContext;
        private readonly D2D.Factory d2dFactory;
        private readonly D3D11.Device1 d3dDevice;
        private readonly Device dxgiDevice;
        private readonly ImagingFactory2 imagingFactory;

        public RendererUtil()
        {
            var defaultDevice = new D3D11.Device(D3D.DriverType.Hardware, D3D11.DeviceCreationFlags.BgraSupport);

            d3dDevice = defaultDevice.QueryInterface<D3D11.Device1>();
            dxgiDevice = d3dDevice.QueryInterface<Device>();

            d2dFactory = new D2D.Factory(D2D.FactoryType.MultiThreaded);

            d2dDevice = new D2D.Device(dxgiDevice);
            d2dDeviceContext = new D2D.DeviceContext(d2dDevice, D2D.DeviceContextOptions.None);

            imagingFactory = new ImagingFactory2();
        }

        public D3D11.Device1 D3DDevice
        {
            get { return d3dDevice; }
        }

        public D2D.Device D2DDevice
        {
            get { return d2dDevice; }
        }

        public D2D.DeviceContext D2DDeviceContext
        {
            get { return d2dDeviceContext; }
        }

        public D2D.Factory D2DFactory
        {
            get { return d2dFactory; }
        }

        public ImagingFactory2 ImagingFactory
        {
            get { return imagingFactory; }
        }

        public Device DxgiDevice
        {
            get { return dxgiDevice; }
        }

        public void Dispose()
        {
            d2dDevice.Dispose();
            d3dDevice.Dispose();

            d2dDeviceContext.Dispose();

            d2dFactory.Dispose();
            imagingFactory.Dispose();

            dxgiDevice.Dispose();
        }
    }
}
