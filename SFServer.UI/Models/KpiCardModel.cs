namespace SFServer.UI.Models
{
    public class KpiCardModel
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public double Change { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}
