namespace DecentraCloud.API.DTOs
{
    public class NodeStatusDto
    {
        public string NodeId { get; set; }
        public List<DateTime> Uptime { get; set; } = new List<DateTime>();
        public List<Dictionary<string, object>> Downtime { get; set; } = new List<Dictionary<string, object>>();
        public StorageStatsDto StorageStats { get; set; }
        public StorageStatsDto AllocatedFileStorage { get; set; }
        public StorageStatsDto AllocatedDeploymentStorage { get; set; }
        public bool IsOnline { get; set; }
        public Dictionary<string, object> Availability { get; set; }
        public string NodeName { get; set; }
        public string Endpoint { get; set; }
        public string Region { get; set; }
    }
}
