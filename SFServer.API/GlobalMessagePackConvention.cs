using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace SFServer.API
{
    public class GlobalMessagePackConvention : IApplicationModelConvention
    {
        private readonly string _mediaType;
        public GlobalMessagePackConvention(string mediaType) => _mediaType = mediaType;

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                foreach (var action in controller.Actions)
                {
                    // Add filters that enforce the use of "application/x-msgpack" on every action.
                    action.Filters.Add(new ConsumesAttribute(_mediaType));
                    action.Filters.Add(new ProducesAttribute(_mediaType));
                }
            }
        }
    }
}