using System;
using SFServer.Shared.Client.Base;

namespace SFServer.Shared.Client.Connection
{
    public class CheckConnectionResponse : ISFResponse
    {
        public DateTime ServerTime { get; set; }
        public bool DebugMode { get; set; }
    }
}