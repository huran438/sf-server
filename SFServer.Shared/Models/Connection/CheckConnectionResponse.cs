using System;

namespace SFServer.Shared.Models.Connection
{
    public class CheckConnectionResponse : ISFServerModel
    {
        public DateTime ServerTime { get; set; }
        public bool DebugMode { get; set; }
    }
}