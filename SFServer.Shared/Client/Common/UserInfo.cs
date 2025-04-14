using System;
using MessagePack;

namespace SFServer.Shared.Client.Common
{
    [MessagePackObject]
    public class DeviceInfo
    {
        [Key(0)]
        public string DeviceModel { get; set; }

        [Key(1)]
        public string DeviceName { get; set; }

        [Key(2)]
        public string DeviceType { get; set; }

        [Key(3)]
        public string OperatingSystem { get; set; }

        [Key(4)]
        public string ProcessorType { get; set; }

        [Key(5)]
        public int ProcessorCount { get; set; }

        [Key(6)]
        public int SystemMemorySize { get; set; }

        [Key(7)]
        public string GraphicsDeviceName { get; set; }
        
        [Key(8)]
        public string GraphicsDeviceVendor { get; set; }

        [Key(9)]
        public int GraphicsMemorySize { get; set; }

        [Key(10)]
        public string GraphicsDeviceVersion { get; set; }

        [Key(11)]
        public string GraphicsDeviceType { get; set; }
        
        [Key(12)]
        public string GraphicsShaderLevel { get; set; }
        
        [Key(13)]
        public int ScreenWidth { get; set; }
        
        [Key(14)]
        public int ScreenHeight { get; set; }
        
        [Key(15)]
        public float ScreenDpi { get; set; }

        [Key(16)]
        public bool FullScreen { get; set; }
    }
}