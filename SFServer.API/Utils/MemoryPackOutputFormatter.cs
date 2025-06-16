using System.IO.Compression;
using MemoryPack;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace SFServer.API.Utils;

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

        // context.ObjectType can be null when MVC cannot determine the declared
        // return type. Falling back to the runtime type avoids passing a null
        // key into MemoryPack's internal type cache which causes a
        // ConcurrentDictionary exception.
        var objectType = context.ObjectType ?? context.Object?.GetType() ?? typeof(object);
        var buffer = MemoryPackSerializer.Serialize(objectType, context.Object);
        await using var brotliStream = new BrotliStream(httpContext.Response.Body, CompressionMode.Compress, leaveOpen: true);
        await brotliStream.WriteAsync(buffer);
    }
}