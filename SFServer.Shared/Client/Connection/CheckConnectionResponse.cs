using System;
using MemoryPack;
using SFServer.Shared.Client.Base;

namespace SFServer.Shared.Client.Connection
{
    [MemoryPackable]
    public partial class CheckConnectionResponse : ISFResponse
    {
        public DateTime ServerTime { get; set; }
        
        public bool DebugMode { get; set; }
    }
}