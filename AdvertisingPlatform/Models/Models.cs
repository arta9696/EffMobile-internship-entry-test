namespace AdvertisingPlatform.Models
{
    public class AdvertisingPlatformModel
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Locations { get; set; } = new();
    }

    public class SearchResult
    {
        public string Location { get; set; } = string.Empty;
        public List<string> Platforms { get; set; } = new();
    }

    public class LoadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int LoadedPlatformsCount { get; set; }
    }
}
