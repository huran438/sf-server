using System;
using SFServer.Shared.Client.Common;

namespace SFServer.Shared.Server.UserProfile
{
    public class UserDevice
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

        public void SetInfo(DeviceInfo deviceInfo)
        {
            DeviceModel = deviceInfo.DeviceModel;
            DeviceName = deviceInfo.DeviceName;
            DeviceType = deviceInfo.DeviceType;
            OperatingSystem = deviceInfo.OperatingSystem;
            ProcessorType = deviceInfo.ProcessorType;
            ProcessorCount = deviceInfo.ProcessorCount;
            SystemMemorySize = deviceInfo.SystemMemorySize;
            GraphicsDeviceName = deviceInfo.GraphicsDeviceName;
            GraphicsDeviceVendor = deviceInfo.GraphicsDeviceVendor;
            GraphicsMemorySize = deviceInfo.GraphicsMemorySize;
            GraphicsDeviceVersion = deviceInfo.GraphicsDeviceVersion;
            GraphicsDeviceType = deviceInfo.GraphicsDeviceType;
            GraphicsShaderLevel = deviceInfo.GraphicsShaderLevel;
            ScreenWidth = deviceInfo.ScreenWidth;
            ScreenHeight = deviceInfo.ScreenHeight;
            ScreenDpi = deviceInfo.ScreenDpi;
            FullScreen = deviceInfo.FullScreen;
        }
    }
}