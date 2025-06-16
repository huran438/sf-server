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

        // MVC might pass a null object when an action returns NotFound() or
        // similar results. Skip serialization in that case so the response body
        // stays empty and the status code is preserved.
        if (context.Object is null)
        {
            return;
        }

        // context.ObjectType can be null when MVC cannot determine the
        // declared return type. Falling back to the runtime type avoids passing
        // a null key into MemoryPack's internal type cache.
        var objectType = context.ObjectType ?? context.Object.GetType();

        var buffer = MemoryPackSerializer.Serialize(objectType, context.Object);
        await using var brotliStream = new BrotliStream(httpContext.Response.Body, CompressionMode.Compress, leaveOpen: true);
        await brotliStream.WriteAsync(buffer);
    }
}
