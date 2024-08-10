namespace DecentraCloud.API.Helpers
{
    public static class RegionHelper
    {
        private static readonly Dictionary<string, Dictionary<string, string>> regionMap = new Dictionary<string, Dictionary<string, string>>
        {
            // United States Regions
            {
                "United States", new Dictionary<string, string>
                {
                    { "New York", "us-east-1" },
                    { "Washington D.C.", "us-east-1" },
                    { "San Francisco", "us-west-1" },
                    { "Los Angeles", "us-west-2" },
                    { "Chicago", "us-central-1" },
                    { "Dallas", "us-central-1" },
                    { "Miami", "us-south-1" },
                    { "Seattle", "us-west-2" }
                }
            },
            {
                "US", new Dictionary<string, string>
                {
                    { "New York", "us-east-1" },
                    { "Washington D.C.", "us-east-1" },
                    { "San Francisco", "us-west-1" },
                    { "Los Angeles", "us-west-2" },
                    { "Chicago", "us-central-1" },
                    { "Dallas", "us-central-1" },
                    { "Miami", "us-south-1" },
                    { "Seattle", "us-west-2" }
                }
            },
            // Canada Regions
            {
                "Canada", new Dictionary<string, string>
                {
                    { "Toronto", "ca-central-1" },
                    { "Montreal", "ca-east-1" },
                    { "Vancouver", "ca-west-1" }
                }
            },
            {
                "CA", new Dictionary<string, string>
                {
                    { "Toronto", "ca-central-1" },
                    { "Montreal", "ca-east-1" },
                    { "Vancouver", "ca-west-1" }
                }
            },
            // Europe Regions
            {
                "Germany", new Dictionary<string, string>
                {
                    { "Frankfurt", "eu-central-1" },
                    { "Berlin", "eu-central-1" }
                }
            },
            {
                "DE", new Dictionary<string, string>
                {
                    { "Frankfurt", "eu-central-1" },
                    { "Berlin", "eu-central-1" }
                }
            },
            {
                "United Kingdom", new Dictionary<string, string>
                {
                    { "London", "eu-west-2" }
                }
            },
            {
                "GB", new Dictionary<string, string>
                {
                    { "London", "eu-west-2" }
                }
            },
            {
                "France", new Dictionary<string, string>
                {
                    { "Paris", "eu-west-3" }
                }
            },
            {
                "FR", new Dictionary<string, string>
                {
                    { "Paris", "eu-west-3" }
                }
            },
            {
                "Ireland", new Dictionary<string, string>
                {
                    { "Dublin", "eu-west-1" }
                }
            },
            {
                "IE", new Dictionary<string, string>
                {
                    { "Dublin", "eu-west-1" }
                }
            },
            // Asia-Pacific Regions
            {
                "Singapore", new Dictionary<string, string>
                {
                    { "Singapore", "ap-southeast-1" }
                }
            },
            {
                "SG", new Dictionary<string, string>
                {
                    { "Singapore", "ap-southeast-1" }
                }
            },
            {
                "Japan", new Dictionary<string, string>
                {
                    { "Tokyo", "ap-northeast-1" },
                    { "Osaka", "ap-northeast-3" }
                }
            },
            {
                "JP", new Dictionary<string, string>
                {
                    { "Tokyo", "ap-northeast-1" },
                    { "Osaka", "ap-northeast-3" }
                }
            },
            {
                "Australia", new Dictionary<string, string>
                {
                    { "Sydney", "ap-southeast-2" }
                }
            },
            {
                "AU", new Dictionary<string, string>
                {
                    { "Sydney", "ap-southeast-2" }
                }
            },
            // South America Regions
            {
                "Brazil", new Dictionary<string, string>
                {
                    { "São Paulo", "sa-east-1" }
                }
            },
            {
                "BR", new Dictionary<string, string>
                {
                    { "São Paulo", "sa-east-1" }
                }
            }
        };

        public static string DetermineRegion(string country, string city)
        {
            // Normalize the country input to handle both full name and abbreviation
            country = country.Trim();

            if (regionMap.TryGetValue(country, out var cities))
            {
                if (cities.TryGetValue(city, out var region))
                {
                    return region;
                }
            }

            return "global"; // Default region if country or city not found
        }
    }
}
