using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Train.Search.WebApplication.Models.Configuration;
using Train.Search.WebApplication.Pagination;

namespace Train.Search.WebApplication.ExternalHttpServices;

public class RailHttpClient : IRailHttpClient
{
    private readonly HttpClient _client;
    private readonly ILogger<RailHttpClient> _logger;
    private RailUris _configuration;

    public RailHttpClient(IHttpClientFactory httpClientFactory, ILogger<RailHttpClient> logger, IOptions<RailUris> options)
    {
        _client = httpClientFactory.CreateClient("irail-client");
        _logger = logger;
        _configuration = options.Value;
    }

    public async Task<HttpResponseMessage> GetAllConnectionsAsync()
    {
        _logger.LogInformation("Connections request started.");
        var result = await _client.GetAsync($"{_configuration.ConnectionsUri}");
        if (result.IsSuccessStatusCode)
        {
            _logger.LogInformation("Connections request failed");
        }

        return result;
    }
    
    private async Task<HttpResponseMessage> GetAllStationsRequestAsync()
    {
        _logger.LogInformation("Stations request started.");
        Console.WriteLine("opa uri:"+_configuration.StationsUri);
        var result = await _client.GetAsync($"{_configuration.StationsUri}");
        if (!result.IsSuccessStatusCode)
        {
            _logger.LogInformation("Stations request failed");
        }
        string jsonContent = await result.Content.ReadAsStringAsync();
        //var deserialized = JsonConvert.DeserializeObject<StationIncomingData>(jsonContent);
        return result;
    }
    
    private async Task<HttpResponseMessage> GetAllDisturbancesRequestAsync()
    {
        _logger.LogInformation("Disturbances request started.");
        Console.WriteLine("opa uri:"+_configuration.StationsUri);
        var result = await _client.GetAsync($"{_configuration.DisturbancesUri}");
        if (!result.IsSuccessStatusCode)
        {
            _logger.LogInformation("Disturbances request failed");
        }
        string jsonContent = await result.Content.ReadAsStringAsync();
        //var deserialized = JsonConvert.DeserializeObject<Disturbance>(jsonContent);
        return result;
    }
    
    public async Task<List<Station>> GetAllStationsAsync()
    {
        _logger.LogInformation("Stations retrieval started");
        var result = await GetAllStationsRequestAsync();
        string jsonContent = await result.Content.ReadAsStringAsync();

        var stationData = JsonConvert.DeserializeObject<StationData>(jsonContent);
        //var totalCount = stationData.Stations.Count();
        var stations = stationData?.Stations
            /*.Skip(page.PageIndex * page.PageSize)
            .Take(page.PageSize)*/
            .ToList();
        return stations ?? [];
    }
    
    public async Task<List<Disturbance>> GetAllDisturbancesAsync()
    {
        _logger.LogInformation("Disturbances retrieval started");
        var result = await GetAllDisturbancesRequestAsync();
        string jsonContent = await result.Content.ReadAsStringAsync();

        var disturbanceData = JsonConvert.DeserializeObject<DisturbanceData>(jsonContent);
        var disturbances = disturbanceData?.Disturbances
            .ToList();
        return disturbances ?? [];
    }
    
    public async Task<PagedResult<Disturbance>> GetAllDisturbancesWithPaginationAsync(Page page)
    {
        _logger.LogInformation("Disturbances retrieval started");
        var result = await GetAllDisturbancesRequestAsync();
        string jsonContent = await result.Content.ReadAsStringAsync();

        var disturbanceData = JsonConvert.DeserializeObject<DisturbanceData>(jsonContent);
        var totalCount = disturbanceData!.Disturbances.Count;
        var disturbances = disturbanceData?.Disturbances
            .Skip(page.PageIndex * page.PageSize)
            .Take(page.PageSize)
            .ToList();
        var pagedResult = new PagedResult<Disturbance>
        {
            Items = disturbances ?? [],
            TotalCount = totalCount
        };
        return pagedResult;
    }

    public async Task<PagedResult<Disturbance>> GetAllDisturbancesWithPaginationAndSortedAsync(
        Page page,
        string? sortingCriteria = null)
    {
        _logger.LogInformation("Disturbances retrieval started");
        var result = await GetAllDisturbancesRequestAsync();
        string jsonContent = await result.Content.ReadAsStringAsync();

        var disturbanceData = JsonConvert.DeserializeObject<DisturbanceData>(jsonContent);
        var totalCount = disturbanceData!.Disturbances.Count;
        var disturbances = disturbanceData?.Disturbances
            .Skip(page.PageIndex * page.PageSize)
            .Take(page.PageSize)
            .ToList();
        var pagedResult = DetermineSortingCriteria(disturbances ?? new List<Disturbance>(), sortingCriteria ?? string.Empty, totalCount);
        return pagedResult;
    }

    private PagedResult<Disturbance> DetermineSortingCriteria(
        List<Disturbance> disturbances,
        string sortingCriteria,
        int totalCount
        )
    {
        return sortingCriteria switch
        {
            "title" => new PagedResult<Disturbance>{
                Items = disturbances.OrderBy(d => d.Title).ToList(),
                TotalCount = disturbances.Count 
            },
            "date" => new PagedResult<Disturbance>{
                Items = disturbances.OrderByDescending(d => d.Timestamp).ToList(),
                TotalCount = disturbances.Count 
            },
            _ => new PagedResult<Disturbance>{
                Items = disturbances,
                TotalCount = totalCount 
            }
        };
    }
    
    public class StationData
    {
        public StationData(string version, string timestamp, List<Station> stations)
        {
            Version = version;
            Timestamp = timestamp;
            Stations = stations;
        }

        public string Version { get; set; }
        public string Timestamp { get; set; }
        [JsonProperty("station")]
        public List<Station> Stations { get; set; }
    }
    public class DisturbanceData
    {
        public DisturbanceData(string version, string timestamp, List<Disturbance> disturbances)
        {
            Version = version;
            Timestamp = timestamp;
            Disturbances = disturbances;
        }

        public string Version { get; set; }
        public string Timestamp { get; set; }
        [JsonProperty("disturbance")]
        public List<Disturbance> Disturbances { get; set; }
    }

    public class Station
    {
        public Station(string id, string name, string locationX, string locationY, string standardname)
        {
            Id = id;
            Name = name;
            LocationX = locationX;
            LocationY = locationY;
            Standardname = standardname;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string LocationX { get; set; }
        public string LocationY { get; set; }
        public string Standardname { get; set; }
    }
    public class Disturbance
    {
        public Disturbance(string id, string title, string description, string link, DateTime timestamp)
        {
            Id = id;
            Title = title;
            Description = description;
            Link = link;
            Timestamp = timestamp;
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime Timestamp { get; set; }
    }
    
    public class UnixTimestampConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) return DateTime.MinValue;
        
            long timestamp = Convert.ToInt64(reader.Value);
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime dateTime)
            {
                writer.WriteValue(new DateTimeOffset(dateTime).ToUnixTimeSeconds());
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }
    }
}

