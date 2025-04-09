using System;
using MessagePack;
using SFServer.Shared.Models.Base;
using SFServer.Shared.Models.Common;

namespace SFServer.Shared.Models.Auth
{
    
    [MessagePackObject]
    public class LoginRequestBase : ISFServerMsgPackRequest
    {
        [Key(0)]
        public string Credential { get; set; }
        
        [Key(1)]
        public string DeviceId { get; set; }

        [Key(2)]
        public DeviceInfo DeviceInfo { get; set; }

        [Key(3)]
        public ApplicationInfo ApplicationInfo { get; set; }
    }
}