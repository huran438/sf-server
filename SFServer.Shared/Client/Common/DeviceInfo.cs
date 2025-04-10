using MessagePack;

namespace SFServer.Shared.Client.Common
{
    [MessagePackObject]
    public class DeviceInfo
    {
        [Key(0)]
        public string deviceModel;

        [Key(1)]
        public string deviceName;

        [Key(2)]
        public string deviceType;

        [Key(3)]
        public string operatingSystem;

        [Key(4)]
        public string processorType;

        [Key(5)]
        public int processorCount;

        [Key(6)]
        public int systemMemorySize;

        [Key(7)]
        public string graphicsDeviceName;

        [Key(8)]
        public string graphicsDeviceVendor;

        [Key(9)]
        public int graphicsMemorySize;

        [Key(10)]
        public string graphicsDeviceVersion;

        [Key(11)]
        public string graphicsDeviceType;

        [Key(12)]
        public string graphicsShaderLevel;

        [Key(13)]
        public string batteryStatus;

        [Key(14)]
        public float batteryLevel;

        [Key(15)]
        public int screenWidth;

        [Key(16)]
        public int screenHeight;

        [Key(17)]
        public float screenDpi;

        [Key(18)]
        public bool fullScreen;
    }
}