using System;
using SFServer.Shared.Models.Base;

namespace SFServer.Shared.Models.Connection
{
    public class CheckConnectionResponse : ISFServerModel
    {
        public DateTime ServerTime { get; set; }
        public bool DebugMode { get; set; }
    }
}