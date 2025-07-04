using System.IO.Compression;
using MemoryPack;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace SFServer.API.Utils
{
    public class MemoryPackInputFormatter : InputFormatter
    {
        public MemoryPackInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/x-memorypack"));
        }

        protected override bool CanReadType(Type type)
        {
            return true;
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            Stream bodyStream = new BrotliStream(request.Body, CompressionMode.Decompress);
            using var memoryStream = new MemoryStream();
            await bodyStream.CopyToAsync(memoryStream);
            byte[] buffer = memoryStream.ToArray();
            object result = MemoryPackSerializer.Deserialize(context.ModelType, buffer);
            return await InputFormatterResult.SuccessAsync(result);
        }
    }
}