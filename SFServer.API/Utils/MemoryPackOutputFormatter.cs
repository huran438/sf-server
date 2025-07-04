using System.IO.Compression;
using MemoryPack;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace SFServer.API.Utils
{
    public class MemoryPackOutputFormatter : OutputFormatter
    {
        public MemoryPackOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/x-memorypack"));
        }

        protected override bool CanWriteType(Type type)
        {
            return true;
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            var httpContext = context.HttpContext;
            httpContext.Response.ContentType = "application/x-memorypack";
            var buffer = MemoryPackSerializer.Serialize(context.ObjectType!, context.Object);
            await using var brotliStream = new BrotliStream(httpContext.Response.Body, CompressionMode.Compress, leaveOpen: true);
            await brotliStream.WriteAsync(buffer);
        }
    }
}