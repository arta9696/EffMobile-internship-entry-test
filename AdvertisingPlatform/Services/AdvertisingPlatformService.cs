using AdvertisingPlatform.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdvertisingPlatform.Services
{
    public class AdvertisingPlatformService
    {
        private readonly object _lock = new();
        private List<AdvertisingPlatformModel> _platforms = new();
        private Dictionary<string, List<string>> _locationIndex = new(); //For fast search

        public async Task<LoadResult> LoadPlatformsFromFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new LoadResult
                {
                    Success = false,
                    Message = "File is empty or not provided"
                };
            }

            try
            {
                var platforms = new List<AdvertisingPlatformModel>();

                using var reader = new StreamReader(file.OpenReadStream());
                string? line;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split(':');
                    if (parts.Length != 2)
                        continue;

                    var name = parts[0].Trim();
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    var locationsStr = parts[1].Trim();
                    var locations = locationsStr.Split(',')
                        .Select(l => l.Trim())
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .ToList();
                    if (!locations.Any())
                        continue;

                    platforms.Add(new AdvertisingPlatformModel
                    {
                        Name = name,
                        Locations = locations
                    });
                }

                lock (_lock)
                {
                    _platforms = platforms;
                    _locationIndex.Clear();

                    foreach (var platform in _platforms)
                    {
                        foreach (var location in platform.Locations)
                        {
                            if (_locationIndex.ContainsKey(location))
                                continue;
                            _locationIndex[location] = new List<string>();
                        }
                    }

                    foreach (var location in _locationIndex.Keys)
                    {
                        _locationIndex[location].AddRange(from platform in _platforms
                                                          from platformLocation in platform.Locations
                                                          where location.StartsWith(platformLocation)
                                                          select platform.Name);
                    }
                }

                return new LoadResult
                {
                    Success = true,
                    Message = "Platforms loaded successfully",
                    LoadedPlatformsCount = platforms.Count
                };
            }
            catch (Exception ex)
            {
                return new LoadResult
                {
                    Success = false,
                    Message = $"Error loading platforms: {ex.Message}"
                };
            }
        }

        public SearchResult SearchPlatforms(string locationStr)
        {
            if (string.IsNullOrWhiteSpace(locationStr))
            {
                return new SearchResult
                {
                    Location = locationStr ?? string.Empty,
                    Platforms = new List<string>()
                };
            }

            locationStr = locationStr.Trim();
            if (!locationStr.StartsWith("/"))
            {
                locationStr = "/" + locationStr;
            }

            var result = new List<string>();
            lock (_lock)
            {
                if (_locationIndex.TryGetValue(locationStr, out var matches))
                {
                    result.AddRange(matches);
                }
            }

            return new SearchResult
            {
                Location = locationStr,
                Platforms = result.Distinct().OrderBy(p => p).ToList()
            };
        }
    }
}
