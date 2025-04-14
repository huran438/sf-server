using System;
using MessagePack;
using SFServer.Shared.Client.Base;

namespace SFServer.Shared.Client.Connection
{
    [MessagePackObject]
    public class CheckConnectionResponse : ISFResponse
    {
        [Key(0)]
        public DateTime ServerTime { get; set; }

        [Key(1)]
        public bool DebugMode { get; set; }
    }
}