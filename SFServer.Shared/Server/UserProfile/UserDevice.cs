using System;
using MemoryPack;
using SFServer.Shared.Client.Common;

namespace SFServer.Shared.Server.UserProfile
{
    [MemoryPackable]
    public partial class UserDevice
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string DeviceId { get; set; }
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

        public void SetInfo(UserDeviceInfo userDeviceInfo)
        {
            DeviceModel = userDeviceInfo.DeviceModel;
            DeviceName = userDeviceInfo.DeviceName;
            DeviceType = userDeviceInfo.DeviceType;
            OperatingSystem = userDeviceInfo.OperatingSystem;
            ProcessorType = userDeviceInfo.ProcessorType;
            ProcessorCount = userDeviceInfo.ProcessorCount;
            SystemMemorySize = userDeviceInfo.SystemMemorySize;
            GraphicsDeviceName = userDeviceInfo.GraphicsDeviceName;
            GraphicsDeviceVendor = userDeviceInfo.GraphicsDeviceVendor;
            GraphicsMemorySize = userDeviceInfo.GraphicsMemorySize;
            GraphicsDeviceVersion = userDeviceInfo.GraphicsDeviceVersion;
            GraphicsDeviceType = userDeviceInfo.GraphicsDeviceType;
            GraphicsShaderLevel = userDeviceInfo.GraphicsShaderLevel;
            ScreenWidth = userDeviceInfo.ScreenWidth;
            ScreenHeight = userDeviceInfo.ScreenHeight;
            ScreenDpi = userDeviceInfo.ScreenDpi;
            FullScreen = userDeviceInfo.FullScreen;
        }
    }
}