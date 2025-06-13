using MemoryPack;

namespace SFServer.Shared.Client.Common
{
    [MemoryPackable]
    public partial class DeviceInfo
    {
        public string DeviceModel { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string OperatingSystem { get; set; }
        public string ProcessorType { get; set; }
        public int ProcessorCount { get; set; }
        public int SystemMemorySize { get; set; }
        public string GraphicsDeviceName { get; set; }
        public string GraphicsDeviceVendor { get; set; }
        public int GraphicsMemorySize { get; set; }
        public string GraphicsDeviceVersion { get; set; }
        public string GraphicsDeviceType { get; set; }
        public string GraphicsShaderLevel { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public float ScreenDpi { get; set; }
        public bool FullScreen { get; set; }
    }
}