namespace SFServer.Shared.Models.Connection
{
    public class CheckConnectionRequest : ISFServerModel
    {
        public string Credential { get; set; }
    }
}