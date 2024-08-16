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
        public Dictionary<string, object> Availability { get; set; } = new Dictionary<string, object>();
        public string NodeName { get; set; }
        public string Endpoint { get; set; }
        public string Region { get; set; }
    }
}
