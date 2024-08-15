using System;
using System.Collections.Generic;
using DecentraCloud.API.Models;

namespace DecentraCloud.API.Helpers
{
    public static class NodeStatusHelper
    {
        private static readonly int PingRTTThreshold = 200; // Example threshold in milliseconds
        private static readonly int HighCpuUsageThreshold = 80; // Example CPU usage threshold in percentage
        private static readonly int HighMemoryUsageThreshold = 80; // Example Memory usage threshold in percentage
        private static readonly int FailedAuthAttemptsThreshold = 3; // Example failed attempts threshold

        public static void RecordNodeAvailable(Node node)
        {
            if (node.Availability.ContainsKey("Critical level") && node.Availability["Critical level"].ToString() == "Medium")
            {
                node.Availability["Critical level"] = "None";
                node.Availability["Reason"] = "Node available";
                node.Availability["Timestamp"] = DateTime.UtcNow;
            }
        }

        public static void RecordNodeUnavailable(Node node)
        {
            if (!node.Availability.ContainsKey("Critical level") || node.Availability["Critical level"].ToString() == "None")
            {
                node.Availability["Critical level"] = "Medium";
                node.Availability["Reason"] = "Node unavailable";
                node.Availability["Timestamp"] = DateTime.UtcNow;
                node.Downtime.Add(new Dictionary<string, object>
                {
                    { "Critical level", "Medium" },
                    { "Reason", "Node unavailable" },
                    { "Timestamp", DateTime.UtcNow }
                });
            }
        }

        public static void RecordNodeOffline(Node node)
        {
            node.IsOnline = false;
            node.Availability["Critical level"] = "High";
            node.Availability["Reason"] = "Node offline";
            node.Availability["Timestamp"] = DateTime.UtcNow;
            node.Downtime.Add(new Dictionary<string, object>
            {
                { "Critical level", "High" },
                { "Reason", "Node offline" },
                { "Timestamp", DateTime.UtcNow }
            });
        }

        public static void RecordPoorNetworkConnection(Node node)
        {
            node.Availability["Critical level"] = "Low";
            node.Availability["Reason"] = "Poor network connection";
            node.Availability["Timestamp"] = DateTime.UtcNow;
            node.Downtime.Add(new Dictionary<string, object>
            {
                { "Critical level", "Low" },
                { "Reason", "Poor network connection" },
                { "Timestamp", DateTime.UtcNow }
            });
        }

        public static void RecordExcessiveLatency(Node node)
        {
            node.Availability["Critical level"] = "Medium";
            node.Availability["Reason"] = "Excessive latency";
            node.Availability["Timestamp"] = DateTime.UtcNow;
            node.Downtime.Add(new Dictionary<string, object>
            {
                { "Critical level", "Medium" },
                { "Reason", "Excessive latency" },
                { "Timestamp", DateTime.UtcNow }
            });
        }

        public static void RecordHighResourceUsage(Node node)
        {
            node.Availability["Critical level"] = "Medium";
            node.Availability["Reason"] = "High resource usage";
            node.Availability["Timestamp"] = DateTime.UtcNow;
            node.Downtime.Add(new Dictionary<string, object>
            {
                { "Critical level", "Medium" },
                { "Reason", "High resource usage" },
                { "Timestamp", DateTime.UtcNow }
            });
        }

        public static void RecordFailedAuthAttempts(Node node)
        {
            node.Availability["Critical level"] = "High";
            node.Availability["Reason"] = "Failed authentication attempts";
            node.Availability["Timestamp"] = DateTime.UtcNow;
            node.IsOnline = false;
            node.Downtime.Add(new Dictionary<string, object>
            {
                { "Critical level", "High" },
                { "Reason", "Failed authentication attempts" },
                { "Timestamp", DateTime.UtcNow }
            });
        }

        public static bool CheckPingLatency(int latency, Node node)
        {
            if (latency > PingRTTThreshold)
            {
                RecordExcessiveLatency(node);
                return false;
            }
            return true;
        }

        public static bool CheckResourceUsage(int cpuUsage, int memoryUsage, Node node)
        {
            if (cpuUsage > HighCpuUsageThreshold || memoryUsage > HighMemoryUsageThreshold)
            {
                RecordHighResourceUsage(node);
                return false;
            }
            return true;
        }

        public static bool CheckFailedAuthAttempts(int failedAttempts, Node node)
        {
            if (failedAttempts >= FailedAuthAttemptsThreshold)
            {
                RecordFailedAuthAttempts(node);
                return false;
            }
            return true;
        }
    }
}
