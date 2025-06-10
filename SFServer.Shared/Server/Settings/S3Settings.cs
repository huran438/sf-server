using System;
using MemoryPack;

namespace SFServer.Shared.Server.Settings
{
    [MemoryPackable]
    public partial class S3Settings
    {
        public Guid Id { get; set; }
        public string Bucket { get; set; } = string.Empty;
        public string AccessKeyId { get; set; } = string.Empty;
        public string SecretAccessKey { get; set; } = string.Empty;
        public string Region { get; set; } = "us-east-1";
    }
}
