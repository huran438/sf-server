using System;
using MemoryPack;
using SFServer.Shared.Client.Base;

namespace SFServer.Shared.Client.Session
{
    [MemoryPackable]
    public partial class EndSessionRequest : SFRequest
    {
        public Guid SessionId { get; set; }

        public override string Endpoint => "Session/end";
    }
}