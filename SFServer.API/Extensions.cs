using Newtonsoft.Json;

namespace SFServer.API;

public static class Extensions
{
    public static string ToJson(this object obj, bool indented = false)
    {
        return JsonConvert.SerializeObject(obj, indented ? Formatting.Indented : Formatting.None);
    }
}