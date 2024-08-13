namespace DecentraCloud.API.DTOs
{
    public class NodeStatusDto
    {
        public string NodeId { get; set; }
        public List<DateTime> Uptime { get; set; } = new List<DateTime>();
        public List<Dictionary<string, object>> Downtime { get; set; } = new List<Dictionary<string, object>>();
        public StorageStatsDto StorageStats { get; set; }
        public bool IsOnline { get; set; }
        public string CauseOfDowntime { get; set; }
    }
}
